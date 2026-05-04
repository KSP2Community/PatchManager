using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.Shared;

namespace PatchManager.LuaPatching;

[MoonSharpUserData]
public class JsonUserData
{
    [MoonSharpHidden] public JToken Token;

    public JsonUserData(JToken token)
    {
        Token = token;
    }

    public static JToken GetJTokenForDynValue(DynValue dv) => GetJTokenForDynValue(null, dv);

    protected static JToken GetJTokenForDynValue([CanBeNull] JToken previous, DynValue value)
    {
        switch (value.Type)
        {
            case DataType.Nil:
                return new JValue((object) null);
            case DataType.Boolean:
                return new JValue(value.Boolean);
            case DataType.Number:
                if (previous?.Type == JTokenType.Float)
                {
                    return new JValue(value.Number);
                }

                if (previous?.Type == JTokenType.Integer)
                {
                    return new JValue((long)value.Number);
                }

                var lVal = (long)value.Number;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (lVal == value.Number)
                {
                    return new JValue(lVal); 
                }
                return new JValue(value.Number);
            case DataType.String:
                return new JValue(value.String);
            case DataType.Table:
                var table = value.Table;
                var pairs = table.Pairs.ToArray();
                if (table.Length != 0)
                {
                    var array = new JArray();
                    foreach (var p in pairs)
                    {
                        array.Add(GetJTokenForDynValue(null, p.Value));
                    }
                    return array;
                }
                if (pairs.Length == 0 && previous?.Type == JTokenType.Array)
                {
                    return new JArray();
                }
                var obj = new JObject();
                foreach (var p in pairs)
                {
                    obj[p.Key.CastToString()]  = GetJTokenForDynValue(null, p.Value);
                }
                return obj;
            case DataType.Tuple:
                return GetJTokenForDynValue(previous, value.Tuple[0]);
            case DataType.UserData:
                if (value.UserData.Object is JsonUserData jsonUserData)
                {
                    return jsonUserData.Token;
                }

                throw new Exception($"Unexpected user data type {value.UserData.Object.GetType()}");
            default:
                throw new Exception($"Unexpected value type {value.Type}");
        }
    }
    
    /// <summary>
    /// 1 Indexed array index
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="Exception"></exception>
    public virtual DynValue this[int index]
    {
        get
        {
            if (Token.Type != JTokenType.Array)
            {
                Logging.LogDebug($"Read [{index}] on non-array JSON ({Token.Type}); returning nil");
                return DynValue.Nil;
            }

            var array = (JArray)Token;
            index -= 1;

            if (index < 0 || index >= array.Count) return DynValue.Nil;
            return GetFromJToken(array[index]);
        }
        set
        {
            if (Token.Type != JTokenType.Array)
            {
                throw new Exception("JSON Object is not an array");
            }
            var array = (JArray)Token;
            index -= 1;
            if (index < 0 || index > array.Count) throw new Exception("Invalid array index!");
            if (index == array.Count)
            {
                array.Add(GetJTokenForDynValue(null, value));
            }
            else
            {
                array[index] = GetJTokenForDynValue(array[index], value);
            }
        }
    }

    public virtual DynValue this[string index]
    {
        get
        {
            if (Token.Type != JTokenType.Object)
            {
                Logging.LogDebug($"Read .{index} on non-object JSON ({Token.Type}); returning nil");
                return DynValue.Nil;
            }
            var obj = (JObject)Token;
            return GetFromJToken(obj[index]);
        }
        set
        {
            if (Token.Type != JTokenType.Object)
            {
                throw new Exception("JSON Object is not an object");
            }
            var obj = (JObject)Token;
            obj[index] = GetJTokenForDynValue(obj[index], value);
        }
    }

