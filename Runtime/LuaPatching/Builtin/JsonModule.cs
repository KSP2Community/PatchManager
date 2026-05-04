using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// This is a JSON specif
/// </summary>
[MoonSharpModule(Namespace = "J")]
public class JsonModule
{
    [MoonSharpModuleMethod]
    public DynValue Empty(ScriptExecutionContext context, CallbackArguments args)
    {
        return UserData.Create(new JsonUserData(new JArray()));
    }

    [MoonSharpModuleMethod]
    public DynValue Int(ScriptExecutionContext context, CallbackArguments args)
    {
        return UserData.Create(new JsonUserData(new JValue((long)args[0].CastToNumber()!)));
    }
}