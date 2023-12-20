using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.Missions.Builtins;

[SassyLibrary("builtin","missions")]
[UsedImplicitly]
public class MissionsBuiltins
{
    
    [SassyMethod("get-property-watcher"), UsedImplicitly]
    public static string GetPropertyWatcher([SassyName("property-watcher-name")] string propertyWatcherName) =>
        MissionsTypes.PropertyWatchers[propertyWatcherName].AssemblyQualifiedName!;

    [SassyMethod("get-message"), UsedImplicitly]
    public static string GetMessage([SassyName("message-name")] string messageName) =>
        MissionsTypes.Messages[messageName].AssemblyQualifiedName!;
}