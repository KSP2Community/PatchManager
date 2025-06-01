using System.Reflection;
using System.Reflection.Emit;
/*using HarmonyLib;
using KSP.VolumeCloud;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shoemaker.Patches
{
    public static class CloudRenderHelperPatches
    {
        // [HarmonyPatch(typeof(CloudRenderHelper))]
        // [HarmonyPatch(nameof(CloudRenderHelper.LoadingConfigCompleted))]
        // [HarmonyTranspiler]
        // public static IEnumerable<CodeInstruction> ModifyFunction(IEnumerable<CodeInstruction> instructions)
        // {
        //     yield return new CodeInstruction(OpCodes.Ldarg_1);
        //     yield return new CodeInstruction(OpCodes.Call,typeof(CloudRenderHelper).GetMethod(nameof(UpdateConfiguration),BindingFlags.Public | BindingFlags.Static));
        //     foreach (var instruction in instructions)
        //     {
        //         yield return instruction;
        //     }
        // }

        public static void UpdateConfiguration(AsyncOperationHandle<VolumeCloudConfiguration> configurationHandle)
        {
            if (configurationHandle.Status != AsyncOperationStatus.Succeeded) return;
            var bodyName = configurationHandle.Result.bodyName;
            if (!OverrideManager.VolumeCloudOverrides.TryGetValue(bodyName.ToLowerInvariant(),
                    out var volumeCloudConfigurationOverride)) return;
            //   LogInfo($"Found a configuration override for {bodyName}");
            volumeCloudConfigurationOverride.ApplyTo(configurationHandle.Result);
        }
    }
}*/

