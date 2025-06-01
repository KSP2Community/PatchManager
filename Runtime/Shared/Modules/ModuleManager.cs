using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace PatchManager.Shared.Modules
{
    /// <summary>
    /// Manages the loading of PatchManager DLL modules.
    /// </summary>
    internal static class ModuleManager
    {
        internal static readonly List<IModule> Modules = new();

        /// <summary>
        /// Registers a PatchManager module DLL to be loaded. The module must contain a single class that inherits
        /// from <see cref="IModule"/>.
        /// </summary>
        /// <param name="path">Path to the module DLL file</param>
        [PublicAPI]
        public static void Register(Type moduleType)
        {
            if (!typeof(IModule).IsAssignableFrom(moduleType))
            {
                Logging.LogError(
                    $"module type {moduleType} is not a subclass of IModule"
                );
                return;
            }

            var instance = (IModule)Activator.CreateInstance(moduleType);
            Modules.Add(instance);
        }

        internal static void InitAll()
        {
            foreach (var module in Modules)
            {
                module.Init();
            }
        }

        internal static void LoadAll()
        {
            foreach (var module in Modules)
            {
                module.Load();
            }
        }

        internal static void PreLoadAll()
        {
        
            foreach (var module in Modules)
            {
                module.PreLoad();
            }
        }
    }
}