using HarmonyLib;
using KSP.Rendering.Planets;
using PatchManager.Shared;

namespace PatchManager.Planets.Patches;

[HarmonyPatch(typeof(VolumeCloudManager))]
internal static class VolumeCloudManagerPatches
{
    [HarmonyPatch(nameof(VolumeCloudManager.OnScaledCloudModelLoaded))]
    [HarmonyPrefix]
    public static void ScaleClouds(VolumeCloudManager __instance, string bodyName, ScaledCloudConfiguration model)
    {
        Logging.LogInfo($"Loaded a scaled cloud configuration for {bodyName}");
        Logging.LogInfo("It has the following layers");
        foreach (var layer in model.scaledCloudLayers)
        {
            Logging.LogInfo(layer.CloudLayerName);
            Logging.LogInfo($"- Radius: {layer.Radius}");
        }
    }
}