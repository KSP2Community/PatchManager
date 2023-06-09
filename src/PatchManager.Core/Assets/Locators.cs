using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Assets;

/// <summary>
/// A class that holds all the custom resource locators for patched asset files.
/// </summary>
public static class Locators
{
    private static readonly List<IResourceLocator> ResourceLocators = new();

    /// <summary>
    /// Register a custom resource locator.
    /// </summary>
    /// <param name="locator">Locator to register.</param>
    public static void Register(IResourceLocator locator)
    {
        ResourceLocators.Add(locator);
    }

    /// <summary>
    /// Locate asset(s) by label and type.
    /// </summary>
    /// <param name="label">Label of the asset(s) to be located.</param>
    /// <param name="type">Type of the asset(s).</param>
    /// <param name="locations">List of locations of the found asset(s).</param>
    /// <returns>True if any assets were found, false otherwise.</returns>
    public static bool LocateAll(string label, Type type, out List<IResourceLocation> locations)
    {
        locations = new List<IResourceLocation>();
        foreach (var locator in ResourceLocators)
        {
            locator.Locate(label, type, out var foundLocations);
            locations.AddRange(foundLocations);
        }

        return locations.Count > 0;
    }
}