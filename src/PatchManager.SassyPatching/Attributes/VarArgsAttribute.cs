namespace PatchManager.SassyPatching.Attributes;

/// <summary>
/// Used on a parameter for a builtin method to change it from matching List&lt;Value&gt; to all the remaining arguments
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class VarArgsAttribute : Attribute
{
    
}