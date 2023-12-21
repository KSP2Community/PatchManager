using KSP.Game.Missions;
using KSP.Messages;
using KSP.Messages.PropertyWatchers;

namespace PatchManager.Missions;

public static class MissionsTypes
{
    public static Dictionary<string, Type> Conditions = new();
    public static Dictionary<string, Type> Actions = new();
    public static Dictionary<string, Type> Messages = new();
    public static Dictionary<string, Type> PropertyWatchers = new();

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