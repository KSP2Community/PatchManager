using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace PatchManager.Runtime.LuaPatching.Utility;
public class IndexerFallbackDescriptor : IUserDataDescriptor
{
    private readonly IUserDataDescriptor _inner;

    public IndexerFallbackDescriptor(IUserDataDescriptor inner) => _inner = inner;
    public string Name => _inner.Name;
    public Type Type => _inner.Type;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    public bool IsTypeCompatible(Type type, object obj) => _inner.IsTypeCompatible(type, obj);
}