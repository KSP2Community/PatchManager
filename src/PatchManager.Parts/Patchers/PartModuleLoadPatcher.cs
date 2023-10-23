using System.Reflection;
using HarmonyLib;
using KSP.Game;
using KSP.Game.Flow;
using KSP.Game.Load;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using KSP.Sim.State;
using PatchManager.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.Parts.Patchers;

[HarmonyPatch]
internal static class PartModuleLoadPatcher
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAssemblyPartTracker), nameof(ObjectAssemblyPartTracker.OnPartPrefabLoaded))]
    internal static void ApplyOnGameObjectOAB(IObjectAssemblyAvailablePart obj, ref GameObject prefab)
    {
        // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} beginning patch");
        foreach (var module in obj.PartData.serializedPartModules)
        {
            var behaviourType = module.BehaviourType;
            // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} testing {behaviourType.FullName}");
            if (prefab.GetComponent(behaviourType) == null)
            {
                // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} adding {behaviourType.FullName}");
                var instance = prefab.AddComponent(behaviourType);
                Logging.LogInfo($"Attempting to setup serialized fields on {obj.Name} of type {behaviourType}");
                foreach (var field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                             .Concat(behaviourType.GetFields(BindingFlags.Public | BindingFlags.Instance)))
                {
                    // Logging.LogInfo($"Found field: {field.Name} of type {field.FieldType}"); 
                    if (field.GetCustomAttributes(typeof(SerializeField), false).Any())
                    {
                        // Logging.LogInfo($"Field has SerializeField attribute");
                        if (field.FieldType.IsSubclassOf(typeof(ModuleData)))
                        {
                            // Logging.LogInfo($"Field type {field.FieldType} is subclass of ModuleData, setting value");
                            var data = module.ModuleData.FirstOrDefault(x => x.DataObject.GetType() == field.FieldType);
                            data.DataObject.RebuildDataContext();
                            field.SetValue(instance, data.DataObject);
                        }
                    }
                }
            }
        }

        foreach (var component in prefab.GetComponents<PartBehaviourModule>())
        {
            // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} checking {component.GetType().FullName}");
            var t = component.GetType();
            if (obj.PartData.serializedPartModules.All(x => x.BehaviourType != t))
            {
                // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} removing {component.GetType().FullName}");
                Object.Destroy(component);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimulationObjectView), nameof(SimulationObjectView.InitializeView))]
    internal static void ApplyOnGameObjectFlight(
        GameObject instance,
        IUniverseView universe,
        SimulationObjectModel model
    )
    {
        if (!model.IsPart)
            return;

        var part = model.Part;
        // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} beginning patch");
        foreach (var module in part.PartData.serializedPartModules)
        {
            var behaviourType = module.BehaviourType;
            // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} testing {behaviourType.FullName}");
            if (instance.GetComponent(behaviourType) == null)
            {
                // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} adding {behaviourType.FullName}");
                var inst = instance.AddComponent(behaviourType);
                // Logging.LogInfo($"Attempting to setup serialized fields on {part.Name} of type {behaviourType}");
                foreach (var field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                             .Concat(behaviourType.GetFields(BindingFlags.Public | BindingFlags.Instance)))
                {
                    // Logging.LogInfo($"Found field: {field.Name} of type {field.FieldType}"); 
                    if (field.GetCustomAttributes(typeof(SerializeField), false).Any())
                    {
                        // Logging.LogInfo($"Field has SerializeField attribute");
                        if (field.FieldType.IsSubclassOf(typeof(ModuleData)))
                        {
                            // Logging.LogInfo($"Field type {field.FieldType} is subclass of ModuleData, setting value");
                            var data = module.ModuleData.FirstOrDefault(x => x.DataObject.GetType() == field.FieldType);
                            data.DataObject.RebuildDataContext();
                            field.SetValue(inst, data.DataObject);
                        }
                    }
                }
            }
        }

        foreach (var component in instance.GetComponents<PartBehaviourModule>())
        {
            // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} checking {component.GetType().FullName}");
            var t = component.GetType();
            if (part.PartData.serializedPartModules.All(x => x.BehaviourType != t))
            {
                // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} removing {component.GetType().FullName}");
                Object.Destroy(component);
            }
        }
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