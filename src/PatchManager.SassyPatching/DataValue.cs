using System.Globalization;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace PatchManager.SassyPatching;

/// <summary>
/// The basic value type that the sassy patching engine uses
/// It can be one of 7 different types of values, 6 of which correspond fully to JSON types w/ one extra type/value meant to "delete" whatever it is assigned to
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
        Deletion
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
            return "'" + Regex.Escape(String) + "'";
        }

        if (IsList)
        {
            return "[" + string.Join(",",List.Select(x => x.ToString())) + "]";
        }

        if (IsDictionary)
        {
            return "{" + string.Join(",",Dictionary.Select(x => "'" + Regex.Escape(x.Key) + $"':{x.Value}")) + "}";
        }

        if (IsDeletion)
        {
            return "@delete";
        }

        return "<unknown>";
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
}