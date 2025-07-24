using System.Collections.Generic;
using KSP.Game.Flow;
using PatchManager.Shared;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Loading;

namespace PatchManager.Core.Flow
{
    /// <summary>
    /// Manages injection of game loading flow actions.
    /// </summary>
    public static class FlowManager
    {
        private static readonly List<IFlowAction> Actions = new();

        private static readonly Dictionary<IFlowAction, string> ActionsAfter = new();

        /// <summary>
        /// Registers an action to be executed at the end of game loading.
        /// </summary>
        /// <param name="action">Action to be executed.</param>
        public static void RegisterAction(IFlowAction action)
        {
            Actions.Add(action);
        }

        /// <summary>
        /// Registers an action to be executed during game loading.
        /// </summary>
        /// <param name="action">Action to be executed.</param>
        /// <param name="after">Action name to insert the new action after, null to insert at the beginning.</param>
        public static void RegisterActionAfter(IFlowAction action, string after)
        {
            ActionsAfter.Add(action, after);
        }

        internal static void AddActionsToFlow(SequentialFlow loadingFlow)
        {
            foreach (var action in Actions)
            {
                loadingFlow.AddAction(new GenericFlowAction(action.Name, action.DoAction));
                Logging.LogDebug($"Registering flow action at the end: \"{action.Name}\"");
            }

            foreach (var (action, after) in ActionsAfter)
            {
                SaveLoad.AddFlowActionToGameLoadAfter(action, after);
                Logging.LogDebug($"Registering flow action \"{action.Name}\" after \"{after}\"");
            }
        }
    }
}