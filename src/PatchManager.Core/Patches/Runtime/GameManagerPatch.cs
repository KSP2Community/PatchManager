using HarmonyLib;
using KSP.Game;
using PatchManager.Core.Flow;

namespace PatchManager.Core.Patches.Runtime;

/// <summary>
/// Patches the <see cref="GameManager.StartBootstrap"/> method.
/// </summary>
[HarmonyPatch]
public static class GameManagerPatch
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartBootstrap))]
    [HarmonyPostfix]
    private static void StartBootstrap_Postfix()
    {
        FlowManager.RunPatch();
    }
}