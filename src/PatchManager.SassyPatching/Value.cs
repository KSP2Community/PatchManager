namespace PatchManager.SassyPatching;

public class Value
{
    public enum ValueType
    {
        None,
        Boolean,
        Number,
        String,
        List,
        Object,
        Deletion
    }

    public ValueType Type;
    public object Object;

    public Value(ValueType type, object o)
    {
        Type = type;
        Object = o;
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
        return new Value(ValueType.Object, d);
    }
}