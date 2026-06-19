using JetBrains.Annotations;
using ReduxLib.Logging;

namespace PatchManager.Shared
{
    /// <summary>
    /// Logging helper class for use in the PatchManager modules.
    /// </summary>
    [PublicAPI]
    public static class Logging
    {
        private static ILogger _logger;

        // NOTE: _logger is intentionally NOT reset on Play Mode enter. It is set once via Initialize()
        // from PatchManager.Awake (mod load), which only runs once per domain. With Domain Reload
        // disabled, nulling it here would leave it null on later Play Mode sessions (Awake does not
        // re-run), and the first PatchManager log — e.g. ArchiveResourceLocator.Locate logging a cache
        // miss — would throw a NullReferenceException. Letting the (still valid) logger persist mirrors
        // the Domain Reload behavior.

        /// <summary>
        /// Initializes the logging helper class with a BepInEx log source.
        /// </summary>
        /// <param name="logger">Log source to use for logging.</param>
        public static void Initialize(ILogger logger)
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
            // Defensive: never throw from the logging helper if a log is emitted before Initialize().
            if (_logger == null)
            {
                UnityEngine.Debug.Log(message);
                return;
            }
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
}