using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// Lua module exposed under the global <c>J</c> namespace, providing helpers for constructing JSON literals
/// that MoonSharp's auto-conversion does not produce.
/// </summary>
[MoonSharpModule(Namespace = "J")]
public class JsonModule
{
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
