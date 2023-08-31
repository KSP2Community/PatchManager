using System.Reflection;
using HarmonyLib;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
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
        foreach (var behaviourType in obj.PartData.serializedPartModules.Select(module => module.BehaviourType))
        {
            // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} testing {behaviourType.FullName}");
            if (prefab.GetComponent(behaviourType) == null)
            {
                // Debug.Log($"ApplyOnGameObjectOAB - {obj.PartData.partName} adding {behaviourType.FullName}");
                prefab.AddComponent(behaviourType);
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
        if (!model.IsPart) return;

        var part = model.Part;
        // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} beginning patch");
        foreach (var behaviourType in part.PartData.serializedPartModules.Select(module => module.BehaviourType))
        {
            // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} testing {behaviourType.FullName}");
            if (instance.GetComponent(behaviourType) == null)
            {
                // Debug.Log($"ApplyOnGameObjectFlight - {model.Part.PartName} adding {behaviourType.FullName}");
                instance.AddComponent(behaviourType);
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
}