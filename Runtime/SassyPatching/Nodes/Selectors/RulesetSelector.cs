using System.Collections.Generic;
using System.Linq;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors
{
    /// <summary>
    /// Represents a selector that defines the ruleset that following selectors follow
    /// </summary>
    public class RulesetSelector : Selector
    {
        /// <summary>
        /// The name of the ruleset
        /// </summary>
        public readonly string RulesetName;
        internal RulesetSelector(Coordinate c, string rulesetName) : base(c)
        {
            RulesetName = rulesetName;
        }

        /// <inheritdoc />
        public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
        {
            return new();
        }

        /// <inheritdoc />
        public override List<SelectableWithEnvironment> SelectAllTopLevel(ISelectable selectable, Environment baseEnvironment)
        {
            return new List<SelectableWithEnvironment> { new ()
            {
                Selectable = selectable,
                Environment = new Environment(baseEnvironment.GlobalEnvironment, baseEnvironment)
            }};
        }
        

        public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
        {
            if (!Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
            {
                throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
            }

            var newObject = ruleSet.CreateNew(rulesetArguments);
            newAsset = newObject;
            return
                new List<SelectableWithEnvironment>
                {
                    new()
                    {
                        Selectable = newAsset.Selectable,
                        Environment = new Environment(baseEnvironment.GlobalEnvironment, baseEnvironment)
                    }
                };

        }
    }
}