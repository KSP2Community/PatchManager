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
/// Reads against the wrong token type are non-fatal. They log a debug message and return
/// <see cref="DynValue.Nil" /> or an empty result. Writes against the wrong token type throw, since
/// silently accepting a write would lose data. Subclasses (<see cref="Utility.IndexedListUserData" />,
/// <see cref="Utility.ExtensibleJsonUserData" />) override these members to layer name-indexed lookup or
/// virtual properties on top of the underlying JSON.
/// </remarks>
[MoonSharpUserData]
public class JsonUserData
{
    /// <summary>
    /// The wrapped JSON token. Hidden from Lua. C# callers may read or replace it directly.
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
    /// Wraps a Lua value as a <see cref="JsonUserData" />: an existing wrapper is returned as-is, anything else
    /// is wrapped fresh from its JSON token.
    /// </summary>
    /// <param name="dv">The Lua value to wrap.</param>
    /// <returns>The value as a <see cref="JsonUserData" />.</returns>
    public static JsonUserData Wrap(DynValue dv) =>
        dv.UserData?.Object as JsonUserData ?? new JsonUserData(GetJTokenForDynValue(dv));

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

    #region C# leaf conversions

    // MoonSharp consults these only in explicit numeric/bool coercion, never for truthiness (userdata is always
    // truthy), so they cannot change how Lua reads wrappers.
    public static implicit operator double(JsonUserData u) => u.Token.Value<double>();
    public static implicit operator float(JsonUserData u) => u.Token.Value<float>();
    public static implicit operator long(JsonUserData u) => u.Token.Value<long>();
    public static implicit operator int(JsonUserData u) => u.Token.Value<int>();
    public static implicit operator bool(JsonUserData u) => u.Token.Value<bool>();
    public static implicit operator string(JsonUserData u) => u.Token.Value<string>();

    public static implicit operator JsonUserData(double v) => new(new JValue(v));
    public static implicit operator JsonUserData(float v) => new(new JValue(v));
    public static implicit operator JsonUserData(long v) => new(new JValue(v));
    public static implicit operator JsonUserData(int v) => new(new JValue(v));
    public static implicit operator JsonUserData(bool v) => new(new JValue(v));
    public static implicit operator JsonUserData(string v) => new(new JValue(v));

    #endregion

    #region Indexers (C# path) and LuaGet/LuaSet (Lua path)

    // C# index path. Lua never reaches these - IndexerFallbackDescriptor forces member-only resolution on
    // JsonUserData and routes its index access to LuaGet/LuaSet.
    [MoonSharpHidden]
    public JsonUserData this[string key]
    {
        get => GetChild(DynValue.NewString(key));
        set => LuaSet(DynValue.NewString(key), GetFromJToken(value?.Token));
    }

    // 0-based for C#. Lua's 1-based access goes through LuaGet.
    [MoonSharpHidden]
    public JsonUserData this[LuaIndex index]
    {
        get => GetChild(DynValue.NewNumber(index.Value + 1));
        set => LuaSet(DynValue.NewNumber(index.Value + 1), GetFromJToken(value?.Token));
    }

    /// <summary>
    /// Lua read path. Returns a native <see cref="DynValue" /> for leaf tokens, with no wrapper allocation, and
    /// wraps only objects and arrays. Reached through <c>IndexerFallbackDescriptor</c>, not member dispatch.
    /// </summary>
    /// <param name="key">The Lua index, a string property name or a 1-based array position.</param>
    /// <returns>The value for the key, or <see cref="DynValue.Nil" /> when absent.</returns>
    [MoonSharpHidden]
    public DynValue LuaGet(DynValue key)
    {
        var special = TryGetVirtual(key);
        if (special != null) return special;
        var token = RawChildToken(key);
        return token == null ? DynValue.Nil : GetFromJToken(token);
    }

