using System;
using ReduxLib.GameInterfaces;

namespace PatchManager.Core.Flow
{
    /// <summary>
    /// Represents a general action to be executed during game loading.
    /// </summary>
    public class FlowAction : IFlowAction
    {
        private readonly Action<Action,Action<string>> _doAction;

        /// <summary>
        /// Creates a new instance of <see cref="FlowAction"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="doAction"></param>
        public FlowAction(string name, Action<Action, Action<string>> doAction)
        {
            Name = name;
            Description = name;
            _doAction = doAction;
        }

        /// <inheritdoc />
        public string Name { get; }

        public string Description { get; }

        /// <inheritdoc />
        public void DoAction(Action resolve, Action<string> reject)
        {
            _doAction(resolve, reject);
        }
    }
}