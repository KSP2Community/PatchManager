---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/Sim/impl/lua/ViewControllerLua.cs

-- View is a dot-style module table registered via RegisterScriptObject("View")
-- plus SetCallback. The nested ActiveVehicle and FlightCamera sub-tables are
-- registered with RegisterScriptObject("Name", View) so they hang off the View
-- global (View.ActiveVehicle, View.FlightCamera). The Vessel / ActiveVessel /
-- NearbyVessels members and the per-vessel handle are installed from Lua by
-- InstallVesselHandleHelpers. All members are plain function fields - call with
-- a dot, never a colon. The View global itself is declared in lua-globals.d.lua.

---Represents a Lua scripting bridge for ViewController, exposing view-layer functionality to a Lua script environment.
---@class View
---The active-vessel convenience table (operates on the current active vessel, no guid argument).
---@field ActiveVehicle ViewActiveVehicle
---The flight-camera control table.
---@field FlightCamera ViewFlightCamera
local View = {}

---Sets the active vessel to the simulation object with the given guid. Returns true when found and activated.
---@param instanceGuid string
---@param controllable? boolean
---@return boolean
function View.SetActiveVessel(instanceGuid, controllable) end

---Returns the guid of the active vehicle, or nil when there is none.
---@return string?
function View.GetActiveVessel() end

---Re-parents tagged game objects whose name starts with childName to the active vessel view object.
---@param childName string
---@param tag string
---@return string
function View.ReParentToActiveVessel(childName, tag) end

---Returns whether the game is currently in the 3D map view.
---@return boolean
function View.MapIsEnabled() end

---Sets the current localization language if the language key is available. Returns true when set.
---@param languageKey string
---@return boolean
function View.SetLanguage(languageKey) end

---Starts a cutscene defined by a list of shot tables { cameraChildName, animChildName, shotLengthSeconds } on the named game object.
---@param gameObjectName string
---@param shotList table[]
---@return boolean
function View.StartCutscene(gameObjectName, shotList) end

---Builds a vessel handle table for the given guid, or nil when guid is empty.
---@param guid string
---@return ViewVesselHandle?
function View.Vessel(guid) end

---Returns a vessel handle for the active vessel, or nil when there is none.
---@return ViewVesselHandle?
function View.ActiveVessel() end

---Returns vessel handles for vessels near the active vessel.
---@return ViewVesselHandle[]
function View.NearbyVessels() end

---The low-level vessel-info form. Prefer the handles from View.Vessel(guid).
---@param guid string
---@return table
function View._VesselGetInfo(guid) end

---@param guid string
---@return table
function View._VesselGetTelemetry(guid) end

---@param guid string
---@return table
function View._VesselGetOrbit(guid) end

---@param guid string
---@return table
function View._VesselGetAero(guid) end

---@param guid string
---@return table
function View._VesselGetGroundClearance(guid) end

---@param guid string
---@return table
function View._VesselGetControls(guid) end

---@param guid string
---@return table
function View._VesselGetControlPoint(guid) end

---@param guid string
---@return table[]
function View._VesselGetNearbyVessels(guid) end

---@param guid string
---@param values table
---@return boolean
function View._VesselSetControls(guid, values) end

---@param guid string
---@param value number
---@return boolean
function View._VesselSetThrottle(guid, value) end

---@param guid string
---@param values table
---@return table
function View._VesselSetAttitude(guid, values) end

---@param guid string
---@param values table
---@return table
function View._VesselSetSAS(guid, values) end

---@param guid string
---@return boolean
function View._VesselStage(guid) end

---@param guid string
---@return number
function View._VesselGetStageFuel(guid) end

---@param guid string
---@return ActionGroupStates?
function View._VesselGetActionGroups(guid) end

---@param guid string
---@param group KSPActionGroup
---@return boolean?
function View._VesselGetActionGroup(guid, group) end

---@param guid string
---@param group KSPActionGroup
---@param value boolean
---@param callback? fun(guid: string)
---@return boolean
function View._VesselSetActionGroup(guid, group, value, callback) end

---@param guid string
---@param options table
---@return table[]
function View._VesselGetResources(guid, options) end

---@param guid string
---@param options table
---@return table[]
function View._VesselGetParts(guid, options) end

---@param guid string
---@param ut number
---@param vector table
---@return string
function View._VesselCreateManeuverNode(guid, ut, vector) end

---@param guid string
---@param nodeId string
---@return table
function View._VesselGetManeuverNode(guid, nodeId) end

---@param guid string
---@return table[]
function View._VesselGetManeuverNodes(guid) end

---@param guid string
---@return table
function View._VesselGetNextManeuverNode(guid) end

---@param guid string
---@param nodeId string
---@param ut number
---@return table
function View._VesselSetManeuverNodeTime(guid, nodeId, ut) end

---@param guid string
---@param nodeId string
---@param vector table
---@return table
function View._VesselSetManeuverNodeVector(guid, nodeId, vector) end

