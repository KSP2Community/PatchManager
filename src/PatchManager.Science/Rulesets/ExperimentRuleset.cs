using JetBrains.Annotations;
using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Rulesets;

/// <summary>
/// Ruleset for the ScienceExperiment part module.
/// </summary>
[PatcherRuleset("experiments","scienceExperiment"),UsedImplicitly]
public class ExperimentRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => label == "scienceExperiment";

    /// <inheritdoc />
    public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
        new ExperimentSelectable(JObject.Parse(jsonData));

    /// <inheritdoc />
    public INewAsset CreateNew(List<DataValue> dataValues)
    {
        var core = new ExperimentCore
        {
            data = new ExperimentDefinition
            {
                ExperimentID = dataValues[0].String
            }
        };
        return new NewGenericAsset("scienceExperiment", dataValues[0].String,
            new ExperimentSelectable(JObject.FromObject(core)));
    }
}