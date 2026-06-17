using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using Newtonsoft.Json.Linq;
using PatchManager.Core.Cache;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Shared;
using ReduxLib.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// The central orchestrator for Lua-based patching: loads patch scripts, registers patches and new assets,
    /// sorts stages topologically, and runs the resulting pipeline against each patched asset.
    /// </summary>
    /// <remarks>
    /// Converter and submodule discovery is performed by <see cref="LuaPatchingModule" /> at module-init time,
    /// which populates <see cref="Converters" /> and <see cref="SubmoduleTypes" /> by scanning every loaded
    /// assembly. Each universe instance then constructs its own <see cref="PatchManagerCore" /> and live
    /// submodule instances, exposes them as the global <c>PM</c> table, and tracks per-mod patch state. Mods
    /// load Lua patches via the <c>LoadPatch*</c> methods, which are expected to register patches and stages
    /// through <c>PM</c>. Once loading is done, <see cref="SetupPatchesForRun" /> finalizes ordering and the
    /// <c>RunAllPatchesFor</c> overloads execute the chain against each asset.
    /// </remarks>
    public class Universe
    {
        /// <summary>
        /// Logs an error from patch loading or execution. Supplied by the host.
        /// </summary>
        public readonly Action<string> ErrorLogger;

        /// <summary>
        /// Logs an informational message from patch loading or execution. Supplied by the host.
        /// </summary>
        public readonly Action<string> MessageLogger;

        /// <summary>
        /// The set of mod IDs the universe was constructed with, used by stage scheduling and
        /// <see cref="PatchManagerCore.Loaded" />.
        /// </summary>
        public readonly HashSet<string> AllMods;

        /// <summary>
        /// Accumulates per-patch apply, skip, removal, and error events across a patching run.
        /// </summary>
        public Summary Summary = new();

        /// <summary>
        /// Creates a new universe, instantiates each registered submodule, and initializes the per-mod stage state.
        /// </summary>
        /// <param name="errorLogger">Callback for error-level logging.</param>
        /// <param name="messageLogger">Callback for informational logging.</param>
        /// <param name="allMods">The mod IDs participating in patching, in load order.</param>
        public Universe(Action<string> errorLogger, Action<string> messageLogger, List<string> allMods)
        {
            ErrorLogger = errorLogger;
            MessageLogger = messageLogger;
            AllMods = Enumerable.ToHashSet(allMods);
            var pmc = new PatchManagerCore(this);
            PatchManagerLibraryInstance = UserData.Create(pmc);
            foreach (var (k, v) in SubmoduleTypes)
            {
                Submodules[k] = UserData.Create(Activator.CreateInstance(v, pmc, this));
            }
        }

        /// <summary>
        /// The <c>PM</c> global, bound to the per-universe <see cref="PatchManagerCore" /> instance.
        /// </summary>
        public DynValue PatchManagerLibraryInstance;

        /// <summary>
        /// Live submodule instances keyed by name. Populated from <see cref="SubmoduleTypes" /> at construction time.
        /// </summary>
        public Dictionary<string, DynValue> Submodules = new();

        /// <summary>
        /// Submodule types discovered at module-init time by <see cref="LuaPatchingModule" />, keyed by their
        /// <see cref="PatchManagerModuleAttribute.SubmoduleName" />.
        /// </summary>
        public static Dictionary<string, Type> SubmoduleTypes = new();

        /// <summary>
        /// Converter instances discovered at module-init time by <see cref="LuaPatchingModule" />, keyed by their
        /// <see cref="ConverterAttribute.Name" />.
        /// </summary>
        public static Dictionary<string, IConverter> Converters = new();

        /// <summary>
        /// Per-mod "previous implicit stage" pointer used by <see cref="PatchManagerCore.ImplicitStage" /> to
        /// chain stages declared by the same mod.
        /// </summary>
        public readonly Dictionary<string, string> LastImplicitWithinMod = new();

        /// <summary>
        /// Global "previous implicit stage" pointer used by <see cref="PatchManagerCore.GlobalStage" /> and
        /// as the fallback for <see cref="LastImplicitWithinMod" />.
        /// </summary>
        public string LastImplicitGlobal = "";

        static Universe()
        {
            UserData.RegistrationPolicy = new FallbackRegistrationPolicy();
        }

        #region Patch/Stage Registering

        /// <summary>
        /// Assets queued for creation via <see cref="PatchManagerCore.New" />.
        /// </summary>
        public List<LuaAsset> AllNewAssets = new();

        /// <summary>
        /// Registered patches keyed by addressables label. Sorted in <see cref="SetupPatchesForRun" /> by stage priority.
        /// </summary>
        public Dictionary<string, List<PatchDefinition>> AllPatches = new();

        /// <summary>
        /// True while patch definitions are being registered. The loading lifecycle closes it once every mod
        /// body has run, after which the Lua definition entrypoints (<c>PM:Patch</c>/<c>PM:New</c>) throw. Patch
        /// application (Do callbacks) and builder/query helpers stay valid regardless of this flag.
        /// </summary>
        public bool RegistrationOpen = true;

        /// <summary>
        /// Registers a patch and records its label in <see cref="PatchedLabels" />.
        /// </summary>
        /// <param name="patch">The patch to register.</param>
        public void AddPatch(PatchDefinition patch)
        {
            if (AllPatches.TryGetValue(patch.Label, out var l))
            {
                l.Add(patch);
            }
            else
            {
                AllPatches[patch.Label] = new List<PatchDefinition> { patch };
            }

            PatchedLabels.Add(patch.Label);
            TotalPatchCount++;
        }

        /// <summary>
        /// Queues a new asset for creation and records its label in <see cref="PatchedLabels" />.
        /// </summary>
        /// <param name="asset">The asset to queue.</param>
        public void AddAsset(LuaAsset asset)
        {
            AllNewAssets.Add(asset);
            PatchedLabels.Add(asset.Label);
        }

        #endregion

        #region Patch Running

        /// <summary>
        /// Total number of patches registered with the universe.
        /// </summary>
        public int TotalPatchCount;

        /// <summary>
        /// The set of addressables labels with at least one patch or new-asset registered. Replaced by
        /// <see cref="SetupPatchesForRun" /> with the keys of <see cref="AllPatches" />.
        /// </summary>
        public HashSet<string> PatchedLabels = new();

        /// <summary>
        /// Per-label, per-pass bucket maps. Each pass holds its own <see cref="LabelPatchBuckets" /> so
        /// pass execution iterates only the patches in that pass.
        /// </summary>
        public Dictionary<string, Dictionary<PatchDefinition.PatchPass, LabelPatchBuckets>> AllPatchesBuckets = new();

        private static readonly char[] WildcardChars = { '*', '?' };

        /// <summary>
        /// Finalizes the patch registry: rebuilds <see cref="PatchedLabels" /> from <see cref="AllPatches" />, then
        /// sorts each label's patches by stage priority.
        /// </summary>
        /// <remarks>
        /// Must run after all patches have been registered and before any <c>RunAllPatchesFor</c> call.
        /// </remarks>
        public void SetupPatchesForRun()
        {
            // Two-phase setup so :Needs/:Conflicts can resolve patches in any label and any pass.
            // Phase 1: run mod-constraint filtering on every label's patches and union the surviving
            // names into a global existence set.
            var perLabelModFiltered = new Dictionary<string, List<PatchDefinition>>(AllPatches.Count);
            var globalAllPatches = new HashSet<string>();
            foreach (var (label, patches) in AllPatches)
            {
                var modConstrained = ApplyModConstraints(patches);
                perLabelModFiltered[label] = modConstrained;
                foreach (var p in modConstrained)
                {
                    globalAllPatches.Add(p.Name);
                }
            }

            // Phase 2: per-label patch-constraint filter against the global set, then sort.
            foreach (var (label, modConstrained) in perLabelModFiltered)
            {
                SetupLabelForRun(label, modConstrained, globalAllPatches);
            }
        }

        private List<PatchDefinition> ApplyModConstraints(List<PatchDefinition> patches)
        {
            var modConstrained = new List<PatchDefinition>(patches.Count);
            foreach (var patch in patches)
            {
                foreach (var mod in patch.NeedsMods)
                {
                    if (!AllMods.Contains(mod))
                    {
                        Summary.Remove(patch.Name, "MISSING", $"mod - {mod}");
                        goto continue_mod;
                    }
                }

                foreach (var mod in patch.ConflictsMods)
                {
                    if (AllMods.Contains(mod))
                    {
                        Summary.Remove(patch.Name, "CONFLICT", $"mod - {mod}");
                        goto continue_mod;
                    }
                }

                modConstrained.Add(patch);
                continue_mod:;
            }
            return modConstrained;
        }

        private void SetupLabelForRun(string label, List<PatchDefinition> modConstrained, HashSet<string> globalAllPatches)
        {
            // Patch-constraint filter against the global existence set. Needs/Conflicts span every
            // label and every pass: a patch in this label's Default pass can require a patch declared
            // in another label's Late pass, and vice versa.
            var patchConstrained = new List<PatchDefinition>(modConstrained.Count);
            foreach (var patch in modConstrained)
            {
                foreach (var id in patch.NeedsPatches)
                {
                    if (!globalAllPatches.Contains(id))
                    {
                        Summary.Remove(patch.Name, "MISSING", $"patch - {id}");
                        goto continue_patch;
                    }
                }

                foreach (var id in patch.ConflictsPatches)
                {
                    if (globalAllPatches.Contains(id))
                    {
                        Summary.Remove(patch.Name, "CONFLICT", $"patch - {id}");
                        goto continue_patch;
                    }
                }

                patchConstrained.Add(patch);
                continue_patch:;
            }

            var perPassBuckets = new Dictionary<PatchDefinition.PatchPass, LabelPatchBuckets>();
            AllPatchesBuckets[label] = perPassBuckets;

            // Within each pass, sort each ordering bucket independently. Before/After targets in other
            // buckets are silently filtered out (treated as if the target did not exist).
            foreach (PatchDefinition.PatchPass pass in Enum.GetValues(typeof(PatchDefinition.PatchPass)))
            {
                var passPatches = patchConstrained.Where(p => p.Pass == pass).ToList();
                if (passPatches.Count == 0)
                {
                    perPassBuckets[pass] = new LabelPatchBuckets();
                    continue;
                }

                var orderedSorted = new List<PatchDefinition>(passPatches.Count);
                foreach (var ord in OrderingSequence)
                {
                    var orderingBucket = passPatches.Where(p => p.Ordering == ord).ToList();
                    orderedSorted.AddRange(SortBucket(orderingBucket));
                }

                for (var i = 0; i < orderedSorted.Count; i++)
                {
                    orderedSorted[i].Order = i;
                }

                var exactGroups = new Dictionary<string, List<PatchDefinition>>();
                var matchAll = new List<PatchDefinition>();
                var wildcard = new List<WildcardEntry>();

                foreach (var p in orderedSorted)
                {
                    if (p.Names.Count == 0 || p.Names.Any(x => x == "*"))
                    {
                        matchAll.Add(p);
                    }
                    else
                    {
                        foreach (var pattern in p.Names)
                        {
                            if (pattern.IndexOfAny(WildcardChars) < 0)
                            {
                                if (!exactGroups.TryGetValue(pattern, out var groups))
                                {
                                    exactGroups[pattern] = groups = new List<PatchDefinition>();
                                }
                                groups.Add(p);
                            }
                            else
                            {
                                wildcard.Add(new WildcardEntry(NamePattern.Get(pattern), p));
                            }
                        }
                    }
                }

                var buckets = new LabelPatchBuckets
                {
                    MatchAll = matchAll.ToArray(),
                    Wildcard = wildcard.ToArray(),
                };
                foreach (var (k, v) in exactGroups)
                {
                    buckets.Exact[k] = v.ToArray();
                }

                perPassBuckets[pass] = buckets;
            }
        }

        private static readonly PatchDefinition.PatchOrdering[] OrderingSequence =
        {
            PatchDefinition.PatchOrdering.First,
            PatchDefinition.PatchOrdering.Default,
            PatchDefinition.PatchOrdering.Last
        };

        private List<PatchDefinition> SortBucket(List<PatchDefinition> bucket)
        {
            if (bucket.Count == 0) return new List<PatchDefinition>();

            var bucketNames = new HashSet<string>(bucket.Count);
            foreach (var p in bucket) bucketNames.Add(p.Name);

            var explosion = new Dictionary<string, List<string>>();
            foreach (var patch in bucket)
            {
                if (!explosion.TryGetValue(patch.PatchModId, out var list))
                {
                    explosion[patch.PatchModId] = list = new List<string>();
                }
                list.Add(patch.Name);
            }

            var afters = new Dictionary<string, HashSet<string>>(bucket.Count);
            var befores = new Dictionary<string, HashSet<string>>(bucket.Count);

            foreach (var patch in bucket)
            {
                var afterSet = new HashSet<string>();
                foreach (var pre in patch.AfterPatches)
                {
                    if (bucketNames.Contains(pre)) afterSet.Add(pre);
                }
                foreach (var mod in patch.AfterMods)
                {
                    if (explosion.TryGetValue(mod, out var list))
                    {
                        foreach (var p in list) afterSet.Add(p);
                    }
                }
                afters[patch.Name] = afterSet;

                var beforeSet = new HashSet<string>();
                foreach (var suc in patch.BeforePatches)
                {
                    if (bucketNames.Contains(suc)) beforeSet.Add(suc);
                }
                foreach (var mod in patch.BeforeMods)
                {
                    if (explosion.TryGetValue(mod, out var list))
                    {
                        foreach (var p in list) beforeSet.Add(p);
                    }
                }
                befores[patch.Name] = beforeSet;
            }

            var inDegree = new Dictionary<string, int>(bucket.Count);
            var outEdges = new Dictionary<string, HashSet<string>>(bucket.Count);
            var namePatchMap = new Dictionary<string, PatchDefinition>(bucket.Count);

            foreach (var patch in bucket)
            {
                inDegree[patch.Name] = 0;
                outEdges[patch.Name] = new HashSet<string>();
                namePatchMap[patch.Name] = patch;
            }

            foreach (var patch in bucket)
            {
                foreach (var pre in afters[patch.Name])
                {
                    if (pre == patch.Name) continue;
                    if (outEdges[pre].Add(patch.Name)) inDegree[patch.Name]++;
                }

                foreach (var suc in befores[patch.Name])
                {
                    if (suc == patch.Name) continue;
                    if (outEdges[patch.Name].Add(suc)) inDegree[suc]++;
                }
            }

            var queue = new Queue<string>();
            foreach (var (n, d) in inDegree)
            {
                if (d == 0) queue.Enqueue(n);
            }

            var sorted = new List<PatchDefinition>(bucket.Count);
            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                sorted.Add(namePatchMap[name]);
                foreach (var suc in outEdges[name])
                {
                    if (--inDegree[suc] == 0) queue.Enqueue(suc);
                }
            }

            if (sorted.Count != bucket.Count)
            {
                var unsorted = bucket.Where(k => !sorted.Contains(k));
                foreach (var patch in unsorted)
                {
                    Summary.Remove(patch.Name, "CYCLE", "patch was caught in a dependency cycle");
                }
            }

            return sorted;
        }

        /// <summary>
        /// Runs every patch matching <paramref name="label" /> / <paramref name="name" /> in
        /// <paramref name="pass" /> against the given JSON, returning the final result.
        /// </summary>
        /// <remarks>
        /// Patches are chained: each one operates on the previous patch's <see cref="DynValue" /> when the converters
        /// match, otherwise the chain is flushed back to JSON, lifted by the new converter, and chaining resumes.
        /// Returns <paramref name="data" /> unchanged when no patch applied. Returns <c>null</c> when a patch
        /// removed the asset.
        /// </remarks>
        /// <param name="label">The asset's addressables label.</param>
        /// <param name="name">The asset's addressables address.</param>
        /// <param name="data">The asset's parsed JSON.</param>
        /// <param name="pass">The pass to run.</param>
        /// <param name="patchCount">Set to the number of patches that ran successfully.</param>
        /// <param name="errorCount">Set to the number of patches that threw.</param>
        /// <returns>The patched JSON, or <c>null</c> when the asset was removed.</returns>
        public JToken RunAllPatchesFor(string label, string name, JToken data, PatchDefinition.PatchPass pass, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            IConverter? previousConverter = null;
            DynValue? previousInstance = null;
            foreach (var patch in GetAllSortedPatchesFor(label, name, pass))
            {
                if (previousInstance == null)
                {
                    previousConverter = patch.ConverterInstance;
                    previousInstance = previousConverter.FromJson(data);
                }

                if (!ReferenceEquals(previousConverter, patch.ConverterInstance))
                {
                    var jValue = previousConverter.ToJson(previousInstance);
                    previousConverter = patch.ConverterInstance;
                    previousInstance = previousConverter.FromJson(jValue);
                }

                if (patch.Apply(previousInstance, Summary, out var removed, out var errored))
                {
                    patchCount++;
                    if (removed)
                    {
                        return null;
                    }
                }

                if (errored) errorCount++;
            }

            return previousConverter?.ToJson(previousInstance) ?? data;
        }

        /// <summary>
        /// Runs every patch matching the given new asset in <paramref name="pass" /> against its
        /// <see cref="LuaAsset.CurrentValue" />, returning the final JSON.
        /// </summary>
        /// <param name="asset">The new asset to patch.</param>
        /// <param name="pass">The pass to run.</param>
        /// <param name="patchCount">Set to the number of patches that ran successfully.</param>
        /// <param name="errorCount">Set to the number of patches that threw.</param>
        /// <returns>The patched JSON for the asset.</returns>
        public JToken RunAllPatchesFor(LuaAsset asset, PatchDefinition.PatchPass pass, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            foreach (var patch in GetAllSortedPatchesFor(asset.Label, asset.Name, pass))
            {

                if (!ReferenceEquals(asset.ConverterInstance, patch.ConverterInstance))
                {
                    var jValue = asset.ConverterInstance.ToJson(asset.CurrentValue);
                    asset.ConverterInstance = patch.ConverterInstance;
                    asset.CurrentValue = asset.ConverterInstance.FromJson(jValue);
                }

                if (patch.Apply(asset.CurrentValue, Summary, out var removed, out var errored))
                {
                    patchCount++;
                    if (removed)
                    {
                        return null;
                    }
                }

                if (errored) errorCount++;
            }

            return asset.ConverterInstance.ToJson(asset.CurrentValue);
        }


        /// <summary>
        /// Returns the patches registered for <paramref name="label" /> in <paramref name="pass" />, in
        /// stage-sorted order, filtered by name pattern.
        /// </summary>
        /// <param name="label">The addressables label to look up.</param>
        /// <param name="name">The asset's addressables address, matched against each patch's name pattern.</param>
        /// <param name="pass">The pass to look up.</param>
        /// <returns>The matching patches, or an empty sequence when no patches are registered for the label in this pass.</returns>
        public IEnumerable<PatchDefinition> GetAllSortedPatchesFor(string label, string name, PatchDefinition.PatchPass pass)
        {
            if (!AllPatchesBuckets.TryGetValue(label, out var perPass)) yield break;
            if (!perPass.TryGetValue(pass, out var buckets)) yield break;

            var exact = buckets.Exact.TryGetValue(name, out var e) ? e : Array.Empty<PatchDefinition>();
            var matchAll = buckets.MatchAll;
            var wildcard = buckets.Wildcard;
            var exactI = 0;
            var matchAllI = 0;
            var wildCardI = 0;

            HashSet<PatchDefinition> alreadyYielded = new(exact.Length+matchAll.Length+wildcard.Length);

            while (true)
            {
                while (wildCardI < wildcard.Length && !wildcard[wildCardI].Pattern.Matches(name)) wildCardI++;

                var exactPatch = exactI < exact.Length ? exact[exactI] : null;
                var matchAllPatch = matchAllI < matchAll.Length ? matchAll[matchAllI] : null;
                var wildcardPatch = wildCardI < wildcard.Length ? wildcard[wildCardI].Patch : null;

                if (exactPatch == null && matchAllPatch == null && wildcardPatch == null) yield break;

                var best = exactPatch;
                if (matchAllPatch != null && (best == null || matchAllPatch.Order < best.Order))
                    best = matchAllPatch;
                if (wildcardPatch != null && (best == null || wildcardPatch.Order < best.Order))
                    best = wildcardPatch;

                if (ReferenceEquals(best, exactPatch)) exactI++;
                else if (ReferenceEquals(best, matchAllPatch)) matchAllI++;
                else wildCardI++;

                if (alreadyYielded.Add(best) && !best.DisallowedNames.Any(x => x.Matches(name))) yield return best;
            }
        }

        /// <summary>
        /// Checks whether any patch matches the given label and name across every pass.
        /// </summary>
        /// <param name="label">The addressables label.</param>
        /// <param name="name">The asset's addressables address.</param>
        /// <returns>True if any patch matches, false otherwise.</returns>
        public bool HasAnyPatchFor(string label, string name)
        {
            if (!AllPatchesBuckets.TryGetValue(label, out var perPass)) return false;
            foreach (var (_, buckets) in perPass)
            {
                if (buckets.MatchAll.Length > 0) return true;
                if (buckets.Exact.ContainsKey(name)) return true;
                if (buckets.Wildcard.Any(w => w.Pattern.Matches(name))) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether any patch in <paramref name="pass" /> matches the given label and name.
        /// </summary>
        /// <param name="label">The addressables label.</param>
        /// <param name="name">The asset's addressables address.</param>
        /// <param name="pass">The pass to check.</param>
        /// <returns>True if any patch in <paramref name="pass" /> matches, false otherwise.</returns>
        public bool HasAnyPatchInPass(string label, string name, PatchDefinition.PatchPass pass)
        {
            if (!AllPatchesBuckets.TryGetValue(label, out var perPass)) return false;
            if (!perPass.TryGetValue(pass, out var buckets)) return false;
            if (buckets.MatchAll.Length > 0) return true;
            if (buckets.Exact.ContainsKey(name)) return true;
            return buckets.Wildcard.Any(w => w.Pattern.Matches(name));
        }

        /// <summary>
        /// Returns the primary Addressables address for every <see cref="UnityEngine.TextAsset" /> under
        /// <paramref name="label" />, keyed by the asset's Unity object name (the file basename without
        /// extension).
        /// </summary>
        /// <param name="label">The Addressables label whose assets to enumerate.</param>
        /// <returns>A map from asset Unity name to primary address, empty when the label resolves to nothing.</returns>
        public Dictionary<string, string> BuildPrimaryKeyMapForLabel(string label)
        {
            var map = new Dictionary<string, string>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(label, typeof(UnityEngine.TextAsset), out var locs) && locs != null)
                {
                    foreach (var loc in locs)
                    {
                        var name = Path.GetFileNameWithoutExtension(loc.PrimaryKey);
                        if (!string.IsNullOrEmpty(name))
                        {
                            map[name] = loc.PrimaryKey;
                        }
                    }
                }
            }

            return map;
        }

        #endregion

        #region utilities

        /// <summary>
        /// A wildcard patch paired with the compiled name pattern it matches against.
        /// </summary>
        public readonly struct WildcardEntry
        {
            /// <summary>
            /// The pattern this patch applies to.
            /// </summary>
            public readonly NamePattern Pattern;

            /// <summary>
            /// The patch itself.
            /// </summary>
            public readonly PatchDefinition Patch;

            /// <summary>
            /// Creates a new wildcard entry.
            /// </summary>
            /// <param name="pattern">The pattern the patch applies to.</param>
            /// <param name="patch">The patch itself.</param>
            public WildcardEntry(NamePattern pattern, PatchDefinition patch)
            {
                Pattern = pattern;
                Patch = patch;
            }
        }

        /// <summary>
        /// The patches for a single label in a single pass, grouped by how they match an asset name.
        /// </summary>
        public sealed class LabelPatchBuckets
        {
            /// <summary>
            /// Patches keyed by the exact asset name they match.
            /// </summary>
            public Dictionary<string, PatchDefinition[]> Exact = new();

            /// <summary>
            /// Patches that match every asset name.
            /// </summary>
            public PatchDefinition[] MatchAll = Array.Empty<PatchDefinition>();

            /// <summary>
            /// Patches that match an asset name by wildcard pattern.
            /// </summary>
            public WildcardEntry[] Wildcard = Array.Empty<WildcardEntry>();
        }

        #endregion
    }
}