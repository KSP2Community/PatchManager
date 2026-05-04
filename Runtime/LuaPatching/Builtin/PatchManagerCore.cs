using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Builtin;

[MoonSharpUserData]
public class PatchManagerCore
{
    private Universe _universe;
    public PatchManagerCore(Universe universe)
    {
        _universe = universe;
    }

    public DynValue this[string name] => _universe.Submodules[name];


    #region Core Methods
    public LuaPatch PatchAll(Script context, string converter, string label, Func<DynValue, string> method)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
        }

        if (method == null)
        {
            throw new Exception($"Registering {converter}:{label} with null method");
        }

        var newPatch = new LuaPatch
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = null,
            PatchMethod = method,
            Stage = context.Globals.Get("ModId").CastToString()
        };
        _universe.AddPatch(newPatch);
        return newPatch;
    }

    public LuaPatch Patch(Script context, string converter, string label, string name, Func<DynValue, string> method)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
        }
        if (method == null)
        {
            throw new Exception($"Registering {converter}:{label}:{name} with null method");
        }

        var newPatch = new LuaPatch
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = name,
            PatchMethod = method,
            Stage = context.Globals.Get("ModId").CastToString()
        };
        _universe.AddPatch(newPatch);
        return newPatch;
    }

    public void New(string converter, string label, string name, DynValue newObject)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
        }

        var newPatch = new LuaAsset()
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = name,
            CurrentValue = newObject,
        };

        _universe.AddAsset(newPatch);
    }

    public Stage ImplicitStage(Script context, string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        stage.RunsAfter.Add(_universe.LastImplicitWithinMod.GetValueOrDefault(
            context.Globals.Get("ModId").CastToString(), _universe.LastImplicitGlobal));
        return stage;
    }

    public Stage GlobalStage(string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        stage.RunsAfter.Add(_universe.LastImplicitGlobal);
        return stage;
    }

    public Stage Stage(string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        return stage;
    }

    #endregion

    #region Other Methods

    public bool Loaded(string modId)
    {
        return _universe.AllMods.Contains(modId);
    }
    #endregion
}
