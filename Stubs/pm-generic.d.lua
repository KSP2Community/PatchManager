---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/Generic/GenericLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Generic/JsonConverter.cs

---Lua submodule exposed as `PM.JSON`, providing label-untyped patch and asset-creation helpers backed by
---the generic `JSON` converter.
---@class GenericLuaModule
local GenericLuaModule = {}

---Registers a JSON patch under label with the given namespaced patch name.
---@param label string The addressables label to patch.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<JsonUserData, any> patch The registered patch.
function GenericLuaModule:Patch(label, name) end

---Queues a brand-new JSON asset for creation under the given label and name.
---@param label string The addressables label to tag the asset with.
---@param name string The asset's addressables address.
---@param value any The asset's Lua-facing value (typically a JsonUserData wrapping a `JObject` or `JArray`).
function GenericLuaModule:New(label, name, value) end