    public virtual void Remove(int index)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new Exception("JSON Object is not an array");
        }

        var array = (JArray)Token;
        index -= 1;
        if (index < 0 || index >= array.Count) throw new Exception("Invalid array index!");
        array.RemoveAt(index);
    }

    public virtual void Remove(string key)
    {
        if (Token.Type != JTokenType.Object)
        {
            throw new Exception("JSON Object is not an object");
        }
        var obj = (JObject)Token;
        obj.Remove(key);
    }

    public virtual int Count
    {
        get
        {
            if (Token is JArray array) return array.Count;
            Logging.LogDebug($"Read .Count on non-array JSON ({Token.Type}); returning 0");
            return 0;
        }
    }

    [MoonSharpUserDataMetamethod("__pairs")]
    [MoonSharpUserDataMetamethod("__ipairs")]
    public virtual DynValue Pairs()
    {
        if (Token.Type == JTokenType.Array)
        {
            var index = 1;
            return DynValue.NewCallback((ctx, itArgs) =>
            {
                if (index <= Count)
                {
                    var res = DynValue.NewTuple(
                        DynValue.NewNumber(index + 1),
                        this[index]
                    );
                    index++;
                    return res;
                }

                return DynValue.Nil;
            });
        }

        if (Token.Type == JTokenType.Object)
        {
            var index = 0;
            string[] keys = Keys().ToArray();

            return DynValue.NewCallback((ctx, itArgs) =>
            {
                if (index < keys.Length)
                {
                    var res = DynValue.NewTuple(
                        DynValue.NewString(keys[index]),
                        this[keys[index]]
                    );
                    index++;
                    return res;
                }

                return DynValue.Nil;
            });
        }

        Logging.LogDebug($"Iterated non-object/non-array JSON ({Token.Type}); returning empty iterator");
        return DynValue.NewCallback((ctx, args) => DynValue.Nil);
    }

    /// <summary>
    /// Clear out an array
    /// </summary>
    public virtual void Clear()
    {
        if (Token.Type == JTokenType.Array)
        {
            ((JArray)Token).Clear();
        }
        else
        {
            throw new Exception("JSON Object is not an array");
        }
    }

    public virtual void Insert(int index, DynValue value)
    {
        if (Token.Type == JTokenType.Array)
        {
            ((JArray)Token).Insert(index - 1, GetJTokenForDynValue(value));
        }
        else
        {
            throw new Exception("JSON Object is not an array");
        }
    }

    public virtual void Append(DynValue value)
    {
        if (Token.Type == JTokenType.Array)
        {
            ((JArray)Token).Add(GetJTokenForDynValue(value));
        }
        else
        {
            throw new Exception("JSON Object is not an array");
        }
    }
    
    public virtual IEnumerable<string> Keys()
    {
        if (Token.Type != JTokenType.Object)
        {
            Logging.LogDebug($"Read .Keys() on non-object JSON ({Token.Type}); returning empty");
            yield break;
        }
        foreach (var value in (JObject)Token)
        {
            yield return value.Key;
        }
    }
    
    public virtual bool HasKey(string key) => Keys().Contains(key);

    public virtual void Patch(string key, Action<DynValue> callback)
    {
        if (HasKey(key))
        {
            callback(this[key]);
        }
    }
    
    /// <summary>
    /// Get a dynamic value from a JToken
    /// </summary>
    /// <param name="token">The token to get the dynamic value from</param>
    /// <returns></returns>
    [MoonSharpHidden]
    public static DynValue GetFromJToken(JToken token)
    {
        if (token.Type is JTokenType.Array or JTokenType.Object)
        {
            return UserData.Create(new JsonUserData(token));
        }

        return token.Type switch
        {
            JTokenType.None or JTokenType.Null or JTokenType.Undefined => DynValue.Nil,
            JTokenType.Integer => DynValue.NewNumber(token.Value<long>()),
            JTokenType.Float => DynValue.NewNumber(token.Value<double>()),
            JTokenType.String => DynValue.NewString(token.Value<string>()),
            JTokenType.Boolean => DynValue.NewBoolean(token.Value<bool>()),
            _ => throw new Exception($"Unexpected token type {token.Type}")
        };
    }
}