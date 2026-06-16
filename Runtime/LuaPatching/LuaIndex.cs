namespace PatchManager.LuaPatching;

/// <summary>
/// A position index that bridges the C# 0-based and Lua 1-based array conventions so one method body can serve
/// both languages. C# callers pass a plain int, used as-is (0-based). Lua numbers are turned into a LuaIndex by
/// the global MoonSharp converter, which subtracts 1, so part[1] in Lua and part[0] in C# address the same
/// element. This is the parameter type for every position-taking member (the integer indexer, RemoveAt, Insert).
/// </summary>
public readonly struct LuaIndex
{
    /// <summary>The 0-based index value.</summary>
    public readonly int Value;

    /// <summary>Creates a LuaIndex from a 0-based value.</summary>
    public LuaIndex(int value) => Value = value;

    /// <summary>Treats a C# int as a 0-based LuaIndex.</summary>
    public static implicit operator LuaIndex(int i) => new(i);

    /// <summary>Unwraps to the 0-based int value.</summary>
    public static implicit operator int(LuaIndex li) => li.Value;
}
