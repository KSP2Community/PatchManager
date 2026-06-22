---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/Root/CheatSystem.cs
-- Source: ksp2redux/Assets/Code/Root/DebugVisualizer.cs
-- Source: ksp2redux/Assets/Code/KSP/DebugTools/QuickActionDebugTools.cs
-- Source: ksp2redux/Assets/Code/KSP/DebugTools/FXDebugTools.cs

---Manages runtime cheat and debug options, exposing toggles for physics, aerodynamics, thermodynamics, part unlocking, and resource cheats.
---DOT-style module table (registered via RegisterScriptObject + SetCallback). Call as Cheats.SetNoCrashDamage(true).
---@class Cheats
local Cheats = {}

function Cheats.QuickDebugEnable() end

function Cheats.QuickDebugRestore() end

---@param noCrashDamage boolean
---@return boolean
function Cheats.SetNoCrashDamage(noCrashDamage) end

---@return boolean
function Cheats.GetNoCrashDamage() end

---@param infinitePropellant boolean
---@return boolean
function Cheats.SetInfinitePropellant(infinitePropellant) end

---@return boolean
function Cheats.GetInfinitePropellant() end

---@param infiniteElectricity boolean
---@return boolean
function Cheats.SetInfiniteElectricity(infiniteElectricity) end

---@return boolean
function Cheats.GetInfiniteElectricity() end

---@param unlockAllParts boolean
---@return boolean
function Cheats.SetUnlockAllParts(unlockAllParts) end

---@return boolean
function Cheats.GetUnlockAllParts() end

---@param disableGravity boolean
---@return boolean
function Cheats.SetDisableGravity(disableGravity) end

---@return boolean
function Cheats.GetDisableGravity() end

---@param disableAerodynamics boolean
---@return boolean
function Cheats.SetDisableAerodynamics(disableAerodynamics) end

---@return boolean
function Cheats.GetDisableAerodynamics() end

---@param disableThermodynamics boolean
---@return boolean
function Cheats.SetDisableThermodynamics(disableThermodynamics) end

---@return boolean
function Cheats.GetDisableThermodynamics() end

function Cheats.ResetAllValues() end

---@param ignoreMaxTemp boolean
---@return boolean
function Cheats.SetIgnoreMaxTemp(ignoreMaxTemp) end

---@return boolean
function Cheats.GetIgnoreMaxTemp() end

---Manages in-game debug visualizations and exposes debug tool toggles to the script environment.
---DOT-style module table.
---@class DebugVisualizer
local DebugVisualizer = {}

---@param show boolean
---@return integer
function DebugVisualizer.ShowFrameTimes(show) end

---@return boolean
function DebugVisualizer.GetShowFrameTimes() end

---@param textPos string
---@return boolean
function DebugVisualizer.SetTextPosition(textPos) end

---@return boolean
function DebugVisualizer.ToggleLocalizationDebugMode() end

---@return boolean
function DebugVisualizer.QAPrintPartsInBuildersCSVs() end

function DebugVisualizer.ToggleVesselTools() end

function DebugVisualizer.ToggleVesselScience() end

function DebugVisualizer.ToggleKerbalTools() end

function DebugVisualizer.ToggleScienceTools() end

function DebugVisualizer.ToggleScienceExperimentResearchReportTools() end

function DebugVisualizer.ToggleSimObjectTools() end

function DebugVisualizer.ToggleOABAssemblyOverlay() end

function DebugVisualizer.ToggleOABSizeLimits() end

function DebugVisualizer.ToggleFXTools() end

function DebugVisualizer.ToggleRenderingTools() end

function DebugVisualizer.ToggleTerrainTools() end

function DebugVisualizer.TogglePlanetViewer() end

function DebugVisualizer.ToggleTeleportWindow() end

function DebugVisualizer.ToggleTeleportBookmarkWalkWindow() end

function DebugVisualizer.ToggleLogConsole() end

function DebugVisualizer.ToggleThermalDebugTool() end

function DebugVisualizer.ToggleVesselCoordinateLocation() end

---@param text string
---@param isTextOnly boolean
function DebugVisualizer.PassiveTierNotification(text, isTextOnly) end

---@param titleText string
---@param firstLineItemtext string
---@param secondLineItemtext string
---@param thirdLineItemtext string
---@param timerDuration number
---@param isHighLevelOfImportance boolean
---@param isMediumLevelOfImportance boolean
function DebugVisualizer.AlertTierNotification(titleText, firstLineItemtext, secondLineItemtext, thirdLineItemtext, timerDuration, isHighLevelOfImportance, isMediumLevelOfImportance) end

---@param titleContentText string
---@param bodyContentText string
---@param isTextOnly boolean
---@param isTextAndIcon boolean
function DebugVisualizer.AdminTierNotification(titleContentText, bodyContentText, isTextOnly, isTextAndIcon) end

