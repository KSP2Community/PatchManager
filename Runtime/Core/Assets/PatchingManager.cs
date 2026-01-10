using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using KSP.Game.Flow;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Utility;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.Shared;
using SpaceWarp2.API.Mods;
using UniLinq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Assets
{
    internal static class PatchingManager
    {
        internal static Universe Universe;

        private static readonly PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

        private static int _initialLibraryCount;
        private static Dictionary<string, List<(string name, ISelectable data)>> _createdAssets = new();

        internal static int TotalPatchCount;
        internal static int TotalErrorCount;
        internal static int TotalNewAssetCount;
        internal static int TotalDefinitionPatchCount;

        public static void GenerateUniverse(HashSet<string> singleFileModIds)
        {
            var loadedPlugins = PluginList.AllEnabledAndActivePlugins.Select(x => x.Guid).ToList();
            loadedPlugins.AddRange(singleFileModIds);
            Universe = new(Logging.LogError, Logging.LogMessage,
                loadedPlugins);
            _initialLibraryCount = Universe.AllLibraries.Count;
        }

        // private static void RegisterPatcher(ITextPatcher patcher)
        // {
        //     for (var index = 0; index < Patchers.Count; index++)
        //     {
        //         if (Patchers[index].Priority <= patcher.Priority)
        //         {
        //             continue;
        //         }
        //
        //         Patchers.Insert(index, patcher);
        //         return;
        //     }
        //
        //     Patchers.Add(patcher);
        // }
        //
        // private static void RegisterGenerator(ITextAssetGenerator generator)
        // {
        //     for (var index = 0; index < Generators.Count; index++)
        //     {
        //         if (Generators[index].Priority <= generator.Priority)
        //         {
        //             continue;
        //         }
        //
        //         Generators.Insert(index, generator);
        //         return;
        //     }
        //
        //     Generators.Add(generator);
        // }

        private static string PatchJson(string label, string assetName, string text)
        {
            Logging.LogInfo($"Patching {label}:{assetName}");

            text = Universe.RunAllPatchesFor(label, assetName, text, out var patchCount, out var errorCount);
            TotalErrorCount += errorCount;
            TotalPatchCount += patchCount;
            if (patchCount > 0)
            {
                Logging.LogInfo($"Patched {label}:{assetName} with {patchCount} patches. Total: {TotalPatchCount}");
                TotalDefinitionPatchCount += 1;
            }

            return text;
        }
        
        private static string PatchJson(string label, string assetName, ISelectable data)
        {
            Logging.LogInfo($"Patching {label}:{assetName}");

            var text = Universe.RunAllPatchesFor(label, assetName, data, out var patchCount, out var errorCount);
            TotalErrorCount += errorCount;
            TotalPatchCount += patchCount;
            if (patchCount > 0)
            {
                Logging.LogInfo($"Patched {label}:{assetName} with {patchCount} patches. Total: {TotalPatchCount}");
            }

            return text;
        }


        private static int _previousLibraryCount = -1;

        public static void ImportModPatches(string modName, string modFolder)
        {
            Universe.LoadPatchesInDirectory(new DirectoryInfo(modFolder), modName);

            var currentLibraryCount = Universe.AllLibraries.Count - _initialLibraryCount;

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

        public static void ImportSinglePatch(FileInfo fileInfo)
        {
            Universe.LoadSinglePatchFile(fileInfo, new DirectoryInfo("."));
            CurrentPatchHashes.Patches.Add(fileInfo.FullName, Hash.FromFile(fileInfo.FullName));
        }

        public static void ImportAssetPatch(TextAsset asset, string modId)
        {
            Universe.LoadPatchAsset(asset, modId);
            // TODO: Actually fix the double-loading of addressables rather than just changing Add to TryAdd
            CurrentPatchHashes.Patches.TryAdd($"{modId}/{asset.name}", Hash.FromString(asset.text));
        }

        public static void RegisterPatches()
        {
            Logging.LogInfo($"Registering all patches!");
            Universe.RegisterAllPatches();
            Logging.LogInfo($"{Universe.TotalPatchCount} patchers registered!");
            Logging.LogInfo($"{Universe.Generators.Count} generators registered!");
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
                    var patchedText = PatchJson(label, name, text);
                    if (patchedText == "") continue;
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
                    var patchedText = PatchJson(label, asset.name, asset.text);
                    if (patchedText != asset.text)
                    {
                        unchanged = false;
                    }

                    // Handle deletion
                    if (patchedText == "")
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

            if (handle.Status == AsyncOperationStatus.Failed && !unchanged)
            {
                SaveArchive();
            }

            handle.Completed += results =>
            {
                if (results.Status != AsyncOperationStatus.Succeeded || unchanged)
                {
                    return;
                }

                SaveArchive();
                Addressables.Release(results);
            };

            return handle;
        }

        public static void CreateNewAssets(Action resolve, Action<string> reject)
        {
            foreach (var generator in Universe.Generators)
            {
                try
                {
                    var data = generator.Create(out var label, out var name);
                    Logging.LogDebug($"Generated an asset with the label {label}, and name {name}:\n{data}");

                    if (!_createdAssets.ContainsKey(label))
                    {
                        _createdAssets[label] = new List<(string name, ISelectable data)>();
                    }

                    if (!_createdAssets[label].Any(x => x.name == name))
                    {
                        _createdAssets[label].Add((name, data));
                    }

                }
                catch (Exception e)
                {
                    TotalErrorCount += 1;
                    Logging.LogError($"Failed to generate an asset due to: {e}");
                }
            }
            
            TotalNewAssetCount = Universe.Generators.Count;
            UpdateLoadingBarData();

            resolve();
        }


        public static void RebuildAllCache(Action resolve, Action<string> reject)
        {
            var distinctKeys = Universe.LoadedLabels.Concat(_createdAssets.Keys).Distinct().ToList();

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

        public static void ImportConfigurations(Action resolve, Action<string> reject)
        {
            Universe.ImportConfigs(CacheManager.Inventory.SerializedConfigs);
            resolve();
        }

        public static void ExportConfigurations(Action resolve, Action<string> reject)
        {
            CacheManager.Inventory.SerializedConfigs = Universe.ExportConfigs();
            resolve();
        }
    }
}
