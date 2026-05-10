---@meta

---Lua submodule exposed as `PM.Audio`, providing stock part-audio preset helpers.
---@class AudioLuaModule
local AudioLuaModule = {}

---Ensures the hidden PartAudioPreset module contains a binding for the given stock-audio preset.
---@param part PartUserData
---@param presetId string
---@param targetTransformPath? string
---@param overrideExistingAudio? boolean
function AudioLuaModule:EnsurePreset(part, presetId, targetTransformPath, overrideExistingAudio) end
