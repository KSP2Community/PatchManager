using PatchManager.Shared;
using KSP.Game.Flow;
using SpaceWarp.API.Loading;

namespace PatchManager.Core.Flow;

/// <summary>
/// Manages injection of game loading flow actions.
/// </summary>
public static class FlowManager
{
    private static readonly Dictionary<IAction, string> Actions = new();

    internal static void RunPatch()
    {
        foreach (var (action, after) in Actions)
        {
            SaveLoad.AddFlowActionToGameLoadAfter(new GenericFlowAction(action.Name, action.DoAction), after);
            Logging.LogDebug($"Registering flow action: {action.Name}" + (after != null ? $" after {after}" : ""));
        }
    }

    /// <summary>
    /// Registers an action to be executed during game loading.
    /// </summary>
    /// <param name="action">Action to be executed.</param>
    /// <param name="after">
    /// Name of the action this action should execute after, or null to insert it at the start.
    /// </param>
    public static void RegisterAction(IAction action, string after = null)
    {
        Actions.Add(action, after);
    }
}