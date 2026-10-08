using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using Redux.Packs;

namespace PatchManager.CampaignPacks;

/// <summary>
/// Lua submodule exposed as <c>PM.CampaignPacks</c>, providing patches and creation for campaign packs.
/// </summary>
[PatchManagerModule("CampaignPacks")]
[MoonSharpUserData]
public class CampaignPacksLuaModule
{
    private PatchManagerCore _core;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public CampaignPacksLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
    }

    /// <summary>
    /// Registers a campaign pack patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every campaign pack by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Patch(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context, "JSON", CampaignPackManager.CAMPAIGN_PACK_LABEL, name);
    }

    /// <summary>
    /// Creates a new campaign pack with the given ID and runs <paramref name="callback" /> against it for further
    /// configuration.
    /// </summary>
    /// <remarks>
    /// The new pack starts from the <see cref="CampaignPack" /> defaults, so it uses the default galaxy and layers
    /// until the callback changes them.
    /// </remarks>
    /// <param name="id">The campaign pack ID, which also names the created asset.</param>
    /// <param name="callback">Callback that receives the new campaign pack for further configuration.</param>
    public void CreateCampaignPack(string id, Action<JsonUserData> callback)
    {
        var data = new CampaignPack
        {
            CampaignPackId = id,
            CampaignPackLocalizationKey = $"CampaignPacks/{id}"
        };
        var ud = JsonUserData.GetFromJToken(JObject.FromObject(data));
        callback((JsonUserData)ud.UserData.Object);
        _core.New("JSON", CampaignPackManager.CAMPAIGN_PACK_LABEL, id, ud);
    }
}
