using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching.Utility;

public abstract class IndexedListUserData : JsonUserData
{
    protected readonly JArray List;
    protected readonly Dictionary<string, int> Indices = new();
    protected readonly List<DynValue> Conversions = new();
    
    protected IndexedListUserData(JArray token) : base(token)
    {
        List = token;
        HardRefresh();
    }

    /// <summary>
    /// Refresh the list by clearing and rewriting it
    /// </summary>
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
    public void SoftRefresh()
    {
        for (int i = 0; i < List.Count; i++)
        {
            Indices[Name(List[i])] = i;
        }
    }

    /// <summary>
    /// Get the name of an object in the list
    /// </summary>
    /// <param name="source">The source</param>
    /// <returns>The name</returns>
    public abstract string Name(JToken source);
    
    /// <summary>
    /// Converts a token to an object (used to keep all the caching state)
    /// Runs in HardRefresh()
    /// </summary>
    /// <param name="source">The source</param>
    /// <returns>The conversion</returns>
    [MoonSharpHidden]
    public virtual DynValue Convert(JToken source)
    {
        return GetFromJToken(source);
    }

    public override IEnumerable<string> Keys()
    {
        foreach (var key in Indices.Keys)
        {
            yield return key;
        }
    }

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

    public override void Remove(string key)
    {
        if (!Indices.TryGetValue(key, out var idx)) throw new KeyNotFoundException();
        List.RemoveAt(idx);
        Indices.Remove(key);
        Conversions.RemoveAt(idx);
        SoftRefresh();
    }

    public override void Remove(int index)
    {
        if (index <= 0 || index >= Indices.Count) throw new IndexOutOfRangeException();
        Indices.Remove(Name(List[index-1]));
        List.RemoveAt(index-1);
        Conversions.RemoveAt(index-1);
        SoftRefresh();
    }


    public override void Clear()
    {
        Indices.Clear();
        Conversions.Clear();
        base.Clear();
    }

    public override void Append(DynValue value)
    {
        base.Append(value);
        Indices[Name(List[Count - 1])] = Count - 1;
        Conversions.Add(Convert(List[Count - 1]));
    }

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