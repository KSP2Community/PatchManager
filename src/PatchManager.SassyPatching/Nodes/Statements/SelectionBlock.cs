using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Selectors;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a selection block
/// </summary>
public class SelectionBlock : Node, ISelectionAction
{
    /// <summary>
    /// The attributes applied to this selection block
    /// </summary>
    public readonly List<SelectorAttribute> Attributes;
    /// <summary>
    /// The selector that this selection block matches
    /// </summary>
    public readonly Selector Selector;
    /// <summary>
    /// The actions to be taken upon a match
    /// </summary>
    public readonly List<Node> Actions;
    
    internal SelectionBlock(Coordinate c, List<SelectorAttribute> attributes, Selector selector, List<Node> actions) : base(c)
    {
        Attributes = attributes;
        Selector = selector;
        Actions = actions;
    }


    /// <summary>
    /// Execute this selection block on a dataset
    /// </summary>
    /// <param name="snapshot">The environment that contains this selection block</param>
    /// <param name="datasetType">The type of dataset this is being executed (e.g. part_data)</param>
    /// <param name="dataset">The dataset to execute this patch on</param>
    public bool ExecuteFresh(Environment snapshot, string datasetType, ref string dataset)
    {
        // var subEnvironment = new Environment(snapshot.GlobalEnvironment, snapshot);
        var selections = Selector.SelectAllTopLevel(datasetType, dataset);
        if (selections.Count == 0)
        {
            return false;
        }
        // Get the first matching selection if there are somehow more than one
        var selectable = selections.First();
        var modifiable = selectable.OpenModification();
        var subEnvironment = new Environment(snapshot.GlobalEnvironment, snapshot);
        if (modifiable != null)
        {
            subEnvironment["current"] = modifiable.Get();
        }
        foreach (var action in Actions)
        {
            if (action is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(subEnvironment,selectable,modifiable);
            }
            else
            {
                action.ExecuteIn(subEnvironment);
            }
        }
        
        var newDataSet = selectable.Serialize();
        if (newDataSet == dataset) return false;
        dataset = newDataSet;
        return true;

    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        foreach (var attribute in Attributes)
        {
            switch (attribute)
            {
                case RequireModAttribute requireModAttribute when
                    !environment.GlobalEnvironment.Universe.AllMods.Contains(requireModAttribute.Guid):
                case RequireNotModAttribute requireNotModAttribute when
                    environment.GlobalEnvironment.Universe.AllMods.Contains(requireNotModAttribute.Guid):
                    return;
            }
        }
        var snapshot = environment.Snapshot();
        var patcher = new SassyTextPatcher(snapshot, this);
        environment.GlobalEnvironment.Universe.RegisterPatcher(patcher);
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable = null)
    {
        foreach (var attribute in Attributes)
        {
            switch (attribute)
            {
                case RequireModAttribute requireModAttribute when
                    !environment.GlobalEnvironment.Universe.AllMods.Contains(requireModAttribute.Guid):
                case RequireNotModAttribute requireNotModAttribute when
                    environment.GlobalEnvironment.Universe.AllMods.Contains(requireNotModAttribute.Guid):
                    return;
            }
        }

        var selections = Selector.SelectAll(new List<ISelectable> { selectable });
        if (selections.Count == 0) return;
        // Takes a snapshot of the parent in its current state
        var parentValue = modifiable?.Get();
        foreach (var selection in selections)
        {
            ExecuteOnSingleSelection(environment, selection, parentValue);
        }
    }

    private void ExecuteOnSingleSelection(Environment environment, ISelectable selection, DataValue parentDataValue)
    {
        var subModifiable = selection.OpenModification();
        var subEnvironment = new Environment(environment.GlobalEnvironment, environment);
        if (subModifiable != null)
        {
            subEnvironment["current"] = subModifiable.Get();
        }

        if (parentDataValue != null)
        {
            subEnvironment["parent"] = parentDataValue;
        }

        foreach (var action in Actions)
        {
            if (action is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(subEnvironment, selection, subModifiable);
            }
            else
            {
                action.ExecuteIn(subEnvironment);
            }
        }
    }
}