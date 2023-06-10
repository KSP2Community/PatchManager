using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// This is the reflection library used by Sassy patching, its very simple as not much reflection is needed by the language
/// </summary>
[SassyLibrary("builtin","reflection")]
[PublicAPI]
public static class ReflectionBuiltins
{
    /// <summary>
    /// Gets the type of a value
    /// </summary>
    /// <param name="value">The value to get the type of</param>
    /// <returns>The values type as a lowercase string</returns>
    [SassyMethod("typeof")]
    public static string GetValueType(Value value)
    {
        return value.Type.ToString().ToLowerInvariant();
    }


    /// <summary>
    /// Invokes a method known in the environment w/ the name "functionName"
    /// </summary>
    /// <param name="functionName">The name of the function to invoke</param>
    /// <param name="arguments">The arguments to pass to the function</param>
    /// <param name="environment">Automatically filled in by the engine, the current environment</param>
    /// <returns>The result of the function</returns>
    [SassyMethod("string.invoke")]
    public static Value Invoke(string functionName, [VarArgs] List<Value> arguments, Environment environment)
    {
        throw new NotImplementedException();
    }
}