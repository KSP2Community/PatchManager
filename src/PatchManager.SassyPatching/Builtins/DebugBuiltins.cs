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
    /// <param name="v">The value to log</param>
    [SassyMethod("debug-log")]
    public static void Log(Universe universe, DataValue v)
    {
        universe.MessageLogger(v.Type == DataValue.DataType.String ? v.String : v.ToString());
    }

    /// <summary>
    /// Serializes a value
    /// </summary>
    /// <param name="v">The value to serialize</param>
    /// <returns>The serialized form of the value</returns>
    [SassyMethod("serialize")]
    public static string Serialize(DataValue v)
    {
        return v.ToString();
    }
}