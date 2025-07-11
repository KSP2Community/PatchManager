using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Rulesets
{
    /// <summary>
    /// This ruleset is used to patch the science tech tree.
    /// </summary>
    [PatcherRuleset("science","techNodeData")]
    public class ScienceRuleset : IPatcherRuleSet
    {

        public string[] Labels => new[] { "techNodeData" };

        /// <inheritdoc />
        public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
            new ScienceSelectable(JObject.Parse(jsonData));

        public bool CanIngestSelectable(ISelectable selectable) => selectable is ScienceSelectable
        {
            Deleted: false
        };
        
        // TODO: Evaluate whether or not this is the case
        public bool CanGetAssetNameFromSelectableName => false;
        public string[] SelectableNameToAssetName(string selectableName)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            return new NewGenericAsset("techNodeData", dataValues[0].String,
                new ScienceSelectable(JObject.Parse($"{{\n  \"Version\": 12,\n  \"ID\": \"{dataValues[0].String}\",\n  \"NameLocKey\": \"Science/TechNodes/Names/tNode_4v_docking_01\",\n  \"IconID\": \"ICO-RDCenter-Docking-24x\",\n  \"CategoryID\": \"\",\n  \"HiddenByNodeID\": \"\",\n  \"DescriptionLocKey\": \"Science/TechNodes/Descriptions/tNode_4v_docking_01\",\n  \"RequiredSciencePoints\": 7000,\n  \"UnlockedPartsIDs\": [\n    \"dockingring_4v_inline\",\n    \"dockingport_3v_inline\"\n  ],\n  \"RequiredTechNodeIDs\": [\n    \"tNode_4v_decouplers_01\"\n  ],\n  \"TierToUnlock\": 0,\n  \"TechTreePosition\": {{\n    \"x\": 5220.0,\n    \"y\": 310.0\n  }}\n}}")));
        }
    }
}