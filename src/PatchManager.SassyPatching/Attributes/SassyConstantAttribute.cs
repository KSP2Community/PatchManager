using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[MeansImplicitUse]
public class SassyConstantAttribute : Attribute
{
    public string ConstantName;
    public SassyConstantAttribute(string constantName) => ConstantName = constantName;
}