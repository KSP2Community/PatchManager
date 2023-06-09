using System.Globalization;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace PatchManager.SassyPatching;

/// <summary>
/// The basic value type that the sassy patching engine uses
/// It can be one of 7 different types of values, 6 of which correspond fully to JSON types w/ one extra type/value meant to "delete" whatever it is assigned to
/// </summary>
public class Value
{
    /// <summary>
    /// The types of values that a Value can be
    /// </summary>
    public enum ValueType
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
        /// The type of numbers, stored as a double, corresponds to JSON numbers
        /// </summary>
        Number,
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
    public readonly ValueType Type;

    private readonly object _object;

    /// <summary>
    /// Creates a new value w/ a specified type and stored value
    /// </summary>
    /// <param name="type">The type of this value</param>
    /// <param name="o">The value that is stored in this value, can be null for values of type <see cref="ValueType.None"/> and <see cref="ValueType.Deletion"/></param>
    public Value(ValueType type, object o = null)
    {
        Type = type;
        _object = o;
    }

    private void CheckType(ValueType toCheck)
    {
        if (Type != toCheck)
        {
            throw new IncorrectTypeException($"Attempting to read Value of type {Type} as a value of type {toCheck}");
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.None"/>?
    /// </summary>
    public bool IsNone => Type == ValueType.None;

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.Boolean"/>?
    /// </summary>
    public bool IsBoolean => Type == ValueType.Boolean;

    /// <summary>
    /// Asserts this value is of type <see cref="ValueType.Boolean"/>,
    /// then returns the <see cref="bool"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="ValueType.Boolean"/></exception>
    public bool Boolean
    {
        get
        {
            CheckType(ValueType.Boolean);
            return (bool)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.Number"/>?
    /// </summary>
    public bool IsNumber => Type == ValueType.Number;

    /// <summary>
    /// Asserts this value is of type <see cref="ValueType.Number"/>,
    /// then returns the <see cref="double"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="ValueType.Number"/></exception>
    public double Number
    {
        get
        {
            CheckType(ValueType.Number);
            return (double)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.String"/>?
    /// </summary>
    public bool IsString => Type == ValueType.String;

    /// <summary>
    /// Asserts this value is of type <see cref="ValueType.String"/>,
    /// then returns the <see cref="string"/> contained within
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="ValueType.String"/></exception>
    public string String
    {
        get
        {
            CheckType(ValueType.String);
            return (string)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.List"/>?
    /// </summary>
    public bool IsList => Type == ValueType.List;

    /// <summary>
    /// Asserts this value is of type <see cref="ValueType.List"/>,
    /// then returns the <see cref="List{T}"/> contained within of which generic type argument is <see cref="Value"/>
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="ValueType.List"/></exception>
    public List<Value> List
    {
        get
        {
            CheckType(ValueType.List);
            return (List<Value>)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.Dictionary"/>?
    /// </summary>
    public bool IsDictionary => Type == ValueType.Dictionary;
    
    /// <summary>
    /// Asserts this value is of type <see cref="ValueType.Dictionary"/>,
    /// then returns the <see cref="Dictionary{K,V}"/> contained within of which the key type is <see cref="string"/> and the value type is <see cref="Value"/>
    /// </summary>
    /// <exception cref="IncorrectTypeException">Thrown if this value is not a value of type <see cref="ValueType.Dictionary"/></exception>
    public Dictionary<string, Value> Dictionary
    {
        get
        {
            CheckType(ValueType.Dictionary);
            return (Dictionary<string, Value>)_object;
        }
    }

    /// <summary>
    /// Is the type of this variable <see cref="ValueType.Deletion"/>?
    /// </summary>
    public bool IsDeletion => Type == ValueType.Deletion;
    
    /// <summary>
    /// Does this value get interpreted as true in places where a <see cref="ValueType.Boolean"/> is expected?
    /// That is, is the value a <see cref="ValueType.Boolean"/> and the value stored within true, or is the value anything but <see cref="ValueType.None"/> and <see cref="ValueType.Deletion"/>?
    /// </summary>
    public bool Truthy {
        get
        {
            if (IsBoolean) return Boolean;
            return !(IsNone || IsDeletion);
        }
    }

    /// <summary>
    /// Creates a <see cref="Value"/> from a <see cref="Boolean"/>
    /// </summary>
    /// <param name="b">The value to be stored within the value</param>
    /// <returns>A <see cref="Value"/> with a type of <see cref="ValueType.Boolean"/> and a stored value of <param name="b"></param></returns>
    public static implicit operator Value(bool b)
    {
        return new Value(ValueType.Boolean, b);
    }

    /// <summary>
    /// Creates a <see cref="Value"/> from a <see cref="Double"/>
    /// </summary>
    /// <param name="d">The value to be stored within the value</param>
    /// <returns>A <see cref="Value"/> with a type of <see cref="ValueType.Number"/> and a stored value of <param name="d"></param></returns>
    public static implicit operator Value(double d)
    {
        return new Value(ValueType.Number, d);
    }

    /// <summary>
    /// Creates a <see cref="Value"/> from a <see cref="string"/>
    /// </summary>
    /// <param name="s">The value to be stored within the value</param>
    /// <returns>A <see cref="Value"/> with a type of <see cref="ValueType.String"/> and a stored value of <param name="s"></param></returns>
    public static implicit operator Value(string s)
    {
        return new Value(ValueType.String, s);
    }

    /// <summary>
    /// Creates a <see cref="Value"/> from a <see cref="List{T}"/> with the generic type argument being <see cref="Value"/>
    /// </summary>
    /// <param name="l">The value to be stored within the value</param>
    /// <returns>A <see cref="Value"/> with a type of <see cref="ValueType.List"/> and a stored value of <param name="l"></param></returns>
    public static implicit operator Value(List<Value> l)
    {
        return new Value(ValueType.List, l);
    }

    /// <summary>
    /// Creates a <see cref="Value"/> from a <see cref="Dictionary{K,V}"/> with the key type being <see cref="string"/> and value type being <see cref="Value"/>
    /// </summary>
    /// <param name="d">The value to be stored within the value</param>
    /// <returns>A <see cref="Value"/> with a type of <see cref="ValueType.Dictionary"/> and a stored value of <param name="d"></param></returns>
    public static implicit operator Value(Dictionary<string, Value> d)
    {
        return new Value(ValueType.Dictionary, d);
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

        if (IsNumber)
        {
            return Number.ToString(CultureInfo.InvariantCulture);
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
}