using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.Missions.Builtins;

/// <summary>
/// Builtins for missions.
/// </summary>
[SassyLibrary("builtin","missions")]
[UsedImplicitly]
public class MissionsBuiltins
{
    /// <summary>
    /// Gets the assembly-qualified type name of a property watcher.
    /// </summary>
    /// <param name="propertyWatcherName">The name of the property watcher.</param>
    /// <returns>The assembly-qualified type name of the property watcher.</returns>
    [SassyMethod("get-property-watcher"), UsedImplicitly]
    public static string GetPropertyWatcher([SassyName("property-watcher-name")] string propertyWatcherName) =>
        MissionsTypes.PropertyWatchers[propertyWatcherName].AssemblyQualifiedName!;

    /// <summary>
    /// Gets the assembly-qualified type name of a message.
    /// </summary>
    /// <param name="messageName">The name of the message.</param>
    /// <returns>The assembly-qualified type name of the message.</returns>
    [SassyMethod("get-message"), UsedImplicitly]
    public static string GetMessage([SassyName("message-name")] string messageName) =>
        MissionsTypes.Messages[messageName].AssemblyQualifiedName!;
}