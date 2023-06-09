using KSP.Game.Flow;
using PatchManager.Shared;

namespace PatchManager.Core.Flow;

/// <summary>
/// Manages injection of game loading flow actions.
/// </summary>
public static class FlowManager
{
    private static readonly List<IAction> Actions = new();

    /// <summary>
    /// Registers an action to be executed during game loading.
    /// </summary>
    /// <param name="action">Action to be executed.</param>
    public static void RegisterAction(IAction action)
    {
        Actions.Add(action);
    }

    internal static void AddActionsToFlow(SequentialFlow loadingFlow)
    {
        foreach (var action in Actions)
        {
            loadingFlow.AddAction(new GenericFlowAction(action.Name, action.DoAction));
            Logging.LogDebug($"Registering flow action: {action.Name}");
        }
    }
}