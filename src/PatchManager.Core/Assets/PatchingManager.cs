using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using KSP.Game;
using KSP.Game.Flow;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Patches.Runtime;
using PatchManager.Core.Utility;
using PatchManager.SassyPatching.Execution;
using PatchManager.Shared;
using PatchManager.Shared.Interfaces;
using SpaceWarp;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Versions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Assets;

internal static class PatchingManager
{
    internal static readonly List<ITextPatcher> Patchers = new();
    internal static readonly List<ITextAssetGenerator> Generators = new();
    internal static Universe Universe;

    private static readonly PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

    private static int _initialLibraryCount;
    private static Dictionary<string, List<(string name, string text)>> _createdAssets = new();

    internal static int TotalPatchCount;
    private static readonly Regex VersionPreprocessRegex = new Regex(@"[^0-9.]");
    public static void GenerateUniverse()
    {
        var loadedPlugins = PluginList.AllEnabledAndActivePlugins.Select(x => x.Guid).ToList();
        UniverseLogMessage($"{string.Join(", ", loadedPlugins)}");
        Universe = new(RegisterPatcher, UniverseLogError, UniverseLogMessage, RegisterGenerator,
            loadedPlugins);
        _initialLibraryCount = Universe.AllLibraries.Count;

        void UniverseLogError(string error)
        {
            Debug.Log($"[PatchManager.Universe] [ERR]: {error}");
        }

        void UniverseLogMessage(string message)
        {
            
            Debug.Log($"[PatchManager.Universe] [MSG]: {message}");
        }
    }

    private static void RegisterPatcher(ITextPatcher patcher)
    {
        for (var index = 0; index < Patchers.Count; index++)
        {
            if (Patchers[index].Priority <= patcher.Priority)
            {
                continue;
            }

            Patchers.Insert(index, patcher);
            return;
        }

        Patchers.Add(patcher);
    }
    private static void RegisterGenerator(ITextAssetGenerator generator)
    {
        for (var index = 0; index < Generators.Count; index++)
        {
            if (Generators[index].Priority <= generator.Priority)
            {
                continue;
            }

            Generators.Insert(index, generator);
            return;
        }

        Generators.Add(generator);
    }

    private static string PatchJson(string label, string assetName, string text)
    {
        Console.WriteLine($"Patching {label}:{assetName}");
        var patchCount = 0;

        foreach (var patcher in Patchers)
        {
            var backup = text;
            try
            {
                var wasPatched = patcher.TryPatch(label, assetName, ref text);
                if (wasPatched)
                {
                    patchCount++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Patch of {label}:{assetName} errored due to: {e}");
                text = backup;
            }

            if (text == "")
            {
                break;
            }
        }

        TotalPatchCount += patchCount;
        if (patchCount > 0)
        {
            Console.WriteLine($"Patched {label}:{assetName} with {patchCount} patches. Total: {TotalPatchCount}");
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

    public static void RegisterPatches()
    {
        Logging.LogInfo($"Registering all patches!");
        Universe.RegisterAllPatches();
        Logging.LogInfo($"{Patchers.Count} patchers registered!");
        Logging.LogInfo($"{Generators.Count} generators registered!");
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
                archiveFiles[name] = text;
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
                Console.WriteLine($"Unable to patch {asset.name} due to: {e.Message}");
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

            Console.WriteLine($"Cache for label '{label}' rebuilt.");
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

    private static bool IsUsefulKey(string key)
    {
        key = key.Replace(".bundle", "").Replace(".json", "");
        if (int.TryParse(key, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            return false;
        }
        if (key.Length == 32)
        {
            return !key.All(x => "0123456789abcdef".Contains(x));
        }

        return !key.EndsWith(".prefab") && !key.EndsWith(".png");
    }

    public static void CreateNewAssets(Action resolve, Action<string> reject)
    {
        foreach (var generator in Generators)
        {
            try
            {
                var text = generator.Create(out var label, out var name);
                Logging.LogDebug($"Generated an asset with the label {label}, and name {name}:\n{text}");

                if (!_createdAssets.ContainsKey(label))
                    _createdAssets[label] = new List<(string name, string text)>();
                _createdAssets[label].Add((name, text));
            }
            catch (Exception e)
            {
                Logging.LogError($"Failed to generate an asset due to: {e}");
            }
        }

        resolve();
    }

    public static void RebuildAllCache(Action resolve, Action<string> reject)
    {


        var distinctKeys = Universe.LoadedLabels.Concat(_createdAssets.Keys).Distinct().ToList();

        LoadingBarPatch.InjectPatchManagerTips = true;
        GenericFlowAction CreateIndexedFlowAction(int idx)
        {
            return new GenericFlowAction(
                $"Patch Manager: {distinctKeys[idx]}",
                (resolve2, reject2) =>
                {
                    var handle = RebuildCache(distinctKeys[idx]);
                    var killTips = false;
                    if (idx + 1 < distinctKeys.Count)
                        GameManager.Instance.LoadingFlow._flowActions.Insert(GameManager.Instance.LoadingFlow._flowIndex + 1,
                            CreateIndexedFlowAction(idx+1));
                    else
                    {
                        killTips = true;
                    }
                    CoroutineUtil.Instance.DoCoroutine(WaitForCacheRebuildSingleHandle(handle, resolve2,killTips));
                });
        }

        if (distinctKeys.Count > 0)
        {
            GameManager.Instance.LoadingFlow._flowActions.Insert(GameManager.Instance.LoadingFlow._flowIndex + 1,
                CreateIndexedFlowAction(0));
        }

        resolve();
    }

    private static IEnumerator WaitForCacheRebuildSingleHandle(
        AsyncOperationHandle<IList<TextAsset>> handle,
        Action resolve,
        bool killLoadingBarTips
    )
    {
        while (!handle.IsDone)
        {
            // "Shuffle" it
            GameManager.Instance.Game.UI.LoadingBar.ShuffleLoadingTip();
            yield return null;
        }

        LoadingBarPatch.InjectPatchManagerTips = !killLoadingBarTips;
        resolve();
    }
}