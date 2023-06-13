using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.SassyPatching.Execution;
using PatchManager.Shared;
using PatchManager.Shared.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Assets;

internal static class PatchingManager
{
    internal static readonly List<ITextPatcher> Patchers = new();
    internal static readonly Universe Universe = new(RegisterPatcher, Logging.LogError);

    private static int _totalPatchCount;

    public static void RebuildCache(string label)
    {
        var archive = CacheManager.CreateArchive(label);
        var cacheEntry = new CacheEntry
        {
            Label = label,
            ArchiveFilename = $"{label}.zip",
            Assets = new List<string>()
        };

        Addressables.LoadAssetsAsync<TextAsset>(label, asset =>
        {
            try
            {
                var patchedText = PatchJson(asset.name, asset.text);
                Logging.LogDebug($"Patched {asset.name}.");
                cacheEntry.Assets.Add(asset.name);
                archive.AddFile(asset.name, patchedText);
                Logging.LogDebug($"Added {asset.name} to cache archive '{label}.zip'.");
            }
            catch (Exception e)
            {
                Logging.LogError($"Unable to patch {asset.name} due to: {e.Message}");
            }
        }).Completed += results =>
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception($"Unable to rebuild cache for label '{label}'.");
            }

            CacheManager.Inventory.CacheEntries.Add(label, cacheEntry);
            CacheManager.SaveInventory();
            archive.Save();
            CacheManager.CacheValidLabels.Add(label);
            Addressables.Release(results);
        };
    }

    private static string PatchJson(string assetName, string text)
    {
        var patchCount = 0;

        foreach (var patcher in Patchers)
        {
            var backup = text;
            try
            {
                var wasPatched = patcher.TryPatch("part_data", ref text);
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
}