using HarmonyLib;
using PatchManager.Core.Assets;

namespace PatchManager.Core.Patches.Runtime;

[HarmonyPatch(typeof(LoadingBar))]
public static class LoadingBarPatch
{
    public static bool InjectPatchManagerTips = false;
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LoadingBar.SetTips))]
    public static bool SetTips(ref LoadingBar __instance)
    {
        if (!InjectPatchManagerTips)
            return true;
        __instance.tipsText.text = $"Patch Manager: {PatchingManager.TotalPatchCount} patches";
        
        return false;
    }
}