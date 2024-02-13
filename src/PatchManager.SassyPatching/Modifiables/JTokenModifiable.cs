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
    
    public DataValue GetFieldValue(string fieldName)
    {
        try
        {
            return DataValue.FromJToken(_jToken[fieldName]);
        }
        catch
        {
            return DataValue.Null;
        }
    }

    public void SetFieldValue(string fieldName, DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            if (_jToken[fieldName].Parent is JProperty jProperty)
            {
                jProperty.Remove();
            }
            else
            {
                _jToken[fieldName].Remove();
            }
        }
        else
        {
            if (_jToken is JObject obj && !obj.ContainsKey(fieldName))
                _jToken[fieldName] = dataValue.ToJToken();
            else
                _jToken[fieldName].Replace(dataValue.ToJToken());
        }
    }

    public virtual void Set(DataValue dataValue)
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
    }
}