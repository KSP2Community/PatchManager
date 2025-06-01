using System.Collections.Generic;
using System.Reflection;
using KSP.Game;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using PatchManager.Shared;
using UniLinq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.Parts.Patchers
{
    internal static class PartModuleLoadPatcher
    {
        /// <summary>
        /// This is a map of part names to prefab names. It is populated by the PartDataDeserializePatcher.
        /// </summary>
        internal static Dictionary<string, string> PartPrefabMap { get; } = new();

        internal static void ApplyOnGameObject(ref GameObject gameObject, PartData partData)
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
    }
}