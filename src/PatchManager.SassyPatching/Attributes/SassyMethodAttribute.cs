namespace PatchManager.SassyPatching.Attributes;

/// <summary>
/// Defines this as a method for sassy patcher, it can define any argument type it wants, and reflection will be used to convert them at runtime
/// Method overloading is not allowed
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SassyMethodAttribute : Attribute
{
    /// <summary>
    /// The name of the method inside of the runtime this will be called
    /// </summary>
    public string MethodName;

    /// <summary>
    /// Initializes a new SassyMethodAttribute
    /// </summary>
    /// <param name="methodName">The name of the method in runtime</param>
    public SassyMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
}