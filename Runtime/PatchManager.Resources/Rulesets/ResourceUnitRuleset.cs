using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Resources.Rulesets
{
    /// <summary>
    /// Implements the resource units ruleset from https://github.com/KSP2Community/CommunityResources
    /// </summary>
    [PatcherRuleset("resource_units","resource_units")]
    public class ResourceUnitRuleset : IPatcherRuleSet
    {

        public string[] Labels => new [] {"resource_units"};

        public ISelectable ConvertToSelectable(string type, string name, string jsonData) => new JTokenSelectable(() =>
        {
        }, JObject.Parse(jsonData), name, type);

        public bool CanIngestSelectable(ISelectable selectable) => selectable is JTokenSelectable;

        public bool CanGetAssetNameFromSelectableName => false;

        private static int _globallyIncrementingId = 0;

        public string SelectableNameToAssetName(string selectableName)
        {
            throw new System.NotImplementedException();
        }

        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var id = _globallyIncrementingId++;
            return new NewGenericAsset("resource_units", $"resource_unit_definition_{id}", new JTokenSelectable(
                () => { }, new JObject(), $"resource_unit_definition_{id}", "resource_units"));
        }
    }
}