using System;
using System.Data;
using Castle.Core.Internal;
using JetBrains.Annotations;
using PatchManager.Generic.SassyPatching.Rulesets;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Selectors;
using PatchManager.SassyPatching.Nodes.Statements;
using PatchManager.SassyPatching.Utility;
using UniLinq;

namespace PatchManager.SassyPatching.Execution
{
    /// <summary>
    /// This is the class that all sassy patches get converted to
    /// </summary>
    public class SassyTextPatcher
    {

        // This is a snapshot of the environment before the patch was registered, note it will still reference the same global environment as its file, as that is only mutated by function declarations
        // Same w/ universe environment, as that should only contain stage definitions and such
        private Environment _environmentSnapshot;
        private SelectionBlock _rootSelectionBlock;
        public IPatcherRuleSet RuleSet;
        [CanBeNull] public string[] AssetName;
        [CanBeNull] public string AssetType; // We have one overload for asset types
        internal SassyTextPatcher(Environment environmentSnapshot, SelectionBlock rootSelectionBlock)
        {
            _environmentSnapshot = environmentSnapshot;
            _rootSelectionBlock = rootSelectionBlock;
            OriginalGuid = environmentSnapshot.GlobalEnvironment.ModGuid;
            PriorityString =
                rootSelectionBlock.Attributes.OfType<RunAtStageAttribute>().FirstOrDefault() is { } runAtStageAttribute
                    ? runAtStageAttribute.Stage.Interpolate(environmentSnapshot)
                    : environmentSnapshot.GlobalEnvironment.ModGuid;

            RecursivelyFindRuleSet(rootSelectionBlock.Selector);
            if (RuleSet == null)
            {
                throw new InterpreterException(rootSelectionBlock.Coordinate,
                    "Selection block does not have a ruleset for it");
            }
        }

        private void RecursivelyFindRuleSet(Selector selector)
        {
            if (selector is IntersectionSelector intersectionSelector) 
            {
                if (intersectionSelector.Selectors[0] is RulesetSelector rulesetSelector)
                {
                    if (!Universe.RuleSets.TryGetValue(rulesetSelector.RulesetName, out var ruleSet))
                    {
                        throw new InterpreterException(rulesetSelector.Coordinate,
                            $"Ruleset {rulesetSelector.RulesetName} does not exist!");
                    }
                    RuleSet = ruleSet;
                    if (RuleSet.CanGetAssetNameFromSelectableName && intersectionSelector.Selectors[1] is NameSelector nameSelector && !(nameSelector.NamePattern.Contains('*') || nameSelector.NamePattern.Contains('?')))
                    {
                        AssetName = RuleSet.SelectableNameToAssetName(nameSelector.NamePattern.Interpolate(_environmentSnapshot));
                    }

                    if (RuleSet is JsonRuleset jsonRuleset &&
                        intersectionSelector.Selectors[1] is ElementSelector elementSelector)
                    {
                        AssetType = elementSelector.ElementName.Interpolate(_environmentSnapshot);
                    }
                }
                else
                {
                    RecursivelyFindRuleSet(intersectionSelector.Selectors[0]);
                }
            } 
            else if (selector is ChildSelector childSelector)
            {
                RecursivelyFindRuleSet(childSelector.Parent);
            }
            else if (selector is RulesetSelector rulesetSelector)
            {
                if (!Universe.RuleSets.TryGetValue(rulesetSelector.RulesetName, out var ruleSet))
                {
                    throw new InterpreterException(rulesetSelector.Coordinate,
                        $"Ruleset {rulesetSelector.RulesetName} does not exist!");
                }
                RuleSet = ruleSet;
            }
        }

        public string OriginalGuid { get; }
        public string PriorityString { get; }

        /// <inheritdoc />
        public ulong Priority { get; set; }

        /// <inheritdoc />
        public bool TryPatch(string patchType, string name, ref ISelectable previousSelectable, out bool shouldStop)
        {
            previousSelectable.ClearModified();
            shouldStop = false;
            if (RuleSet.CanIngestSelectable(previousSelectable))
            {
                return _rootSelectionBlock.ExecuteFresh(_environmentSnapshot, previousSelectable);
            }
            var serialized = previousSelectable.Serialize();
            if (serialized.IsNullOrEmpty())
            {
                shouldStop = true;
                return false;
            }
                
            previousSelectable = RuleSet.ConvertToSelectable(patchType, name, serialized);
            return _rootSelectionBlock.ExecuteFresh(_environmentSnapshot, previousSelectable);
        }

        public bool TryPatchBegin(string patchType, string name, string data, out ISelectable selectable)
        {
            selectable = RuleSet.ConvertToSelectable(patchType, name, data);
            return _rootSelectionBlock.ExecuteFresh(_environmentSnapshot, selectable);
        }
    }
}
