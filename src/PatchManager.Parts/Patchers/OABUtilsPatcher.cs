using HarmonyLib;
using KSP.OAB;

namespace PatchManager.Parts.Patchers;

[HarmonyPatch]
internal class OABUtilsPatcher
{
    /// <summary>
    /// This is a map of part names to icon names. It is populated by the PartDataDeserializePatcher.
    /// </summary>
    internal static Dictionary<string,string> PartIconMap { get; } = new();

    /// <summary>
    /// This patch is used to override the icon name for a part.
    /// </summary>
    /// <param name="partName">Part name</param>
    /// <param name="__result">The icon name to return.</param>
    /// <returns>True if the original method should be called, false otherwise.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Utils), nameof(Utils.GetPartIconNameFromPartName))]
    internal static bool Utils_GetPartIconNameFromPartName(string partName, ref string __result)
    {
        if (PartIconMap.TryGetValue(partName, out var iconName))
        {
            __result = iconName;
            return false;
        }

        return true;
    }
}