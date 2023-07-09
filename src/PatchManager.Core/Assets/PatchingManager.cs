using System.Collections;
using KSP.Game;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Utility;
using PatchManager.SassyPatching.Execution;
using PatchManager.Shared;
using PatchManager.Shared.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Assets;

internal static class PatchingManager
{
    private static readonly List<ITextPatcher> Patchers = new();
    private static readonly Universe Universe = new(RegisterPatcher, Logging.LogError, Logging.LogInfo);

    private static readonly PatchHashes CurrentPatchHashes = PatchHashes.CreateDefault();

    private static readonly int InitialLibraryCount = Universe.AllLibraries.Count;
    private static int _totalPatchCount;

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

    private static string PatchJson(string label, string assetName, string text)
    {
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
                Console.WriteLine($"Patch errored due to: {e.Message}");
                text = backup;
            }
        }

        _totalPatchCount += patchCount;
        if (patchCount > 0)
        {
            Logging.LogDebug($"Patched {assetName} with {patchCount} patches. Total: {_totalPatchCount}");
        }

        return text;
    }

    private static int _previousLibraryCount = -1;

    public static void ImportModPatches(string modName, string modFolder)
    {
        Universe.LoadPatchesInDirectory(new DirectoryInfo(modFolder), modName);

        var currentLibraryCount = Universe.AllLibraries.Count - InitialLibraryCount;

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
        Universe.RegisterAllPatches();
        Logging.LogInfo($"{Patchers.Count} patchers registered!");
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
        var archiveFilename = $"{label.Replace("/", "")}.zip";
        var archive = CacheManager.CreateArchive(archiveFilename);

        var labelCacheEntry = new CacheEntry
        {
            Label = label,
            ArchiveFilename = archiveFilename,
            Assets = new List<string>()
        };
        var assetsCacheEntries = new Dictionary<string, CacheEntry>();

        var handle = Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
        {
            try
            {
                var patchedText = PatchJson(label, asset.name, asset.text);
                archive.AddFile(asset.name, patchedText);
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
                Logging.LogError($"Unable to patch {asset.name} due to: {e.Message}");
            }
        });

        handle.Completed += results =>
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }

            archive.Save();

            CacheManager.CacheValidLabels.Add(label);
            CacheManager.Inventory.CacheEntries.Add(label, labelCacheEntry);
            CacheManager.Inventory.CacheEntries.AddRangeUnique(assetsCacheEntries);
            CacheManager.SaveInventory();

            Addressables.Release(results);
            Logging.LogDebug($"Cache for label '{label}' rebuilt.");
        };

        return handle;
    }

    public static void RebuildAllCache(Action resolve, Action<string> reject)
    {
        var keys = GameManager.Instance.Game.Assets._registeredResourceLocators
            .SelectMany(locator => locator.Keys)
            .ToList();
        keys.AddRange(Addressables.ResourceLocators.SelectMany(locator => locator.Keys));

        var handles = keys.Select(key => key.ToString())
            .Distinct()
            .Where(key => !CacheManager.CacheValidLabels.Contains(key))
            .Select(RebuildCache);

        CoroutineUtil.Instance.DoCoroutine(WaitForCacheRebuild(handles, resolve));
    }

    private static IEnumerator WaitForCacheRebuild(
        IEnumerable<AsyncOperationHandle<IList<TextAsset>>> handles,
        Action resolve
    )
    {
        foreach (var handle in handles)
        {
            while (!handle.IsDone)
            {
                yield return null;
            }
        }

        Logging.LogDebug("Cache for all labels rebuilt.");
        resolve();
    }
}