---@param guid string
---@param nodeId string
---@param vector table
---@return table
function View._VesselAddManeuverNodeVector(guid, nodeId, vector) end

---@param guid string
---@param nodeId string
---@return boolean
function View._VesselWarpToManeuverNode(guid, nodeId) end

---@param guid string
---@param nodeId string
---@return boolean
function View._VesselRemoveManeuverNode(guid, nodeId) end

---@param guid string
---@return SpeedDisplayMode?
function View._VesselGetSpeedDisplayMode(guid) end

---@param guid string
---@param mode SpeedDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function View._VesselSetSpeedDisplayMode(guid, mode, callback) end

---@param guid string
---@return AltimeterDisplayMode?
function View._VesselGetAltimeterDisplayMode(guid) end

---@param guid string
---@param mode AltimeterDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function View._VesselSetAltimeterDisplayMode(guid, mode, callback) end

---FlightControlsMode has no interop converter, so it crosses untyped.
---@param guid string
---@return any
function View._VesselGetFlightControlsMode(guid) end

---@param guid string
---@param mode any
---@param callback? fun(guid: string)
---@return boolean
function View._VesselSetFlightControlsMode(guid, mode, callback) end

-- The active-vessel table (View.ActiveVehicle). Same surface as the per-vessel
-- handle but operating on the current active vessel, with no guid argument.
---@class ViewActiveVehicle
local ViewActiveVehicle = {}

---@return string?
function ViewActiveVehicle.GetGuid() end

---@return ActionGroupStates?
function ViewActiveVehicle.GetActionGroups() end

---@param group KSPActionGroup
---@return boolean?
function ViewActiveVehicle.GetActionGroup(group) end

---@param group KSPActionGroup
---@param value boolean
---@param callback? fun(guid: string)
---@return boolean
function ViewActiveVehicle.SetActionGroup(group, value, callback) end

---@return SpeedDisplayMode?
function ViewActiveVehicle.GetSpeedDisplayMode() end

---@param speedMode SpeedDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function ViewActiveVehicle.SetSpeedDisplayMode(speedMode, callback) end

---@return AltimeterDisplayMode?
function ViewActiveVehicle.GetAltimeterDisplayMode() end

---@param altimeterMode AltimeterDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function ViewActiveVehicle.SetAltimeterDisplayMode(altimeterMode, callback) end

---FlightControlsMode has no interop converter, so it crosses untyped.
---@return any
function ViewActiveVehicle.GetFlightControlsMode() end

---@param controlsMode any
---@param callback? fun(guid: string)
---@return boolean
function ViewActiveVehicle.SetFlightControlsMode(controlsMode, callback) end

---@return table
function ViewActiveVehicle.GetInfo() end

---@return table
function ViewActiveVehicle.GetTelemetry() end

---@return table
function ViewActiveVehicle.GetOrbit() end

---@return table
function ViewActiveVehicle.GetAero() end

---@return table
function ViewActiveVehicle.GetGroundClearance() end

---@return table
function ViewActiveVehicle.GetControls() end

---@return table
function ViewActiveVehicle.GetControlPoint() end

---@param values table
---@return boolean
function ViewActiveVehicle.SetControls(values) end

---@param value number
---@return boolean
function ViewActiveVehicle.SetThrottle(value) end

---@param values table
---@return table
function ViewActiveVehicle.SetAttitude(values) end

---@return boolean
function ViewActiveVehicle.Stage() end

---@param values table
---@return table
function ViewActiveVehicle.SetSAS(values) end

---@return number
function ViewActiveVehicle.GetStageFuel() end

---@param options table
---@return table[]
function ViewActiveVehicle.GetResources(options) end

---@param options table
---@return table[]
function ViewActiveVehicle.GetParts(options) end

---@param ut number
---@param vector table
---@return string
function ViewActiveVehicle.CreateManeuverNode(ut, vector) end

---@param nodeId string
---@return table
function ViewActiveVehicle.GetManeuverNode(nodeId) end

---@return table[]
function ViewActiveVehicle.GetManeuverNodes() end

---@return table
function ViewActiveVehicle.GetNextManeuverNode() end

---@param nodeId string
---@param ut number
---@return table
function ViewActiveVehicle.SetManeuverNodeTime(nodeId, ut) end

---@param nodeId string
---@param vector table
---@return table
function ViewActiveVehicle.SetManeuverNodeVector(nodeId, vector) end

---@param nodeId string
---@param vector table
---@return table
function ViewActiveVehicle.AddManeuverNodeVector(nodeId, vector) end

---@param nodeId string
---@return boolean
function ViewActiveVehicle.WarpToManeuverNode(nodeId) end

---@param nodeId string
---@return boolean
function ViewActiveVehicle.RemoveManeuverNode(nodeId) end

-- The flight-camera control table (View.FlightCamera).
---@class ViewFlightCamera
local ViewFlightCamera = {}

