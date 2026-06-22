---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/VesselStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/VesselDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/OrbitDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/OrbiterDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/RigidbodyStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/RigidbodyDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SimulationObjectStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SimulationObjectStateContainerConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SimulationObjectDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SimulationObjectDefinitionContainerConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/PhysicsOwnerStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/PhysicsOwnerDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/UniverseStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/MotionHierarchyConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/TransformHierarchyConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/FramePositionStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/GeographicPositionStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ForceStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/FlightCtrlStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/TelemetryStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/TelemetryDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/StagingStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/StagingDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ManeuverPlanDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SerializedVesselConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SerializedVesselSaveConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SerializedSituationConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SerializedSurfaceLocationConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SerializedRigidbodyStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/GameModeDataConverter.cs

---@class VesselState
-- alt: KSP.Sim.State.VesselState
---@field launchTime number
---@field landedAtBase string
---@field landedAtBaseTime number
---@field FlightCtrlState FlightCtrlState
---@field speedMode SpeedDisplayMode
---@field altimeterMode AltimeterDisplayMode
---@field autopilotMode AutopilotMode
---@field flightControlsMode any
---@field actionGroupStates ActionGroupStates
---@field currentTargetID any
---@field currentControlOwnerPart any

---@class Vessel
-- alt: VesselDefinition, KSP.Sim.Definitions.VesselDefinition
---@field vesselName string
---@field version string
---@field description string
---@field simulationObjectType string
---@field offsetToGround? number

---@class Orbit
-- alt: OrbitState, KSP.Sim.State.OrbitState
---@field referenceBodyGuid string
---@field inclination number
---@field eccentricity number
---@field semiMajorAxis number
---@field longitudeOfAscendingNode number
-- alt read key for longitudeOfAscendingNode: LAN
---@field argumentOfPeriapsis number
---@field meanAnomalyAtEpoch number
---@field epoch number

---@class Orbiter
-- alt: KSP.Sim.Definitions.OrbiterDefinition
---@field orbitColor? Color
---@field nodeColor? Color
---@field lowerCamVsSmaRatio? number
---@field upperCamVsSmaRatio? number

---@class RigidbodyState
-- alt: KSP.Sim.State.RigidbodyState
---@field referenceTransformGuid string
---@field referenceFrameType TransformFrameType
---@field localPosition Vec3d
---@field localRotation Quatd
---@field localVelocity Vec3d
---@field localAngularVelocity Vec3d

---@class Rigidbody
-- alt: RigidbodyDefinition, KSP.Sim.Definitions.RigidbodyDefinition
---No fields are exposed to Lua.

---@class State
-- alt: SimulationObjectState, KSP.Sim.State.SimulationObjectState
---@field position FramePositionState

---@class StateContainer
-- alt: SimulationObjectStateContainer, KSP.Sim.State.SimulationObjectStateContainer
---@field state State
---@field componentStates table

---@class SimulationObjectDefinition
-- alt: KSP.Sim.Definitions.SimulationObjectDefinition
---No fields are exposed to Lua.

---@class DefinitionContainer
-- alt: SimulationObjectDefinitionContainer, KSP.Sim.Definitions.SimulationObjectDefinitionContainer
---@field definition SimulationObjectDefinition
---@field componentDefinitions table

---@class PhysicsOwnerState
-- alt: KSP.Sim.State.PhysicsOwnerState
---@field mass number
---@field physicsMode PhysicsMode

---@class PhysicsOwnerDefinition
-- alt: KSP.Sim.Definitions.PhysicsOwnerDefinition
---No fields are exposed to Lua.

---@class UniverseState
-- alt: KSP.Sim.State.UniverseState
---@field universalTime number

---@class MotionHierarchy
-- alt: KSP.Sim.State.MotionHierarchy
---@field motionGuid string
---@field motionFrameType TransformFrameType
---@field relativeVelocity Vec3d
---@field relativeAngularVelocity Vec3d
---@field children? MotionHierarchy[]

---@class TransformHierarchy
-- alt: KSP.Sim.State.TransformHierarchy
---@field transformGuid string
---@field referenceFrameType TransformFrameType
---@field localPosition Vec3d
---@field localRotation Quatd
---@field children? TransformHierarchy[]

---@class FramePositionState
-- alt: KSP.Sim.State.FramePositionState
---@field referenceTransformGuid string
---@field referenceFrameType TransformFrameType
---@field localPosition Vec3d
---@field localRotation Quatd

---@class GeographicPositionState
-- alt: KSP.Sim.State.GeographicPositionState
---@field referenceBodyGuid string
---@field latitude number
---@field longitude number
---@field altitude number
---@field heading number

---@class ForceState
-- alt: KSP.Sim.State.ForceState
---@field mode ForceType
---@field localPosition Vec3d
---@field localValue Vec3d

---@class FlightCtrlState
-- alt: FlightCtrlStateIncremental, KSP.Sim.State.FlightCtrlStateIncremental
---@field mainThrottle? number
---@field roll? number
---@field yaw? number
---@field pitch? number
---@field rollTrim? number
---@field yawTrim? number
---@field pitchTrim? number
---@field inputRoll? number
---@field inputYaw? number
---@field inputPitch? number
---@field wheelSteer? number
---@field wheelSteerTrim? number
---@field wheelThrottle? number
---@field wheelThrottleTrim? number
---@field X? number
---@field Y? number
---@field Z? number
---@field killRot? boolean
---@field gearUp? boolean
---@field gearDown? boolean
---@field headlight? boolean

---@class TelemetryState
-- alt: KSP.Sim.State.TelemetryState
---No fields are exposed to Lua.

---@class Telemetry
-- alt: TelemetryDefinition, KSP.Sim.Definitions.TelemetryDefinition
---No fields are exposed to Lua.

---@class StagingState
-- alt: KSP.Sim.State.StagingState
---@field availableStages StageParts[]
---@field unassignedParts StageParts

---@class Staging
-- alt: StagingDefinition, KSP.Sim.Definitions.StagingDefinition
---No fields are exposed to Lua.

---@class ManeuverPlan
-- alt: ManeuverPlanDefinition, KSP.Sim.Definitions.ManeuverPlanDefinition
---No fields are exposed to Lua.

---@class Vehicle
-- alt: SerializedVessel, KSP.Sim.SerializedVessel
---@field vesselDefinition Vessel
---@field partDefinitions partcontainer[]
---@field stagingState StagingState
---@field partOwnerState PartOwnerState

---@class VesselSave
-- alt: VehicleSave, KSP.Sim.SerializedVesselSave
---@field vessel Vehicle
---@field situation Situation

---@class Situation
-- alt: KSP.Sim.SerializedSituation
---@field simulationObjectState State
---@field componentDefinitions table<string, any>
---@field componentStates table<string, any>

---@class SerializedSurfaceLocation
-- alt: KSP.Sim.SerializedSurfaceLocation
---@field parentGuid string
---@field objectName string

---@class SerializedRigidbodyState
-- alt: KSP.Sim.SerializedRigidbodyState
---@field referenceTransformGuid string
---@field referenceFrameType TransformFrameType
---@field localVelocity Vec3d
---@field localAngularVelocity Vec3d
---@field localPosition Vec3d
---@field localRotation Quatd

---@class GameMode
-- alt: KSP.Contexts.Game.GameModeData
---@field DefaultStatePath string
---@field Name string
---@field Description string
