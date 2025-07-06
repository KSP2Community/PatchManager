using System.Collections.Generic;
using JetBrains.Annotations;
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

        public string[] Labels => null;

        /// <inheritdoc />
        public ISelectable ConvertToSelectable(string type, string name, string jsonData)
        {
            var obj = JObject.Parse(jsonData);
            if (!obj.ContainsKey("Name") || !obj.ContainsKey("Version") || !obj.ContainsKey("CelestialBodies"))
                return null;
            return new GalaxySelectable(obj, type);
        
        }

        public bool CanIngestSelectable(ISelectable selectable) => selectable is GalaxySelectable
        {
            Deleted: false
        };
        
        public bool CanGetAssetNameFromSelectableName => false;

        public string SelectableNameToAssetName(string selectableName) => $"GalaxyDefinition_{selectableName}";

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
            return new NewGenericAsset($"GalaxyDefinition_{name}", $"GalaxyDefinition_{name}", new GalaxySelectable(JObject.FromObject(def), name));
        }
    }
}