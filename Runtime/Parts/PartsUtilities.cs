using System;
using System.Collections.Generic;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using PatchManager.Parts.Attributes;
using UniLinq;

namespace PatchManager.Parts
{
    /// <summary>
    /// Utilities for parts patching.
    /// </summary>
    public static class PartsUtilities
    {
        private static Dictionary<string, (Type componentModule, Type behaviour)> _componentModules;

        private static void BuildComponentModuleDictionary()
        {
            _componentModules = new();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(type => !type.IsAbstract).Where(type => type.IsSubclassOf(typeof(PartComponentModule))))
                {
                    try
                    {
                        var mod = (PartComponentModule)Activator.CreateInstance(type);
                        var behaviour = mod.PartBehaviourModuleType;
                        var tuple = (type, behaviour);
                        var name1 = type.Name;
                        var name2 = name1.Replace("PartComponent", "");
                        _componentModules[name1] = tuple;
                        if (!name1.Equals(name2))
                        {
                            _componentModules[name2] = tuple;
                        }
                    }
                    catch
                    {
                        //ignored
                    }
                }
            }
        }

        /// <summary>
        /// Map of module short-name (with and without the <c>PartComponent</c> prefix) to the module's component
        /// type and behaviour type. Lazily populated by scanning every loaded assembly.
        /// </summary>
        internal static IReadOnlyDictionary<string, (Type componentModule, Type behaviour)> ComponentModules
        {
            get
            {
                if (_componentModules == null)
                {
                    BuildComponentModuleDictionary();
                }

                return _componentModules;
            }
        }


        private static Dictionary<string, Type> _dataModules;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _componentModules = null;
            _dataModules = null;
        }

        private static void BuildDataModuleDictionary()
        {
            _dataModules = new();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(type => !type.IsAbstract)
                             .Where(type => type.IsSubclassOf(typeof(ModuleData))))
                {
                    var name1 = type.Name;
                    var name2 = name1.Replace("Data_", "");
                    _dataModules[name1] = type;
                    if (!name1.Equals(name2))
                    {
                        _dataModules[name2] = type;
                    }
                }
            }
        }

        /// <summary>
        /// Map of data-module short-name (with and without the <c>Data_</c> prefix) to its <see cref="ModuleData" />
        /// type. Lazily populated by scanning every loaded assembly.
        /// </summary>
        internal static IReadOnlyDictionary<string, Type> DataModules
        {
            get
            {
                if (_dataModules == null)
                {
                    BuildDataModuleDictionary();
                }

                return _dataModules;
            }
        }

        /// <summary>
        /// Map of <see cref="ModuleData" /> type to the registered Lua-facing adapter type. Populated by
        /// <see cref="GrabModuleDataAdapters" /> and <see cref="RegisterModuleDataAdapter{T}" />.
        /// </summary>
        internal static readonly Dictionary<Type, Type> ModuleDataAdapters = new();

        /// <summary>
        /// Discovers every type marked with <see cref="Attributes.ModuleDataAdapterAttribute" /> across all loaded
        /// assemblies and registers them in <see cref="ModuleDataAdapters" />.
        /// </summary>
        internal static void GrabModuleDataAdapters()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(x => x.GetTypes())
                         .Where(x => x.GetCustomAttributes(typeof(ModuleDataAdapterAttribute),
                                 false)
                             .Any())
                         .Select(x => (type: x, attr: (ModuleDataAdapterAttribute)x.GetCustomAttributes(typeof(ModuleDataAdapterAttribute),
                                 false)
                             .FirstOrDefault())))
            {
                foreach (var dataType in type.attr.ValidTypes)
                {
                    ModuleDataAdapters[dataType] = type.type;
                }
            }
        }

        /// <summary>
        /// Registers a module data adapter for the given types.
        /// </summary>
        /// <param name="validTargets">The types that this adapter is valid for.</param>
        /// <typeparam name="T">The type of the adapter.</typeparam>
        public static void RegisterModuleDataAdapter<T>(params Type[] validTargets)
        {
            foreach (var type in validTargets)
            {
                ModuleDataAdapters[type] = typeof(T);
            }
        }
    }
}
