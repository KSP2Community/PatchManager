---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/CampaignPacks/CampaignPacksLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Generic/JsonConverter.cs
-- Source: Assets/Code/Redux/Packs/CampaignPackManager.cs
-- Source: Assets/Code/Redux/Packs/CampaignPack.cs

---Lua submodule exposed as `PM.CampaignPacks`, providing patches and creation for campaign packs.
---@class CampaignPacksLuaModule
local CampaignPacksLuaModule = {}

---Registers a campaign pack patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<CampaignPackUserData, any> patch The registered patch.
function CampaignPacksLuaModule:Patch(name) end

---Creates a new campaign pack with the given ID and runs callback against it for further
---configuration.
---@param id string The campaign pack ID, which also names the created asset.
---@param callback fun(pack: CampaignPackUserData) Callback that receives the new campaign pack for further configuration.
function CampaignPacksLuaModule:CreateCampaignPack(id, callback) end

---@class CampaignPackUserData : _CampaignPack, JsonUserData

---The data model for a single campaign pack. That can be tied to a campaign.
---@class _CampaignPack : _JsonUserDataBase
---@field CampaignPackId string This is the internal identifier for the campaign pack
---@field CampaignPackLocalizationKey string This is the localization key for the campaign pack
---@field TechTreeLayers JsonList<string> The list of tech tree layers that this campaign pack filters for
---@field MissionLayers JsonList<string> The list of mission layers that this campaign pack filters for
---@field Galaxy string The key of the galaxy definition this campaign pack uses

---@alias CampaignPack _CampaignPack | { CampaignPackId: string, CampaignPackLocalizationKey: string, TechTreeLayers: JsonList<string>, MissionLayers: JsonList<string>, Galaxy: string }
