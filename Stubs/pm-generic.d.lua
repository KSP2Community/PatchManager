---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Generic/GenericLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Generic/JsonConverter.cs

---Lua submodule exposed as `PM.JSON`, providing label-untyped patch and asset-creation helpers backed by
---the generic `JSON` converter.
---@class GenericLuaModule
local GenericLuaModule = {}

---Registers a JSON patch that runs against every asset under label.
---@param label string                              The addressables label to patch.
---@param callback fun(data: JsonUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                 The registered patch.
function GenericLuaModule:PatchAll(label, callback) end

---Registers a JSON patch that runs against assets under label whose name matches name.
---@param label string                              The addressables label to patch.
---@param name string                               The asset name pattern (supports `*` and `?` wildcards).
---@param callback fun(data: JsonUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                 The registered patch.
function GenericLuaModule:Patch(label, name, callback) end

---Queues a brand-new JSON asset for creation under the given label and name.
---@param label string The addressables label to tag the asset with.
---@param name string  The asset's addressables address.
---@param value any    The asset's Lua-facing value (typically a JsonUserData wrapping a `JObject` or `JArray`).
function GenericLuaModule:New(label, name, value) end
