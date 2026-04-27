using System.Collections.Generic;
using System;
using System.Reflection;
using KSP.Game;
using KSP.Sim.Definitions;
using PatchManager.Shared;
using Redux.Ksp1Import.Modules;
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
                if (behaviourType == null || !typeof(Component).IsAssignableFrom(behaviourType))
                {
                    Logging.LogWarning(
                        $"Skipping invalid part module behaviour on {partData.partName}: {behaviourType}");
                    continue;
                }

                // Debug.Log($"ApplyOnGameObject - {partData.partName} testing {behaviourType.FullName}");
                if (obj.GetComponent(behaviourType) != null)
                {
                    continue;
                }

                // Debug.Log($"ApplyOnGameObject - {partData.partName} adding {behaviourType.FullName}");
                var instance = obj.AddComponent(behaviourType);
                if (instance == null)
                {
                    Logging.LogWarning(
                        $"Unable to add part module behaviour {behaviourType.FullName} to {partData.partName}.");
                    continue;
                }

                Logging.LogInfo(
                    $"Attempting to setup serialized fields on {partData.partName} of type {behaviourType}");
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
                    var data = default(SerializedModuleData);
                    var foundData = false;
                    foreach (var moduleData in module.ModuleData)
                    {
                        if (moduleData.DataObject != null && moduleData.DataObject.GetType() == field.FieldType)
                        {
                            data = moduleData;
                            foundData = true;
                            break;
                        }
                    }

                    if (!foundData)
                    {
                        Logging.LogWarning(
                            $"No serialized data of type {field.FieldType.FullName} found for module {behaviourType.FullName} on {partData.partName}.");
                        continue;
                    }

                    data.DataObject?.RebuildDataContext();
                    field.SetValue(instance, data.DataObject);
                }

                Ksp1PartModuleRuntimeSetup.Configure(instance, obj, partData);
            }

            foreach (var component in obj.GetComponents<PartBehaviourModule>())
            {
                // Debug.Log($"ApplyOnGameObject - {partData.partName} checking {component.GetType().FullName}");
                var t = component.GetType();
                if (partData.serializedPartModules.All(x => x.BehaviourType != t))
                {
                    if (IsRequiredBySerializedModule(t, partData))
                    {
                        continue;
                    }

                    // Debug.Log($"ApplyOnGameObject - {partData.partName} removing {component.GetType().FullName}");
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(component);
                    }
                    else
                    {
                        Object.Destroy(component);
                    }
                }
            }

            gameObject = obj;
        }

        private static bool IsRequiredBySerializedModule(Type componentType, PartData partData)
        {
            foreach (var module in partData.serializedPartModules)
            {
                var behaviourType = module.BehaviourType;
                if (behaviourType == null)
                {
                    continue;
                }

                foreach (var attribute in behaviourType.GetCustomAttributes(typeof(RequireComponent), true))
                {
                    if (attribute is RequireComponent requireComponent && RequiresComponent(requireComponent, componentType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool RequiresComponent(RequireComponent requireComponent, Type componentType)
        {
            return RequiresComponent(requireComponent.m_Type0, componentType) ||
                   RequiresComponent(requireComponent.m_Type1, componentType) ||
                   RequiresComponent(requireComponent.m_Type2, componentType);
        }

        private static bool RequiresComponent(Type requiredType, Type componentType)
        {
            return requiredType != null && requiredType.IsAssignableFrom(componentType);
        }
    }
}
