using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// JSON-array UserData base class that exposes the array as a name-indexed lookup table.
/// </summary>
/// <remarks>
/// Subclasses provide an item -> name function via <see cref="Name" /> (and optionally a custom item conversion via
/// <see cref="Convert" />), and the base class maintains parallel <see cref="Indices" /> and <see cref="Conversions" />
/// caches that translate string keys to array positions and to wrapped item values. Lua scripts then treat the array
/// as if it were a Lua table keyed by item name -- string indexing, <see cref="Remove(string)" />, and iteration via
/// <see cref="Pairs" /> all operate on names rather than raw JSON object keys. Mutations (<see cref="Append" />,
/// <see cref="Insert" />, <see cref="Remove(int)" />, <see cref="Clear" />) update the caches in step.
/// String-indexed assignment is intentionally unsupported and always throws -- callers must use the explicit
/// add/remove methods so the name-to-index mapping stays consistent.
/// </remarks>
public abstract class IndexedListUserData : JsonUserData
{
    /// <summary>
    /// The wrapped JSON array. Same instance as the base <see cref="JsonUserData.Token" />, exposed as a typed
    /// <see cref="JArray" /> for subclass convenience.
    /// </summary>
    protected readonly JArray List;

    /// <summary>
    /// Maps item name to its position in <see cref="List" />. Maintained by the base class; subclasses should not mutate it directly.
    /// </summary>
    protected readonly Dictionary<string, int> Indices = new();

    /// <summary>
    /// Cached Lua-facing <see cref="DynValue" /> for each item in <see cref="List" />, parallel by position.
    /// Populated via <see cref="Convert" />.
    /// </summary>
    protected readonly List<DynValue> Conversions = new();

    /// <summary>
    /// Creates a new indexed-list wrapper around the given JSON array and populates the name-index and conversion caches.
    /// </summary>
    /// <param name="token">The JSON array to wrap.</param>
    protected IndexedListUserData(JArray token) : base(token)
    {
        List = token;
        HardRefresh();
    }

    /// <summary>
    /// Rebuilds both the name-index map and the cached Lua conversions from scratch.
    /// </summary>
    /// <remarks>
    /// Call after a structural change that invalidates cached <see cref="DynValue" /> wrappers, such as replacing
    /// an item's underlying token. <see cref="SoftRefresh" /> is sufficient when only positions changed.
    /// </remarks>
    protected void HardRefresh()
    {
        Indices.Clear();
        Conversions.Clear();
        var index = 0;
        foreach (var value in List)
        {
            Indices[Name(value)] = index++;
            Conversions.Add(Convert(value));
        }
    }

    /// <summary>
    /// Rebuilds the name-index map without rebuilding the cached Lua conversions.
    /// </summary>
    /// <remarks>
    /// Use after a change that shifts existing items' positions but leaves their underlying tokens (and therefore
    /// their wrapped <see cref="DynValue" />s) intact. Call <see cref="HardRefresh" /> instead when an item's
    /// token has been replaced.
    /// </remarks>
    public void SoftRefresh()
    {
        for (int i = 0; i < List.Count; i++)
        {
            Indices[Name(List[i])] = i;
        }
    }

    /// <summary>
    /// Returns the lookup name for the given item.
    /// </summary>
    /// <remarks>
    /// Implementations typically read a known property off <paramref name="source" /> (for example <c>"name"</c>
    /// or <c>"engineID"</c>). The returned string becomes the key Lua scripts use to address the item.
    /// </remarks>
    /// <param name="source">The item to extract the name from.</param>
    /// <returns>The lookup name for the item.</returns>
    public abstract string Name(JToken source);

    /// <summary>
    /// Wraps an item's JSON token in the Lua-facing value cached for that slot.
    /// </summary>
    /// <remarks>
    /// The default returns whatever <see cref="JsonUserData.GetFromJToken" /> produces. Subclasses may override
    /// to wrap each item in a specialized <see cref="JsonUserData" /> subtype.
    /// </remarks>
    /// <param name="source">The item's JSON token.</param>
    /// <returns>The cached Lua value for the item.</returns>
    [MoonSharpHidden]
    public virtual DynValue Convert(JToken source)
    {
        return GetFromJToken(source);
    }

    /// <inheritdoc />
    public override IEnumerable<string> Keys()
    {
        foreach (var key in Indices.Keys)
        {
            yield return key;
        }
    }

    /// <inheritdoc />
    public override DynValue Pairs()
    {
        var iterator = Indices.GetEnumerator();
        return DynValue.NewCallback((sec, args) =>
        {
            if (iterator.MoveNext())
            {
                return DynValue.NewTuple(DynValue.NewString(iterator.Current.Key),Conversions[iterator.Current.Value]);
            }
            return DynValue.Nil;
        });
    }

    /// <inheritdoc />
    public override DynValue this[string index]
    {
        get
        {
            if (Indices.TryGetValue(index, out var idx))
            {
                return Conversions[idx];
            }
            return DynValue.Nil;
        }
        set => throw new Exception("Indexed lists are read only, except when using the methods for them");
    }

    /// <inheritdoc />
    public override void Remove(string key)
    {
        if (!Indices.TryGetValue(key, out var idx)) throw new KeyNotFoundException();
        List.RemoveAt(idx);
        Indices.Remove(key);
        Conversions.RemoveAt(idx);
        SoftRefresh();
    }

    /// <inheritdoc />
    public override void Remove(int index)
    {
        if (index <= 0 || index >= Indices.Count) throw new IndexOutOfRangeException();
        Indices.Remove(Name(List[index-1]));
        List.RemoveAt(index-1);
        Conversions.RemoveAt(index-1);
        SoftRefresh();
    }


    /// <inheritdoc />
    public override void Clear()
    {
        Indices.Clear();
        Conversions.Clear();
        base.Clear();
    }

    /// <inheritdoc />
    public override void Append(DynValue value)
    {
        base.Append(value);
        Indices[Name(List[Count - 1])] = Count - 1;
        Conversions.Add(Convert(List[Count - 1]));
    }

    /// <inheritdoc />
    public override void Insert(int index, DynValue value)
    {
        base.Insert(index, value);
        Indices[Name(List[index])] = index;
        Conversions.Insert(index, Convert(List[index]));
        SoftRefresh();
    }

    // public override void Patch(Script script, string index, DynValue value)
    // {
    //     if (!Indices.TryGetValue(index, out var idx)) return;
    //     var oldName = Name(List[idx]);
    //     script.Call(value, Conversions[idx]);
    //     var newName = Name(List[idx]);
    //     if (oldName != newName)
    //     {
    //         HardRefresh();
    //     }
    // }
}
