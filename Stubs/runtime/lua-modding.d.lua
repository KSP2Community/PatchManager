---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/Modding/KSP2ModManager.cs

---Manages discovery, loading, and versioned API tracking for KSP2Mod instances.
---A dot-style module table exposing the mod management callbacks.
---@class KSP2ModManager
local KSP2ModManager = {}

---Loads all currently inactive mods. Returns true on success.
---@return boolean
function KSP2ModManager.LoadMods() end

---Logs the current status of every known mod to the mod log.
function KSP2ModManager.ShowModList() end

---Fires the `OnDebugTest` event on every loaded mod.
function KSP2ModManager.ModEvent() end

---Toggles the visibility of the mod manager dialog.
function KSP2ModManager.ShowModDialog() end
