namespace PatchManager.SassyPatching;

public class IncorrectTypeException : Exception
{
    public IncorrectTypeException(string message) : base(message)
    {
    }
}

public class Value
{
    public enum ValueType
    {
        None,
        Boolean,
        Number,
        String,
        List,
        Dictionary,
        Deletion
    }

    public ValueType Type;
    public object Object;

    public Value(ValueType type, object o)
    {
        Type = type;
        Object = o;
    }

    private void CheckType(ValueType toCheck)
    {
        if (Type != toCheck)
        {
            throw new IncorrectTypeException($"Attempting to read Value of type {Type} as a value of type {toCheck}");
        }
    }

    public bool IsNone => Type == ValueType.None;

    public bool IsBoolean => Type == ValueType.Boolean;

    public bool Boolean
    {
        get
        {
            CheckType(ValueType.Boolean);
            return (bool)Object;
        }
    }

    public bool IsNumber => Type == ValueType.Number;

    public double Number
    {
        get
        {
            CheckType(ValueType.Number);
            return (double)Object;
        }
    }

    public bool IsString => Type == ValueType.String;

    public string String
    {
        get
        {
            CheckType(ValueType.String);
            return (string)Object;
        }
    }

    public bool IsList => Type == ValueType.List;

    public List<Value> List
    {
        get
        {
            CheckType(ValueType.List);
            return (List<Value>)Object;
        }
    }

    public bool IsDictionary => Type == ValueType.Dictionary;

    public Dictionary<string, Value> Dictionary
    {
        get
        {
            CheckType(ValueType.Dictionary);
            return (Dictionary<string, Value>)Object;
        }
    }

    public bool IsDeletion => Type == ValueType.Deletion;
    
    public bool Truthy {
        get
        {
            if (IsBoolean) return Boolean;
            if (IsNumber) return Number != 0;
            return !(IsNone || IsDeletion);
        }
    }

    public static implicit operator Value(bool b)
    {
        return new Value(ValueType.Boolean, b);
    }

    public static implicit operator Value(double d)
    {
        return new Value(ValueType.Number, d);
    }

    public static implicit operator Value(string s)
    {
        return new Value(ValueType.String, s);
    }

    public static implicit operator Value(List<Value> l)
    {
        return new Value(ValueType.List, l);
    }

    public static implicit operator Value(Dictionary<string, Value> d)
    {
        return new Value(ValueType.Dictionary, d);
    }
}