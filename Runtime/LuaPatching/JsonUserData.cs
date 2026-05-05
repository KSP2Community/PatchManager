using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.Shared;

namespace PatchManager.LuaPatching;

/// <summary>
/// Lua-facing wrapper exposing a Newtonsoft <see cref="JToken" /> as a table-like UserData.
/// </summary>
/// <remarks>
/// Patch scripts read and mutate JSON assets through this type: object keys become string indices,
/// array elements become 1-indexed integer indices (matching Lua convention), and iteration is exposed
/// through the <c>__pairs</c> / <c>__ipairs</c> metamethods on <see cref="Pairs" />.
/// Reads against the wrong token type are non-fatal -- they log a debug message and return
/// <see cref="DynValue.Nil" /> or an empty result. Writes against the wrong token type throw, since
/// silently accepting a write would lose data. Subclasses (<see cref="Utility.IndexedListUserData" />,
/// <see cref="Utility.ExtensibleJsonUserData" />) override these members to layer name-indexed lookup or
/// virtual properties on top of the underlying JSON.
/// </remarks>
[MoonSharpUserData]
public class JsonUserData
{
    /// <summary>
    /// The wrapped JSON token. Hidden from Lua; C# callers may read or replace it directly.
    /// </summary>
    [MoonSharpHidden] public JToken Token;

    /// <summary>
    /// Creates a new wrapper around the given JSON token.
    /// </summary>
    /// <param name="token">The token to wrap.</param>
    public JsonUserData(JToken token)
    {
        Token = token;
    }

    /// <summary>
    /// Converts a Lua <see cref="DynValue" /> into the equivalent <see cref="JToken" />.
    /// </summary>
    /// <remarks>
    /// Lua numbers become integers when they have no fractional part. Lua tables become JSON arrays when
    /// their integer length is non-zero, JSON objects otherwise. Nested <see cref="JsonUserData" /> values
    /// are unwrapped to their underlying token. Use the protected overload to disambiguate against an existing
    /// slot's type.
    /// </remarks>
    /// <param name="dv">The Lua value to convert.</param>
    /// <returns>The JSON representation of <paramref name="dv" />.</returns>
    public static JToken GetJTokenForDynValue(DynValue dv) => GetJTokenForDynValue(null, dv);

    /// <summary>
    /// Converts a Lua <see cref="DynValue" /> into a <see cref="JToken" />, using <paramref name="previous" />'s type
    /// as a hint for ambiguous numeric and empty-table cases.
    /// </summary>
    /// <remarks>
    /// Used by indexer setters so that assigning a Lua number to an existing integer slot keeps it integer,
    /// and so that an empty Lua table replacing a JSON array does not collapse into an empty object.
    /// </remarks>
    /// <param name="previous">The token currently at the target slot, or <c>null</c> when none.</param>
    /// <param name="value">The Lua value to convert.</param>
    /// <returns>The JSON representation of <paramref name="value" />.</returns>
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

