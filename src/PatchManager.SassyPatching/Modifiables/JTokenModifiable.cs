using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Modifiables;

public class JTokenModifiable : IModifiable
{
    private JToken _jToken;
    private readonly Action _setDirty;

    private static void RemoveToken(JToken token)
    {
        if (token.Parent is JProperty)
        {
            token.Parent.Remove();
        }
        else
        {
            token.Remove();
        }
    }
    
    public JTokenModifiable(JToken jToken, Action setDirty)
    {
        _jToken = jToken;
        this._setDirty = setDirty;
    }

    public Value GetFieldByNumber(string fieldName, ulong index)
    {
        return Value.FromJToken(_jToken[fieldName][(int)index]);
    }

    public Value GetFieldByElement(string fieldName, string elementName)
    {
        return Value.FromJToken(_jToken[fieldName][elementName]);
    }

    public Value GetFieldByClass(string fieldName, string className)
    {
        var field = _jToken[fieldName];
        foreach (var subField in field)
        {
            if (subField.Contains(className))
            {
                return Value.FromJToken(field);
            }

        }

        return new Value(Value.ValueType.None);
    }

    public void SetFieldByNumber(string fieldName, ulong index, Value value)
    {
        _setDirty();
        if (value.IsDeletion)
        {
            RemoveToken(_jToken[fieldName][(int)index]);
        }
        else
        {
            _jToken[fieldName][(int)index].Replace(value.ToJToken());
        }
    }

    public void SetFieldByElement(string fieldName, string elementName, Value value)
    {
        _setDirty();
        if (value.IsDeletion)
        {
            // _jToken[fieldName][elementName].Remove();
            RemoveToken(_jToken[fieldName][elementName]);
        }
        else {
            _jToken[fieldName][elementName].Replace(value.ToJToken());
        }
    }

    /// <inheritdoc />
    public void SetFieldByClass(string fieldName, string className, Value value)
    {
        _setDirty();
        var field = _jToken[fieldName];
        foreach (var subField in field.ToList().Where(subField => subField.Contains(className)))
        {
            if (value.IsDeletion)
            {
                // subField.Remove();
                RemoveToken(subField);
            }
            else
            {
                if (subField is JProperty property)
                {
                    property.Value.Replace(value.ToJToken());
                }
                else
                {
                    subField.Replace(value.ToJToken());
                }
            }
            break;
        }
    }

    public Value GetFieldValue(string fieldName)
    {
        return Value.FromJToken(_jToken[fieldName]);
    }

    public void SetFieldValue(string fieldName, Value value)
    {
        _setDirty();
        if (value.IsDeletion)
        {
            _jToken[fieldName].Remove();
        }
        else
        {
            _jToken[fieldName].Replace(value.ToJToken());
        }
    }

    public void Set(Value value)
    {
        _setDirty();
        if (value.IsDeletion)
        {
            // _jToken.Remove();
            RemoveToken(_jToken);
        }
        else
        {
            _jToken.Replace(value.ToJToken());
        }
    }

    public Value Get()
    {
        return Value.FromJToken(_jToken);
    }

    public void Complete()
    {
        return;
    }
}