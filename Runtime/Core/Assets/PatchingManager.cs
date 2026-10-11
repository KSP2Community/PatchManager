using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Utility;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Builtin;
using PatchManager.Shared;
using Redux.Features;
using ReduxLib.Configuration;
using SpaceWarp2.API.Features;
using SpaceWarp2.API.Mods;
using UniLinq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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

        private static PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

        private static Dictionary<string, List<(string name, LuaAsset data)>> _createdAssets = new();

        internal static bool UseIndentedOutput;

        // Assets loaded into a label's rebuild by their own key, for stock assets that carry no label of their own
        private static readonly Dictionary<string, HashSet<string>> LabelMembers = new();

        /// <summary>
        /// Makes the asset with the given key part of a label's rebuild, so patches and duplicates on the label reach it.
        /// </summary>
        /// <remarks>
        /// The asset is still loaded by its own key afterwards, since the archive keeps it under its own name.
        /// </remarks>
        /// <param name="label">The label whose rebuild loads the asset.</param>
        /// <param name="key">The asset's addressables key.</param>
        internal static void AddLabelMember(string label, string key)
        {
            if (!LabelMembers.TryGetValue(label, out var members))
            {
                LabelMembers[label] = members = new HashSet<string>();
            }

            members.Add(key);
        }

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Universe = null;
            CurrentPatchHashes = PatchHashes.CreateDefault();
            _createdAssets = new Dictionary<string, List<(string name, LuaAsset data)>>();
            UseIndentedOutput = false;
            TotalPatchCount = 0;
            TotalErrorCount = 0;
            TotalNewAssetCount = 0;
            TotalDefinitionPatchCount = 0;
            _rebuildStates = null;
        }

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
            Universe.FeatureRegistry = FeatureRegistry.Instance;
        }

        /// <summary>
        /// Picks the feature set this boot patches for: the main menu umbrella plus every installed mod's default.
        /// </summary>
        /// <remarks>
        /// Runs once registration has closed, so features declared from Lua are in the registry.
        /// </remarks>
        public static void SelectBootFeatures()
        {
            var installed = PluginList.AllEnabledAndActivePlugins.Select(descriptor => descriptor.Guid);
            Universe.EnabledFeatures = new HashSet<string>(
                FeatureGate.BootSet(Universe.FeatureRegistry, installed),
                StringComparer.OrdinalIgnoreCase
            );
            Logging.LogInfo($"Patching for {Universe.EnabledFeatures.Count} enabled feature(s)");
            FeatureAssetIndex.Activate(Universe.EnabledFeatures);
        }

        /// <summary>
        /// Keeps the script hashes that can affect a feature set's output: those of mods whose default feature is
        /// in the set, those of Redux, and those whose owner cannot be told.
        /// </summary>
        /// <remarks>
        /// Every mod patch needs its mod's default feature, so a mod outside the set contributes nothing to the set's
        /// output and editing its scripts leaves the set's cache valid.
        /// </remarks>
        /// <param name="hashes">Script hashes keyed by file path or by <c>modId/asset</c> for addressable scripts.</param>
        /// <param name="folderByMod">Each mod's folder, by mod id.</param>
        /// <param name="enabled">The set's enabled features.</param>
        /// <returns>The hashes that belong in the set's cache key.</returns>
        internal static Dictionary<string, string> ScopeHashesToSet(
            IReadOnlyDictionary<string, string> hashes,
            IReadOnlyDictionary<string, string> folderByMod,
            ISet<string> enabled
        )
        {
            var scoped = new Dictionary<string, string>();
            foreach (var (key, hash) in hashes)
            {
                var owner = OwnerOf(key, folderByMod);
                if (owner == null ||
                    string.Equals(owner, FeatureGate.EXPLICIT_MOD_ID, StringComparison.OrdinalIgnoreCase) ||
                    enabled.Contains(FeatureIds.DefaultFor(owner)))
                {
                    scoped[key] = hash;
                }
            }

            return scoped;
        }

        // A file path is owned by the mod whose folder holds it, and an addressable script by the mod its key names
        [CanBeNull]
        private static string OwnerOf(string key, IReadOnlyDictionary<string, string> folderByMod)
        {
            var path = key.Replace('\\', '/');
            foreach (var (modId, folder) in folderByMod)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                var prefix = folder.Replace('\\', '/').TrimEnd('/') + "/";
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return modId;
            }

            var slash = path.IndexOf('/');
            if (slash <= 0)
                return null;

            var head = path.Substring(0, slash);
            return folderByMod.Keys.FirstOrDefault(modId => string.Equals(modId, head, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Opens the cache of the enabled feature set, which no other set reads or invalidates.
        /// </summary>
        public static void OpenSetCache()
        {
            CacheManager.UseSet(SetIdOf(Universe.EnabledFeatures));
        }

        /// <summary>
        /// Names a feature set's cache by its features, whatever order they come in.
        /// </summary>
        /// <param name="features">The set's enabled features.</param>
        /// <returns>The set's id.</returns>
        internal static string SetIdOf(IEnumerable<string> features)
        {
            var ordered = features.Select(id => id.ToLowerInvariant()).OrderBy(id => id, StringComparer.Ordinal);
            return Hash.FromString(string.Join("\n", ordered));
        }

        /// <summary>
        /// Finalizes the universe's patch registry and logs the total registered-patch and new-asset counts.
        /// </summary>
        public static void RegisterPatches()
        {
            Logging.LogInfo("Registering all patches!");
            CSharpPatching.FluentPatchRegistry.Flush(Universe);
            Universe.SetupPatchesForRun();
            Logging.LogInfo($"{Universe.TotalPatchCount} patchers registered!");
            Logging.LogInfo($"{Universe.AllNewAssets.Count} assets created!");
        }

        /// <summary>
        /// Hashes every mod's Lua files into the patch-cache checksum.
        /// </summary>
        /// <remarks>
        /// Runs before the cache-validity decision, so the decision sees whether any patch source changed since
        /// last launch. Uses the same string hash as <see cref="CollectScriptResults" /> so the two agree.
        /// </remarks>
        public static void HashScriptFiles()
        {
            foreach (var descriptor in PluginList.AllEnabledAndActivePlugins)
            {
                // KSP1 mods carry no Lua and their folders are whole GameData trees, so sweeping them is a lot
                // of directory walking for nothing.
                if (descriptor.IsKsp1)
                    continue;

                HashScriptsForMod(descriptor.Folder?.FullName, descriptor.ScriptFiles, CurrentPatchHashes);
            }
        }

        /// <summary>
        /// Hashes one mod's declared script files plus every other Lua file in its folder into
        /// <paramref name="into" />.
        /// </summary>
        /// <remarks>
        /// The folder sweep is what catches the <c>_</c>-prefixed library files. Those are excluded from a
        /// descriptor's declared scripts because they are pulled in with <c>require</c> rather than run as
        /// bodies, but they shape patch output all the same, so leaving them out of the key lets an edited
        /// library serve stale archives. Sweeping the folder also covers a library required across mods, since
        /// every mod's own folder gets swept in turn.
        /// </remarks>
        /// <param name="modFolder">The mod's folder, or null when it has none.</param>
        /// <param name="declaredScriptFiles">The script files the descriptor declared at discovery.</param>
        /// <param name="into">The patch hash set to record into.</param>
        internal static void HashScriptsForMod(
            string modFolder,
            IEnumerable<string> declaredScriptFiles,
            PatchHashes into
        )
        {
            foreach (var file in declaredScriptFiles)
            {
                if (File.Exists(file))
                {
                    into.Patches[file] = Hash.FromString(File.ReadAllText(file));
                }
            }

            if (string.IsNullOrEmpty(modFolder) || !Directory.Exists(modFolder))
                return;

            // Keyed by path, so a file the descriptor already declared just gets the same hash written twice.
            foreach (var file in Directory.EnumerateFiles(modFolder, "*.lua", SearchOption.AllDirectories))
            {
                into.Patches[file] = Hash.FromString(File.ReadAllText(file));
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
        /// Decides whether the set's cache still holds what patching would produce now, and starts it afresh
        /// when it does not.
        /// </summary>
        /// <remarks>
        /// Only scripts and mod manifests of mods in the set count, so changing a mod the set leaves out keeps
        /// the set's cache.
        /// </remarks>
        /// <returns>True if the cache is valid, false if it was invalidated.</returns>
        public static bool InvalidateCacheIfNeeded()
        {
            var folderByMod = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var descriptor in PluginList.AllEnabledAndActivePlugins)
            {
                folderByMod[descriptor.Guid] = descriptor.Folder?.FullName;
                // A mod's manifest stands in for its content assets, whose versions it names
                CurrentPatchHashes.Patches[$"{descriptor.Guid}/<swinfo>"] =
                    Hash.FromString(JsonConvert.SerializeObject(descriptor.SWInfo));
            }

            CurrentPatchHashes.Patches = ScopeHashesToSet(CurrentPatchHashes.Patches, folderByMod, Universe.EnabledFeatures);
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
            var enabledAssets = Universe.StageNewAssetsForRun();
            foreach (var generator in enabledAssets)
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

            TotalNewAssetCount = enabledAssets.Count;
            UpdateLoadingBarData();

            resolve();
        }


        private static Dictionary<string, LabelRebuildState> _rebuildStates;

        /// <summary>
        /// Schedules per-(pass, label) flow actions in pass-major order: every label's actions for one pass come
        /// before any label's actions for the next, in the order the passes sort into.
        /// </summary>
        /// <remarks>
        /// A label only receives an action for a pass if it has a patch in that pass. The first action a label
        /// receives lazily loads its addressables. The last action writes the label's archive and releases its
        /// load handle. A final action persists totals and inventory.
        /// </remarks>
        /// <param name="resolve">Callback invoked once scheduling finishes.</param>
        /// <param name="reject">Reject callback (currently unused).</param>
        public static void RebuildAllCache(Action resolve, Action<string> reject)
        {
            Universe.BeginRun();
            var labels = Universe.PatchedLabels.Concat(_createdAssets.Keys).Distinct().ToList();

            if (labels.Count == 0)
            {
                Universe.EndRun();
                resolve();
                return;
            }

            InitRebuildStates(labels);

            var activePassesPerLabel = new Dictionary<string, List<string>>(labels.Count);
            foreach (var label in labels)
            {
                activePassesPerLabel[label] = ActivePassesFor(label);
            }

            var insertIdx = GameManager.Instance.LoadingFlow.flowIndex + 1;
            var actions = new List<GenericFlowAction>();

            foreach (var pass in Universe.OrderedPasses)
            {
                foreach (var label in labels)
                {
                    var active = activePassesPerLabel[label];
                    if (!active.Contains(pass))
                        continue;

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
                    Universe.EndRun();
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

        private static List<string> ActivePassesFor(string label)
        {
            var result = new List<string>();
            foreach (var pass in Universe.OrderedPasses)
            {
                if (HasPatchesInPass(label, pass)) result.Add(pass);
            }
            if (result.Count == 0
                && _rebuildStates.TryGetValue(label, out var state)
                && state.CreatedAssets.Count > 0)
            {
                result.Add(PassDefinition.DEFAULT);
            }
            return result;
        }

        private static bool HasPatchesInPass(string label, string pass)
        {
            if (!Universe.AllPatchesBuckets.TryGetValue(label, out var perPass))
                return false;
            if (!perPass.TryGetValue(pass, out var buckets))
                return false;
            return buckets.MatchAll.Length > 0
                || buckets.Wildcard.Length > 0
                || buckets.Exact.Count > 0;
        }

        private static GenericFlowAction MakePassAction(string label, string pass, bool loadFirst, bool writeLast)
        {
            var labelCopy = label;
            var passCopy = pass;
            var loadCopy = loadFirst;
            var writeCopy = writeLast;

            var passSuffix = pass == PassDefinition.DEFAULT ? "" : $" [{pass.ToUpperInvariant()}]";

            return new GenericFlowAction(
                $"Patching: {label}{passSuffix}",
                (resolve, _) => CoroutineUtil.Instance.DoCoroutine(
                    RunPassActionCoroutine(labelCopy, passCopy, loadCopy, writeCopy, resolve))
            );
        }

        private static IEnumerator RunPassActionCoroutine(
            string label,
            string pass,
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
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state))
                yield break;

            var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(TextAsset));
            while (!locHandle.IsDone)
            {
                UpdateLoadingBarData();
                yield return null;
            }
            // Only the locations the feature set lets in reach the patch run, so the archive holds nothing else
            var locations = locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null
                ? FeatureAssetIndex.Filter(locHandle.Result, label)
                : new List<IResourceLocation>();
            Addressables.Release(locHandle);

            if (locations.Count > 0)
            {
                var stateRef = state;
                var handle = Addressables.LoadAssetsAsync<TextAsset>(locations, asset =>
                {
                    if (string.IsNullOrEmpty(asset.text))
                        return;
                    stateRef.RawTexts[asset.name] = asset.text;
                });
                state.LoadHandle = handle;

                while (!handle.IsDone)
                {
                    UpdateLoadingBarData();
                    yield return null;
                }
            }

            if (LabelMembers.TryGetValue(label, out var members))
            {
                foreach (var key in members)
                {
                    // A member can have a same-named twin in another feature, so the gate picks which one loads
                    if (FeatureAssetIndex.TryResolveKey(key, typeof(TextAsset), label, out var memberLocation) && memberLocation == null)
                        continue;

                    var memberHandle = memberLocation != null
                        ? Addressables.LoadAssetAsync<TextAsset>(memberLocation)
                        : Addressables.LoadAssetAsync<TextAsset>(key);
                    while (!memberHandle.IsDone)
                    {
                        UpdateLoadingBarData();
                        yield return null;
                    }

                    // The text is copied out, so the handle is released at once rather than held to the archive write
                    if (memberHandle.Status == AsyncOperationStatus.Succeeded && memberHandle.Result != null
                        && !string.IsNullOrEmpty(memberHandle.Result.text))
                    {
                        state.RawTexts[memberHandle.Result.name] = memberHandle.Result.text;
                        state.PrimaryKeyMap[memberHandle.Result.name] = key;
                    }

                    Addressables.Release(memberHandle);
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

        private static void RunPassForLabel(string label, string pass)
        {
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state))
                return;

            Universe.Summary.BeginPass(pass);
            Universe.Summary.BeginLabel(label);

            var duplicates = new List<Universe.DuplicatedAsset>();
            var assetNames = state.RawTexts.Keys.Concat(state.Tokens.Keys).Distinct().ToList();
            foreach (var assetName in assetNames)
            {
                if (!Universe.HasAnyPatchInPass(label, assetName, pass))
                    continue;

                var token = EnsureParsed(state, assetName);
                if (token == null)
                    continue;

                var address = state.PrimaryKeyMap.TryGetValue(assetName, out var a) ? a : "<unknown>";
                Universe.Summary.BeginAsset(assetName, address);

                var result = Universe.RunAllPatchesFor(label, assetName, token, pass, duplicates, -1, out var pc, out var ec);
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
                Universe.RunAllPatchesFor(luaAsset, pass, duplicates, out var pc, out var ec);
                TotalPatchCount += pc;
                TotalErrorCount += ec;
            }

            AddDuplicates(state, label, pass, duplicates);
            UpdateLoadingBarData();
        }

        // Keeps the copies a pass's duplicate patches made, each patched by the rest of that pass. The list grows
        // while it is walked, so a copy that is itself duplicated later in the pass is handled the same way.
        private static void AddDuplicates(LabelRebuildState state, string label, string pass,
            List<Universe.DuplicatedAsset> duplicates)
        {
            for (var i = 0; i < duplicates.Count; i++)
            {
                var duplicate = duplicates[i];
                if (HasAsset(state, duplicate.Name))
                {
                    TotalErrorCount++;
                    Universe.Summary.BeginAsset(duplicate.Name, null);
                    Universe.Summary.Error(duplicate.PatchName,
                        $"an asset named {duplicate.Name} already exists in {label}, so the copy was dropped");
                    continue;
                }

                var alias = duplicate.Name.EndsWith(".json") ? null : duplicate.Name + ".json";
                if (alias != null) state.AddressAliases[duplicate.Name] = alias;
                Universe.Summary.BeginNewAsset(duplicate.Name, alias);
                state.Unchanged = false;

                var result = Universe.RunAllPatchesFor(label, duplicate.Name, duplicate.Data, pass, duplicates,
                    duplicate.AfterOrder, out var pc, out var ec);
                TotalPatchCount += pc;
                TotalErrorCount += ec;
                if (result != null)
                {
                    state.Tokens[duplicate.Name] = result;
                }
            }
        }

        private static bool HasAsset(LabelRebuildState state, string assetName) =>
            state.Tokens.ContainsKey(assetName) ||
            state.RawTexts.ContainsKey(assetName) ||
            state.CreatedAssets.Any(created => created.name == assetName);

        private static JToken EnsureParsed(LabelRebuildState state, string assetName)
        {
            if (state.Tokens.TryGetValue(assetName, out var token))
                return token;
            if (!state.RawTexts.TryGetValue(assetName, out var rawText))
                return null;
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
            if (_rebuildStates == null || !_rebuildStates.TryGetValue(label, out var state))
                return;

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
            var formatting = UseIndentedOutput ? Formatting.Indented : Formatting.None;
            var entries = new List<(string name, string text)>();

            foreach (var (assetName, token) in state.Tokens)
            {
                var serialized = token?.ToString(formatting);
                if (!string.IsNullOrEmpty(serialized))
                {
                    entries.Add((assetName, serialized));
                }
            }

            foreach (var (assetName, rawText) in state.RawTexts)
            {
                if (!string.IsNullOrEmpty(rawText))
                {
                    entries.Add((assetName, rawText));
                }
            }

            foreach (var (name, luaAsset) in state.CreatedAssets)
            {
                try
                {
                    var serialized = luaAsset.ConverterInstance.ToJson(luaAsset.CurrentValue)?.ToString(formatting);
                    if (!string.IsNullOrEmpty(serialized))
                    {
                        entries.Add((name, serialized));
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

            // Named by content, so a set whose label patches out the same as another set's reuses its file
            var archiveFilename = CacheManager.Store.WriteArchive(entries);

            var labelCacheEntry = new CacheEntry
            {
                Label = state.Label,
                ArchiveFilename = archiveFilename,
                Assets = new List<string>()
            };
            var assetsCacheEntries = new Dictionary<string, CacheEntry>();

            void AddKey(string key, string assetName)
            {
                assetsCacheEntries[key] = new CacheEntry
                {
                    Label = key,
                    ArchiveFilename = archiveFilename,
                    Assets = new List<string> { assetName }
                };
            }

            foreach (var (assetName, _) in entries)
            {
                labelCacheEntry.Assets.Add(assetName);
                AddKey(assetName, assetName);
                if (state.PrimaryKeyMap.TryGetValue(assetName, out var address) && address != assetName)
                {
                    AddKey(address, assetName);
                }

                // Created assets and copies made by duplicate patches have no address of their own, so they take an alias
                if (state.AddressAliases.TryGetValue(assetName, out var alias))
                {
                    AddKey(alias, assetName);
                }
            }

            CacheManager.CacheValidLabels.Add(state.Label);
            CacheManager.Inventory.CacheEntries.Add(state.Label, labelCacheEntry);
            CacheManager.Inventory.CacheEntries.AddRangeUnique(assetsCacheEntries);
        }

        private class LabelRebuildState
        {
            public string Label;
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
