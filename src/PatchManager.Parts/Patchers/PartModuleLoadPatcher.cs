using System.Reflection;
using HarmonyLib;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.Parts.Patchers;

internal static class PartModuleLoadPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAssemblyPartTracker), nameof(ObjectAssemblyPartTracker.OnPartPrefabLoaded))]
    internal static void ApplyOnGameObjectOAB(IObjectAssemblyAvailablePart obj, ref GameObject prefab)
    {
        foreach (var behaviourType in obj.PartData.serializedPartModules.Select(module => module.BehaviourType))
        {
            if (prefab.GetComponent(behaviourType) == null)
            {
                prefab.AddComponent(behaviourType);
            }
        }

        foreach (var component in prefab.GetComponents<PartBehaviourModule>())
        {
            var t = component.GetType();
            if (obj.PartData.serializedPartModules.All(x => x.BehaviourType != t))
            {
                Object.Destroy(component);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimulationObjectView), nameof(SimulationObjectView.InitializeView))]
    internal static void ApplyOnGameObjectFlight(GameObject instance, IUniverseView universe, SimulationObjectModel model)
    {
        var part = model.Part;
        foreach (var behaviourType in part.PartData.serializedPartModules.Select(module => module.BehaviourType))
        {
            if (instance.GetComponent(behaviourType) == null)
            {
                instance.AddComponent(behaviourType);
            }
        }

        foreach (var component in instance.GetComponents<PartBehaviourModule>())
        {
            var t = component.GetType();
            if (part.PartData.serializedPartModules.All(x => x.BehaviourType != t))
            {
                Object.Destroy(component);
            }
        }
    }
}