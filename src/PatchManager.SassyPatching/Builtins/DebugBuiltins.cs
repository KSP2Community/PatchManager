using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Execution;
using PatchManager.Shared;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// Contains a lot of builtin debug libraries for the sassy patch engine to use
/// </summary>
[SassyLibrary("builtin","debug"),PublicAPI]
public class DebugBuiltins
{
    /// <summary>
    /// Logs a value into the console for debugging
    /// </summary>
    /// <param name="universe">The universe in which this function is being called</param>
    /// <param name="value">The value to log</param>
    [SassyMethod("debug-log")]
    public static void Log(Universe universe, DataValue value)
    {
        universe.MessageLogger(value.Type == DataValue.DataType.String ? value.String : value.ToString());
    }

    /// <summary>
    /// Serializes a value
    /// </summary>
    /// <param name="value">The value to serialize</param>
    /// <returns>The serialized form of the value</returns>
    [SassyMethod("serialize")]
    public static string Serialize(DataValue value)
    {
        return value.ToString();
    }
}