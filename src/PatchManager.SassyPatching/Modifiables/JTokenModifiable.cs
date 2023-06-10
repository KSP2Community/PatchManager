using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Modifiables;

public class JTokenModifiable : IModifiable
{
    private JToken _jToken;

    public JTokenModifiable(JToken jToken)
    {
        _jToken = jToken;
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
        var idx = 0;
        foreach (var subField in field)
        {
            if (subField.Contains(className))
            {
                return Value.FromJToken(field);
            }

            idx++;
        }

        return new Value(Value.ValueType.None);
    }

    public void SetFieldByNumber(string fieldName, ulong index, Value value)
    {
        if (value.IsDeletion)
        {
            _jToken[fieldName][(int)index].Remove();
        }
        else
        {
            _jToken[fieldName][(int)index].Replace(value.ToJToken());
        }
    }

    public void SetFieldByElement(string fieldName, string elementName, Value value)
    {
        if (value.IsDeletion)
        {
            _jToken[fieldName][elementName].Remove();
        }
        else {
            _jToken[fieldName][elementName].Replace(value.ToJToken());
        }
    }

    /// <inheritdoc />
    public void SetFieldByClass(string fieldName, string className, Value value)
    {
        var field = _jToken[fieldName];
        foreach (var subField in field)
        {
            if (subField.Contains(className))
            {
                if (value.IsDeletion)
                {
                    subField.Remove();
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
    }

    public Value GetFieldValue(string fieldName)
    {
        return Value.FromJToken(_jToken[fieldName]);
    }

    public void SetFieldValue(string fieldName, Value value)
    {
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
        if (value.IsDeletion)
        {
            _jToken.Remove();
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