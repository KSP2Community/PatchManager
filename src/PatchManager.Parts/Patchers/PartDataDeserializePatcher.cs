using System.Text.RegularExpressions;
using HarmonyLib;
using KSP.Sim.Definitions;

namespace PatchManager.Parts.Patchers;

[HarmonyPatch]
internal static class PartDataDeserializePatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartCore), nameof(PartCore.UpgradeAndRead))]
    // ReSharper disable once InconsistentNaming
    internal static void PartCore_UpgradeAndRead(string rawFileData, ref PartCore __result)
    {
        var prefabAddress = Regex.Match(rawFileData, "\"PrefabAddress\":\\s*\"([^\"]*)\"").Groups[1].Value;
        if (!string.IsNullOrEmpty(prefabAddress))
        {
            PartModuleLoadPatcher.PartPrefabMap[__result.data.partName] = prefabAddress;
        }

        var iconAddress = Regex.Match(rawFileData, "\"IconAddress\":\\s*\"([^\"]*)\"").Groups[1].Value;
        if (!string.IsNullOrEmpty(iconAddress))
        {
            OabUtilsPatcher.PartIconMap[__result.data.partName] = iconAddress;
        }
    }
}