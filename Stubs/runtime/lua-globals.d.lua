---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Lifecycle/ModScriptRuntime.cs

--#region SpaceWarp core

---The SpaceWarp library global. Registers lifecycle closures and named loggers.
---@type SwLibrary
SW = nil

---The calling mod's default logger.
---@type ModLogger
Log = nil

---Exposes mod values and functions to the in-game console.
---@type ConsoleLibrary
Console = nil

---The calling mod's id.
---@type string
ModId = nil

---The calling mod's location on disk, or nil for the console environment.
---@type string?
Location = nil

--#endregion

--#region Config

---The calling mod's config namespace. Define, get, and reach other mods' config entries.
---@type ModConfig
Config = nil

---The string config type, passed to a config builder's Type method.
---@type StringType
String = nil

---The integer config type, passed to a config builder's Type method.
---@type IntegerType
Integer = nil

---The floating-point config type, passed to a config builder's Type method.
---@type DoubleType
Double = nil

---The boolean config type, passed to a config builder's Type method.
---@type BooleanType
Boolean = nil

---The color config type, passed to a config builder's Type method.
---@type ColorType
Color = nil

--#endregion

--#region Lifecycle

---Registers live-game lifecycle closures (Update, scene callbacks, coroutines).
---@type GameLifecycleLibrary
Game = nil

---Maps each high-level game-state name to its numeric value, for comparison against scene callback arguments.
---@type GameState
GameState = nil

--#endregion

--#region Stock game globals

---View-layer scripting bridge: active vessel, flight camera, and vessel queries.
---@type View
View = nil

---Space-simulation scripting bridge: teleport, debug, and Hand of God forces.
---@type SpaceSimulation
SpaceSimulation = nil

---Builds lightweight in-game windows, controls, and dialogs from a script.
---@type LuaUIMgr
UI = nil

---Mission scripting interface: activation and state-management callbacks.
---@type KSP2MissionLuaInterface
Missions = nil

---Colony scripting bridge: colony debug and test callbacks.
---@type ColonyManager
Colonies = nil

---Mod management scripting bridge: discovery, loading, and version tracking.
---@type KSP2ModManager
Modding = nil

--#endregion

--#region Debug and dev tools

---Lighting graphics debug settings: atmospheric scattering, global illumination, reflections.
---@type LightingDebug
LightingDebug = nil

---Planet rendering debug settings: scatter, collider visibility, low-quality mode.
---@type PlanetDebug
PlanetDebug = nil

---Post-processing and tutorial overlay debug settings.
---@type PostProcessingDebug
PostProcessingDebug = nil

---Volumetric cloud debug settings: enable state and raymarch step size.
---@type VolumeCloud
VolumeCloud = nil

---Runtime cheat toggles: physics, aerodynamics, thermodynamics, part unlocking, resources.
---@type Cheats
Cheats = nil

---In-game debug visualizations and debug tool toggles.
---@type DebugVisualizer
DebugVisualizer = nil

---Quick-action debug callbacks that spawn vessels in various game states.
---@type QuickActions
QuickActions = nil

---Debug window for inspecting and controlling visual effects on parts and vessels.
---@type FXDebugTools
FXDebugTools = nil

--#endregion

--#region Script utilities

---Keyboard input queries for the current frame.
---@type Input
Input = nil

---The calling script's own identity, logging, waits, and file access.
---@type Script
Script = nil

--#endregion