    /// <summary>
    /// Lua write path. Honors the subclass set-intercept first, then writes the raw JSON.
    /// </summary>
    /// <param name="key">The Lua index, a string property name or a 1-based array position.</param>
    /// <param name="value">The value to assign.</param>
    [MoonSharpHidden]
    public void LuaSet(DynValue key, DynValue value)
    {
        if (TrySetVirtual(key, value)) return;
        RawSet(key, value);
    }

    // Every value wrapper is a JsonUserData, so an intercepted value is returned as itself and a leaf token is wrapped.
    private JsonUserData GetChild(DynValue key)
    {
        if (TryGetVirtual(key) is { } intercept) return intercept.UserData?.Object as JsonUserData;
        var token = RawChildToken(key);
        return token == null ? null : new JsonUserData(token);
    }

    #endregion

    #region Resolution seams

    /// <summary>
    /// Subclass read intercept: returns a value for keys the subclass synthesizes (name-indexed list elements,
    /// virtual or computed properties), or <c>null</c> to fall through to the raw JSON. The base layers nothing.
    /// </summary>
    /// <param name="key">The index being read.</param>
    /// <returns>The intercepted value, or <c>null</c> when not subclass-handled.</returns>
    [MoonSharpHidden]
    protected virtual DynValue TryGetVirtual(DynValue key) => null;

    /// <summary>
    /// Subclass write intercept: returns true when the subclass consumed the assignment, false to fall through to
    /// the raw JSON write. The base layers nothing.
    /// </summary>
    /// <param name="key">The index being written.</param>
    /// <param name="value">The value being assigned.</param>
    /// <returns>True when the subclass handled the write.</returns>
    [MoonSharpHidden]
    protected virtual bool TrySetVirtual(DynValue key, DynValue value) => false;

    // Returns null on a miss, an out-of-range index, or a type mismatch.
    private JToken RawChildToken(DynValue key)
    {
        if (key.Type == DataType.Number)
        {
            if (Token.Type != JTokenType.Array)
            {
                Logging.LogDebug($"Read [{key.Number}] on non-array JSON ({Token.Type}); returning nil");
                return null;
            }

            var array = (JArray)Token;
            var index = (int)key.Number - 1;
            if (index < 0 || index >= array.Count) return null;
            return array[index];
        }

        if (key.Type == DataType.String)
        {
            if (Token.Type != JTokenType.Object)
            {
                Logging.LogDebug($"Read .{key.String} on non-object JSON ({Token.Type}); returning nil");
                return null;
            }

            return ((JObject)Token)[key.String];
        }

        return null;
    }

    // Keeps the original array-append and type-coercion rules.
    private void RawSet(DynValue key, DynValue value)
    {
        if (key.Type == DataType.Number)
        {
            if (Token.Type != JTokenType.Array)
                throw new ScriptRuntimeException($"Cannot write [{key.Number}] on non-array JSON ({Token.Type}).");

            var array = (JArray)Token;
            var oneBased = (int)key.Number;
            var index = oneBased - 1;
            if (index < 0 || index > array.Count)
                throw new ScriptRuntimeException($"Array index [{oneBased}] out of range; array has {array.Count} element(s).");

            if (index == array.Count)
                array.Add(GetJTokenForDynValue(null, value));
            else
                array[index] = GetJTokenForDynValue(array[index], value);
            return;
        }

        if (key.Type == DataType.String)
        {
            if (Token.Type != JTokenType.Object)
                throw new ScriptRuntimeException($"Cannot write .{key.String} on non-object JSON ({Token.Type}).");

            var obj = (JObject)Token;
            obj[key.String] = GetJTokenForDynValue(obj[key.String], value);
            return;
        }

        throw new ScriptRuntimeException($"Cannot index JSON with a {key.Type} key.");
    }

    #endregion

