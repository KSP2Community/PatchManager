using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Interfaces;
// ReSharper disable CognitiveComplexity

namespace PatchManager.SassyPatching.Modifiables;

public abstract class CustomJTokenModifiable : IModifiable
{
    /// <summary>
    /// This is the data stored w/in this modifiable strucure
    /// </summary>
    protected readonly JToken JToken;
    private readonly Action _setDirty;

    /// <summary>
    /// Deletes a token, or if its a property, its parent
    /// </summary>
    /// <param name="token"></param>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="jToken"></param>
    /// <param name="setDirty"></param>
    protected CustomJTokenModifiable(JToken jToken, Action setDirty)
    {
        JToken = jToken;
        _setDirty = setDirty;
    }

    /// <summary>
    /// Sets a JToken to a value, taking care of deletions
    /// </summary>
    /// <param name="jToken">The JToken being set or replaced</param>
    /// <param name="dataValue">The Value to replace it with</param>
    protected static void Set(JToken jToken, DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            RemoveToken(jToken);
        }
        else
        {
            jToken.Replace(dataValue.ToJToken());
        }
    }

    /// <inheritdoc />
    public virtual DataValue GetFieldValue(string fieldName)
    {
        try
        {
            return DataValue.FromJToken(JToken[fieldName]);
        }
        catch
        {
            return new DataValue(DataValue.DataType.None);
        }
    }

    /// <inheritdoc />
    public virtual void SetFieldValue(string fieldName, DataValue dataValue)
    {
        _setDirty();
        if (dataValue.IsDeletion)
        {
            if (JToken is JObject obj && obj.ContainsKey(fieldName))
                JToken[fieldName]!.Remove();
        }
        else
        {
            if (JToken is JObject obj && !obj.ContainsKey(fieldName))
                JToken[fieldName] = dataValue.ToJToken();
            else
                JToken[fieldName]!.Replace(dataValue.ToJToken());
        }
    }

    /// <inheritdoc />
    public virtual void Set(DataValue dataValue)
    {
        _setDirty();
        Set(JToken, dataValue);
    }

    /// <inheritdoc />
    public virtual DataValue Get()
    {
        return DataValue.FromJToken(JToken);
    }

    /// <inheritdoc />
    public virtual void Complete()
    {
    }
}