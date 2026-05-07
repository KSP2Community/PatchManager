using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// Lua module exposed under the global <c>J</c> namespace, providing helpers for constructing JSON
/// literals that MoonSharp's auto-conversion does not produce. Also installed as callable: invoking
/// <c>J(value)</c> converts <paramref name="value" /> to a <see cref="JsonUserData" />.
/// </summary>
[MoonSharpModule(Namespace = "J")]
public class JsonModule
{
    /// <summary>
    /// Registers the <c>J</c> module on <paramref name="script" /> and installs a <c>__call</c>
    /// metamethod so <c>J(value)</c> from Lua converts <paramref name="value" /> to a
    /// <see cref="JsonUserData" />.
    /// </summary>
    /// <param name="script">The script to register the module on.</param>
    public static void Register(Script script)
    {
        script.Globals.RegisterModuleType<JsonModule>();

        var jTable = script.Globals.Get("J").Table;
        if (jTable == null) return;

        var meta = new Table(script);
        meta.Set("__call", DynValue.NewCallback(InvokeCall));
        jTable.MetaTable = meta;
    }

    private static DynValue InvokeCall(ScriptExecutionContext context, CallbackArguments args)
    {
        if (args.Count < 2)
        {
            throw new ScriptRuntimeException("J(...) requires a value to convert");
        }
        var token = JsonUserData.GetJTokenForDynValue(args[1]);
        return UserData.Create(new JsonUserData(token));
    }

    /// <summary>
    /// Returns an empty JSON array as a <see cref="JsonUserData" />.
    /// </summary>
    /// <remarks>
    /// Use this when assigning into a fresh JSON slot that must round-trip as an array rather than an object.
    /// MoonSharp cannot distinguish an empty Lua table from an empty Lua array, so without an existing slot to
    /// take its type from, an empty <c>{}</c> becomes a JSON object by default. Existing array slots already
    /// preserve their type via <see cref="JsonUserData.GetJTokenForDynValue(JToken,DynValue)" />; this helper is
    /// for the no-previous-slot case.
    /// </remarks>
    /// <param name="context">The MoonSharp execution context.</param>
    /// <param name="args">The call arguments (unused).</param>
    /// <returns>A <see cref="JsonUserData" /> wrapping an empty <c>JArray</c>.</returns>
    [MoonSharpModuleMethod]
    public DynValue Empty(ScriptExecutionContext context, CallbackArguments args)
    {
        return UserData.Create(new JsonUserData(new JArray()));
    }

    /// <summary>
    /// Wraps the first argument as an integer JSON value, forcing the integer slot type.
    /// </summary>
    /// <remarks>
    /// Use this when assigning a Lua number into a fresh JSON slot that must round-trip as an integer rather
    /// than a float (for example IDs or counts). Existing integer slots already preserve their type during
    /// assignment via <see cref="JsonUserData.GetJTokenForDynValue(JToken,DynValue)" />; this helper is for
    /// the no-previous-slot case.
    /// </remarks>
    /// <param name="context">The MoonSharp execution context.</param>
    /// <param name="args">Call arguments; <c>args[0]</c> is cast to a number and truncated to a long.</param>
    /// <returns>A <see cref="JsonUserData" /> wrapping a <c>JValue</c> of integer type.</returns>
    [MoonSharpModuleMethod]
    public DynValue Int(ScriptExecutionContext context, CallbackArguments args)
    {
        return UserData.Create(new JsonUserData(new JValue((long)args[0].CastToNumber())));
    }
}
