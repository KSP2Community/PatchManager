using MoonSharp.Interpreter;
using ReduxLib.GameInterfaces;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// PatchManager's contribution to each mod environment the runtime creates.
/// </summary>
/// <remarks>
/// Adds only the patch-specific globals (<c>PM</c> and, once per script, <c>J</c>). The general mod-loading
/// globals (ModId, Location, require) are contributed by the runtime itself.
/// </remarks>
public sealed class PatchManagerEnvContributor : IModEnvContributor
{
    /// <inheritdoc />
    public void Contribute(Table globals)
    {
        var universe = Core.CoreModule.CurrentUniverse;
        if (universe == null)
        {
            return;
        }

        globals["PM"] = universe.PatchManagerLibraryInstance;

        var script = globals.OwnerScript;
        if (script.Globals.Get("J").IsNil())
        {
            JsonModule.Register(script);
        }
    }
}
