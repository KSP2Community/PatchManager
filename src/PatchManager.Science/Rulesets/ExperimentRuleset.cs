using JetBrains.Annotations;
using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Rulesets;

[PatcherRuleset("experiments","scienceExperiment"),UsedImplicitly]
public class ExperimentRuleset : IPatcherRuleSet
{
    public bool Matches(string label) => label == "scienceExperiment";

    public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
        new ExperimentSelectable(JObject.Parse(jsonData));

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