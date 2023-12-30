using System.Collections;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;

namespace PatchManager.SassyPatching;

/// <summary>
/// The basic value type that the sassy patching engine uses
/// It can be one of 8 different types of values, 6 of which correspond fully to JSON types w/ one extra type/value meant to "delete" whatever it is assigned to, and then closures
/// </summary>
public class DataValue
{
    /// <summary>
    /// The types of values that a Value can be
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// The type of null, corresponds to JSON null values
        /// </summary>
        None,
        /// <summary>
        /// The type of true/false, corresponds to JSON true/false values
        /// </summary>
        Boolean,
        /// <summary>
        /// The type of real numbers, stored as a double, corresponds to JSON floats
        /// </summary>
        Real,
        /// <summary>
        /// The type of integers, stored as a long, corresponds to JSON integers
        /// </summary>
        Integer,
        /// <summary>
        /// The type of strings, corresponds to JSON strings
        /// </summary>
        String,
        /// <summary>
        /// The type of lists, corresponds to JSON lists
        /// </summary>
        List,
        /// <summary>
        /// The type of dictionaries, corresponds to JSON objects
        /// </summary>
        Dictionary,
        /// <summary>
        /// The type of a value that when assigned to a variable/field deletes that variable/field
        /// </summary>
        Deletion,

