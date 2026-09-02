using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace PatchManager.Shared.Modules
{
    /// <summary>
    /// Manages the loading of PatchManager DLL modules.
    /// </summary>
    internal static class ModuleManager
    {
        internal static readonly List<IModule> Modules = new();

        private static bool _playSessionActionsRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPlaySessionState()
        {
            _playSessionActionsRegistered = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RestorePlaySessionActions()
        {
            // On the first run the modules do not exist until PatchManager.Awake, which calls InitAll below.
            // On later editor runs with Domain Reload disabled the module instances persist, but SpaceWarp's
            // loading registries have just been reset and must be populated again before its loading flow is built.
            if (Modules.Count > 0)
            {
                RegisterPlaySessionActionsAll();
            }
        }

        /// <summary>
        /// Registers a PatchManager module DLL to be loaded. The module must contain a single class that inherits
        /// from <see cref="IModule" />.
        /// </summary>
        /// <param name="moduleType">The module's type; must implement <see cref="IModule" /> and have a parameterless constructor.</param>
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

            RegisterPlaySessionActionsAll();
        }

        private static void RegisterPlaySessionActionsAll()
        {
            if (_playSessionActionsRegistered)
            {
                return;
            }

            foreach (var module in Modules)
            {
                module.RegisterPlaySessionActions();
            }

            _playSessionActionsRegistered = true;
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
