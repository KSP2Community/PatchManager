---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/Rendering/LightingSettingsLua.cs
-- Source: ksp2redux/Assets/Code/KSP/Rendering/PlanetSettingsLua.cs
-- Source: ksp2redux/Assets/Code/KSP/Rendering/PostProcessingSettingsLua.cs
-- Source: ksp2redux/Assets/Code/KSP/VolumeCloud/lua/VolumeCloudLua.cs

---Exposes lighting graphics settings to a Lua script environment by registering callbacks for atmospheric scattering, global illumination, and reflection parameters.
---DOT-style module table (registered via RegisterScriptObject + SetCallback). Call as LightingDebug.SetAtmosphericScatteringEnabled(true).
---@class LightingDebug
local LightingDebug = {}

---@param enabled boolean
---@return integer
function LightingDebug.SetAtmosphericScatteringEnabled(enabled) end

---@return boolean
function LightingDebug.GetAtmosphericScatteringEnabled() end

---@param enabled boolean
---@return integer
function LightingDebug.SetGlobalIlluminationEnabled(enabled) end

---@return boolean
function LightingDebug.GetGlobalIlluminationEnabled() end

---@param enabled boolean
---@return integer
function LightingDebug.SetGlobalIlluminationObserverEnabled(enabled) end

---@return boolean
function LightingDebug.GetGlobalIlluminationObserverEnabled() end

---@param enabled boolean
---@return integer
function LightingDebug.SetReflectionsEnabled(enabled) end

---@return boolean
function LightingDebug.GetReflectionsEnabled() end

---@param intensity integer
---@return integer
function LightingDebug.SetReflectionIntensity(intensity) end

---@return integer
function LightingDebug.GetReflectionIntensity() end

---@param maxSteps integer
---@return integer
function LightingDebug.SetReflectionMipBiasMaxSteps(maxSteps) end

---@return integer
function LightingDebug.GetReflectionMipBiasMaxSteps() end

---@param rate integer
---@return integer
function LightingDebug.SetReflectionMipBiasGrowthRate(rate) end

---@return integer
function LightingDebug.GetReflectionMipBiasGrowthRate() end

---@param offset integer
---@return integer
function LightingDebug.SetReflectionMipBiasOffset(offset) end

---@return integer
function LightingDebug.GetReflectionMipBiasOffset() end

---Registers planet rendering settings (scatter, collider visibility, and low-quality mode) as callbacks on a Lua script environment.
---DOT-style module table.
---@class PlanetDebug
local PlanetDebug = {}

---@param enabled boolean
---@return integer
function PlanetDebug.SetPlanetScatterEnabled(enabled) end

---@return boolean
function PlanetDebug.GetPlanetScatterEnabled() end

---@param enabled boolean
---@return integer
function PlanetDebug.SetPlanetRenderCollidersEnabled(enabled) end

---@return boolean
function PlanetDebug.GetRenderPlanetCollidersEnabled() end

---@param enabled boolean
---@return integer
function PlanetDebug.SetPlanetLowQualityEnabled(enabled) end

---@return boolean
function PlanetDebug.GetPlanetLowQualityEnabled() end

---The Lua scripting bridge for post-processing and tutorial overlay graphics settings.
---DOT-style module table.
---@class PostProcessingDebug
local PostProcessingDebug = {}

---@param enabled boolean
---@return integer
function PostProcessingDebug.SetPostProcessingEnabled(enabled) end

---@return boolean
function PostProcessingDebug.GetPostProcessingEnabled() end

---@param enabled boolean
---@return integer
function PostProcessingDebug.SetTutorialOverlayEnabled(enabled) end

---@return boolean
function PostProcessingDebug.GetTutorialOverlayEnabled() end

---Lua binding for VolumeCloudRenderer, registering cloud enable state and raymarch step-size callbacks in a script environment.
---DOT-style module table.
---@class VolumeCloud
local VolumeCloud = {}

---@param enableClouds boolean
---@return integer
function VolumeCloud.EnableClouds(enableClouds) end

---@return boolean
function VolumeCloud.IsEnabledClouds() end

---@param stepSize integer
---@return integer
function VolumeCloud.SetRaymarchStepSize(stepSize) end

---@return integer
function VolumeCloud.GetRaymarchStepSize() end
