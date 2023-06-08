using BepInEx.Logging;
using JetBrains.Annotations;

namespace PatchManager.Shared;

/// <summary>
/// Logging helper class for use in the PatchManager modules.
/// </summary>
[PublicAPI]
public static class Logging
{
    private static ManualLogSource _logger;

    /// <summary>
    /// Initializes the logging helper class with a BepInEx log source.
    /// </summary>
    /// <param name="logger">Log source to use for logging.</param>
    public static void Initialize(ManualLogSource logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="level">Log level of the message.</param>
    /// <param name="message">Message to be logged.</param>
    public static void Log(LogLevel level, object message)
    {
        _logger.Log(level, message);
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogDebug(object message)
    {
        Log(LogLevel.Debug, message);
    }

    /// <summary>
    /// Logs a message-level message.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogMessage(object message)
    {
        Log(LogLevel.Message, message);
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogInfo(object message)
    {
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogWarning(object message)
    {
        Log(LogLevel.Warning, message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogError(object message)
    {
        Log(LogLevel.Error, message);
    }

    /// <summary>
    /// Logs a fatal message. Only used in the case of fatal errors that will force the game to crash.
    /// </summary>
    /// <param name="message">Message to be logged.</param>
    public static void LogFatal(object message)
    {
        Log(LogLevel.Fatal, message);
    }
}