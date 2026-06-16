using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using PatchManager.Core.Assets;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;
using PatchManager.Shared;
using PatchManager.Shared.Modules;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Discovers C# [PMPatch] classes across loaded assemblies and registers their patch methods with the
    /// universe, mirroring how Lua patch scripts register through PM. Runs in PreLoad, after CoreModule has
    /// created the universe.
    /// </summary>
    public class CSharpPatchingModule : BaseModule
    {
        /// <inheritdoc />
        public override void PreLoad()
        {
            var universe = PatchingManager.Universe;
            if (universe == null)
            {
                Logging.LogWarning("CSharpPatchingModule: universe not created, skipping C# patch discovery");
                return;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    var pmPatch = type.GetCustomAttribute<PMPatchAttribute>();
                    if (pmPatch == null) continue;

                    object owner;
                    try
                    {
                        owner = Activator.CreateInstance(type);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError($"CSharpPatchingModule: failed to construct [PMPatch] class {type.FullName}: {ex.Message}");
                        continue;
                    }

                    var modId = string.IsNullOrEmpty(pmPatch.ModId) ? PatchModId.Resolve(type) : pmPatch.ModId;

                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Instance | BindingFlags.Static))
                    {
                        var target = method.GetCustomAttribute<PatchAttribute>();
                        if (target == null) continue;
                        RegisterPatch(universe, owner, method, target, modId);
                    }
                }
            }
        }

        private static void RegisterPatch(Universe universe, object owner, MethodInfo method, PatchAttribute target,
            string modId)
        {
            if (!Universe.Converters.TryGetValue(target.Converter, out var converter))
            {
                Logging.LogError(
                    $"CSharpPatchingModule: patch {method.DeclaringType?.Name}.{method.Name} uses unknown converter '{target.Converter}'");
                return;
            }

            var patch = new PatchDefinition
            {
                ConverterInstance = converter,
                Label = target.Label,
                Name = modId + ':' + method.Name,
                PatchModId = modId
            };

            foreach (var modifier in method.GetCustomAttributes().OfType<IPatchModifier>())
            {
                modifier.Apply(patch, owner);
            }

            patch.Do(BuildPatchMethod(owner, method));
            universe.AddPatch(patch);
            Logging.LogInfo(
                $"CSharpPatchingModule: registered C# patch {patch.Name} ({target.Converter}/{target.Label})");
        }

        // Wraps the C# method into the PatchDefinition's Func<DynValue, string> apply callback. The asset wrapper
        // produced by the converter is unwrapped from the DynValue and handed to the method.
        private static Func<DynValue, string> BuildPatchMethod(object owner, MethodInfo method)
        {
            var instance = method.IsStatic ? null : owner;
            return dv =>
            {
                var asset = dv.UserData?.Object;
                var result = method.Invoke(instance, new[] { asset });
                return result is PatchResult.Remove ? "remove" : null;
            };
        }
    }
}
