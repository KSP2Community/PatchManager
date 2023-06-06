using BepInEx.Logging;
using JetBrains.Annotations;

namespace PatchManager.Core.Utility;

[PublicAPI]
internal static class Logging
{
    private static ManualLogSource _logger;

    public static void Initialize(ManualLogSource logger)
    {
        _logger = logger;
    }

    public static void Log(LogLevel level, object message)
    {
        _logger.Log(level, message);
    }

    public static void LogDebug(object message)
    {
        Log(LogLevel.Debug, message);
    }

    public static void LogMessage(object message)
    {
        Log(LogLevel.Message, message);
    }

    public static void LogInfo(object message)
    {
        Log(LogLevel.Info, message);
    }

    public static void LogWarning(object message)
    {
        Log(LogLevel.Warning, message);
    }

    public static void LogError(object message)
    {
        Log(LogLevel.Error, message);
    }

    public static void LogFatal(object message)
    {
        Log(LogLevel.Fatal, message);
    }
}