﻿using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that don't have the class defined in this
/// </summary>
public class WithoutClassSelector : Selector
{
    /// <summary>
    /// The class to match against
    /// </summary>
    public readonly string ClassName;
    internal WithoutClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        return selectableWithEnvironments.Where(selectable => !selectable.Selectable.MatchesClass(ClassName)).ToList();
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type,string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }

    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
    {
        newAsset = null;
        return new();
    }
}