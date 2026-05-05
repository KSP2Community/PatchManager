using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Shared;
using ReduxLib.Logging;
using Unity.VisualScripting;
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
    /// through <c>PM</c>; once loading is done, <see cref="SetupPatchesForRun" /> finalizes ordering and the
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

        public Summary Summary = new();
        
        /// <summary>
        /// Creates a new universe, instantiates each registered submodule, and seeds the per-mod stage priorities.
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

        private static PatchManagerScriptLoader _managerScriptLoader = new();

        #region Patch Loading

        /// <summary>
        /// Number of Lua library files (filenames starting with <c>_</c>) discovered across loaded patch directories.
        /// </summary>
        public int LibraryCount = 0;

        /// <summary>
        /// Loads and runs a single <c>.patch</c> Lua file, registering whatever patches it declares.
        /// </summary>
        /// <param name="file">The patch file to load.</param>
        /// <param name="directoryInfo">The directory the patch was discovered in; exposed to the script as the <c>Location</c> global.</param>
        /// <param name="summary">The summary instance to log errors to</param>
        public void LoadSinglePatchFile(FileInfo file, DirectoryInfo directoryInfo)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = Path.GetFileNameWithoutExtension(file.Name),
                    ["Location"] = directoryInfo.FullName,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            
            patchScript.Globals.RegisterModuleType<JsonModule>();
            var relativeName = file.FullName.MakeRelativePathTo(Directory.GetCurrentDirectory());
            try
            {
                patchScript.DoString(File.ReadAllText(file.FullName),
                    codeFriendlyName: relativeName);
            }
            catch (InterpreterException e)
            {
                Summary.ErrorFile(relativeName, e);
                ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                Summary.ErrorFile(relativeName, e);
                ErrorLogger($"{file.FullName} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }

            AllMods.Add(Path.GetFileNameWithoutExtension(file.Name));
        }

        /// <summary>
        /// Loads every <c>.lua</c> file under the given directory (excluding files starting with <c>_</c>) and runs
        /// each one to register its patches.
        /// </summary>
        /// <remarks>
        /// Library files (filenames starting with <c>_</c>) are not executed but counted in <see cref="LibraryCount" />.
        /// All scripts share a single <see cref="Script" /> with the given <paramref name="modId" />.
        /// </remarks>
        /// <param name="directory">The directory containing the patch files.</param>
        /// <param name="modId">The mod ID; exposed to scripts as the <c>ModId</c> global and used as their default stage.</param>
        /// <param name="summary">The summary for logging errors</param>
        public void LoadPatchesInDirectory(DirectoryInfo directory, string modId)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = modId,
                    ["Location"] = directory.FullName,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            AllMods.Add(modId);
            patchScript.Globals.RegisterModuleType<JsonModule>();

            foreach (var file in directory.EnumerateFiles("*.lua", SearchOption.AllDirectories)
                         .Where(f => !f.Name.StartsWith("_")))
            {
                var relativeName = file.FullName.MakeRelativePathTo(Directory.GetCurrentDirectory());
                try
                {
                    patchScript.DoString(File.ReadAllText(file.FullName));
                }
                catch (InterpreterException e)
                {
                    Summary.ErrorFile(relativeName, e);
                    ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    Summary.ErrorFile(relativeName, e);
                    ErrorLogger($"{file.FullName} failed to load: {e.Message}");
                    ErrorLogger(e.ToString());
                }
            }

            LibraryCount += directory.EnumerateFiles("_*.lua", SearchOption.AllDirectories).Count();
        }

        /// <summary>
        /// Loads and runs a single Lua patch from a <see cref="TextAsset" />.
        /// </summary>
        /// <param name="textAsset">The text asset whose contents are the patch script.</param>
        /// <param name="modId">The mod ID; exposed to the script as the <c>ModId</c> global.</param>
        /// <param name="summary">The summary to log errors to</param>
        public void LoadPatchAsset(TextAsset textAsset, string modId)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = modId,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            AllMods.Add(modId);
            patchScript.Globals.RegisterModuleType<JsonModule>();
            try
            {
                patchScript.DoString(textAsset.text);
            }
            catch (InterpreterException e)
            {
                Summary.ErrorFile(textAsset.name, e);
                ErrorLogger($"{textAsset.name} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                Summary.ErrorFile(textAsset.name, e);
                ErrorLogger($"{textAsset.name} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }
        }

        #endregion

        #region Patch/Stage Registering

        /// <summary>
        /// Assets queued for creation via <see cref="PatchManagerCore.New" />.
        /// </summary>
        public List<LuaAsset> AllNewAssets = new();

        /// <summary>
        /// Registered patches keyed by addressables label. Sorted in <see cref="SetupPatchesForRun" /> by stage priority.
        /// </summary>
        public Dictionary<string, List<LuaPatch>> AllPatches = new();

        /// <summary>
        /// Registers a patch and records its label in <see cref="PatchedLabels" />.
        /// </summary>
        /// <param name="patch">The patch to register.</param>
        public void AddPatch(LuaPatch patch)
        {
            if (AllPatches.TryGetValue(patch.Label, out var l))
            {
                l.Add(patch);
            }
            else
            {
                AllPatches[patch.Label] = new List<LuaPatch> { patch };
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
        /// All buckets for each patch type
        /// </summary>
        public Dictionary<string, LabelPatchBuckets> AllPatchesBuckets = new();

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
            foreach (var (label, patch) in AllPatches)
            {
                SetupLabelForRun(label, patch);
            }
        }

        private void SetupLabelForRun(string label, List<LuaPatch> patches)
        {
            // First we do mod constraint filtering
            List<LuaPatch> modConstrained = new(patches.Count);
            foreach (var patch in patches)
            {
                foreach (var mod in patch.NeedsMods)
                {
                    if (!AllMods.Contains(mod))
                    {
                        Summary.Remove(patch.Name, "MISSING", $"mod - {mod}");
                        goto continue_outer;
                    }
                }

                foreach (var mod in patch.ConflictsMods)
                {
                    if (AllMods.Contains(mod))
                    {
                        Summary.Remove(patch.Name, "CONFLICT", $"mod - {mod}");
                        goto continue_outer;
                    }
                }
                
                modConstrained.Add(patch);
                continue_outer:;
            }
            // Second we do patch constraint filter
            var allPatches = Enumerable.ToHashSet(modConstrained.Select(x => x.Name));
            var patchConstrained = new List<LuaPatch>(modConstrained.Count);
            
            foreach (var patch in patches)
            {
                foreach (var id in patch.NeedsPatches)
                {
                    if (!allPatches.Contains(id))
                    {
                        Summary.Remove(patch.Name, "MISSING", $"patch - {id}");
                        goto continue_outer;
                    }
                }

                foreach (var id in patch.ConflictsPatches)
                {
                    if (allPatches.Contains(id))
                    {
                        Summary.Remove(patch.Name, "CONFLICT", $"patch - {id}");
                        goto continue_outer;
                    }
                }
                
                patchConstrained.Add(patch);
                continue_outer:;
            }
            
            // Third we explode out all after/before mod dependencies
            Dictionary<string, List<string>> explosion = new();

            // Setup the explosion array
            foreach (var patch in patchConstrained)
            {
                if (!explosion.TryGetValue(patch.PatchModId, out var list))
                {
                    explosion[patch.PatchModId] = list = new();
                }
                list.Add(patch.Name);
            }

            // And explode
            foreach (var patch in patchConstrained)
            {
                foreach (var beforeMod in patch.BeforeMods)
                {
                    if (explosion.TryGetValue(beforeMod, out var list))
                    {
                        patch.BeforePatches.UnionWith(list);
                    }
                }

                foreach (var afterMod in patch.AfterMods)
                {
                    
                    if (explosion.TryGetValue(afterMod, out var list))
                    {
                        patch.AfterPatches.UnionWith(list);
                    }
                }
                
                patch.BeforePatches.IntersectWith(allPatches);
                patch.AfterPatches.IntersectWith(allPatches);
            }
            

            // Then we do a topological sort on the patches for this label using kahn's algorithm
            var n = patchConstrained.Count;

            var inDegree = new Dictionary<string, int>(n);
            var outEdges = new Dictionary<string, HashSet<string>>(n);
            var namePatchMap = new Dictionary<string, LuaPatch>(n);

            foreach (var patch in patchConstrained)
            {
                inDegree[patch.Name] = 0;
                outEdges[patch.Name] = new();
                namePatchMap[patch.Name] = patch;
            }

            foreach (var patch in patchConstrained)
            {
                foreach (var pre in patch.AfterPatches)
                {
                    if (pre == patch.Name) continue;
                    if (outEdges[pre].Add(patch.Name)) inDegree[patch.Name]++;
                }

                foreach (var suc in patch.BeforePatches)
                {
                    if (suc == patch.Name) continue;
                    if (outEdges[patch.Name].Add(suc)) inDegree[suc]++;
                }
            }
            
            var queue = new Queue<string>();
            foreach (var (name, d) in inDegree)
            {
                if (d == 0) queue.Enqueue(name);
            }

            var sortedPatches = new List<LuaPatch>();
            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                sortedPatches.Add(namePatchMap[name]);
                foreach (var suc in outEdges[name])
                {
                    if (--inDegree[suc] == 0) queue.Enqueue(suc);
                }
            }

            if (sortedPatches.Count != patchConstrained.Count)
            {
                var unsorted = patchConstrained.Where(k => !sortedPatches.Contains(k));
                foreach (var patch in unsorted)
                {
                    Summary.Remove(patch.Name, "CYCLE", "patch was caught in a dependency cycle");
                }
            }

            for (var i = 0; i < sortedPatches.Count; i++)
            {
                sortedPatches[i].Order = i;
            }

            // Finally we bucket out the patches
            
            var exactGroups = new Dictionary<string, List<LuaPatch>>();
            var matchAll = new List<LuaPatch>();
            var wildcard = new List<WildcardEntry>();

            foreach (var p in patches)
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
                                exactGroups[pattern] = groups = new List<LuaPatch>();
                            }
                            groups.Add(p);
                        }
                        else
                        {
                            wildcard.Add(new WildcardEntry(NamePattern.Get(p.Name), p));
                        }
                    }
                }
            }
            
            var buckets = new LabelPatchBuckets
            {
                MatchAll = matchAll.ToArray(),
                Wildcard = wildcard.ToArray(),
            };

            foreach (var (p, i) in exactGroups)
            {
                buckets.Exact[p] = i.ToArray();
            }

            AllPatchesBuckets[label] = buckets;
        }

        /// <summary>
        /// Runs every patch matching <paramref name="label" /> / <paramref name="name" /> against the given JSON,
        /// returning the final result.
        /// </summary>
        /// <remarks>
        /// Patches are chained: each one operates on the previous patch's <see cref="DynValue" /> when the converters
        /// match, otherwise the chain is flushed back to JSON, lifted by the new converter, and chaining resumes.
        /// Returns <paramref name="data" /> unchanged when no patch applied; returns <c>null</c> when a patch
        /// removed the asset.
        /// </remarks>
        /// <param name="label">The asset's addressables label.</param>
        /// <param name="name">The asset's addressables address.</param>
        /// <param name="data">The asset's parsed JSON.</param>
        /// <param name="patchCount">Set to the number of patches that ran successfully.</param>
        /// <param name="errorCount">Set to the number of patches that threw.</param>
        /// <returns>The patched JSON, or <c>null</c> when the asset was removed.</returns>
        public JToken RunAllPatchesFor(string label, string name, JToken data, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            IConverter? previousConverter = null;
            DynValue? previousInstance = null;
            foreach (var patch in GetAllSortedPatchesFor(label, name))
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
        /// Runs every patch matching the given new asset against its <see cref="LuaAsset.CurrentValue" />, returning
        /// the final JSON.
        /// </summary>
        /// <param name="asset">The new asset to patch.</param>
        /// <param name="patchCount">Set to the number of patches that ran successfully.</param>
        /// <param name="errorCount">Set to the number of patches that threw.</param>
        /// <returns>The patched JSON for the asset.</returns>
        public JToken RunAllPatchesFor(LuaAsset asset, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            foreach (var patch in GetAllSortedPatchesFor(asset.Label, asset.Name))
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
        /// Returns the patches registered for <paramref name="label" />, in stage-sorted order, filtered by name pattern.
        /// </summary>
        /// <param name="label">The addressables label to look up.</param>
        /// <param name="name">The asset's addressables address; matched against each patch's name pattern.</param>
        /// <returns>The matching patches, or an empty sequence when no patches are registered for the label.</returns>
        public IEnumerable<LuaPatch> GetAllSortedPatchesFor(string label, string name)
        {
            if (!AllPatchesBuckets.TryGetValue(label, out var buckets)) yield break;
            
            var exact = buckets.Exact.TryGetValue(name, out var e) ? e : Array.Empty<LuaPatch>();
            var matchAll = buckets.MatchAll;
            var wildcard = buckets.Wildcard;
            var exactI = 0;
            var matchAllI = 0;
            var wildCardI = 0;

            HashSet<LuaPatch> alreadyYielded = new(exact.Length+matchAll.Length+wildcard.Length);

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
                
                if (alreadyYielded.Add(best)) yield return best;
            }
        }

        /// <summary>
        /// Check if there are any patches that match the given label/name combo
        /// </summary>
        /// <param name="label">The label</param>
        /// <param name="name">The name</param>
        /// <returns>true if any patches match</returns>
        public bool HasAnyPatchFor(string label, string name)
        {
            if (!AllPatchesBuckets.TryGetValue(label, out var buckets)) return false;
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
        /// <returns>A map from asset Unity name to primary address; empty when the label resolves to nothing.</returns>
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
        /// Stores information about wildcard patches
        /// </summary>
        public readonly struct WildcardEntry
        {
            /// <summary>
            /// The pattern that this patch applies to
            /// </summary>
            public readonly NamePattern Pattern;

            /// <summary>
            /// The patch itself
            /// </summary>
            public readonly LuaPatch Patch;

            /// <summary>
            /// Creates a new WildcardEntry instance
            /// </summary>
            /// <param name="pattern">The pattern the patch applies to</param>
            /// <param name="patch">The patch itself</param>
            public WildcardEntry(NamePattern pattern, LuaPatch patch)
            {
                Pattern = pattern;
                Patch = patch;
            }
        }

        /// <summary>
        /// Buckets for each label
        /// </summary>
        public sealed class LabelPatchBuckets
        {
            /// <summary>
            /// Exact name matching patches
            /// </summary>
            public Dictionary<string, LuaPatch[]> Exact = new();

            /// <summary>
            /// All matching patches
            /// </summary>
            public LuaPatch[] MatchAll = Array.Empty<LuaPatch>();

            /// <summary>
            /// Wildcard matching patches
            /// </summary>
            public WildcardEntry[] Wildcard = Array.Empty<WildcardEntry>();
        }

        #endregion
    }
}