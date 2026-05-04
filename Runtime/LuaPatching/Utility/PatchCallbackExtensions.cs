using System;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// Extension methods that adapt typed user callbacks into the <see cref="Func{DynValue, String}" /> signature
/// the patch pipeline expects, unwrapping the wrapped UserData on the way in.
/// </summary>
public static class PatchCallbackExtensions
{
    /// <summary>
    /// Adapts an <see cref="Action{T}" /> callback into a patch method that always returns <c>null</c>
    /// (i.e. never signals removal).
    /// </summary>
    /// <typeparam name="T">The expected UserData wrapper type.</typeparam>
    /// <param name="action">The user callback that mutates the wrapped value.</param>
    /// <returns>A patch method that unwraps the <see cref="DynValue" />, invokes <paramref name="action" />, and returns <c>null</c>.</returns>
    public static Func<DynValue, string> ToPatchMethod<T>(this Action<T> action) where T : class =>
        dv =>
        {
            action((T)dv.UserData.Object);
            return null;
        };

    /// <summary>
    /// Adapts a <see cref="Func{T, String}" /> callback into a patch method that forwards the user-supplied
    /// return value (e.g. <c>"remove"</c>) to the patch pipeline.
    /// </summary>
    /// <typeparam name="T">The expected UserData wrapper type.</typeparam>
    /// <param name="func">The user callback that mutates the wrapped value and optionally signals removal.</param>
    /// <returns>A patch method that unwraps the <see cref="DynValue" />, invokes <paramref name="func" />, and forwards the result.</returns>
    public static Func<DynValue, string> ToPatchMethod<T>(this Func<T, string> func) where T : class =>
        dv => func((T)dv.UserData.Object);
}
