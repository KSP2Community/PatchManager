using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using KSP.Game.Flow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Utility;
using PatchManager.LuaPatching;
using PatchManager.Shared;
using SpaceWarp2.API.Mods;
using UniLinq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Assets
{
    /// <summary>
    /// Bridges the Lua patch <see cref="LuaPatching.Universe" /> to addressables: imports patch files, runs the
    /// patching pipeline against each addressable, and writes the results into the on-disk archive cache.
    /// </summary>
    internal static class PatchingManager
    {
        /// <summary>
        /// The current patch universe, created by <see cref="GenerateUniverse" />.
        /// </summary>
        internal static Universe Universe;

        private static readonly PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

        private static Dictionary<string, List<(string name, LuaAsset data)>> _createdAssets = new();

        internal static bool UseIndentedOutput;

        /// <summary>
        /// Total patches successfully applied this run.
        /// </summary>
        internal static int TotalPatchCount;

        /// <summary>
        /// Total patch errors encountered this run.
        /// </summary>
        internal static int TotalErrorCount;

        /// <summary>
        /// Total new assets created this run.
        /// </summary>
        internal static int TotalNewAssetCount;

        /// <summary>
        /// Total distinct asset definitions modified by patches this run.
        /// </summary>
        internal static int TotalDefinitionPatchCount;

        /// <summary>
        /// Constructs a new patch <see cref="LuaPatching.Universe" /> seeded with every loaded SpaceWarp plugin's
        /// GUID plus the supplied single-file mod IDs.
        /// </summary>
        /// <param name="singleFileModIds">Mod IDs for single-file patches that are not registered as plugins.</param>
        public static void GenerateUniverse(HashSet<string> singleFileModIds)
        {
            var loadedPlugins = PluginList.AllEnabledAndActivePlugins.Select(x => x.Guid).ToList();
            loadedPlugins.AddRange(singleFileModIds);
            Universe = new(Logging.LogError, Logging.LogMessage,
                loadedPlugins);
        }

        /// <summary>
        /// Finalizes the universe's patch registry and logs the total registered-patch and new-asset counts.
        /// </summary>
        public static void RegisterPatches()
        {
            Logging.LogInfo($"Registering all patches!");
            CSharpPatching.FluentPatchRegistry.Flush(Universe);
            Universe.SetupPatchesForRun();
            Logging.LogInfo($"{Universe.TotalPatchCount} patchers registered!");
            Logging.LogInfo($"{Universe.AllNewAssets.Count} assets created!");
        }

        /// <summary>
        /// Hashes every mod's declared script files into the patch-cache checksum.
        /// </summary>
        /// <remarks>
        /// Runs before the cache-validity decision (and before the bodies run), so the decision sees whether the
        /// patch files changed since last launch. Addressable-sourced scripts are not files and are gated by mod
        /// version instead. Uses the same string hash as <see cref="CollectScriptResults" /> so the two agree.
        /// </remarks>
        public static void HashScriptFiles()
        {
            foreach (var descriptor in PluginList.AllEnabledAndActivePlugins)
            {
                foreach (var file in descriptor.ScriptFiles)
                {
                    if (File.Exists(file))
                    {
                        CurrentPatchHashes.Patches[file] = Hash.FromString(File.ReadAllText(file));
                    }
                }
            }
        }

        /// <summary>
        /// Collects what the runtime exposed per descriptor: every ran script's hash into the patch-cache
        /// checksum, and every script error into the patch summary.
        /// </summary>
        public static void CollectScriptResults()
        {
            foreach (var descriptor in PluginList.AllEnabledAndActivePlugins)
            {
                foreach (var script in descriptor.LoadedScripts)
                {
                    CurrentPatchHashes.Patches[script.Key] = Hash.FromString(script.Value);
                }

                foreach (var error in descriptor.ScriptErrors)
                {
                    Universe.Summary.ErroredFiles.Add(error);
                }
            }
        }

        /// <summary>
        /// Invalidates the cache if the checksum is different.
        /// </summary>
        /// <returns>True if the cache is valid, false if it was invalidated.</returns>
        public static bool InvalidateCacheIfNeeded()
        {
            var checksum = Hash.FromJsonObject(CurrentPatchHashes);

            if (CacheManager.Inventory.Checksum == checksum)
            {
                Logging.LogInfo("Cache is valid, skipping rebuild.");
                CacheManager.CacheValidLabels.AddRange(CacheManager.Inventory.CacheEntries.Keys);
                return true;
            }

            Logging.LogInfo("Cache is invalid, rebuilding.");
            CacheManager.InvalidateCache();
            CacheManager.Inventory.Checksum = checksum;
            CacheManager.Inventory.Patches = CurrentPatchHashes;

            return false;
        }

        /// <summary>
        /// Collects every queued new asset from the universe into the per-label staging dictionary, then resolves
        /// the supplied callback.
        /// </summary>
        /// <param name="resolve">Callback invoked once collection finishes.</param>
        /// <param name="reject">Reject callback (currently unused).</param>
        public static void CreateNewAssets(Action resolve, Action<string> reject)
        {
            foreach (var generator in Universe.AllNewAssets)
            {
                try
                {
                    Logging.LogDebug($"Generated an asset with the label {generator.Label}, and name {generator.Name}");

                    if (!_createdAssets.ContainsKey(generator.Label))
                    {
                        _createdAssets[generator.Label] = new List<(string name, LuaAsset data)>();
                    }

                    if (!_createdAssets[generator.Label].Any(x => x.name == generator.Name))
                    {
                        _createdAssets[generator.Label].Add((generator.Name, generator));
                    }

                }
                catch (Exception e)
                {
                    TotalErrorCount += 1;
                    Logging.LogError($"Failed to generate an asset due to: {e}");
                    Universe.Summary.BeginLabel(generator.Label);
                    Universe.Summary.BeginAsset(generator.Name, null);
                    Universe.Summary.Error(generator.Name, e);
                }
            }

            TotalNewAssetCount = Universe.AllNewAssets.Count;
            UpdateLoadingBarData();

            resolve();
        }


        private static Dictionary<string, LabelRebuildState> _rebuildStates;

        private static readonly PatchDefinition.PatchPass[] OrderedPasses =
        {
            PatchDefinition.PatchPass.Early,
            PatchDefinition.PatchPass.Default,
            PatchDefinition.PatchPass.Late
        };

        /// <summary>
        /// Schedules per-(pass, label) flow actions in pass-major order (every label's Early before any
        /// Default, every label's Default before any Late). A label only receives an action for a pass
        /// if it has a patch in that pass. The first action a label receives lazily loads its
        /// addressables. The last action writes the label's archive and releases its load handle. A
        /// final action persists totals and inventory.
        /// </summary>
        /// <param name="resolve">Callback invoked once scheduling finishes.</param>
        /// <param name="reject">Reject callback (currently unused).</param>
        public static void RebuildAllCache(Action resolve, Action<string> reject)
        {
            var labels = Universe.PatchedLabels.Concat(_createdAssets.Keys).Distinct().ToList();

            if (labels.Count == 0)
            {
                resolve();
                return;
            }

            InitRebuildStates(labels);

            var activePassesPerLabel = new Dictionary<string, List<PatchDefinition.PatchPass>>(labels.Count);
            foreach (var label in labels)
            {
                activePassesPerLabel[label] = ActivePassesFor(label);
            }

            var insertIdx = GameManager.Instance.LoadingFlow.flowIndex + 1;
            var actions = new List<GenericFlowAction>();

            foreach (var pass in OrderedPasses)
            {
                foreach (var label in labels)
                {
                    var active = activePassesPerLabel[label];
                    if (!active.Contains(pass)) continue;

                    var isFirst = active[0] == pass;
                    var isLast = active[active.Count - 1] == pass;
                    actions.Add(MakePassAction(label, pass, isFirst, isLast));
                }
            }

            actions.Add(new GenericFlowAction(
                "Patching: Finalize",
                (r, _) =>
                {
                    FinalizeRebuild();
                    r();
                }
            ));

            for (var i = actions.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.LoadingFlow.FlowActions.Insert(insertIdx, actions[i]);
            }

            resolve();
        }

        private static void InitRebuildStates(List<string> labels)
        {
            _rebuildStates = new Dictionary<string, LabelRebuildState>(labels.Count);
            foreach (var label in labels)
            {
                var state = new LabelRebuildState
                {
                    Label = label,
                    ArchiveFilename = $"{label.Replace("/", "")}.zip",
                    PrimaryKeyMap = Universe.BuildPrimaryKeyMapForLabel(label)
                };
                _rebuildStates[label] = state;

                if (_createdAssets.TryGetValue(label, out var created))
                {
                    foreach (var (name, luaAsset) in created)
                    {
                        state.CreatedAssets.Add((name, luaAsset));
                        state.Unchanged = false;
                        var addressAlias = name.EndsWith(".json") ? null : name + ".json";
                        if (addressAlias != null) state.AddressAliases[name] = addressAlias;
                    }
                    created.Clear();
                    _createdAssets.Remove(label);
                }
            }
        }

        private static List<PatchDefinition.PatchPass> ActivePassesFor(string label)
        {
            var result = new List<PatchDefinition.PatchPass>();
            foreach (var pass in OrderedPasses)
            {
                if (HasPatchesInPass(label, pass)) result.Add(pass);
            }
            if (result.Count == 0
                && _rebuildStates.TryGetValue(label, out var state)
                && state.CreatedAssets.Count > 0)
            {
                result.Add(PatchDefinition.PatchPass.Default);
            }
            return result;
        }

        private static bool HasPatchesInPass(string label, PatchDefinition.PatchPass pass)
        {
            if (!Universe.AllPatchesBuckets.TryGetValue(label, out var perPass)) return false;
            if (!perPass.TryGetValue(pass, out var buckets)) return false;
            return buckets.MatchAll.Length > 0
                || buckets.Wildcard.Length > 0
                || buckets.Exact.Count > 0;
        }

        private static GenericFlowAction MakePassAction(string label, PatchDefinition.PatchPass pass, bool loadFirst, bool writeLast)
        {
            var labelCopy = label;
            var passCopy = pass;
            var loadCopy = loadFirst;
            var writeCopy = writeLast;

            var passSuffix = pass switch
            {
                PatchDefinition.PatchPass.Default => "",
                _ => $" [{pass.ToString().ToUpperInvariant()}]"
            };

            return new GenericFlowAction(
                $"Patching: {label}{passSuffix}",
                (resolve, _) => CoroutineUtil.Instance.DoCoroutine(
                    RunPassActionCoroutine(labelCopy, passCopy, loadCopy, writeCopy, resolve))
            );
        }

        private static IEnumerator RunPassActionCoroutine(
            string label,
            PatchDefinition.PatchPass pass,
            bool loadFirst,
            bool writeLast,
            Action resolve
        )
        {
            if (loadFirst) yield return LoadLabel(label);
            RunPassForLabel(label, pass);
            if (writeLast) WriteAndReleaseLabel(label);
            resolve();
        }

        private static IEnumerator LoadLabel(string label)
        {
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state)) yield break;

            var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(TextAsset));
            while (!locHandle.IsDone)
            {
                UpdateLoadingBarData();
                yield return null;
            }
            var hasLocations = locHandle.Status == AsyncOperationStatus.Succeeded
                               && locHandle.Result != null
                               && locHandle.Result.Count > 0;
            Addressables.Release(locHandle);

            if (hasLocations)
            {
                var stateRef = state;
                var handle = Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
                {
                    if (string.IsNullOrEmpty(asset.text)) return;
                    stateRef.RawTexts[asset.name] = asset.text;
                });
                state.LoadHandle = handle;

                while (!handle.IsDone)
                {
                    UpdateLoadingBarData();
                    yield return null;
                }
            }

            if (state.CreatedAssets.Count > 0)
            {
                Universe.Summary.BeginLabel(label);
                foreach (var (name, _) in state.CreatedAssets)
                {
                    state.AddressAliases.TryGetValue(name, out var alias);
                    Universe.Summary.BeginNewAsset(name, alias);
                }
            }
        }

        private static void RunPassForLabel(string label, PatchDefinition.PatchPass pass)
        {
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state)) return;

            Universe.Summary.BeginPass(pass);
            Universe.Summary.BeginLabel(label);

            var assetNames = state.RawTexts.Keys.Concat(state.Tokens.Keys).Distinct().ToList();
            foreach (var assetName in assetNames)
            {
                if (!Universe.HasAnyPatchInPass(label, assetName, pass)) continue;

                var token = EnsureParsed(state, assetName);
                if (token == null) continue;

                var address = state.PrimaryKeyMap.TryGetValue(assetName, out var a) ? a : "<unknown>";
                Universe.Summary.BeginAsset(assetName, address);

                var result = Universe.RunAllPatchesFor(label, assetName, token, pass, out var pc, out var ec);
                TotalPatchCount += pc;
                TotalErrorCount += ec;
                if (pc > 0)
                {
                    state.Unchanged = false;
                    if (state.PatchedAssetNames.Add(assetName))
                    {
                        TotalDefinitionPatchCount++;
                    }
                }
                if (result == null)
                {
                    state.Tokens.Remove(assetName);
                }
                else
                {
                    state.Tokens[assetName] = result;
                }
            }

            foreach (var (name, luaAsset) in state.CreatedAssets)
            {
                state.AddressAliases.TryGetValue(name, out var alias);
                Universe.Summary.BeginAsset(name, alias);
                Universe.RunAllPatchesFor(luaAsset, pass, out var pc, out var ec);
                TotalPatchCount += pc;
                TotalErrorCount += ec;
            }

            UpdateLoadingBarData();
        }

        private static JToken EnsureParsed(LabelRebuildState state, string assetName)
        {
            if (state.Tokens.TryGetValue(assetName, out var token)) return token;
            if (!state.RawTexts.TryGetValue(assetName, out var rawText)) return null;
            try
            {
                token = JToken.Parse(rawText);
            }
            catch (Exception e)
            {
                TotalErrorCount += 1;
                Logging.LogError($"Failed to parse {state.Label}:{assetName}: {e.Message}");
                Universe.Summary.BeginLabel(state.Label);
                Universe.Summary.BeginAsset(assetName, null);
                Universe.Summary.Error(assetName, e);
                state.RawTexts.Remove(assetName);
                return null;
            }
            state.Tokens[assetName] = token;
            state.RawTexts.Remove(assetName);
            return token;
        }

        private static void WriteAndReleaseLabel(string label)
        {
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state)) return;

            if (!state.Unchanged) WriteArchive(state);

            if (state.LoadHandle.IsValid()
                && state.LoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(state.LoadHandle);
            }

            _rebuildStates.Remove(label);
        }

        private static void FinalizeRebuild()
        {
            CacheManager.SetTotalPatchCount(TotalPatchCount);
            CacheManager.SetTotalErrorCount(TotalErrorCount);
            CacheManager.SetTotalDefinitionCount(TotalDefinitionPatchCount);
            CacheManager.SetTotalAssetCount(TotalNewAssetCount);
            CacheManager.SaveInventory();

            if (_rebuildStates != null)
            {
                foreach (var (_, state) in _rebuildStates)
                {
                    if (state.LoadHandle.IsValid()
                        && state.LoadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Addressables.Release(state.LoadHandle);
                    }
                }
                _rebuildStates = null;
            }

            UpdateLoadingBarData();
        }

        private static void WriteArchive(LabelRebuildState state)
        {
            var labelCacheEntry = new CacheEntry
            {
                Label = state.Label,
                ArchiveFilename = state.ArchiveFilename,
                Assets = new List<string>()
            };
            var assetsCacheEntries = new Dictionary<string, CacheEntry>();
            var archive = CacheManager.CreateArchive(state.ArchiveFilename);

            void AddAssetEntry(string assetName, string serialized)
            {
                archive.AddFile(assetName, serialized);
                labelCacheEntry.Assets.Add(assetName);
                assetsCacheEntries[assetName] = new CacheEntry
                {
                    Label = assetName,
                    ArchiveFilename = state.ArchiveFilename,
                    Assets = new List<string> { assetName }
                };
                if (state.PrimaryKeyMap.TryGetValue(assetName, out var address) && address != assetName)
                {
                    assetsCacheEntries[address] = new CacheEntry
                    {
                        Label = address,
                        ArchiveFilename = state.ArchiveFilename,
                        Assets = new List<string> { assetName }
                    };
                }
            }

            foreach (var (assetName, token) in state.Tokens)
            {
                if (token == null) continue;
                var serialized = token.ToString(UseIndentedOutput ? Formatting.Indented : Formatting.None);
                if (string.IsNullOrEmpty(serialized)) continue;
                AddAssetEntry(assetName, serialized);
            }

            foreach (var (assetName, rawText) in state.RawTexts)
            {
                if (string.IsNullOrEmpty(rawText)) continue;
                AddAssetEntry(assetName, rawText);
            }

            foreach (var (name, luaAsset) in state.CreatedAssets)
            {
                try
                {
                    var jResult = luaAsset.ConverterInstance.ToJson(luaAsset.CurrentValue);
                    if (jResult == null) continue;
                    var serialized = jResult.ToString(UseIndentedOutput ? Formatting.Indented : Formatting.None);
                    if (string.IsNullOrEmpty(serialized)) continue;
                    archive.AddFile(name, serialized);
                    labelCacheEntry.Assets.Add(name);
                    assetsCacheEntries[name] = new CacheEntry
                    {
                        Label = name,
                        ArchiveFilename = state.ArchiveFilename,
                        Assets = new List<string> { name }
                    };
                    if (state.AddressAliases.TryGetValue(name, out var alias))
                    {
                        assetsCacheEntries[alias] = new CacheEntry
                        {
                            Label = alias,
                            ArchiveFilename = state.ArchiveFilename,
                            Assets = new List<string> { name }
                        };
                    }
                }
                catch (Exception e)
                {
                    TotalErrorCount += 1;
                    Logging.LogError($"Failed to serialize {state.Label}:{name}: {e.Message}");
                    Universe.Summary.BeginLabel(state.Label);
                    Universe.Summary.BeginAsset(name, null);
                    Universe.Summary.Error(name, e);
                }
            }

            archive.Save();

            CacheManager.CacheValidLabels.Add(state.Label);
            CacheManager.Inventory.CacheEntries.Add(state.Label, labelCacheEntry);
            CacheManager.Inventory.CacheEntries.AddRangeUnique(assetsCacheEntries);
        }

        private sealed class LabelRebuildState
        {
            public string Label;
            public string ArchiveFilename;
            public Dictionary<string, string> RawTexts = new();
            public Dictionary<string, JToken> Tokens = new();
            public Dictionary<string, string> PrimaryKeyMap;
            public List<(string name, LuaAsset asset)> CreatedAssets = new();
            public Dictionary<string, string> AddressAliases = new();
            public HashSet<string> PatchedAssetNames = new();
            public bool Unchanged = true;
            public AsyncOperationHandle<IList<TextAsset>> LoadHandle;
        }

        private static void UpdateLoadingBarData()
        {
            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerDefinitionsModifiedCount =
                TotalDefinitionPatchCount;

            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerErrorCount =
                TotalErrorCount;

            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerNewAssetCount =
                TotalNewAssetCount;

            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerPatchCount = TotalPatchCount;
        }
    }
}
