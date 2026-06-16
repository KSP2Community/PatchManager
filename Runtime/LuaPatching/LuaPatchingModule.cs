using System;
using System.Linq;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Utility;
using PatchManager.Shared;
using PatchManager.Shared.Modules;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// IModule that performs the late assembly scan for the Lua patching universe, registering Lua-exposed types,
    /// PatchManager submodules, and converters with MoonSharp once all mod assemblies are loaded.
    /// </summary>
    public class LuaPatchingModule : BaseModule
    {
        /// <summary>
        /// Walks every loaded assembly to register Lua-exposed types with MoonSharp and to populate
        /// <see cref="Universe.SubmoduleTypes" /> and <see cref="Universe.Converters" />.
        /// </summary>
        public override void Init()
        {
            UserData.RegistrationPolicy = new FallbackRegistrationPolicy();

            // PatchManager contributes PM/J into each mod env the SpaceWarp runtime forks.
            if (!ReduxLib.GameInterfaces.ModRuntime.Contributors.OfType<Builtin.PatchManagerEnvContributor>().Any())
            {
                ReduxLib.GameInterfaces.ModRuntime.Contributors.Add(new Builtin.PatchManagerEnvContributor());
            }

            // Bridge Lua's 1-based array indices to the 0-based LuaIndex used by the C#-facing position members.
            // Only fires when a CLR method parameter is typed LuaIndex, so it is inert until the wrappers adopt it.
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Number, typeof(LuaIndex),
                dv => new LuaIndex((int)dv.Number - 1));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                UserData.RegisterAssembly(assembly, false);
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attributes = type.GetCustomAttributes(true);
                    if (attributes.OfType<MoonSharpUserDataAttribute>().Any())
                    {
                        DelegateRegistry.RegisterDelegatesFromMethodsOf(type);
                    }

                    if (attributes.OfType<PatchManagerModuleAttribute>().FirstOrDefault() is { } pmma)
                    {
                        if (!attributes.OfType<MoonSharpUserDataAttribute>().Any())
                        {
                            Logging.LogWarning($"LuaPatchingModule: found Patch Manager module {pmma.SubmoduleName} without MoonSharpUserData attribute, skipping!");
                            continue;
                        }

                        Universe.SubmoduleTypes[pmma.SubmoduleName] = type;
                    }

                    if (attributes.OfType<ConverterAttribute>().FirstOrDefault() is { } conv)
                    {
                        if (!typeof(IConverter).IsAssignableFrom(type))
                        {
                            Logging.LogWarning($"LuaPatchingModule: found Patch Manager converter {conv.Name} that does not implement IConverter, skipping");
                            continue;
                        }
                        Universe.Converters[conv.Name] = (IConverter)Activator.CreateInstance(type);
                    }
                }
            }
        }
    }
}
