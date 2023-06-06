using HarmonyLib;
using PatchManager.Core.Flow;
using PatchManager.Core.Utility;
using KSP.Game;
using SpaceWarp.API.Loading;

namespace PatchManager.Core.HarmonyPatches;

[HarmonyPatch]
public static class GameManagerPatch
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartBootstrap))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void StartBootstrap_Postfix(GameManager __instance)
    {
        Logging.LogInfo("GameManager.StartBootstrap");

        SaveLoad.AddFlowActionToGameLoadAfter(
            new PatchPartDataFlowAction(PatchManagerPlugin.Instance.CachePath, PatchManagerPlugin.Instance.PatchesPath),
            null
        );
    }
}