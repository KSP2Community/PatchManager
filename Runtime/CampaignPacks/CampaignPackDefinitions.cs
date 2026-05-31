using System.Collections.Generic;

namespace PatchManager.CampaignPacks
{
    /// <summary>
    /// Runtime JSON-facing representation of a campaign pack.
    /// </summary>
    public sealed class CampaignPackDefinition
    {
        /// <summary>
        /// Stable campaign pack identifier used by other campaign pack definitions and extensions.
        /// </summary>
        public string Id = string.Empty;

        /// <summary>
        /// Localization key used for the player-facing campaign pack name.
        /// </summary>
        public string NameLocKey = string.Empty;

        /// <summary>
        /// Localization key used for the player-facing campaign pack description.
        /// </summary>
        public string DescriptionLocKey = string.Empty;

        /// <summary>
        /// Addressable or data key of the galaxy definition used by this campaign pack.
        /// </summary>
        public string GalaxyDefinitionKey = string.Empty;

        /// <summary>
        /// Optional identifier of the tech tree set referenced by this campaign pack.
        /// </summary>
        public string TechTreeSetId = string.Empty;

        /// <summary>
        /// Optional identifier of the mission set referenced by this campaign pack.
        /// </summary>
        public string MissionSetId = string.Empty;

        /// <summary>
        /// Optional identifier of the science set referenced by this campaign pack.
        /// </summary>
        public string ScienceSetId = string.Empty;
    }

    /// <summary>
    /// Runtime JSON-facing representation of a tech tree set.
    /// </summary>
    public sealed class TechTreeSetDefinition
    {
        /// <summary>
        /// Stable tech tree set identifier.
        /// </summary>
        public string Id = string.Empty;

        /// <summary>
        /// Tech tree node identifiers included by this set before extensions are applied.
        /// </summary>
        public List<string> TechNodeIds = new();
    }

    /// <summary>
    /// Runtime JSON-facing representation of a mission set.
    /// </summary>
    public sealed class MissionSetDefinition
    {
        /// <summary>
        /// Stable mission set identifier.
        /// </summary>
        public string Id = string.Empty;

        /// <summary>
        /// Mission identifiers included by this set before extensions are applied.
        /// </summary>
        public List<string> MissionIds = new();
    }

    /// <summary>
    /// Runtime JSON-facing representation of a science set.
    /// </summary>
    public sealed class ScienceSetDefinition
    {
        /// <summary>
        /// Stable science set identifier.
        /// </summary>
        public string Id = string.Empty;

        /// <summary>
        /// Science experiment identifiers included by this set before extensions are applied.
        /// </summary>
        public List<string> ExperimentIds = new();

        /// <summary>
        /// Science region identifiers included by this set before extensions are applied.
        /// </summary>
        public List<string> ScienceRegionIds = new();

        /// <summary>
        /// Discoverable identifiers included by this set before extensions are applied.
        /// </summary>
        public List<string> DiscoverableIds = new();
    }

    /// <summary>
    /// Runtime JSON-facing representation of campaign pack extension add/remove operations.
    /// </summary>
    public sealed class CampaignPackExtensionDefinition
    {
        /// <summary>
        /// Stable campaign pack extension identifier.
        /// </summary>
        public string Id = string.Empty;

        /// <summary>
        /// Optional campaign pack identifier this extension applies to.
        /// </summary>
        public string TargetCampaignPackId = string.Empty;

        /// <summary>
        /// Optional tech tree set identifier this extension applies to.
        /// </summary>
        public string TargetTechTreeSetId = string.Empty;

        /// <summary>
        /// Optional mission set identifier this extension applies to.
        /// </summary>
        public string TargetMissionSetId = string.Empty;

        /// <summary>
        /// Optional science set identifier this extension applies to.
        /// </summary>
        public string TargetScienceSetId = string.Empty;

        /// <summary>
        /// Tech tree node identifiers added to matching campaign packs.
        /// </summary>
        public List<string> AddTechNodeIds = new();

        /// <summary>
        /// Tech tree node identifiers removed from matching campaign packs after additions are applied.
        /// </summary>
        public List<string> RemoveTechNodeIds = new();

        /// <summary>
        /// Mission identifiers added to matching campaign packs.
        /// </summary>
        public List<string> AddMissionIds = new();

        /// <summary>
        /// Mission identifiers removed from matching campaign packs after additions are applied.
        /// </summary>
        public List<string> RemoveMissionIds = new();

        /// <summary>
        /// Science experiment identifiers added to matching campaign packs.
        /// </summary>
        public List<string> AddExperimentIds = new();

        /// <summary>
        /// Science experiment identifiers removed from matching campaign packs after additions are applied.
        /// </summary>
        public List<string> RemoveExperimentIds = new();

        /// <summary>
        /// Science region identifiers added to matching campaign packs.
        /// </summary>
        public List<string> AddScienceRegionIds = new();

        /// <summary>
        /// Science region identifiers removed from matching campaign packs after additions are applied.
        /// </summary>
        public List<string> RemoveScienceRegionIds = new();

        /// <summary>
        /// Discoverable identifiers added to matching campaign packs.
        /// </summary>
        public List<string> AddDiscoverableIds = new();

        /// <summary>
        /// Discoverable identifiers removed from matching campaign packs after additions are applied.
        /// </summary>
        public List<string> RemoveDiscoverableIds = new();
    }

    /// <summary>
    /// Fully resolved campaign pack contents after base sets and matching extensions are applied.
    /// </summary>
    public sealed class EffectiveCampaignPack
    {
        /// <summary>
        /// Identifier of the campaign pack that was resolved.
        /// </summary>
        public string CampaignPackId = string.Empty;

        /// <summary>
        /// Localization key used for the resolved campaign pack name.
        /// </summary>
        public string NameLocKey = string.Empty;

        /// <summary>
        /// Localization key used for the resolved campaign pack description.
        /// </summary>
        public string DescriptionLocKey = string.Empty;

        /// <summary>
        /// Galaxy definition key selected by the resolved campaign pack.
        /// </summary>
        public string GalaxyDefinitionKey = string.Empty;

        /// <summary>
        /// Effective tech tree node identifiers after base sets and extensions are applied.
        /// </summary>
        public List<string> TechNodeIds = new();

        /// <summary>
        /// Effective mission identifiers after base sets and extensions are applied.
        /// </summary>
        public List<string> MissionIds = new();

        /// <summary>
        /// Effective science experiment identifiers after base sets and extensions are applied.
        /// </summary>
        public List<string> ExperimentIds = new();

        /// <summary>
        /// Effective science region identifiers after base sets and extensions are applied.
        /// </summary>
        public List<string> ScienceRegionIds = new();

        /// <summary>
        /// Effective discoverable identifiers after base sets and extensions are applied.
        /// </summary>
        public List<string> DiscoverableIds = new();

        /// <summary>
        /// Identifiers of extensions that matched and were applied while resolving this campaign pack.
        /// </summary>
        public List<string> AppliedExtensionIds = new();
    }
}
