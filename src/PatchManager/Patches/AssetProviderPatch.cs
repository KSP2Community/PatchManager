// [HarmonyPatch(typeof(LoadPartDataFilesFlowAction), nameof(LoadPartDataFilesFlowAction.DoAction))]
// [HarmonyPrefix]
// private static bool LoadPartDataFilesFlowAction_DoAction_Prefix(
//     Action resolve,
//     Action<string> reject,
//     // ReSharper disable once InconsistentNaming
//     LoadPartDataFilesFlowAction __instance
// )
// {
//     Logger.LogDebug("LoadPartDataFilesFlowAction_DoAction_Prefix");
//     Instance.CacheResourceLocator.Locate("parts_data", typeof(TextAsset), out var locations);
//     Addressables.LoadAssetsAsync<TextAsset>(locations, __instance.OnPartDataLoaded).Completed +=
//         results =>
//         {
//             if (results.Status == AsyncOperationStatus.Succeeded)
//             {
//                 __instance._game.Parts.IsDataLoaded = true;
//                 Addressables.Release(results);
//                 resolve();
//                 return;
//             }
//
//             Logger.LogError("Provider unable to find assets with label 'parts_data'.");
//             reject("Provider unable to find assets with label 'parts_data'.");
//         };
//
//     return false;
// }