using System;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using PatchManager.LuaPatching;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// Wraps an <see cref="IUserDataDescriptor" /> so that string-key lookups that miss the type's declared
/// members fall through to the type's string indexer.
/// </summary>
/// <remarks>
/// Default MoonSharp dispatch resolves <c>obj.foo</c> as a member lookup, returning nil when no member
/// named <c>foo</c> exists. With this wrapper, a nil result on a string index is retried with
/// <c>isDirectIndexing = false</c>, which routes through the type's <c>this[string]</c> indexer.
/// Lets <see cref="JsonUserData" /> subclasses expose their JSON properties as if they were declared C# members.
/// Installed automatically for every type with a string-typed indexer via <see cref="FallbackRegistrationPolicy" />.
/// </remarks>
public class IndexerFallbackDescriptor : IUserDataDescriptor
{
    private readonly IUserDataDescriptor _inner;

    /// <summary>
    /// Creates a wrapper that delegates to <paramref name="inner" /> with the indexer-fallback behavior layered on top.
    /// </summary>
    /// <param name="inner">The descriptor to wrap.</param>
    public IndexerFallbackDescriptor(IUserDataDescriptor inner) => _inner = inner;

    /// <inheritdoc />
    public string Name => _inner.Name;

    /// <inheritdoc />
    public Type Type => _inner.Type;

    /// <summary>
    /// Resolves an index access. On a member-lookup miss a JsonUserData is routed to its native LuaGet path, and
    /// any other string-indexer type retries with <c>isDirectIndexing = false</c> to reach its <c>this[string]</c>.
    /// </summary>
    /// <param name="script">The active script.</param>
    /// <param name="obj">The instance being indexed.</param>
    /// <param name="index">The index value (member name for direct indexing).</param>
    /// <param name="isDirectIndexing">True when MoonSharp is performing member-style access.</param>
    /// <returns>The resolved value, or <see cref="DynValue.Nil" /> when neither lookup matches.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when the wrapped descriptor surfaces a CLR exception. Rewrapped so MoonSharp decorates the error with the Lua call site.</exception>
    public DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
    {
        try
        {
            if (obj is JsonUserData jud)
            {
                // Resolve declared members (methods, Count, ...) but never the CLR indexer. Forcing direct
                // indexing keeps MoonSharp out of get_Item, whose presence would otherwise bypass the native Lua
                // path. On a member miss, route string and number keys to LuaGet.
                var member = _inner.Index(script, obj, index, isDirectIndexing: true);
                if (member != null && !member.IsNil()) return member;
                if (index.Type == DataType.String || index.Type == DataType.Number) return jud.LuaGet(index);
                return member;
            }

            var result = _inner.Index(script, obj, index, isDirectIndexing);
            if ((result == null || result.IsNil()) && index.Type == DataType.String)
            {
                return _inner.Index(script, obj, index, false);
            }

            return result;
        }
        catch (Exception e) when (ShouldWrap(e))
        {
            throw Wrap(e, obj, index, isWrite: false);
        }
    }

    /// <summary>
    /// Routes a string-key assignment that the wrapped descriptor declined to handle through the type's
    /// <c>this[string]</c> indexer.
    /// </summary>
    /// <param name="script">The active script.</param>
    /// <param name="obj">The instance being indexed.</param>
    /// <param name="index">The index value (member name for direct indexing).</param>
    /// <param name="value">The value being assigned.</param>
    /// <param name="isDirectIndexing">True when MoonSharp is performing member-style access.</param>
    /// <returns>True if the assignment was handled, false if neither lookup accepts it.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when the wrapped descriptor surfaces a CLR exception. Rewrapped so MoonSharp decorates the error with the Lua call site.</exception>
    public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
    {
        try
        {
            if (obj is JsonUserData jud)
            {
                // Force direct indexing so MoonSharp never reaches set_Item. On a member miss, route to LuaSet.
                if (_inner.SetIndex(script, obj, index, value, isDirectIndexing: true)) return true;
                if (index.Type == DataType.String || index.Type == DataType.Number)
                {
                    jud.LuaSet(index, value);
                    return true;
                }

                return false;
            }

            var handled = _inner.SetIndex(script, obj, index, value, isDirectIndexing);
            if (!handled && isDirectIndexing && index.Type == DataType.String)
            {
                return _inner.SetIndex(script, obj, index, value, false);
            }

            return handled;
        }
        catch (Exception e) when (ShouldWrap(e))
        {
            throw Wrap(e, obj, index, isWrite: true);
        }
    }

    private static bool ShouldWrap(Exception e) =>
        e is not ScriptRuntimeException && e is not InterpreterException;

    private ScriptRuntimeException Wrap(Exception e, object obj, DynValue index, bool isWrite)
    {
        var inner = e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;
        var typeName = obj?.GetType().Name ?? Type?.Name ?? "<unknown>";
        var key = index.Type == DataType.String ? index.String : index.ToPrintString();
        var op = isWrite ? "writing" : "reading";
        return new ScriptRuntimeException($"{inner.GetType().Name} {op} {typeName}.{key}: {inner.Message}");
    }

    /// <inheritdoc />
    public string AsString(object obj) =>  _inner.AsString(obj);

    /// <inheritdoc />
    public DynValue MetaIndex(Script script, object obj, string metaname) => _inner.MetaIndex(script, obj, metaname);

    /// <inheritdoc />
    public bool IsTypeCompatible(Type type, object obj) => _inner.IsTypeCompatible(type, obj);
}
