using System.Collections.Generic;
using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Interfaces
{
    // Essentially the ruleset requires ISelectable
    /// <summary>
    /// A ruleset for the patcher (the ":..." selectors)
    /// </summary>
    public interface IPatcherRuleSet
    {
        /// <summary>
        /// What type of labels will this ruleset match, used for optimization purposes
        /// </summary>
        [CanBeNull]
        public string[] Labels { get; }

        /// <summary>
        /// This converts json data to an ISelectable for the rest of the engine to use
        /// </summary>
        /// <param name="type">The type of data to convert to an ISelectable</param>
        /// <param name="name">The name of the data</param>
        /// <param name="jsonData">The data to convert to an ISelectable</param>
        /// <returns>The selectable representing the data</returns>
        public ISelectable ConvertToSelectable(string type, string name, string jsonData);
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectable"></param>
        /// <returns>If the selectable from the previous patch can be ingested again</returns>
        public bool CanIngestSelectable(ISelectable selectable);
        
        public bool CanGetAssetNameFromSelectableName { get; }

        public string SelectableNameToAssetName(string selectableName);
        
        /// <summary>
        /// Creates a new asset for the patcher
        /// </summary>
        /// <param name="dataValues">The data values to create the asset from</param>
        /// <returns>The new asset</returns>
        public INewAsset CreateNew(List<DataValue> dataValues);
    }
}