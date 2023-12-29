using KSP.Sim;
using Newtonsoft.Json.Linq;
using PatchManager.Planets.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;

namespace PatchManager.Planets.Rulesets;

/// <inheritdoc />
[PatcherRuleset("galaxy", "GalaxyDefinition_Default")]
public class GalaxyRuleset
    : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => true;

    /// <inheritdoc />
    public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
        new GalaxySelectable(JObject.Parse(jsonData), type);
    /// <inheritdoc />
    public INewAsset CreateNew(List<DataValue> dataValues)
    {
        var name = dataValues[0].String;
        var version = dataValues.Count > 1 ? dataValues[1].String : "1.0.0";
        var def = new SerializedGalaxyDefinition
        {
            Name = name,
            Version = version,
            CelestialBodies = []
        };
        return new NewGenericAsset(name, name, new GalaxySelectable(JObject.FromObject(def), name));
    }
}