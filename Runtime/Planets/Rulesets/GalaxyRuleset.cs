using System.Collections.Generic;
using KSP.Sim;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Planets.Selectables;

namespace PatchManager.Planets.Rulesets
{
    /// <inheritdoc />
    [PatcherRuleset("galaxy", "GalaxyDefinition_Default")]
    public class GalaxyRuleset : IPatcherRuleSet
    {
        /// <inheritdoc />
        public bool Matches(string label) => true;

        /// <inheritdoc />
        public ISelectable ConvertToSelectable(string type, string name, string jsonData)
        {
            var obj = JObject.Parse(jsonData);
            if (!obj.ContainsKey("Name") || !obj.ContainsKey("Version") || !obj.ContainsKey("CelestialBodies"))
                return null;
            return new GalaxySelectable(obj, type);
        
        }
        /// <inheritdoc />
        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var name = dataValues[0].String;
            var version = dataValues.Count > 1 ? dataValues[1].String : "1.0.0";
            var def = new SerializedGalaxyDefinition
            {
                Name = name,
                Version = version,
                CelestialBodies = new List<SerializedCelestialBody>()
            };
            return new NewGenericAsset(name, name, new GalaxySelectable(JObject.FromObject(def), name));
        }
    }
}