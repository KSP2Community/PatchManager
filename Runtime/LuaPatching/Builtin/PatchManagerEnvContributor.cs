using MoonSharp.Interpreter;
using ReduxLib.GameInterfaces;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// PatchManager's contribution to each mod environment the runtime creates. Seeds only the patch-specific
/// globals (<c>PM</c> and, once per script, <c>J</c>). The general mod-loading globals (ModId, Location,
/// require) are seeded by the runtime itself.
/// </summary>
public class PatchManagerEnvContributor : IModEnvContributor
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
        globals["Config"] = UserData.Create(universe.BuildLuaConfig(globals.Get("ModId").CastToString(), null));

        var script = globals.OwnerScript;
        if (script.Globals.Get("J").IsNil())
        {
            JsonModule.Register(script);
        }
    }
}
