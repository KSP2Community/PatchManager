using System;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Utility;

public static class PatchCallbackExtensions
{
    public static Func<DynValue, string> ToPatchMethod<T>(this Action<T> action) where T : class =>
        dv =>
        {
            action((T)dv.UserData.Object);
            return null;
        };

    public static Func<DynValue, string> ToPatchMethod<T>(this Func<T, string> func) where T : class =>
        dv => func((T)dv.UserData.Object);
}