        /// <summary>
        /// The type of a closure, a function in a value
        /// </summary>
        Closure,
    }

    /// <summary>
    /// The type of this Value
    /// </summary>
    public readonly DataType Type;

    private readonly object _object;

    /// <summary>
    /// Creates a new value w/ a specified type and stored value
    /// </summary>
    /// <param name="type">The type of this value</param>
    /// <param name="o">The value that is stored in this value, can be null for values of type <see cref="DataType.None"/> and <see cref="DataType.Deletion"/></param>
    public DataValue(DataType type, object o = null)
    {
        Type = type;
        _object = o;
    }

    private void CheckType(DataType toCheck)
    {
        if (Type != toCheck)
        {
            throw new IncorrectTypeException($"Attempting to read Value of type {Type} as a value of type {toCheck}");
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.None"/>?
    /// </summary>
    public bool IsNone => Type == DataType.None;

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Boolean"/>?
    /// </summary>
    public bool IsBoolean => Type == DataType.Boolean;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.Boolean"/>,
    /// then returns the <see cref="bool"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.Boolean"/></exception>
    public bool Boolean
    {
        get
        {
            CheckType(DataType.Boolean);
            return (bool)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Real"/>?
    /// </summary>
    public bool IsReal => Type == DataType.Real;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.Real"/>,
    /// then returns the <see cref="double"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.Real"/></exception>
    public double Real
    {
        get
        {
            CheckType(DataType.Real);
            return (double)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Integer"/>?
    /// </summary>
    public bool IsInteger => Type == DataType.Integer;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.Integer"/>,
    /// then returns the <see cref="long"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.Integer"/></exception>
    public long Integer
    {
        get
        {
            CheckType(DataType.Integer);
            return (long)_object;
        }
    }


    /// <summary>
    /// Is the type of this variable <see cref="DataType.String"/>?
    /// </summary>
    public bool IsString => Type == DataType.String;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.String"/>,
    /// then returns the <see cref="string"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.String"/></exception>
    public string String
    {
        get
        {
            CheckType(DataType.String);
            return (string)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.List"/>?
    /// </summary>
    public bool IsList => Type == DataType.List;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.List"/>,
    /// then returns the <see cref="List{T}"/> contained within of which generic type argument is <see cref="DataValue"/>
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.List"/></exception>
    public List<DataValue> List
    {
        get
        {
            CheckType(DataType.List);
            return (List<DataValue>)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Dictionary"/>?
    /// </summary>
    public bool IsDictionary => Type == DataType.Dictionary;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.Dictionary"/>,
    /// then returns the <see cref="Dictionary{K,V}"/> contained within of which the key type is <see cref="string"/> and the value type is <see cref="DataValue"/>
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.Dictionary"/></exception>
    public Dictionary<string, DataValue> Dictionary
    {
        get
        {
            CheckType(DataType.Dictionary);
            return (Dictionary<string, DataValue>)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Deletion"/>?
    /// </summary>
    public bool IsDeletion => Type == DataType.Deletion;

    /// <summary>
    /// Is the type of this variable <see cref="DataType.Closure"/>?
    /// </summary>
    public bool IsClosure => Type == DataType.Closure;

    /// <summary>
    /// Asserts this value is of type <see cref="DataType.Closure"/>,
    /// then returns the <see cref="PatchFunction"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="DataType.Closure"/></exception>
    public PatchFunction Closure
    {
        get
        {
            CheckType(DataType.Closure);
            return (PatchFunction)_object;
        }
    }


    /// <summary>
    /// Does this value get interpreted as true in places where a <see cref="DataType.Boolean"/> is expected?
    /// That is, is the value a <see cref="DataType.Boolean"/> and the value stored within true, or is the value anything but <see cref="DataType.None"/> and <see cref="DataType.Deletion"/>?
    /// </summary>
    public bool Truthy {
        get
        {
            if (IsBoolean) return Boolean;
            return !(IsNone || IsDeletion);
        }
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="Boolean"/>
    /// </summary>
    /// <param name="b">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.Boolean"/> and a stored value of <param name="b"></param></returns>
    public static implicit operator DataValue(bool b)
    {
        return new DataValue(DataType.Boolean, b);
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="Double"/>
    /// </summary>
    /// <param name="d">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.Real"/> and a stored value of <param name="d"></param></returns>
    public static implicit operator DataValue(double d)
    {
        return new DataValue(DataType.Real, d);
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="long"/>
    /// </summary>
    /// <param name="l">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.Integer"/> and a stored value of <param name="l"></param></returns>
    public static implicit operator DataValue(long l)
    {
        return new DataValue(DataType.Integer, l);
    }


    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="string"/>
    /// </summary>
    /// <param name="s">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.String"/> and a stored value of <param name="s"></param></returns>
    public static implicit operator DataValue(string s)
    {
        return new DataValue(DataType.String, s);
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="List{T}"/> with the generic type argument being <see cref="DataValue"/>
    /// </summary>
    /// <param name="l">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.List"/> and a stored value of <param name="l"></param></returns>
    public static implicit operator DataValue(List<DataValue> l)
    {
        return new DataValue(DataType.List, l);
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="Dictionary{K,V}"/> with the key type being <see cref="string"/> and value type being <see cref="DataValue"/>
    /// </summary>
    /// <param name="d">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.Dictionary"/> and a stored value of <param name="d"></param></returns>
    public static implicit operator DataValue(Dictionary<string, DataValue> d)
    {
        return new DataValue(DataType.Dictionary, d);
    }

    /// <summary>
    /// Creates a <see cref="DataValue"/> from a <see cref="PatchFunction"/>
    /// </summary>
    /// <param name="c">The value to be stored within the value</param>
    /// <returns>A <see cref="DataValue"/> with a type of <see cref="DataType.Closure"/> and a stored value of <param name="c"></param></returns>
    public static implicit operator DataValue(PatchFunction c)
    {
        return new DataValue(DataType.Closure, c);
    }

    /// <summary>
    /// Converts a value to a string representation that can be interpreted by the engine as the exact value
    /// </summary>
    /// <returns>The string representation of the value</returns>
    public override string ToString()
    {
        if (IsNone)
        {
            return "null";
        }

        if (IsBoolean)
        {
            return Boolean.ToString();
        }

        if (IsReal)
        {
            return Real.ToString(CultureInfo.InvariantCulture);
        }

        if (IsString)
        {
            // return "'" + Regex.Escape(String) + "'";
            return String.Escape();
        }

        if (IsList)
        {
            return "[" + string.Join(",",List.Select(x => x.ToString())) + "]";
        }

        if (IsDictionary)
        {
            return "{" + string.Join(",",Dictionary.Select(x => x.Key.Escape() + $":{x.Value}")) + "}";
        }

        if (IsDeletion)
        {
            return "@delete";
        }

        if (IsClosure)
        {
            return "<closure>";
        }

        return "<unknown>";
    }

    public T To<T>() => (T)To(typeof(T));

    public object To(Type t)
    {
        if (t == typeof(DataValue))
        {
            return this;
        }

        if (t == typeof(string))
        {
            return String;
        }

        if (t == typeof(Dictionary<string, DataValue>))
        {
            return Dictionary;
        }

        if (t == typeof(List<DataValue>))
        {
            return List;
        }

        if (t == typeof(DataValue[]))
        {
            return List.ToArray();
        }

        if (t == typeof(byte))
        {
            return (byte)Integer;
        }

        if (t == typeof(sbyte))
        {
            return (sbyte)Integer;
        }

        if (t == typeof(short))
        {
            return (short)Integer;
        }

        if (t == typeof(ushort))
        {
            return (ushort)Integer;
        }

        if (t == typeof(int))
        {
            return (int)Integer;
        }

        if (t == typeof(uint))
        {
            return (uint)Integer;
        }

        if (t == typeof(long))
        {
            // ReSharper disable once RedundantCast
            return (long)Integer;
        }

        if (t == typeof(ulong))
        {
            return (ulong)Integer;
        }

        if (t == typeof(float))
        {
            return (float)Real;
        }

        if (t == typeof(double))
        {
            return Real;
        }

        if (t == typeof(bool))
        {
            return Boolean;
        }

        if (t == typeof(PatchFunction))
        {
            return Closure;
        }

        if (Type == DataType.None)
        {
            return null;
        }

        if (ConvertValueToSingleRankArray(t, out var singleRankArray)) return singleRankArray;

        if (ConvertValueToMultiRankArray(t, out var multiRankArray)) return multiRankArray;

        if (ConvertValueToList(t, out var list)) return list;

        if (ConvertValueToDictionary(t, out var convertParameterValue)) return convertParameterValue;

        var obj = Activator.CreateInstance(t);
        var dictionary = Dictionary;
        foreach (var field in t.GetFields())
        {
            field.SetValue(obj, dictionary[field.Name].To(field.FieldType));
        }

        return obj;
    }

    private bool ConvertValueToDictionary(Type type, out object dictionary)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            if (keyType != typeof(string))
            {
                throw new TypeConversionException("Value", type.Name);
            }

            var id = (IDictionary)Activator.CreateInstance(type);
            var dict = Dictionary;
            foreach (var kv in dict)
            {
                id[kv.Key] = kv.Value.To(valueType);
            }

            {
                dictionary = id;
                return true;
            }
        }

        dictionary = null;
        return false;
    }

    private bool ConvertValueToList(Type type, out object list)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var valueType = type.GetGenericArguments()[0];
            var il = (IList)Activator.CreateInstance(type);
            var tab = List;
            foreach (var x in tab)
            {
                il.Add(x.To(valueType));
            }

            {
                list = il;
                return true;
            }
        }

        list = null;
        return false;
    }

    private bool ConvertValueToMultiRankArray(Type type, out object multiRankArray)
    {
        if (type.IsArray)
        {
            var tab = List;
            var rank = type.GetArrayRank();
            var dimensions = new int[rank];
            var dimCounter = tab;
            for (var i = 0; i < rank; i++)
            {
                dimensions[i] = dimCounter.Count;
                if (i != rank - 1)
                {
                    dimCounter = dimCounter[0].List;
                }
            }

            var arr = Array.CreateInstance(type.GetElementType()!, dimensions);

            void BuildArray(Array array, params int[] idx)
            {
                var idxCopy = new int[idx.Length + 1];
                idx.CopyTo(idxCopy, 0);
                if (idx.Length == rank - 1)
                {
                    for (var i = 0; i < array.GetLength(idx.Length); i++)
                    {
                        idxCopy[idx.Length] = i;
                        array.SetValue(tab[i].To(type.GetElementType()), idxCopy);
                    }
                }
                else
                {
                    for (var i = 0; i < array.GetLength(idx.Length); i++)
                    {
                        idxCopy[idx.Length] = i;
                        BuildArray(array, idxCopy);
                    }
                }
            }

            BuildArray(arr);
            {
                multiRankArray = arr;
                return true;
            }
        }

        multiRankArray = null;
        return false;
    }

    private bool ConvertValueToSingleRankArray(Type type, out object singleRankArray)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            var tab = List;
            var arr = Array.CreateInstance(type.GetElementType()!, tab.Count);
            for (var i = 0; i < tab.Count; i++)
            {
                arr.SetValue(tab[i].To(type.GetElementType()), i);
            }

            {
                singleRankArray = arr;
                return true;
            }
        }

        singleRankArray = null;
        return false;
    }

    /// <summary>
    /// Creates a Value from a <see cref="JToken"/>
    /// </summary>
    /// <param name="token">The token to convert to a value</param>
    /// <returns>A value that represents the data stored in the token</returns>
    public static DataValue FromJToken(JToken token)
    {
        if (token == null) return new DataValue(DataType.None);
        while (true)
        {
            switch (token.Type)
            {
                case JTokenType.Property:
                    token = ((JProperty)token).Value;
                    continue;
                case JTokenType.Null or JTokenType.None:
                    return new DataValue(DataType.None);
                case JTokenType.Float:
                    return (double)token;
                case JTokenType.Integer:
                    return (long)token;
                case JTokenType.Boolean:
                    return (bool)token;
                case JTokenType.Date or JTokenType.String:
                    return (string)token;
                case JTokenType.Array:
                    return token.Select(FromJToken).ToList();
                case JTokenType.Object:
                {
                    Dictionary<string, DataValue> values = new();
                    foreach (var jToken in token)
                    {
                        var jProperty = (JProperty)jToken;
                        values[jProperty.Name] = FromJToken(jProperty.Value);
                    }

                    return values;
                }
                default:
                    return new DataValue(DataType.None);
            }
        }
    }

    /// <summary>
    /// Converts a value to a json.net object
    /// </summary>
    /// <returns>A JToken that represents the data in this object</returns>
    public JToken ToJToken()
    {
        if (IsBoolean)
        {
            return new JValue(Boolean);
        }

        if (IsReal)
        {
            return new JValue(Real);
        }

        if (IsInteger)
        {
            return new JValue(Integer);
        }

        if (IsString)
        {
            return new JValue(String);
        }

        if (IsList)
        {
            var array = new JArray();
            foreach (var value in List)
            {
                array.Add(value.ToJToken());
            }
            return array;
        }

        if (IsDictionary)
        {
            var obj = new JObject();
            foreach (var kvPair in Dictionary)
            {
                obj[kvPair.Key] = kvPair.Value.ToJToken();
            }
            return obj;
        }

        return null;
    }
    private static bool ConvertGenericListOrDictionaryToValue(object value, Type type, out DataValue listOrDictionaryDataValue)
    {
        switch (type.IsGenericType)
        {
            case true when type.GetGenericTypeDefinition() == typeof(Dictionary<,>):
            {
                var keyType = type.GetGenericArguments()[0];
                if (keyType != typeof(string))
                {
                    throw new TypeConversionException(type.FullName, "Value");
                }

                var dict = (IDictionary)value;
                Dictionary<string, DataValue> newData = new();
                foreach (var k in dict.Keys)
                {
                    var ks = (string)k;
                    newData[ks] = From(dict[k]);
                }

                {
                    listOrDictionaryDataValue = newData;
                    return true;
                }
            }
            case true when type.GetGenericTypeDefinition() == typeof(List<>):
            {
                listOrDictionaryDataValue = ((from object v in (IList)value select From(v)).ToList());
                return true;
            }
        }

        listOrDictionaryDataValue = null;
        return false;
    }

    private static bool ConvertMultiRankArrayToValue(object value, Type t, out DataValue multiRankArrayDataValue)
    {
        if (t.IsArray)
        {
            var a = (Array)value;
            var outermostDimension = new List<DataValue>();

            void TraverseRank(List<DataValue> containingDimension, Array arr, int r, params int[] idx)
            {
                var size = a.GetLength(r);
                var idxCopy = new int[idx.Length + 1];
                idx.CopyTo(idxCopy, 0);
                if (r == a.Rank - 1)
                {
                    for (var i = 0; i < size; i++)
                    {
                        idxCopy[idx.Length] = i;
                        var v = arr.GetValue(idxCopy);
                        containingDimension.Add(From(v));
                    }
                }
                else
                {
                    for (var i = 0; i < size; i++)
                    {
                        idxCopy[idx.Length] = i;
                        var currentDimension = new List<DataValue>();
                        TraverseRank(currentDimension, arr, r + 1, idxCopy);
                        outermostDimension.Add(currentDimension);
                    }
                }
            }


            TraverseRank(outermostDimension, a, 0);
            {
                multiRankArrayDataValue = outermostDimension;
                return true;
            }
        }

        multiRankArrayDataValue = null;
        return false;
    }

    private static bool ConvertSingleRankArrayToValue(object value, Type type, out DataValue singleRankArray)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            var arr = (Array)value;
            List<DataValue> vs = new List<DataValue>();
            foreach (var obj in arr)
            {
                vs.Add(From(obj));
            }

            {
                singleRankArray = vs;
                return true;
            }
        }

        singleRankArray = null;
        return false;
    }
    public static DataValue From(object value)
    {
        switch (value)
        {
            case null:
                return new DataValue(DataType.None);
            case DataValue v:
                return v;
            case bool b:
                return b;
            case byte bv:
                return bv;
            case sbyte sbv:
                return sbv;
            case short sv:
                return sv;
            case ushort usv:
                return usv;
            case int iv:
                return iv;
            case uint uiv:
                return uiv;
            case long slv:
                return slv;
            case ulong ulv:
                return ulv;
            case float f:
                return f;
            case double d:
                return d;
            case string s:
                return s;
            case List<DataValue> lv:
                return lv;
            case DataValue[] av:
                return av.ToList();
            case Dictionary<string, DataValue> dv:
                return dv;
            case PatchFunction pf:
                return pf;
            case Delegate d:
                return new ManagedPatchFunction(d.Method, d.Target);
        }

        var t = value.GetType();

        if (ConvertSingleRankArrayToValue(value, t, out var singleRankArray)) return singleRankArray;
        if (ConvertMultiRankArrayToValue(value, t, out var multiRankArrayValue)) return multiRankArrayValue;
        if (ConvertGenericListOrDictionaryToValue(value, t, out var listOrDictionaryValue)) return listOrDictionaryValue;
        Dictionary<string, DataValue> objectData = new();


        // Only store public data
        foreach (var field in t.GetFields())
        {
            objectData[field.Name] = From(field.GetValue(value));
        }

        return objectData;
    }

    public static DataValue Null => new(DataType.None);
    
    public static bool operator ==(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (ReferenceEquals(leftHandSide, null) || ReferenceEquals(rightHandSide, null))
        {
            return ReferenceEquals(leftHandSide, rightHandSide);
        }

        if (leftHandSide.Type != rightHandSide.Type) return false;
        
        if (leftHandSide.IsBoolean)
        {
            return leftHandSide.Boolean == rightHandSide.Boolean;
        }

        if (leftHandSide.IsReal)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return leftHandSide.Real == rightHandSide.Real;
        }

        if (leftHandSide.IsInteger)
        {
            return leftHandSide.Integer == rightHandSide.Integer;
        }

        if (leftHandSide.IsString)
        {
            return leftHandSide.String == rightHandSide.String;
        }

        if (leftHandSide.IsList)
        {
            return ListCompare(leftHandSide.List,rightHandSide.List);
        }

        if (leftHandSide.IsDictionary)
        {
            return DictionaryCompare(leftHandSide.Dictionary, rightHandSide.Dictionary);
        }

        if (leftHandSide.IsClosure && rightHandSide.IsClosure)
        {
            return leftHandSide.Closure == rightHandSide.Closure;
        }

        return true;
    }
    private static bool ListCompare(List<DataValue> leftHandSide, List<DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }
        return !leftHandSide.Where((t, index) => t != rightHandSide[index]).Any();
    }

    private static bool DictionaryCompare(Dictionary<string, DataValue> leftHandSide, Dictionary<string, DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }

        foreach (var kv in leftHandSide)
        {
            if (rightHandSide.TryGetValue(kv.Key, out var rvalue))
            {
                if (kv.Value != rvalue)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    public static bool operator !=(DataValue a, DataValue b) => !(a == b);

    public override bool Equals(object obj) => this == (DataValue)obj;

    public override int GetHashCode()
    {
        if (IsNone || IsDeletion)
        {
            return 0;
        }

        if (IsBoolean)
        {
            return Boolean.GetHashCode();
        }

        if (IsInteger)
        {
            return Integer.GetHashCode();
        }

        if (IsReal)
        {
            return Real.GetHashCode();
        }

        if (IsString)
        {
            return String.GetHashCode();
        }

        if (IsList)
        {
            return List.GetHashCode();
        }

        if (IsDictionary)
        {
            return Dictionary.GetHashCode();
        }

        if (IsClosure)
        {
            return Closure.GetHashCode();
        }

        return -1;
    }

    public static DataValue operator +(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.Type)
        {
            case DataType.Real when rightHandSide.IsReal:
                return leftHandSide.Real + rightHandSide.Real;
            case DataType.Real when rightHandSide.IsInteger:
                return leftHandSide.Real + rightHandSide.Integer;
            case DataType.Integer when rightHandSide.IsInteger:
                return leftHandSide.Integer + rightHandSide.Integer;
            case DataType.Integer when rightHandSide.IsReal:
                return leftHandSide.Integer + rightHandSide.Real;
            case DataType.String when rightHandSide.IsString:
                return leftHandSide.String + rightHandSide.String;
            case DataType.List when rightHandSide.IsList:
            {
                // If every value is immutable a shallow copy should be fine
                var newList = new List<DataValue>(leftHandSide.List);
                newList.AddRange(rightHandSide.List);
                return newList;
            }
            case DataType.None:
            case DataType.Boolean:
            case DataType.Dictionary:
            case DataType.Deletion:
            case DataType.Closure:
            default:
                throw new DataValueOperationException($"Cannot add a {leftHandSide.Type} and a {rightHandSide.Type}");
        }
    }

    public static DataValue operator -(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real - rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real - rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer - rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer - rightHandSide.Real;
        }

        if (leftHandSide.IsList && rightHandSide.IsList)
        {
            return leftHandSide.List.Where(x => rightHandSide.List.All(y => x != y)).ToList();
        }

        throw new DataValueOperationException($"Cannot subtract a {leftHandSide.Type} and a {rightHandSide.Type}");
    }
    
    private static DataValue StringRepeat(DataValue str, DataValue amount)
    {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < amount.Integer; i++)
        {
            sb.Append(str.String);
        }

        return sb.ToString();
    }

    private static DataValue ListRepeat(DataValue list, DataValue amount)
    {
        List<DataValue> newList = [];
        for (var i = 0; i < amount.Integer; i++)
        {
            newList.AddRange(list.List);
        }
        return newList;
    }

    
    public static DataValue operator *(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real * rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real * rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer * rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer * rightHandSide.Real;
        }

        if (leftHandSide.IsString && rightHandSide.IsInteger)
        {
            return StringRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsInteger && rightHandSide.IsString)
        {
            return StringRepeat(rightHandSide, leftHandSide);
        }
        
        if (leftHandSide.IsList && rightHandSide.IsInteger)
        {
            return ListRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsInteger && rightHandSide.IsList)
        {
            return ListRepeat(rightHandSide, leftHandSide);
        }

        throw new DataValueOperationException($"Cannot multiply a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static DataValue operator /(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsReal && rightHandSide.IsReal)
        {
            return leftHandSide.Real / rightHandSide.Real;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer / rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer / rightHandSide.Real;
        }

        if (leftHandSide.IsReal && rightHandSide.IsInteger)
        {
            return leftHandSide.Real / rightHandSide.Integer;
        }
        
        throw new DataValueOperationException($"Cannot divide a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static bool operator >(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real > rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real > rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer > rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer > rightHandSide.Real;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) > 0;
        }

        throw new DataValueOperationException($"Cannot relationally compare a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static bool operator <(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real < rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real < rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer < rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer < rightHandSide.Real;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) < 0;
        }
        
        throw new DataValueOperationException($"Cannot relationally compare a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static bool operator >=(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real >= rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real >= rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer >= rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer >= rightHandSide.Real;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) >= 0;
        }

        throw new DataValueOperationException($"Cannot relationally compare a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static bool operator <=(DataValue leftHandSide, DataValue rightHandSide)
    {
        switch (leftHandSide.IsReal)
        {
            case true when rightHandSide.IsReal:
                return leftHandSide.Real <= rightHandSide.Real;
            case true when rightHandSide.IsInteger:
                return leftHandSide.Real <= rightHandSide.Integer;
        }

        switch (leftHandSide.IsInteger)
        {
            case true when rightHandSide.IsInteger:
                return leftHandSide.Integer <= rightHandSide.Integer;
            case true when rightHandSide.IsReal:
                return leftHandSide.Integer <= rightHandSide.Real;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) <= 0;
        }

        throw new DataValueOperationException($"Cannot relationally compare a {leftHandSide.Type} and a {rightHandSide.Type}");
    }

    public static DataValue operator %(DataValue leftHandSide, DataValue rightHandSide) =>
        leftHandSide.IsReal switch
        {
            true when rightHandSide.IsReal => leftHandSide.Real % rightHandSide.Real,
            true when rightHandSide.IsInteger => leftHandSide.Real % rightHandSide.Integer,
            _ => leftHandSide.IsInteger switch
            {
                true when rightHandSide.IsInteger => leftHandSide.Integer % rightHandSide.Integer,
                true when rightHandSide.IsReal => leftHandSide.Integer % rightHandSide.Real,
                _ => throw new DataValueOperationException(
                    $"Cannot take the remainder of a {leftHandSide.Type} and a {rightHandSide.Type}")
            }
        };

    public static DataValue operator -(DataValue child)
    {
        if (child.IsInteger)
        {
            return -child.Integer;
        }

        if (child.IsReal)
        {
            return -child.Real;
        }

        throw new DataValueOperationException($"Cannot negate a {child.Type}");
    }

    public static bool operator !(DataValue child) => !child.Truthy;

    public static DataValue operator +(DataValue child) => child;

    public DataValue this[DataValue rightHandSide]
    {
        get
        {
            if (IsList && rightHandSide.IsInteger)
            {
                try
                {
                    return List[(int)rightHandSide.Integer];
                }
                catch
                {
                    throw new IndexOutOfRangeException(((int)rightHandSide.Real) + " is out of range of the string being indexed");
                }
            }

            if (IsString && rightHandSide.IsInteger)
            {
                try
                {
                    return (double)String[(int)rightHandSide.Integer];
                }
                catch
                {
                    throw new IndexOutOfRangeException(((int)rightHandSide.Real) + " is out of range of the string being indexed");
                }
            }

            if (IsDictionary && rightHandSide.IsString)
            {
                try
                {
                    return Dictionary[rightHandSide.String];
                }
                catch
                {
                    throw new KeyNotFoundException(rightHandSide.String + " is not a key found in the dictionary being indexed");
                }
            }

            throw new DataValueOperationException(
                $"Cannot subscript of a {Type} and a {rightHandSide.Type}");
        }
    }
}