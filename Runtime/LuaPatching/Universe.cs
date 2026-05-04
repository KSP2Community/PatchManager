using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using ReduxLib.Logging;
using UnityEngine;

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
            AllMods = allMods.ToHashSet();
            var pmc = new PatchManagerCore(this);
            PatchManagerLibraryInstance = UserData.Create(pmc);
            foreach (var (k, v) in SubmoduleTypes)
            {
                Submodules[k] = UserData.Create(Activator.CreateInstance(v, pmc, this));
            }

            SetupBasePriorities(allMods);
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

        private void SetupBasePriorities(List<string> modLoadOrder)
        {
            MessageLogger($"Setting up base priorities with mod load order: {string.Join(", ", modLoadOrder)}");
            var lastPost = "";
            foreach (var mod in modLoadOrder)
            {
                var stage = new Stage();
                if (lastPost.Length > 0)
                    stage.RunsAfter.Add(lastPost);
                AllStages[mod] = stage;
                MessageLogger($"Adding stage: {mod}");
                var post = new Stage();
                post.RunsAfter.Add(mod);
                lastPost = $"{mod}:__post";
                AllStages[lastPost] = post;
                MessageLogger($"Adding stage: {lastPost}");
                LastImplicitWithinMod[mod] = mod;
            }

            LastImplicitGlobal = lastPost;
            MessageLogger($"Last implicit global: {lastPost}");
        }

        static Universe()
        {
            MoonSharpExceptionWrapPatch.Install();
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
            try
            {
                patchScript.DoString(File.ReadAllText(file.FullName));
            }
            catch (InterpreterException e)
            {
                ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                ErrorLogger($"{file.FullName} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }
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
            patchScript.Globals.RegisterModuleType<JsonModule>();

            foreach (var file in directory.EnumerateFiles("*.lua", SearchOption.AllDirectories)
                         .Where(f => !f.Name.StartsWith("_")))
            {
                try
                {
                    patchScript.DoString(File.ReadAllText(file.FullName));
                }
                catch (InterpreterException e)
                {
                    ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
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
            patchScript.Globals.RegisterModuleType<JsonModule>();
            try
            {
                patchScript.DoString(textAsset.text);
            }
            catch (InterpreterException e)
            {
                ErrorLogger($"{textAsset.name} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                ErrorLogger($"{textAsset.name} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }
        }

        #endregion

        #region Patch/Stage Registering

        /// <summary>
        /// Every known stage keyed by name, including the implicit per-mod and per-mod-post stages set up at construction.
        /// </summary>
        public Dictionary<string, Stage> AllStages = new();

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

        /// <summary>
        /// Adds a stage to <see cref="AllStages" /> under the given name.
        /// </summary>
        /// <param name="name">The stage name.</param>
        /// <param name="stage">The stage to register.</param>
        public void AddStage(string name, Stage stage)
        {
            AllStages.Add(name, stage);
        }

        #endregion

        #region Stage Sorting

        /// <summary>
        /// Topologically sorted stages, mapped to their ordering priority. Populated by the internal sort step.
        /// </summary>
        public Dictionary<string, ulong> SortedStages = new();

        
        // Reimplemented using Kahn's algorithm for dependency sorting
        private void SortStages()
        {
            MessageLogger($"Sorting {AllStages.Count} stages");
            var hs = AllStages.Keys.ToHashSet();
            foreach (var (k, v) in AllStages)
            {
                v.UpdateRequirements(hs);
            }

            var n = AllStages.Count;
            var inDegree = new Dictionary<string, int>(n);
            var outEdges = new Dictionary<string, HashSet<string>>(n);
            foreach (var name in AllStages.Keys)
            {
                inDegree[name] = 0;
                outEdges[name] = new HashSet<string>();
            }

            foreach (var (name, stage) in AllStages)
            {
                foreach (var pre in stage.RunsAfter)
                {
                    if (pre == name) continue;
                    if (outEdges[pre].Add(name)) inDegree[name]++;
                }

                foreach (var suc in stage.RunsBefore)
                {
                    if (suc == name) continue;
                    if (outEdges[name].Add(suc)) inDegree[suc]++;
                }
            }

            var queue = new Queue<string>();
            foreach (var (name, d) in inDegree)
            {
                if (d == 0) queue.Enqueue(name);
            }

            var sortedStages = new List<string>();
            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                sortedStages.Add(name);
                foreach (var suc in outEdges[name])
                {
                    if (--inDegree[suc] == 0) queue.Enqueue(suc);
                }
            }
            
            if (sortedStages.Count != n)
            {
                var unsorted = AllStages.Keys.Where(k => !sortedStages.Contains(k));
                throw new Exception(
                    $"Unable to sort stages to define patch order, the following stages cause a circular dependency: {string.Join(", ", unsorted)}");
            }
            
            MessageLogger("Sorted stages!");
            ulong p = 0;
            foreach (var stage in sortedStages)
            {
                MessageLogger($"{stage}: {p}");
                SortedStages[stage] = p++;
            }
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
            // We have to sort our stages first, stuff wasn't running in any order prior...
            SortStages();
            PatchedLabels = AllPatches.Keys.ToHashSet();
            foreach (var (label, patches) in AllPatches)
            {
                foreach (var p in patches)
                {
                    p.StagePriority = SortedStages.GetValueOrDefault(p.Stage, ulong.MaxValue);
                }

                patches.Sort((x, y) => x.StagePriority.CompareTo(y.StagePriority));

                var exactGroups = new Dictionary<string, List<LuaPatch>>();
                var matchAll = new List<LuaPatch>();
                var wildcard = new List<WildcardEntry>();

                foreach (var p in patches)
                {
                    if (string.IsNullOrEmpty(p.Name) || p.Name == "*")
                    {
                        matchAll.Add(p);
                    }
                    else if (p.Name.IndexOfAny(WildcardChars) < 0)
                    {
                        if (!exactGroups.TryGetValue(p.Name, out var groups))
                        {
                            exactGroups[p.Name] = groups = new List<LuaPatch>();
                        }

                        groups.Add(p);
                    }
                    else
                    {
                        wildcard.Add(new WildcardEntry(NamePattern.Get(p.Name), p));
                    }
                }

                var buckets = new LabelPatchBuckets
                {
                    MatchAll = matchAll.ToArray(),
                    Wildcard = wildcard.ToArray(),
                };

                foreach (var (n, i) in exactGroups)
                {
                    buckets.Exact[n] = i.ToArray();
                }

                AllPatchesBuckets[label] = buckets;
            }
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
            bool anyApplied = false;
            foreach (var patch in GetAllSortedPatchesFor(label, name))
            {
                try
                {
                    if (previousInstance == null)
                    {
                        previousConverter = patch.ConverterInstance;
                        previousInstance = patch.ApplyFirst(data);
                    }
                    else if (ReferenceEquals(previousConverter, patch.ConverterInstance))
                    {
                        previousInstance = patch.ApplyInChain(previousInstance);
                    }
                    else
                    {
                        var stringValue = previousConverter.ToJson(previousInstance);
                        previousConverter = patch.ConverterInstance;
                        previousInstance = patch.ApplyFirst(stringValue);
                    }

                    anyApplied = true;
                    patchCount++;
                }
                catch (InterpreterException e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {label}:{name} failed due to: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {label}:{name} failed due to {e.Message}");
                    ErrorLogger(e.ToString());
                }
            }

            return anyApplied ? previousConverter!.ToJson(previousInstance) : data;
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
                try
                {
                    if (ReferenceEquals(asset.ConverterInstance, patch.ConverterInstance))
                    {
                        asset.CurrentValue = patch.ApplyInChain(asset.CurrentValue);
                    }
                    else
                    {
                        var stringValue = asset.ConverterInstance.ToJson(asset.CurrentValue);
                        asset.ConverterInstance = patch.ConverterInstance;
                        asset.CurrentValue = patch.ApplyFirst(stringValue);
                    }

                    patchCount++;
                }
                catch (InterpreterException e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {asset.Label}:{asset.Name} failed due to: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {asset.Label}:{asset.Name} failed due to {e.Message}");
                    ErrorLogger(e.ToString());
                }
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

            while (true)
            {
                while (wildCardI < wildcard.Length && !wildcard[wildCardI].Pattern.Matches(name)) wildCardI++;

                var exactPatch = exactI < exact.Length ? exact[exactI] : null;
                var matchAllPatch = matchAllI < matchAll.Length ? matchAll[matchAllI] : null;
                var wildcardPatch = wildCardI < wildcard.Length ? wildcard[wildCardI].Patch : null;

                if (exactPatch == null && matchAllPatch == null && wildcardPatch == null) yield break;

                var best = exactPatch;
                if (matchAllPatch != null && (best == null || matchAllPatch.StagePriority < best.StagePriority))
                    best = matchAllPatch;
                if (wildcardPatch != null && (best == null || wildcardPatch.StagePriority < best.StagePriority))
                    best = wildcardPatch;

                if (ReferenceEquals(best, exactPatch)) exactI++;
                else if (ReferenceEquals(best, matchAllPatch)) matchAllI++;
                else wildCardI++;

                yield return best;
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