using System;
using System.Collections.Generic;
using PatchManager.Core.Cache;
using PatchManager.Shared;
using UniLinq;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Assets
{
    /// <summary>
    /// Locates assets in archives of cached assets.
    /// </summary>
    internal class ArchiveResourceLocator : IResourceLocator
    {
        /// <inheritdoc />
        public string LocatorId => GetType().FullName;

        /// <inheritdoc />
        public IEnumerable<object> Keys => CacheManager.Inventory.CacheEntries.Keys;

        /// <inheritdoc />
        public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
        {
            var label = key.ToString();
            if (!CacheManager.Inventory.CacheEntries.TryGetValue(label, out var cacheEntry))
            {
                locations = new List<IResourceLocation>();
                return false;
            }

            locations = cacheEntry.Assets
                .Select(asset => new ResourceLocationBase(
                    cacheEntry.ArchiveFilename,
                    asset,
                    typeof(ArchiveResourceProvider).FullName,
                    typeof(TextAsset)
                ))
                .Cast<IResourceLocation>()
                .ToList();

            if (locations.Count == 0)
            {
                return false;
            }

            Logging.LogDebug($"Located key '{key}' with {locations.Count} locations.");
            return true;
        }
    }
}
