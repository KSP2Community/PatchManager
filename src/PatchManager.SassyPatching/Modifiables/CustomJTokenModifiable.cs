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
    protected static void RemoveToken(JToken token)
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
    /// <param name="fieldName"></param>
    /// <param name="field"></param>
    /// <param name="classAdaptor"></param>
    /// <param name="indexAdaptor"></param>
    /// <param name="elementAdaptor"></param>
    /// <returns></returns>
    protected abstract bool CustomFieldAdaptor(string fieldName, out JToken field, out Func<JToken, string, bool> classAdaptor, out Func<JToken, ulong, JToken> indexAdaptor, out Func<JToken, string, JToken> elementAdaptor);

    /// <summary>
    /// This is a dictionary from fieldName -> function where if that function is run on a child of that field, then it is determined that that field has that class
    /// </summary>
    protected abstract Dictionary<string, Func<JToken, string, bool>> CustomClassAdaptors { get; }
    /// <summary>
    /// This is a dictionary from fieldName -> function where that function returns a custom value at that index or null
    /// </summary>
    protected abstract Dictionary<string, Func<JToken, ulong, JToken>> CustomIndexAdaptors { get; }
    /// <summary>
    /// This is a dictionary from fieldName -> function where that function returns a custom value that is that element or null
    /// </summary>
    protected abstract Dictionary<string, Func<JToken, string, JToken>> CustomElementAdaptors { get; }
    

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

    /// <inheritdoc />
    public virtual Value GetFieldByNumber(string fieldName, ulong index)
    {
        if (CustomFieldAdaptor(fieldName, out var customField, out _, out var indexAdaptor, out _))
        {
            if (indexAdaptor == null) return Value.FromJToken(customField[(int)index]);
            var result = indexAdaptor(customField, index);
            return Value.FromJToken(result ?? customField[(int)index]);
        }

        var field = JToken[fieldName];
        if (!CustomIndexAdaptors.TryGetValue(fieldName, out var adaptor)) return Value.FromJToken(field[(int)index]);
        var result2 = adaptor(field, index);
        return Value.FromJToken(result2 ?? field[(int)index]);
    }

    /// <inheritdoc />
    public virtual Value GetFieldByElement(string fieldName, string elementName)
    {
        if (CustomFieldAdaptor(fieldName, out var customField, out _, out _, out var elementAdaptor))
        {
            if (elementAdaptor == null) return Value.FromJToken(customField[elementName]);
            var result = elementAdaptor(customField, elementName);
            return Value.FromJToken(result ?? customField[elementName]);
        }
        var field = JToken[fieldName];
        if (!CustomElementAdaptors.TryGetValue(fieldName, out var adaptor)) return Value.FromJToken(field[elementName]);
        var result2 = adaptor(field, elementName);
        return Value.FromJToken(result2 ?? field[elementName]);
    }

    /// <inheritdoc />
    public virtual Value GetFieldByClass(string fieldName, string className)
    {
        if (CustomFieldAdaptor(fieldName, out var customField, out var classAdaptor, out _, out _))
        {
            foreach (var subfield in customField)
            {
                var converted = subfield is JProperty jProperty ? jProperty.Value : subfield;
                if (classAdaptor != null && classAdaptor(converted, className))
                {
                    return Value.FromJToken(subfield);
                }

                if (converted.Contains(className))
                {
                    return Value.FromJToken(subfield);
                }
            }
            return new Value(Value.ValueType.None);
        }
        CustomClassAdaptors.TryGetValue(fieldName, out var adaptor);
        var field = JToken[fieldName];
        foreach (var subfield in field)
        {
            var converted = subfield is JProperty jProperty ? jProperty.Value : subfield;
            if (adaptor != null && adaptor(converted, className))
            {
                return Value.FromJToken(subfield);
            }
            if (converted.Contains(className))
            {
                return Value.FromJToken(subfield);
            }
        }
        return new Value(Value.ValueType.None);
    }

    /// <summary>
    /// Sets a JToken to a value, taking care of deletions
    /// </summary>
    /// <param name="jToken">The JToken being set or replaced</param>
    /// <param name="value">The Value to replace it with</param>
    protected static void Set(JToken jToken, Value value)
    {
        if (value.IsDeletion)
        {
            RemoveToken(jToken);
        }
        else
        {
            jToken.Replace(value.ToJToken());
        }
    }

    /// <inheritdoc />
    public virtual void SetFieldByNumber(string fieldName, ulong index, Value value)
    {
        _setDirty();
        if (CustomFieldAdaptor(fieldName, out var customField, out _, out var indexAdaptor, out _))
        {
            if (indexAdaptor == null)
            {
                Set(customField[(int)index],value);
                return;
            }
            var result = indexAdaptor(customField, index);
            Set(result ?? customField[(int)index], value);
            return;
        }

        var field = JToken[fieldName];
        if (!CustomIndexAdaptors.TryGetValue(fieldName, out var adaptor))
        {
            Set(field[(int)index], value);
            return;
        }
        var result2 = adaptor(field, index);
        Set(result2 ?? field[(int)index], value);
    }

    /// <inheritdoc />
    public virtual void SetFieldByElement(string fieldName, string elementName, Value value)
    {
        _setDirty();
        if (CustomFieldAdaptor(fieldName, out var customField, out _, out _, out var elementAdaptor))
        {
            if (elementAdaptor == null)
            {
                Set(customField[elementName],value);
                return;
            }
            var result = elementAdaptor(customField, elementName);
            Set(result ?? customField[elementName],value);
            return;
        }
        var field = JToken[fieldName];
        if (!CustomElementAdaptors.TryGetValue(fieldName, out var adaptor))
        {
            Set(field[elementName],value);
            return;
        }
        var result2 = adaptor(field, elementName);
        Set(result2 ?? field[elementName],value);
    }

    /// <inheritdoc />
    public virtual void SetFieldByClass(string fieldName, string className, Value value)
    {
        _setDirty();
        if (CustomFieldAdaptor(fieldName, out var customField, out var classAdaptor, out _, out _))
        {
            foreach (var subfield in customField)
            {
                var converted = subfield is JProperty jProperty ? jProperty.Value : subfield;
                if (classAdaptor != null && classAdaptor(converted, className))
                {
                    Set(subfield,value);
                    return;
                }

                if (converted.Contains(className))
                {
                    Set(subfield,value);
                    return;
                }
            }
            return;
        }
        CustomClassAdaptors.TryGetValue(fieldName, out var adaptor);
        var field = JToken[fieldName];
        foreach (var subfield in field)
        {
            var converted = subfield is JProperty jProperty ? jProperty.Value : subfield;
            if (adaptor != null && adaptor(converted, className))
            {
                Set(subfield,value);
                return;
            }
            if (converted.Contains(className))
            {
                Set(subfield,value);
                return;
            }
        }
    }

    /// <inheritdoc />
    public virtual Value GetFieldValue(string fieldName)
    {
        return Value.FromJToken(CustomFieldAdaptor(fieldName, out var customField, out _, out _, out _) ? customField : JToken[fieldName]);
    }

    /// <inheritdoc />
    public virtual void SetFieldValue(string fieldName, Value value)
    {
        _setDirty();
        Set(CustomFieldAdaptor(fieldName, out var customField, out _, out _, out _) ? customField : JToken[fieldName],
            value);
    }

    /// <inheritdoc />
    public virtual void Set(Value value)
    {
        _setDirty();
        Set(JToken, value);
    }

    /// <inheritdoc />
    public virtual Value Get()
    {
        return Value.FromJToken(JToken);
    }

    /// <inheritdoc />
    public virtual void Complete()
    {
        return;
    }
}