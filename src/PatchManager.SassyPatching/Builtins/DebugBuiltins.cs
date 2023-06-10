using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.Shared;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// Contains a lot of builtin debug libraries for the sassy patch engine to use
/// </summary>
[SassyLibrary("builtin","debug"),PublicAPI]
public static class DebugBuiltins
{
    /// <summary>
    /// Logs a value into the console for debugging
    /// </summary>
    /// <param name="v">The value to log</param>
    [SassyMethod("debug-log")]
    public static void Log(Value v)
    {
        Logging.LogInfo(v.ToString());
    }
}