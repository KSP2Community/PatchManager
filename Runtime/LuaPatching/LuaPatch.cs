using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching;

[MoonSharpUserData]
public class LuaPatch
{
    public IConverter ConverterInstance;

    public Func<DynValue, string> PatchMethod;

    public string Label;

    [CanBeNull] public string Name;

    public string Stage;

    public DynValue ApplyFirst(JToken json)
    {
        var instance = ConverterInstance.FromJson(json);
        if (instance.IsNil()) return DynValue.Nil;
        var result = PatchMethod(instance);
        if (result != null && result.Equals("remove", StringComparison.OrdinalIgnoreCase)) return DynValue.Nil;
        return instance;
    }

    public DynValue ApplyInChain(DynValue previous)
    {
        if (previous.IsNil()) return previous;
        var result = PatchMethod(previous);
        if (result != null && result.Equals("remove", StringComparison.OrdinalIgnoreCase)) return DynValue.Nil;
        return previous;
    }

    public LuaPatch OnStage(string stage)
    {
        Stage = stage;
        return this;
    }
}
