using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Expressions;
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
    /// <param name="datasetType">The type of dataset this is being executed (e.g. parts_data)</param>
    /// <param name="dataset">The dataset to execute this patch on</param>
    public bool ExecuteFresh(Environment snapshot, string datasetType, string name, ref string dataset)
    {
        // var subEnvironment = new Environment(snapshot.GlobalEnvironment, snapshot);
        var selections = Selector.SelectAllTopLevel(datasetType, name, dataset, snapshot, out var rulesetMatchingObject);
        
        if (rulesetMatchingObject == null || selections.Count == 0)
        {
            return false;
        }
        // Get the first matching selection if there are somehow more than one
        foreach (var selectable in selections) {
            var modifiable = selectable.Selectable.OpenModification();
            var subEnvironment = new Environment(selectable.Environment.GlobalEnvironment, selectable.Environment);
            if (modifiable != null)
            {
                subEnvironment["current"] = modifiable.Get();
            }

            foreach (var action in Actions)
            {
                if (action is ISelectionAction selectionAction)
                {
                    selectionAction.ExecuteOn(subEnvironment, selectable.Selectable, modifiable);
                }
                else
                {
                    action.ExecuteIn(subEnvironment);
                }
            }


        }
        var newDataSet = rulesetMatchingObject.Serialize();
        if (newDataSet == dataset) return false;
        dataset = newDataSet;
        return true;

    }

    public INewAsset ExecuteCreation(Environment snapshot, List<DataValue> arguments)
    {
        var selections = Selector.CreateNew(arguments, snapshot, out var resultingAsset);
        foreach (var selectable in selections) {
            var modifiable = selectable.Selectable.OpenModification();
            var subEnvironment = new Environment(selectable.Environment.GlobalEnvironment, selectable.Environment);
            if (modifiable != null)
            {
                subEnvironment["current"] = modifiable.Get();
            }

            foreach (var action in Actions)
            {
                if (action is ISelectionAction selectionAction)
                {
                    selectionAction.ExecuteOn(subEnvironment, selectable.Selectable, modifiable);
                }
                else
                {
                    action.ExecuteIn(subEnvironment);
                }
            }
            
        }
        return resultingAsset;
    }


    private void CreateGenerator(Environment environment, List<Expression> arguments)
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
        var args = arguments.Select(x => x.Compute(snapshot)).ToList();
        var generator = new SassyGenerator(snapshot, this, args);
        environment.GlobalEnvironment.Universe.RegisterGenerator(generator);
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
                case NewAttribute na:
                    CreateGenerator(environment, na.Arguments);
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

        var selections = Selector.SelectAll(new List<SelectableWithEnvironment> { new()
        {
            Selectable = selectable,
            Environment = new Environment(environment.GlobalEnvironment,environment.Snapshot())
        } });
        if (selections.Count == 0) return;
        // Takes a snapshot of the parent in its current state
        var parentValue = modifiable?.Get();
        foreach (var selection in selections)
        {
            ExecuteOnSingleSelection(selection.Environment, selection.Selectable, parentValue);
        }
    }

    private void ExecuteOnSingleSelection(Environment environment, ISelectable selection, DataValue parentDataValue)
    {
        var subModifiable = selection.OpenModification();
        var subEnvironment = new Environment(environment.GlobalEnvironment, environment)
        {
            ["current"] = selection.GetValue()
        };

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