---@return CameraMode
function ViewFlightCamera.GetMode() end

---Sets the flight-camera mode and returns the resulting mode.
---@param mode CameraMode
---@return CameraMode
function ViewFlightCamera.SetMode(mode) end

---@return CameraMode[]
function ViewFlightCamera.GetAvailableModes() end

---Sets the gimbal distance and returns the resulting distance.
---@param distance number
---@return number
function ViewFlightCamera.SetDistance(distance) end

---@return number
function ViewFlightCamera.GetDistance() end

---Sets the gimbal heading and returns the resulting heading.
---@param heading number
---@return number
function ViewFlightCamera.SetHeading(heading) end

---@return number
function ViewFlightCamera.GetHeading() end

---Sets the gimbal pitch and returns the resulting pitch.
---@param pitch number
---@return number
function ViewFlightCamera.SetPitch(pitch) end

---@return number
function ViewFlightCamera.GetPitch() end

---Sets the field of view and returns the resulting field of view.
---@param fieldOfView number
---@return number
function ViewFlightCamera.SetFieldOfView(fieldOfView) end

---@return number
function ViewFlightCamera.GetFieldOfView() end

---Resets the flight camera gimbal and camera to defaults.
function ViewFlightCamera.RevertToDefaults() end

-- A per-vessel handle built by View.Vessel(guid). Carries the bound guid and id
-- plus the methods below (each forwards to the matching View._Vessel* callback).
---@class ViewVesselHandle
---@field guid string
---@field id string
local ViewVesselHandle = {}

---@return string
function ViewVesselHandle.GetGuid() end

---@return table
function ViewVesselHandle.GetInfo() end

---@return table
function ViewVesselHandle.GetTelemetry() end

---@return table
function ViewVesselHandle.GetOrbit() end

---@return table
function ViewVesselHandle.GetAero() end

---@return table
function ViewVesselHandle.GetGroundClearance() end

---@return table
function ViewVesselHandle.GetControls() end

---@return table
function ViewVesselHandle.GetControlPoint() end

---Returns handles for vessels near this vessel.
---@return ViewVesselHandle[]
function ViewVesselHandle.NearbyVessels() end

---@param values table
---@return boolean
function ViewVesselHandle.SetControls(values) end

---@param value number
---@return boolean
function ViewVesselHandle.SetThrottle(value) end

---@param values table
---@return table
function ViewVesselHandle.SetAttitude(values) end

---@param values table
---@return table
function ViewVesselHandle.SetSAS(values) end

---@return boolean
function ViewVesselHandle.Stage() end

---@return number
function ViewVesselHandle.GetStageFuel() end

---@return ActionGroupStates?
function ViewVesselHandle.GetActionGroups() end

---@param group KSPActionGroup
---@return boolean?
function ViewVesselHandle.GetActionGroup(group) end

---@param group KSPActionGroup
---@param value boolean
---@param callback? fun(guid: string)
---@return boolean
function ViewVesselHandle.SetActionGroup(group, value, callback) end

---@param options? table
---@return table[]
function ViewVesselHandle.GetResources(options) end

---@param options? table
---@return table[]
function ViewVesselHandle.GetParts(options) end

---@param ut number
---@param vector table
---@return string
function ViewVesselHandle.CreateManeuverNode(ut, vector) end

---@param nodeId string
---@return table
function ViewVesselHandle.GetManeuverNode(nodeId) end

---@return table[]
function ViewVesselHandle.GetManeuverNodes() end

---@return table
function ViewVesselHandle.GetNextManeuverNode() end

---@param nodeId string
---@param ut number
---@return table
function ViewVesselHandle.SetManeuverNodeTime(nodeId, ut) end

---@param nodeId string
---@param vector table
---@return table
function ViewVesselHandle.SetManeuverNodeVector(nodeId, vector) end

---@param nodeId string
---@param vector table
---@return table
function ViewVesselHandle.AddManeuverNodeVector(nodeId, vector) end

---@param nodeId string
---@return boolean
function ViewVesselHandle.WarpToManeuverNode(nodeId) end

---@param nodeId string
---@return boolean
function ViewVesselHandle.RemoveManeuverNode(nodeId) end

---@return SpeedDisplayMode?
function ViewVesselHandle.GetSpeedDisplayMode() end

---@param mode SpeedDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function ViewVesselHandle.SetSpeedDisplayMode(mode, callback) end

---@return AltimeterDisplayMode?
function ViewVesselHandle.GetAltimeterDisplayMode() end

---@param mode AltimeterDisplayMode
---@param callback? fun(guid: string)
---@return boolean
function ViewVesselHandle.SetAltimeterDisplayMode(mode, callback) end

---FlightControlsMode has no interop converter, so it crosses untyped.
---@return any
function ViewVesselHandle.GetFlightControlsMode() end

---@param mode any
---@param callback? fun(guid: string)
---@return boolean
function ViewVesselHandle.SetFlightControlsMode(mode, callback) end