---@param enableTimer boolean
function DebugVisualizer.EnableNotificationTimer(enableTimer) end

function DebugVisualizer.ToggleShowDebugLoginButtons() end

function DebugVisualizer.PlaceKerbal() end

function DebugVisualizer.RestartLegalAcceptance() end

---@param hidden boolean
function DebugVisualizer.ToggleNotificationsCanvasVisibility(hidden) end

function DebugVisualizer.ToggleAuthenticationInfo() end

function DebugVisualizer.ToggleVesselJointsAndModes() end

function DebugVisualizer.ToggleRenderDragBounds() end

---@param show boolean
---@return integer
function DebugVisualizer.ShowTextureStreamingStats(show) end

---@return boolean
function DebugVisualizer.GetShowTextureStreamingStats() end

---@param enabled boolean
---@return integer
function DebugVisualizer.SetTextureStreaming(enabled) end

---@return boolean
function DebugVisualizer.GetTextureStreaming() end

---@param newValue integer
---@return integer
function DebugVisualizer.SetTextureStreamingBudget(newValue) end

---@return integer
function DebugVisualizer.GetTextureStreamingBudget() end

---API for registering quick-action debug script callbacks that spawn vessels in various game states.
---DOT-style module table.
---@class QuickActions
local QuickActions = {}

function QuickActions.ShipInVAB() end

function QuickActions.ShipOnLaunchpad() end

function QuickActions.ShipInOrbit() end

function QuickActions.PlaneInVAB() end

function QuickActions.PlaneOnRunway() end

function QuickActions.VABSurface() end

function QuickActions.VABOrbit() end

function QuickActions.BAESurface() end

function QuickActions.BAEOrbit() end

---A debug window for inspecting and controlling visual effects on simulation parts and vessels.
---DOT-style module table.
---@class FXDebugTools
local FXDebugTools = {}

function FXDebugTools.ShowVFXTestSuite() end

---@param show boolean
function FXDebugTools.SetShowAlternateReentryCalc(show) end

---@return boolean
function FXDebugTools.GetShowAlternateReentryCalc() end

---@param show boolean
function FXDebugTools.SetShowExplosionFX(show) end

---@return boolean
function FXDebugTools.GetShowExplosionFX() end

---@param show boolean
function FXDebugTools.SetShowEngineFX(show) end

---@return boolean
function FXDebugTools.GetShowEngineFX() end

---@param show boolean
function FXDebugTools.SetShowDetachFX(show) end

---@return boolean
function FXDebugTools.GetShowDetachFX() end

---@param show boolean
function FXDebugTools.SetShowSurfaceImpactFX(show) end

---@return boolean
function FXDebugTools.GetShowSurfaceImpactFX() end

---@param show boolean
function FXDebugTools.SetShowGroundBlastFX(show) end

---@return boolean
function FXDebugTools.GetShowGroundBlastFX() end

---@param show boolean
function FXDebugTools.SetShowShockConeFX(show) end

---@return boolean
function FXDebugTools.GetShowShockConeFX() end

---@param show boolean
function FXDebugTools.SetShowContrailsFX(show) end

---@return boolean
function FXDebugTools.GetShowContrailsFX() end

---@param show boolean
function FXDebugTools.SetShowWingtipVorticesFX(show) end

---@return boolean
function FXDebugTools.GetShowWingtipVorticesFX() end

---@param show boolean
function FXDebugTools.SetShowReentryFX(show) end

---@return boolean
function FXDebugTools.GetShowReentryFX() end

---@param show boolean
function FXDebugTools.SetShowPersistantSurfaceContactFX(show) end

---@return boolean
function FXDebugTools.GetShowPersistantSurfaceContactFX() end

---@param show boolean
function FXDebugTools.SetShowWheelSurfaceFX(show) end

---@return boolean
function FXDebugTools.GetShowWheelSurfaceFX() end

---@param show boolean
function FXDebugTools.SetShowKerbalPoofFX(show) end

---@return boolean
function FXDebugTools.GetShowKerbalPoofFX() end

---@param show boolean
function FXDebugTools.SetShowKerbalFootstepFX(show) end

---@return boolean
function FXDebugTools.GetShowKerbalFootstepFX() end

---@param show boolean
function FXDebugTools.SetShowLaunchSmokeFX(show) end

---@return boolean
function FXDebugTools.GetShowLaunchSmokeFX() end

---@param show boolean
function FXDebugTools.SetShowLaunchFireFX(show) end

---@return boolean
function FXDebugTools.GetShowLaunchFireFX() end

---@param show boolean
function FXDebugTools.SetShowLaunchFuelTankIceFX(show) end

---@return boolean
function FXDebugTools.GetShowLaunchFuelTankIceFX() end
