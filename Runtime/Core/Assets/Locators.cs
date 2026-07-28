using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Assets
{
    /// <summary>
    /// A class that holds all the custom resource locators for patched asset files.
    /// </summary>
    public static class Locators
    {
        private static readonly List<IResourceLocator> ResourceLocators = new();

        // CoreModule.RegisterResourceLocator (a loading flow action) calls Register() on every Play Mode
        // enter. With Domain Reload disabled this static list persists across sessions, so without clearing
        // it the locators accumulate and LocateAll returns duplicate locations. A Domain Reload would have
        // emptied it; do the same here.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            ResourceLocators.Clear();
        }

        /// <summary>
        /// Register a custom resource locator.
        /// </summary>
        /// <param name="locator">Locator to register.</param>
        public static void Register(IResourceLocator locator)
        {
            ResourceLocators.Add(locator);
        }

        /// <summary>
        /// Locate assets by label.
        /// </summary>
        /// <param name="label">Label of the assets to be located.</param>
        /// <param name="locations">List of locations of the found assets.</param>
        /// <returns>True if any assets were found, false otherwise.</returns>
        public static bool LocateAll(object label, out List<IResourceLocation> locations)
        {
            return LocateAll(label, typeof(TextAsset), out locations);
        }

        /// <summary>
        /// Locate assets by key and requested type across every Patch Manager
        /// asset-domain locator.
        /// </summary>
        public static bool LocateAll(
            object key,
            System.Type type,
            out List<IResourceLocation> locations
        )
        {
            locations = new List<IResourceLocation>();
            foreach (var locator in ResourceLocators)
            {
                if (
                    locator.Locate(key, type, out var foundLocations)
                    && foundLocations != null
                )
                {
                    locations.AddRange(foundLocations);
                }
            }

            return locations.Count > 0;
        }
    }
}
