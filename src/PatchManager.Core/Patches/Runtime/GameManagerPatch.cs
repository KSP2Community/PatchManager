using HarmonyLib;
using KSP.Game;
using PatchManager.Core.Flow;

namespace PatchManager.Core.Patches.Runtime;

/// <summary>
/// Patches the <see cref="GameManager.StartBootstrap"/> method to add custom flow actions.
/// </summary>
[HarmonyPatch]
public static class GameManagerPatch
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartBootstrap))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void StartBootstrap_Postfix(GameManager __instance)
    {
        FlowManager.AddActionsToFlow(__instance.LoadingFlow);
    }
}