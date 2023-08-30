using HarmonyLib;
using KSP.Assets;
using KSP.Game;
using PatchManager.Core.Assets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Patches.Runtime;

[HarmonyPatch]
internal static class ExternalAssetsPatch
{
    private static AssetProvider Assets => GameManager.Instance.Assets;

    [HarmonyPatch(typeof(AssetProvider), nameof(AssetProvider.LocateAssetInExternalData))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool LocateAssetInExternalData(object key, Type T, out IResourceLocation location, ref bool __result)
    {
        location = null;

        if (Locators.LocateAll(key.ToString(), out var patchedLocations))
        {
            location = patchedLocations[0];
            __result = true;
            return false;
        }

        foreach (var registeredResourceLocator in Assets._registeredResourceLocators)
        {
            if (registeredResourceLocator.Locate(key, T, out var locations))
            {
                location = locations[0];
                __result = true;
                return false;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(AssetProvider), nameof(AssetProvider.LocateAssetsInExternalData))]
    [HarmonyPrefix]
    // ReSharper disable once RedundantAssignment,InconsistentNaming
    private static bool LocateAssetsInExternalData(object reference, ref List<IResourceLocation> __result)
    {
        if (Locators.LocateAll(reference.ToString(), out var patchedLocations))
        {
            __result = patchedLocations;
            return false;
        }

        __result = new List<IResourceLocation>();
        foreach (var registeredResourceLocator in Assets._registeredResourceLocators)
        {
            if (registeredResourceLocator.Locate(reference, typeof(object), out var locations))
            {
                __result.AddRange(locations);
            }
        }

        return false;
    }
}