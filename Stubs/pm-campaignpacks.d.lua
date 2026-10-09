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

---Registers a patch that copies each campaign pack it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the campaign pack to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: CampaignPackUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<CampaignPackUserData, any> patch The registered patch.
function CampaignPacksLuaModule:Duplicate(source, newName, patchMethod) end

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
---@field PartLayers? JsonList<string> The part layers this campaign pack builds its parts from, in order, with the galaxy's part layers stacked on top. Defaults to { "Default" }.
---@field Galaxy string The key of the galaxy definition this campaign pack uses

---@alias CampaignPack _CampaignPack | { CampaignPackId: string, CampaignPackLocalizationKey: string, TechTreeLayers: JsonList<string>, MissionLayers: JsonList<string>, PartLayers?: JsonList<string>, Galaxy: string }
