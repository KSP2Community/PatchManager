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
        /// Creates a new instance of <see cref="FlowAction" />.
        /// </summary>
        /// <param name="name">The action name (also used as its description).</param>
        /// <param name="doAction">The work to perform; receives <c>resolve</c> and <c>reject</c> callbacks.</param>
        public FlowAction(string name, Action<Action, Action<string>> doAction)
        {
            Name = name;
            Description = name;
            _doAction = doAction;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Description shown alongside the action; defaults to <see cref="Name" />.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public void DoAction(Action resolve, Action<string> reject)
        {
            _doAction(resolve, reject);
        }
    }
}
