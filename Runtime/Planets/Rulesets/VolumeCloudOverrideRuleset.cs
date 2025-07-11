using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Planets.Overrides;
using PatchManager.Planets.Selectables;

namespace PatchManager.Planets.Rulesets
{
    [PatcherRuleset("volume-cloud-override","volume_cloud_overrides")]
    public class VolumeCloudOverrideRuleset : IPatcherRuleSet
    {
        public string[] Labels => new [] {"volume_cloud_overrides"};
        public ISelectable ConvertToSelectable(string type, string name, string jsonData) => new VolumeCloudSelectable(JObject.Parse(jsonData));

        public bool CanIngestSelectable(ISelectable selectable) => selectable is VolumeCloudSelectable
        {
            Deleted: false
        };

        public bool CanGetAssetNameFromSelectableName => true;

        public string[] SelectableNameToAssetName(string selectableName) => new [] {$"volume_cloud_override_{selectableName.ToLowerInvariant()}"};

        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var planetName = dataValues[0].String.ToLowerInvariant();
            var data = new VolumeCloudConfigurationOverride
            {
                bodyName = planetName
            };
            return new NewGenericAsset("volume_cloud_overrides", $"volume_cloud_override_{planetName}",
                new VolumeCloudSelectable(JObject.FromObject(data)));
        }
    }
}