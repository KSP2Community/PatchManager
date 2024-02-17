using HarmonyLib;
using JetBrains.Annotations;
using PatchManager.Core.Assets;

namespace PatchManager.Core.Patches.Runtime;

[HarmonyPatch(typeof(LoadingBar))]
internal static class LoadingBarPatch
{
    public static bool InjectPatchManagerTips = false;
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LoadingBar.ShuffleLoadingTip))]
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static bool ShuffleLoadingTip(ref LoadingBar __instance)
    {
        if (!InjectPatchManagerTips)
            return true;
        __instance.tipsText.text = $"Patch Manager: {PatchingManager.TotalPatchCount} patches";
        if (PatchingManager.TotalErrorCount > 0)
            __instance.tipsText.text += $", {PatchingManager.TotalErrorCount} errors";

        return false;
    }
}