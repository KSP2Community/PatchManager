using PatchManager.Parts.SassyPatching.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.SassyPatching.Modifiables;

public class PartModifiable : IModifiable
{
    private PartSelectable _selectable;

    public PartModifiable(PartSelectable selectable)
    {
        _selectable = selectable;
    }

    /// <inheritdoc />
    public Value GetFieldByNumber(string fieldName, ulong index)
    {
        throw new NotImplementedException();
    }

    public Value GetFieldByElement(string fieldName, string elementName)
    {
        throw new NotImplementedException();
    }

    public Value GetFieldByClass(string fieldName, string className)
    {
        throw new NotImplementedException();
    }

    public void SetFieldByNumber(string fieldName, ulong index, Value value)
    {
        throw new NotImplementedException();
    }

    public void SetFieldByElement(string fieldName, string elementName, Value value)
    {
        throw new NotImplementedException();
    }

    public void SetFieldByClass(string fieldName, string className, Value value)
    {
        throw new NotImplementedException();
    }

    public Value GetFieldValue(string fieldName)
    {
        throw new NotImplementedException();
    }

    public void SetFieldValue(string fieldName, Value value)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Set(Value value)
    {
        throw new NotImplementedException();
    }

    public Value Get()
    {
        throw new NotImplementedException();
    }

    public void Complete()
    {
        throw new NotImplementedException();
    }
}