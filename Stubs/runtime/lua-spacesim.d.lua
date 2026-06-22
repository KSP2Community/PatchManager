---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/Sim/impl/lua/SpaceSimulationLua.cs

-- SpaceSimulation is a dot-style module table registered via
-- RegisterScriptObject("SpaceSimulation") plus SetCallback. The nested Debug and
-- HOG (Hand of God) sub-tables are registered with
-- RegisterScriptObject("Name", SpaceSimulation) so they hang off the parent
-- (SpaceSimulation.Debug, SpaceSimulation.HOG). All members are plain function
-- fields - call with a dot, never a colon. The SpaceSimulation global itself is
-- declared in lua-globals.d.lua.

---Lua scripting bridge that registers SpaceSimulation teleport, debug, and Hand of God force callbacks on an IScriptEnvironment.
---@class SpaceSimulation
---Debug and timing sub-table.
---@field Debug SpaceSimulationDebug
---Hand of God force/torque control sub-table.
---@field HOG SpaceSimulationHOG
local SpaceSimulation = {}

---Teleports a simulation object to a rendezvous near a target, offset by a local distance and rotation (degrees).
---@param guid string
---@param target string
---@param distanceX number
---@param distanceY number
---@param distanceZ number
---@param pitchDeg number
---@param yawDeg number
---@param rollDeg number
---@return boolean
function SpaceSimulation.TeleportToRendezvous(guid, target, distanceX, distanceY, distanceZ, pitchDeg, yawDeg, rollDeg) end

---Teleports a simulation object onto a Kepler orbit around the given celestial body.
---@param guid string
---@param celestialBodyGuid string
---@param inclination number
---@param eccentricity number
---@param semiMajorAxis number
---@param longitudeOfAscendingNode number
---@param argumentOfPeriapsis number
---@param meanAnomalyAtEpoch number
---@param epoch number
---@return boolean
function SpaceSimulation.TeleportToOrbit(guid, celestialBodyGuid, inclination, eccentricity, semiMajorAxis, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomalyAtEpoch, epoch) end

---Teleports a simulation object to a surface position on the given celestial body.
---@param guid string
---@param celestialBodyGuid string
---@param altitude number
---@param latitude number
---@param longitude number
---@param verticalSpeed number
---@return boolean
function SpaceSimulation.TeleportToSurface(guid, celestialBodyGuid, altitude, latitude, longitude, verticalSpeed) end

---Sets the active OAB launch site by name.
---@param launchSite string
---@return boolean
function SpaceSimulation.SetLaunchSite(launchSite) end

-- SpaceSimulation.Debug - debug, timing, and physics-mode controls.
---@class SpaceSimulationDebug
local SpaceSimulationDebug = {}

---Prints a message to the script console.
---@param msg string
function SpaceSimulationDebug.Log(msg) end

---Waits the given number of real-time seconds, then invokes the callback.
---@param seconds number
---@param callback fun()
function SpaceSimulationDebug.WaitFor(seconds, callback) end

---Sets the physics mode ("atrest", "orbital", or "rigidbody") for a simulation object.
---@param simObjectGuid string
---@param vesselPhysics string
---@param enablePhysX boolean
---@param enablePartUnpack boolean
---@return boolean
function SpaceSimulationDebug.SetPhysicsMode(simObjectGuid, vesselPhysics, enablePhysX, enablePartUnpack) end

---Breaks into the attached debugger (UnityEngine.Debug.Break).
function SpaceSimulationDebug.Break() end

---Returns the current universe time in seconds.
---@return number
function SpaceSimulationDebug.GetUniversalTime() end

---Sets the simulation time scale.
---@param timeScale number
function SpaceSimulationDebug.SetTimeScale(timeScale) end

---Returns the current simulation time scale.
---@return number
function SpaceSimulationDebug.GetTimeScale() end

---Returns a map of registered interop type names to their nicknames.
---@return table<string, string>
function SpaceSimulationDebug.GetTypes() end

-- SpaceSimulation.HOG - Hand of God forces and torques applied to sim objects.
---@class SpaceSimulationHOG
local SpaceSimulationHOG = {}

---Creates a persistent named force on a simulation object. Returns the force guid.
---@param forceGuid string
---@param simObjGuid string
---@param forceState ForceState
---@return string
function SpaceSimulationHOG.CreateForce(forceGuid, simObjGuid, forceState) end

---Creates a persistent named torque on a simulation object. Returns the force guid.
---@param forceGuid string
---@param simObjGuid string
---@param forceState ForceState
---@return string
function SpaceSimulationHOG.CreateTorque(forceGuid, simObjGuid, forceState) end

---Sets the local force vector on an existing named force.
---@param forceGuid string
---@param forceValue Vector3
function SpaceSimulationHOG.SetLocalForce(forceGuid, forceValue) end

---Sets the local torque vector on an existing named force.
---@param forceGuid string
---@param torqueValue Vector3
function SpaceSimulationHOG.SetLocalTorque(forceGuid, torqueValue) end

---Destroys a named force or torque.
---@param forceGuid string
function SpaceSimulationHOG.Destroy(forceGuid) end

---Destroys a named force (alias of Destroy).
---@param forceGuid string
function SpaceSimulationHOG.DestroyForce(forceGuid) end

---Destroys a named torque (alias of Destroy).
---@param forceGuid string
function SpaceSimulationHOG.DestroyTorque(forceGuid) end

---Applies a one-shot impulse to a simulation object from a force state.
---@param simObjGuid string
---@param forceState ForceState
---@return boolean
function SpaceSimulationHOG.ApplyImpulse(simObjGuid, forceState) end
