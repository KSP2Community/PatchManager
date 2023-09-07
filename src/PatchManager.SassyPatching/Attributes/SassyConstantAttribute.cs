namespace PatchManager.SassyPatching.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SassyConstantAttribute : Attribute
{
    public string ConstantName;
    public SassyConstantAttribute(string constantName) => ConstantName = constantName;
}