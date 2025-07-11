using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Generic.SassyPatching.Rulesets
{
    /// <summary>
    /// This is a generic json patching ruleset
    /// </summary>
    [PatcherRuleset("json")]
    public class JsonRuleset : IPatcherRuleSet
    {
        /// <inheritdoc />
        public bool Matches(string label)
        {
            return true;
        }

        public string[] Labels => null;

        /// <inheritdoc />
        public ISelectable ConvertToSelectable(string type, string name, string jsonData)
        {
            return new JTokenSelectable(() => { }, JToken.Parse(jsonData), name, type);
        }

        public bool CanIngestSelectable(ISelectable selectable) => selectable is JTokenSelectable;

        public bool CanGetAssetNameFromSelectableName => true;

        public string[] SelectableNameToAssetName(string selectableName) => new [] {selectableName};

        /// <inheritdoc />
        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var label = dataValues[0].String;
            var name = dataValues[1].String;
            return new NewGenericAsset(label, name, new JTokenSelectable(() => { }, new JObject(), name, label));
        }
    }
}