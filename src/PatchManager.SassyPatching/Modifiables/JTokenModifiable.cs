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

    public DataValue GetFieldByNumber(string fieldName, ulong index)
    {
        return DataValue.FromJToken(_jToken[fieldName][(int)index]);
    }

    public DataValue GetFieldByElement(string fieldName, string elementName)
    {
        return DataValue.FromJToken(_jToken[fieldName][elementName]);
    }

    public DataValue GetFieldByClass(string fieldName, string className)
    {
        var field = _jToken[fieldName];
        foreach (var subField in field)
        {
            if (subField.Contains(className))
            {
                return DataValue.FromJToken(field);
            }

        }

        return new DataValue(DataValue.DataType.None);
    }

    public void SetFieldByNumber(string fieldName, ulong index, DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            RemoveToken(_jToken[fieldName][(int)index]);
        }
        else
        {
            _jToken[fieldName][(int)index].Replace(dataValue.ToJToken());
        }
    }

    public void SetFieldByElement(string fieldName, string elementName, DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            // _jToken[fieldName][elementName].Remove();
            RemoveToken(_jToken[fieldName][elementName]);
        }
        else {
            _jToken[fieldName][elementName].Replace(dataValue.ToJToken());
        }
    }

    /// <inheritdoc />
    public void SetFieldByClass(string fieldName, string className, DataValue dataValue)
    {
        _setDirty();
        var field = _jToken[fieldName];
        foreach (var subField in field.ToList().Where(subField => subField.Contains(className)))
        {
            if (dataValue.IsDeletion)
            {
                // subField.Remove();
                RemoveToken(subField);
            }
            else
            {
                if (subField is JProperty property)
                {
                    property.Value.Replace(dataValue.ToJToken());
                }
                else
                {
                    subField.Replace(dataValue.ToJToken());
                }
            }
            break;
        }
    }

    public DataValue GetFieldValue(string fieldName)
    {
        try
        {
            return DataValue.FromJToken(_jToken[fieldName]);
        }
        catch
        {
            return new DataValue(DataValue.DataType.None);
        }
    }

    public void SetFieldValue(string fieldName, DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            _jToken[fieldName].Remove();
        }
        else
        {
            if (_jToken is JObject obj && !obj.ContainsKey(fieldName))
                _jToken[fieldName] = dataValue.ToJToken();
            else
                _jToken[fieldName].Replace(dataValue.ToJToken());
        }
    }

    public void Set(DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            // _jToken.Remove();
            RemoveToken(_jToken);
        }
        else
        {
            _jToken.Replace(dataValue.ToJToken());
        }
    }

    public DataValue Get()
    {
        return DataValue.FromJToken(_jToken);
    }

    public void Complete()
    {
        return;
    }
}