                throw new ScriptRuntimeException($"Unexpected user data type {value.UserData.Object.GetType()}");
            default:
                throw new ScriptRuntimeException($"Unexpected value type {value.Type}");
        }
    }

    /// <summary>
    /// Casts <paramref name="token" /> to <see cref="JArray" />, throwing a Lua-decorated error when the cast fails.
    /// </summary>
    /// <param name="token">The token to cast.</param>
    /// <param name="what">A label describing what the token represents, used in the error message.</param>
    /// <returns>The token as a <see cref="JArray" />.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="token" /> is missing or not a JSON array.</exception>
    public static JArray RequireArray(JToken token, string what)
    {
        if (token is not JArray arr)
        {
            throw new ScriptRuntimeException(
                $"Expected {what} to be a JSON array, got {(token == null ? "missing" : token.Type.ToString())}.");
        }
        return arr;
    }

    /// <summary>
    /// Casts <paramref name="token" /> to <see cref="JObject" />, throwing a Lua-decorated error when the cast fails.
    /// </summary>
    /// <param name="token">The token to cast.</param>
    /// <param name="what">A label describing what the token represents, used in the error message.</param>
    /// <returns>The token as a <see cref="JObject" />.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="token" /> is missing or not a JSON object.</exception>
    public static JObject RequireObject(JToken token, string what)
    {
        if (token is not JObject obj)
        {
            throw new ScriptRuntimeException(
                $"Expected {what} to be a JSON object, got {(token == null ? "missing" : token.Type.ToString())}.");
        }
        return obj;
    }

    /// <summary>
    /// Reads <paramref name="token" /> as a JSON string, throwing a Lua-decorated error when it is missing or non-string.
    /// </summary>
    /// <param name="token">The token to read.</param>
    /// <param name="what">A label describing what the token represents, used in the error message.</param>
    /// <returns>The token's string value.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="token" /> is missing or not a JSON string.</exception>
    public static string RequireString(JToken token, string what)
    {
        if (token is not JValue { Type: JTokenType.String } v || v.Value<string>() is not { } s)
        {
            throw new ScriptRuntimeException(
                $"Expected {what} to be a JSON string, got {(token == null ? "missing" : token.Type.ToString())}.");
        }
        return s;
    }

    /// <summary>
    /// Gets or sets the array element at the given 1-indexed position.
    /// </summary>
    /// <remarks>
    /// Reads on a non-array token log a debug message and return <see cref="DynValue.Nil" />; out-of-range
    /// reads also return <see cref="DynValue.Nil" />. Writes throw when the token is not an array; assigning
    /// at <c>Count + 1</c> appends.
    /// </remarks>
    /// <param name="index">The 1-indexed array position.</param>
    /// <returns>The element at the given position, or <see cref="DynValue.Nil" /> if absent or out of range.</returns>
    /// <exception cref="Exception">Thrown when setting on a token whose type is not <see cref="JTokenType.Array" />, or when the index is out of range.</exception>
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
                throw new ScriptRuntimeException($"Cannot write [{index}] on non-array JSON ({Token.Type}).");
            }
            var array = (JArray)Token;
            var oneBased = index;
            index -= 1;
            if (index < 0 || index > array.Count)
            {
                throw new ScriptRuntimeException($"Array index [{oneBased}] out of range; array has {array.Count} element(s).");
            }
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

    /// <summary>
    /// Gets or sets the object property with the given key.
    /// </summary>
    /// <remarks>
    /// Reads on a non-object token log a debug message and return <see cref="DynValue.Nil" />; reads of missing
    /// keys also return <see cref="DynValue.Nil" />. Writes throw when the token is not an object.
    /// </remarks>
    /// <param name="index">The property name.</param>
    /// <returns>The property value, or <see cref="DynValue.Nil" /> if absent.</returns>
    /// <exception cref="Exception">Thrown when setting on a token whose type is not <see cref="JTokenType.Object" />.</exception>
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
                throw new ScriptRuntimeException($"Cannot write .{index} on non-object JSON ({Token.Type}).");
            }
            var obj = (JObject)Token;
            obj[index] = GetJTokenForDynValue(obj[index], value);
        }
    }

    /// <summary>
    /// Removes the array element at the given 1-indexed position.
    /// </summary>
    /// <param name="index">The 1-indexed array position to remove.</param>
    /// <exception cref="Exception">Thrown when the token is not <see cref="JTokenType.Array" /> or when the index is out of range.</exception>
    public virtual void RemoveAt(int index)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new ScriptRuntimeException($"Cannot remove [{index}] from non-array JSON ({Token.Type}).");
        }

        var array = (JArray)Token;
        var oneBased = index;
        index -= 1;
        if (index < 0 || index >= array.Count)
        {
            throw new ScriptRuntimeException($"Array index [{oneBased}] out of range for remove; array has {array.Count} element(s).");
        }
        array.RemoveAt(index);
    }

    /// <summary>
    /// Removes the object property with the given key.
    /// </summary>
    /// <param name="key">The property name to remove.</param>
    /// <exception cref="Exception">Thrown when the token is not <see cref="JTokenType.Object" />.</exception>
    public virtual void Remove(string key)
    {
        if (Token.Type != JTokenType.Object)
        {
            throw new ScriptRuntimeException($"Cannot remove .{key} from non-object JSON ({Token.Type}).");
        }
        var obj = (JObject)Token;
        obj.Remove(key);
    }

    /// <summary>
    /// Gets the number of elements in the wrapped array, or 0 when the token is not an array (also logs a debug message).
    /// </summary>
    public virtual int Count
    {
        get
        {
            if (Token is JArray array) return array.Count;
            Logging.LogDebug($"Read .Count on non-array JSON ({Token.Type}); returning 0");
            return 0;
        }
    }

    /// <summary>
    /// Returns an iterator suitable for Lua's <c>__pairs</c> / <c>__ipairs</c> metamethods.
    /// </summary>
    /// <remarks>
    /// Arrays yield 1-indexed (index, value) tuples; objects yield (key, value) tuples; any other token type
    /// yields nothing and logs a debug message. Bound to both metamethods, so <c>pairs</c> and <c>ipairs</c>
    /// in Lua produce the same iterator.
    /// </remarks>
    /// <returns>The iterator callback.</returns>
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
                        DynValue.NewNumber(index),
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
    /// Removes every element from the wrapped array.
    /// </summary>
    /// <exception cref="Exception">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
    public virtual void Clear()
    {
        if (Token.Type == JTokenType.Array)
        {
            ((JArray)Token).Clear();
        }
        else
        {
            throw new ScriptRuntimeException($"Cannot clear non-array JSON ({Token.Type}).");
        }
    }

    /// <summary>
    /// Inserts an element at the given 1-indexed position, shifting later elements right.
    /// </summary>
    /// <param name="index">The 1-indexed position to insert at.</param>
    /// <param name="value">The element to insert.</param>
    /// <exception cref="Exception">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
    public virtual void Insert(int index, DynValue value)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new ScriptRuntimeException($"Cannot insert at [{index}] on non-array JSON ({Token.Type}).");
        }
        var array = (JArray)Token;
        if (index < 1 || index > array.Count + 1)
        {
            throw new ScriptRuntimeException($"Insert index [{index}] out of range; array has {array.Count} element(s) (valid range: 1..{array.Count + 1}).");
        }
        try
        {
            array.Insert(index - 1, GetJTokenForDynValue(value));
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to insert at [{index}]: {e.Message}");
        }
    }

    /// <summary>
    /// Appends an element to the end of the wrapped array.
    /// </summary>
    /// <param name="value">The element to append.</param>
    /// <exception cref="Exception">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
    public virtual void Append(DynValue value)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new ScriptRuntimeException($"Cannot append to non-array JSON ({Token.Type}).");
        }
        try
        {
            ((JArray)Token).Add(GetJTokenForDynValue(value));
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to append to array: {e.Message}");
        }
    }

    /// <summary>
    /// Enumerates the property names of the wrapped object.
    /// </summary>
    /// <remarks>
    /// Returns an empty sequence (and logs a debug message) when the token is not an object.
    /// </remarks>
    /// <returns>The property names of the wrapped object.</returns>
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

    /// <summary>
    /// Returns whether the wrapped object contains a property with the given key.
    /// </summary>
    /// <param name="key">The property name to test.</param>
    /// <returns>True if the wrapped object contains <paramref name="key" />, false otherwise.</returns>
    public virtual bool HasKey(string key) => Keys().Contains(key);

    /// <summary>
    /// Invokes <paramref name="callback" /> with the value at <paramref name="key" /> when the key is present;
    /// does nothing otherwise.
    /// </summary>
    /// <param name="key">The property name to patch.</param>
    /// <param name="callback">The callback to invoke with the existing value.</param>
    public virtual void Patch(string key, Action<DynValue> callback)
    {
        if (HasKey(key))
        {
            callback(this[key]);
        }
    }

    /// <summary>
    /// Remove all items that match a passed predicate
    /// </summary>
    /// <param name="callback">The predicate to check against</param>
    public virtual void RemoveWhere(Func<DynValue,bool> callback)
    {
        var array = RequireArray(Token, "self");
        for (var i = array.Count - 1; i >= 0; i--)
        {
            if (callback(GetFromJToken(array[i])))
            {
                array.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Lifts a JSON token into the corresponding Lua-facing <see cref="DynValue" />.
    /// </summary>
    /// <remarks>
    /// Objects and arrays are wrapped in a fresh <see cref="JsonUserData" />; primitives become the matching
    /// <see cref="DynValue" /> kind; <see cref="JTokenType.None" />, <see cref="JTokenType.Null" />, and
    /// <see cref="JTokenType.Undefined" /> all map to <see cref="DynValue.Nil" />.
    /// </remarks>
    /// <param name="token">The token to lift.</param>
    /// <returns>The Lua value for the given token.</returns>
    /// <exception cref="Exception">Thrown when <paramref name="token" />'s type is not one of the handled token types.</exception>
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
            _ => throw new ScriptRuntimeException($"Unexpected token type {token.Type}")
        };
    }
}
