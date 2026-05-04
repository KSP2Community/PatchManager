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
        /// The current patch universe; created by <see cref="GenerateUniverse" />.
        /// </summary>
        internal static Universe Universe;

        private static readonly PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

        private static int _initialLibraryCount;
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
            _initialLibraryCount = Universe.LibraryCount;
        }

        private static string PatchJson(string label, string assetName, string text)
        {
            Logging.LogDebug($"Patching {label}:{assetName}");
            var patchCount = 0;
            var errorCount = 0;
            if (text != "")
            {
                var result = Universe.RunAllPatchesFor(label, assetName, JToken.Parse(text), out patchCount, out errorCount);
                text = result == null ? "" : result.ToString(UseIndentedOutput ? Formatting.Indented : Formatting.None);
                TotalErrorCount += errorCount;
                TotalPatchCount += patchCount;
            }
            if (patchCount > 0)
            {
                Logging.LogDebug($"Patched {label}:{assetName} with {patchCount} patches. Total: {TotalPatchCount}");
                TotalDefinitionPatchCount += 1;
            }

            return text;
        }

        private static string PatchJson(LuaAsset data)
        {
            Logging.LogDebug($"Patching {data.Label}:{data.Name}");

            var t = Universe.RunAllPatchesFor(data, out var patchCount, out var errorCount);
            TotalErrorCount += errorCount;
            TotalPatchCount += patchCount;
            if (patchCount > 0)
            {
                Logging.LogDebug($"Patched {data.Label}:{data.Name} with {patchCount} patches. Total: {TotalPatchCount}");
            }

            return t == null ? "" : t.ToString(UseIndentedOutput ? Formatting.Indented : Formatting.None);
        }


        private static int _previousLibraryCount = -1;

        /// <summary>
        /// Loads every <c>.lua</c> patch file under <paramref name="modFolder" /> and records each <c>.patch</c>
        /// file's hash in the cache checksum.
        /// </summary>
        /// <param name="modName">The mod ID; used as the script's <c>ModId</c> global.</param>
        /// <param name="modFolder">The directory containing the mod's patches.</param>
        public static void ImportModPatches(string modName, string modFolder)
        {
            Universe.LoadPatchesInDirectory(new DirectoryInfo(modFolder), modName);

            var currentLibraryCount = Universe.LibraryCount - _initialLibraryCount;

            if (currentLibraryCount > _previousLibraryCount)
            {
                Logging.LogInfo($"{currentLibraryCount} mod libraries loaded!");
                _previousLibraryCount++;
            }

            var patchFiles = Directory.GetFiles(modFolder, "*.patch", SearchOption.AllDirectories);
            foreach (var patchFile in patchFiles)
            {
                var patchHash = Hash.FromFile(patchFile);
                CurrentPatchHashes.Patches.Add(patchFile, patchHash);
            }
        }

        /// <summary>
        /// Loads a single <c>.patch</c> file and records its hash in the cache checksum.
        /// </summary>
        /// <param name="fileInfo">The patch file to load.</param>
        public static void ImportSinglePatch(FileInfo fileInfo)
        {
            Universe.LoadSinglePatchFile(fileInfo, new DirectoryInfo("."));
            CurrentPatchHashes.Patches.Add(fileInfo.FullName, Hash.FromFile(fileInfo.FullName));
        }

        /// <summary>
        /// Loads a patch from a <see cref="TextAsset" /> and records its hash in the cache checksum.
        /// </summary>
        /// <param name="asset">The text asset whose contents are the patch script.</param>
        /// <param name="modId">The mod ID to associate the patch with.</param>
        public static void ImportAssetPatch(TextAsset asset, string modId)
        {
            Universe.LoadPatchAsset(asset, modId);
            // TODO: Actually fix the double-loading of addressables rather than just changing Add to TryAdd
            CurrentPatchHashes.Patches.TryAdd($"{modId}/{asset.name}", Hash.FromString(asset.text));
        }

        /// <summary>
        /// Finalizes the universe's patch registry and logs the total registered-patch and new-asset counts.
        /// </summary>
        public static void RegisterPatches()
        {
            Logging.LogInfo($"Registering all patches!");
            Universe.SetupPatchesForRun();
            Logging.LogInfo($"{Universe.TotalPatchCount} patchers registered!");
            Logging.LogInfo($"{Universe.AllNewAssets.Count} assets created!");
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

        private static AsyncOperationHandle<IList<TextAsset>> RebuildCache(string label)
        {
            Logging.LogInfo($"Patching: {label}");
            var archiveFilename = $"{label.Replace("/", "")}.zip";

            var archiveFiles = new Dictionary<string, string>();

            var labelCacheEntry = new CacheEntry
            {
                Label = label,
                ArchiveFilename = archiveFilename,
                Assets = new List<string>()
            };
            var assetsCacheEntries = new Dictionary<string, CacheEntry>();
            var unchanged = !_createdAssets.ContainsKey(label);


            if (_createdAssets.TryGetValue(label, out var createdAsset))
            {
                foreach (var (name, text) in createdAsset)
                {
                    var patchedText = PatchJson(text);
                    if (string.IsNullOrEmpty(patchedText)) continue;
                    archiveFiles[name] = patchedText;
                    labelCacheEntry.Assets.Add(name);
                    assetsCacheEntries.Add(name, new CacheEntry
                    {
                        Label = name,
                        ArchiveFilename = archiveFilename,
                        Assets = new List<string> { name }
                    });
                }

                createdAsset.Clear();
                _createdAssets.Remove(label);
            }

            var handle = Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
            {
                try
                {
                    string patchedText;
                    if (Universe.HasAnyPatchFor(label, asset.name))
                    {
                        patchedText = PatchJson(label, asset.name, asset.text);
                        unchanged = false; // Any patch can change
                    }
                    else
                    {
                        patchedText = asset.text;
                    }

                    // Handle deletion
                    if (string.IsNullOrEmpty(patchedText))
                    {
                        return;
                    }

                    archiveFiles[asset.name] = patchedText;
                    labelCacheEntry.Assets.Add(asset.name);
                    assetsCacheEntries.Add(asset.name, new CacheEntry
                    {
                        Label = asset.name,
                        ArchiveFilename = archiveFilename,
                        Assets = new List<string> { asset.name }
                    });
                }
                catch (Exception e)
                {
                    Logging.LogError($"Unable to patch {asset.name} due to: {e.Message}, {e.StackTrace}");
                }
            });


            void SaveArchive()
            {
                var archive = CacheManager.CreateArchive(archiveFilename);
                foreach (var archiveFile in archiveFiles)
                {
                    archive.AddFile(archiveFile.Key, archiveFile.Value);
                }

                archive.Save();

                CacheManager.CacheValidLabels.Add(label);
                CacheManager.Inventory.CacheEntries.Add(label, labelCacheEntry);
                CacheManager.Inventory.CacheEntries.AddRangeUnique(assetsCacheEntries);
                CacheManager.SaveInventory();

                Logging.LogInfo($"Cache for label '{label}' rebuilt.");
            }

            handle.Completed += results =>
            {
                try
                {
                    if (unchanged)
                    {
                        return;
                    }

                    SaveArchive();
                }
                finally
                {
                    if (results.Status == AsyncOperationStatus.Succeeded)
                    {
                        Addressables.Release(results);
                    }
                }
            };

            return handle;
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
                }
            }

            TotalNewAssetCount = Universe.AllNewAssets.Count;
            UpdateLoadingBarData();

            resolve();
        }


        /// <summary>
        /// Schedules a per-label cache-rebuild flow action for every label that has either patches or queued new
        /// assets, then resolves the supplied callback.
        /// </summary>
        /// <param name="resolve">Callback invoked once scheduling finishes.</param>
        /// <param name="reject">Reject callback (currently unused).</param>
        public static void RebuildAllCache(Action resolve, Action<string> reject)
        {
            var distinctKeys = Universe.PatchedLabels.Concat(_createdAssets.Keys).Distinct().ToList();

            GenericFlowAction CreateIndexedFlowAction(int idx)
            {
                return new GenericFlowAction(
                    $"Patch Manager: {distinctKeys[idx]}",
                    (resolve2, _) =>
                    {
                        var handle = RebuildCache(distinctKeys[idx]);
                        CoroutineUtil.Instance.DoCoroutine(WaitForCacheRebuildSingleHandle(handle, resolve2, idx + 1 == distinctKeys.Count));
                    });
            }

            if (distinctKeys.Count > 0)
            {
                for (var i = distinctKeys.Count - 1; i >= 0; i--)
                {
                    GameManager.Instance.LoadingFlow.FlowActions.Insert(
                        GameManager.Instance.LoadingFlow.flowIndex + 1,
                        CreateIndexedFlowAction(i)
                    );
                }
            }

            resolve();
        }

        private static IEnumerator WaitForCacheRebuildSingleHandle(
            AsyncOperationHandle<IList<TextAsset>> handle,
            Action resolve,
            bool isFinalHandle
        )
        {
            while (!handle.IsDone)
            {
                // "Shuffle" it
                UpdateLoadingBarData();
                yield return null;
            }

            if (isFinalHandle)
            {
                CacheManager.SetTotalPatchCount(TotalPatchCount);
                CacheManager.SetTotalErrorCount(TotalErrorCount);
                CacheManager.SetTotalDefinitionCount(TotalDefinitionPatchCount);
                CacheManager.SetTotalAssetCount(TotalNewAssetCount);
            }
            resolve();
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