    /// <summary>
    /// Removes the array element at the given position (0-based from C#, 1-based from Lua via the LuaIndex converter).
    /// </summary>
    /// <param name="index">The array position to remove.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when the token is not <see cref="JTokenType.Array" /> or when the index is out of range.</exception>
    public virtual void RemoveAt(LuaIndex index)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new ScriptRuntimeException($"Cannot remove [{index.Value + 1}] from non-array JSON ({Token.Type}).");
        }

        var array = (JArray)Token;
        if (index.Value < 0 || index.Value >= array.Count)
        {
            throw new ScriptRuntimeException($"Array index [{index.Value + 1}] out of range for remove; array has {array.Count} element(s).");
        }
        array.RemoveAt(index.Value);
    }

    /// <summary>
    /// Removes the object property with the given key.
    /// </summary>
    /// <param name="key">The property name to remove.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when the token is not <see cref="JTokenType.Object" />.</exception>
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
    /// Arrays yield 1-indexed (index, value) tuples. Objects yield (key, value) tuples. Any other token type
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
                        LuaGet(DynValue.NewNumber(index))
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
                        LuaGet(DynValue.NewString(keys[index]))
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
    /// <exception cref="ScriptRuntimeException">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
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
    /// Inserts an element at the given position (0-based from C#, 1-based from Lua), shifting later elements right.
    /// </summary>
    /// <param name="index">The position to insert at.</param>
    /// <param name="value">The element to insert.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
    public virtual void Insert(LuaIndex index, DynValue value)
    {
        if (Token.Type != JTokenType.Array)
        {
            throw new ScriptRuntimeException($"Cannot insert at [{index.Value + 1}] on non-array JSON ({Token.Type}).");
        }
        var array = (JArray)Token;
        if (index.Value < 0 || index.Value > array.Count)
        {
            throw new ScriptRuntimeException($"Insert index [{index.Value + 1}] out of range; array has {array.Count} element(s) (valid range: 1..{array.Count + 1}).");
        }
        try
        {
            array.Insert(index.Value, GetJTokenForDynValue(value));
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to insert at [{index.Value + 1}]: {e.Message}");
        }
    }

    /// <summary>
    /// C# overload that inserts a wrapped or implicitly-converted value.
    /// </summary>
    [MoonSharpHidden]
    public void Insert(LuaIndex index, JsonUserData value) => Insert(index, GetFromJToken(value?.Token));

    /// <summary>
    /// Appends an element to the end of the wrapped array.
    /// </summary>
    /// <param name="value">The element to append.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when the token is not <see cref="JTokenType.Array" />.</exception>
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
    /// C# overload that appends a wrapped or implicitly-converted value.
    /// </summary>
    [MoonSharpHidden]
    public void Append(JsonUserData value) => Append(GetFromJToken(value?.Token));

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
    /// Invokes <paramref name="callback" /> with the value at <paramref name="key" /> when the key is present.
    /// Does nothing otherwise.
    /// </summary>
    /// <param name="key">The property name to patch.</param>
    /// <param name="callback">The callback to invoke with the existing value.</param>
    public virtual void Patch(string key, Action<DynValue> callback)
    {
        if (HasKey(key))
        {
            callback(LuaGet(DynValue.NewString(key)));
        }
    }

    /// <summary>
    /// Removes every array element that matches the given predicate.
    /// </summary>
    /// <param name="callback">The predicate to test each element against.</param>
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
    /// A null token (missing JSON key) maps to <see cref="DynValue.Nil" />. Objects and arrays are wrapped in
    /// a fresh <see cref="JsonUserData" />. Primitives become the matching <see cref="DynValue" /> kind, and
    /// <see cref="JTokenType.None" />, <see cref="JTokenType.Null" />, and <see cref="JTokenType.Undefined" />
    /// all map to <see cref="DynValue.Nil" />.
    /// </remarks>
    /// <param name="token">The token to lift, or <c>null</c> for a missing key.</param>
    /// <returns>The Lua value for the given token.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="token" />'s type is not one of the handled token types.</exception>
    [MoonSharpHidden]
    public static DynValue GetFromJToken(JToken token)
    {
        if (token == null) return DynValue.Nil;

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
