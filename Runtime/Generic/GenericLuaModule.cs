using System;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Generic;

[PatchManagerModule("JSON")]
[MoonSharpUserData]
public class GenericLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public GenericLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public LuaPatch PatchAll(Script script, string label, Func<JsonUserData, string> callback)
    {
        return _core.PatchAll(script, "JSON", label, callback.ToPatchMethod());
    }

    public LuaPatch Patch(Script script, string label, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "JSON", label, name, callback.ToPatchMethod());
    }

    public void New(string label, string name, DynValue value)
    {
        _core.New("JSON", label, name, value);
    }
}
