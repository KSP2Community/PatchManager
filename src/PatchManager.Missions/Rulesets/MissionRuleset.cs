using KSP.Game.Missions.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.Missions.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;

namespace PatchManager.Missions.Rulesets;

[PatcherRuleset("missions","missions")]
public class MissionRuleset : IPatcherRuleSet
{
    public bool Matches(string label) => label == "missions";

    public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
        new MissionSelectable(JObject.Parse(jsonData));

    public INewAsset CreateNew(List<DataValue> dataValues)
    {
        var missionDataObject = new MissionData
        {
            name = dataValues[0].String
        };

        return new NewGenericAsset("missions", dataValues[0].String,
            new MissionSelectable(JObject.FromObject(missionDataObject)));
    }
}