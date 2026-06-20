using System;
using System.Collections.Generic;
using System.Reflection;
using SpaceWarp2.API.Mods;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Resolves the mod ID a C# patch belongs to from its declaring assembly, mapping each SpaceWarp plugin's
    /// assembly to its mod ID. Assemblies with no SpaceWarp plugin - the game's Assembly-CSharp and PatchManager's
    /// own assembly aside - resolve to Redux's internal mod ID.
    /// </summary>
    internal static class PatchModId
    {
        // Assembly-CSharp and any unmapped assembly belong to Redux.
        private const string ReduxModId = "Ksp2Redux";

        private static Dictionary<Assembly, string> _byAssembly;

        /// <summary>
        /// Resolves the mod ID for the assembly that declares <paramref name="type" />.
        /// </summary>
        public static string Resolve(Type type) => Resolve(type.Assembly);

        /// <summary>
        /// Resolves the mod ID for <paramref name="assembly" />, defaulting to Redux's mod ID when the assembly
        /// is not a registered SpaceWarp plugin.
        /// </summary>
        public static string Resolve(Assembly assembly)
        {
            _byAssembly ??= Build();
            return _byAssembly.TryGetValue(assembly, out var modId) ? modId : ReduxModId;
        }

        private static Dictionary<Assembly, string> Build()
        {
            var map = new Dictionary<Assembly, string>();
            foreach (var descriptor in PluginList.AllPlugins)
            {
                // The plugin's own assembly covers internal and single-assembly mods.
                if (descriptor.Plugin != null)
                {
                    map[descriptor.Plugin.GetType().Assembly] = descriptor.Guid;
                }

                // The descriptor's assembly list covers external mods that ship extra library DLLs.
                foreach (var assembly in descriptor.Assemblies)
                {
                    map[assembly] = descriptor.Guid;
                }
            }

            return map;
        }
    }
}
