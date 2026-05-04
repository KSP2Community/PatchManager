using System;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts;

[PatchManagerModule("Parts")]
[MoonSharpUserData]
public class PartsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public PartsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public LuaPatch PatchAll(Script script, Func<PartUserData, string> callback)
    {
        return _core.PatchAll(script, "Part", "parts_data", callback.ToPatchMethod());
    }

    public LuaPatch Patch(Script script, string name, Func<PartUserData, string> callback)
    {
        return _core.Patch(script, "Part", "parts_data", name, callback.ToPatchMethod());
    }
}
