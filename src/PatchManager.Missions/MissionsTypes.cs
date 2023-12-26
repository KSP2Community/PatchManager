using KSP.Game.Missions;
using KSP.Messages;
using KSP.Messages.PropertyWatchers;

namespace PatchManager.Missions;

/// <summary>
/// This class is used to get all the types of the missions.
/// </summary>
public static class MissionsTypes
{
    /// <summary>
    /// Dictionary of all the conditions.
    /// </summary>
    public static readonly Dictionary<string, Type> Conditions = new();
    /// <summary>
    /// Dictionary of all the actions.
    /// </summary>
    public static readonly Dictionary<string, Type> Actions = new();
    /// <summary>
    /// Dictionary of all the messages.
    /// </summary>
    public static readonly Dictionary<string, Type> Messages = new();
    /// <summary>
    /// Dictionary of all the property watchers.
    /// </summary>
    public static readonly Dictionary<string, Type> PropertyWatchers = new();

    static MissionsTypes()
    {
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(t => !t.IsAbstract))
        {
            if (type.IsSubclassOf(typeof(Condition)))
            {
                Conditions.Add(type.Name,type);
            }

            if (typeof(IMissionAction).IsAssignableFrom(type))
            {
                Actions.Add(type.Name, type);
            }

            if (type.IsSubclassOf(typeof(MessageCenterMessage)))
            {
                Messages.Add(type.Name, type);
            }

            if (type.IsSubclassOf(typeof(PropertyWatcher)))
            {
                PropertyWatchers.Add(type.Name, type);
            }
        }
    }
}