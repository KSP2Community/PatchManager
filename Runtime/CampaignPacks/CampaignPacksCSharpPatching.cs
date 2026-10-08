using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;
using Redux.Packs;

namespace PatchManager.CampaignPacks
{
    /// <summary>Patches campaign packs, mirroring PM.CampaignPacks.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchCampaignPackAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "JSON";

        /// <inheritdoc />
        public override string Label => CampaignPackManager.CAMPAIGN_PACK_LABEL;
    }

    /// <summary>Fluent PM extensions for campaign pack patches.</summary>
    public static class CampaignPacksPatchExtensions
    {
        /// <summary>Registers a campaign pack patch.</summary>
        public static PatchBuilder<JsonUserData> PatchCampaignPack(this PmScope scope, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "JSON", CampaignPackManager.CAMPAIGN_PACK_LABEL, name);

        /// <summary>Registers a patch that copies each matching asset under a new name in the same label. Do runs on the copy.</summary>
        public static PatchBuilder<JsonUserData> DuplicateCampaignPack(this PmScope scope, string source, string newName)
            => Patching.Duplicate<JsonUserData>(scope.ModId, "JSON", CampaignPackManager.CAMPAIGN_PACK_LABEL, source, newName);
    }
}
