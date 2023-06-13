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
        Logging.LogInfo($"Patched {assetName} with {patchCount} patches. Total: {_totalPatchCount}");
        return text;
    }

    public static void ImportModPatches(string modName, string modFolder)
    {
        Universe.LoadPatchesInDirectory(new DirectoryInfo(modFolder), modName);

        Logging.LogInfo($"{Universe.AllLibraries.Count - InitialLibraryCount} libraries loaded!");

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
    
    public static void InvalidateCacheIfNeeded()
    {
        var checksum = Hash.FromJsonObject(CurrentPatchHashes);

        if (CacheManager.Inventory.Checksum == checksum)
        {
            Logging.LogInfo("Cache is valid, skipping rebuild.");
            CacheManager.CacheValidLabels.AddRange(CacheManager.Inventory.CacheEntries.Keys);
            return;
        }

        Logging.LogInfo("Cache is invalid, rebuilding.");
        CacheManager.InvalidateCache();
        CacheManager.Inventory.Checksum = checksum;
        CacheManager.Inventory.Patches = CurrentPatchHashes;
    }

    public static void RebuildCache(string label, Action onComplete)
    {
        var archiveFilename = $"{label}.zip";
        var archive = CacheManager.CreateArchive(archiveFilename);
        var cacheEntry = new CacheEntry
        {
            Label = label,
            ArchiveFilename = archiveFilename,
            Assets = new List<string>()
        };

        Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
        {
            try
            {
                var patchedText = PatchJson(label, asset.name, asset.text);
                cacheEntry.Assets.Add(asset.name);
                archive.AddFile(asset.name, patchedText);
            }
            catch (Exception e)
            {
                Logging.LogError($"Unable to patch {asset.name} due to: {e.Message}");
            }
        }).Completed += results =>
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                Logging.LogWarning($"Unable to rebuild cache for label '{label}'.");
            }

            CacheManager.Inventory.CacheEntries.Add(label, cacheEntry);
            CacheManager.SaveInventory();
            archive.Save();
            CacheManager.CacheValidLabels.Add(label);
            Addressables.Release(results);
            Logging.LogDebug($"Cache for label '{label}' rebuilt.");

            onComplete();
        };
    }
}