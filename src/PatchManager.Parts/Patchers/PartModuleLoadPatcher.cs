using System.Reflection;
using HarmonyLib;
using KSP.Game;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using PatchManager.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.Parts.Patchers;

[HarmonyPatch]
internal static class PartModuleLoadPatcher
{
    /// <summary>
    /// This is a map of part names to prefab names. It is populated by the PartDataDeserializePatcher.
    /// </summary>
    internal static Dictionary<string, string> PartPrefabMap { get; } = new();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAssemblyPartTracker), nameof(ObjectAssemblyPartTracker.OnPartPrefabLoaded))]
    internal static void ApplyOnGameObjectOAB(IObjectAssemblyAvailablePart obj, ref GameObject prefab)
    {
        ApplyOnGameObject(ref prefab, obj.PartData);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimulationObjectView), nameof(SimulationObjectView.InitializeView))]
    internal static void ApplyOnGameObjectFlight(
        ref GameObject instance,
        IUniverseView universe,
        SimulationObjectModel model
    )
    {
        if (!model.IsPart)
        {
            return;
        }

        ApplyOnGameObject(ref instance, model.Part.PartData);
    }

    private static void ApplyOnGameObject(ref GameObject gameObject, PartData partData)
    {
        var obj = gameObject;

        if (PartPrefabMap.TryGetValue(partData.partName, out var prefabName))
        {
            var prefab = GameManager.Instance.Assets.LoadAssetAsync<GameObject>(prefabName).WaitForCompletion();
            obj = Object.Instantiate(prefab);
        }

        foreach (var module in partData.serializedPartModules)
        {
            var behaviourType = module.BehaviourType;
            // Debug.Log($"ApplyOnGameObject - {partData.partName} testing {behaviourType.FullName}");
            if (obj.GetComponent(behaviourType) != null)
            {
                continue;
            }

            // Debug.Log($"ApplyOnGameObject - {partData.partName} adding {behaviourType.FullName}");
            var instance = obj.AddComponent(behaviourType);
            Logging.LogInfo($"Attempting to setup serialized fields on {partData.partName} of type {behaviourType}");
            foreach (var field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                         .Concat(behaviourType.GetFields(BindingFlags.Public | BindingFlags.Instance)))
            {
                // Logging.LogInfo($"Found field: {field.Name} of type {field.FieldType}");
                if (!field.GetCustomAttributes(typeof(SerializeField), false).Any())
                {
                    continue;
                }

                // Logging.LogInfo($"Field has SerializeField attribute");
                if (!field.FieldType.IsSubclassOf(typeof(ModuleData)))
                {
                    continue;
                }

                // Logging.LogInfo($"Field type {field.FieldType} is subclass of ModuleData, setting value");
                var data = module.ModuleData.FirstOrDefault(x => x.DataObject.GetType() == field.FieldType);
                data.DataObject?.RebuildDataContext();
                field.SetValue(instance, data.DataObject);
            }
        }

        foreach (var component in obj.GetComponents<PartBehaviourModule>())
        {
            // Debug.Log($"ApplyOnGameObject - {partData.partName} checking {component.GetType().FullName}");
            var t = component.GetType();
            if (partData.serializedPartModules.All(x => x.BehaviourType != t))
            {
                // Debug.Log($"ApplyOnGameObject - {partData.partName} removing {component.GetType().FullName}");
                Object.Destroy(component);
            }
        }

        gameObject = obj;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PartComponent), nameof(PartComponent.SetDefinition))]
    internal static void SetDefinition(object definitionData)
    {
        if (definitionData is PartDefinition partDefinition)
        {
            var name = partDefinition.Properties.partName;
            var def = GameManager.Instance.Game.Parts.Get(name);
            for (var i = partDefinition.Modules.Count - 1; i >= 0; i--)
            {
                var i2 = i;
                if (def.data.serializedPartModules.All(x => x.Name != partDefinition.Modules[i2].Name))
                {
                    partDefinition.Modules.RemoveAt(i);
                }
            }

            foreach (var mod in def.data.serializedPartModules)
            {
                if (partDefinition.Modules.All(x => x.Name != mod.Name))
                {
                    partDefinition.Modules.Add(mod);
                }
            }
        }
    }
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(SaveLoadManager), nameof(SaveLoadManager.PrivateLoadCommon))]
    // internal static void LoadCommon(
    //     LoadOrSaveCampaignTicket loadOrSaveCampaignTicket,
    //     LoadGameData loadGameData,
    //     SequentialFlow loadingFlow,
    //     OnLoadOrSaveCampaignFinishedCallback onLoadOrSaveCampaignFinishedCallback)
    // {
    //     void UpdateVessels(Action resolve, Action<string> reject)
    //     {
    //         foreach (var vessel in loadGameData.SavedGame.Vessels)
    //         {
    //             // Lets change only a few things
    //             // Add modules, change resource containers
    //             foreach (var part in vessel.parts)
    //             {
    //                 var name = part.partName;
    //                 var def = GameManager.Instance.Game.Parts.Get(name);
    //                 for (var i = part.PartModulesState.Count - 1; i >= 0; i--)
    //                 {
    //                     var i2 = i;
    //                     if (def.data.serializedPartModules.All(x => x.Name != part.PartModulesState[i2].Name))
    //                     {
    //                         part.PartModulesState.RemoveAt(i);
    //                     }
    //                 }
    //
    //                 foreach (var mod in def.data.serializedPartModules)
    //                 {
    //                     if (part.PartModulesState.All(x => x.Name != mod.Name))
    //                     {
    //                         part.PartModulesState.Add(mod);
    //                     }
    //                 }
    //
    //                 foreach (var resource in def.data.resourceContainers)
    //                 {
    //                     if (!part.partState.resources.ContainsKey(resource.name))
    //                     {
    //                         part.partState.resources[resource.name] = new ContainedResourceState
    //                         {
    //                             name = resource.name,
    //                             storedUnits = resource.initialUnits,
    //                             capacityUnits = resource.capacityUnits
    //                         };
    //                     }
    //                 }
    //             }
    //         }
    //         resolve();
    //     }
    //     loadingFlow.AddAction(new GenericFlowAction("Updating serialized vessels...", UpdateVessels));
    // }

}