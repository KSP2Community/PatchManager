using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// This represents a selection action to be taken on a selection
/// </summary>
public interface ISelectionAction
{
    /// <summary>
    /// Execute this selection action on a selectable
    /// </summary>
    /// <param name="environment">The environment that this action is taking place in</param>
    /// <param name="selectable">The selectable that this action is being acted upon</param>
    /// <param name="modifiable">The modifiable state of the selectable, null if its not modifiable</param>
    public void ExecuteOn(Environment environment, ISelectable selectable, [CanBeNull] IModifiable modifiable);
}