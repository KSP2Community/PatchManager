using System;
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
    /// Resolves a string-key access; on a member-lookup miss, retries with <c>isDirectIndexing = false</c> so the
    /// access falls through to the wrapped type's <c>this[string]</c> indexer.
    /// </summary>
    /// <param name="script">The active script.</param>
    /// <param name="obj">The instance being indexed.</param>
    /// <param name="index">The index value (member name for direct indexing).</param>
    /// <param name="isDirectIndexing">True when MoonSharp is performing member-style access.</param>
    /// <returns>The resolved value, or <see cref="DynValue.Nil" /> when neither lookup matches.</returns>
    public DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
    {
        var result = _inner.Index(script, obj, index, isDirectIndexing);

        // 2. If it's nil/void and we are using a string index (like .Planets)
        if ((result == null || result.IsNil()) && index.Type == DataType.String)
        {
            return _inner.Index(script, obj, index, false);
        }

        return result;
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
    public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
    {
        var handled = _inner.SetIndex(script, obj, index, value, isDirectIndexing);

        if (!handled && isDirectIndexing && index.Type == DataType.String)
        {
            return _inner.SetIndex(script, obj, index, value, false);
        }

        return handled;
    }

    /// <inheritdoc />
    public string AsString(object obj) =>  _inner.AsString(obj);

    /// <inheritdoc />
    public DynValue MetaIndex(Script script, object obj, string metaname) => _inner.MetaIndex(script, obj, metaname);

    /// <inheritdoc />
    public bool IsTypeCompatible(Type type, object obj) => _inner.IsTypeCompatible(type, obj);
}
