---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/KSPActionGroupConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CameraModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/AutopilotModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/AutopilotStatusConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/EngineTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ForceTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/KerbalTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/PhysicsModeTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceFlowModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceTransferModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SpeedDisplayModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/AltimeterDisplayModeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ExplosionTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/PartRelationshipTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/OABEditorCategoryConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/MetaAssemblySizeFilterTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/AssemblyPartStageTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/TransformFrameTypeConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/RosterStatusConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ActionGroupStatesConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/AttachRulesConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResolutionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector2Converter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector3Converter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector3dConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector4Converter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector2dConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/Vector4dConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/QuaternionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/QuaternionDConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ColorConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/EnumConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/MoonSharpBinding.cs

---A vessel action group. `KSPActionGroup` is a [Flags] enum, but interop
---marshals a single value as its member name string.
---@alias KSPActionGroup
---| "None"
---| "Stage"
---| "Gear"
---| "Lights"
---| "RCS"
---| "SAS"
---| "Brakes"
---| "Abort"
---| "SolarPanels"
---| "RadiatorPanels"
---| "Science"
---| "ResourceScan"
---| "Custom01"
---| "Custom02"
---| "Custom03"
---| "Custom04"
---| "Custom05"
---| "Custom06"
---| "Custom07"
---| "Custom08"
---| "Custom09"
---| "Custom10"

---The available camera modes for the simulation camera.
---@alias CameraMode
---| "None"
---| "Celestial"
---| "Body"
---| "Horizon"
---| "Capture"
---| "PhotoCapture"
---| "PhotoHorizon"
---| "Stationary"
---| "Chase"
---| "Auto"
---| "Orbital"
---| "Cinematic"

---Autopilot heading modes available to a vessel's stability assist system.
---@alias AutopilotMode
---| "StabilityAssist"
---| "Prograde"
---| "Retrograde"
---| "Normal"
---| "Antinormal"
---| "RadialIn"
---| "RadialOut"
---| "Target"
---| "AntiTarget"
---| "Maneuver"
---| "Navigation"
---| "Autopilot"

---The category of propulsion technology for an engine module.
---@alias EngineTypeInterop
---| "Generic"
---| "SolidBooster"
---| "Methalox"
---| "Piston"
---| "Turbine"
---| "ScramJet"
---| "Electric"
---| "Nuclear"
---| "MonoProp"
---| "Helium3"
---| "MetallicHydrogen"
---| "NuclearSaltwater"
---| "Antimatter"

---The type of force applied in a physics simulation, distinguishing mass-dependent force from mass-independent acceleration.
---@alias ForceType
---| "Force"
---| "Acceleration"

---The body type of a Kerbal.
---@alias KerbalType
---| "CIRCLE"
---| "SQUARE"
---| "BUILDER"

---The physics simulation mode applied to a simulated object.
---@alias PhysicsMode
---| "None"
---| "AtRest"
---| "Orbital"
---| "RigidBody"

---Flow mode governing how resources are distributed between parts in a vessel.
---@alias ResourceFlowModeInterop
---| "NULL"
---| "NO_FLOW"
---| "ALL_VESSEL"
---| "STAGE_PRIORITY_FLOW"
---| "STACK_PRIORITY_SEARCH"
---| "STAGE_STACK_FLOW_BALANCE"

---The physical phase of a resource.
---@alias ResourceType
---| "solid"
---| "liquid"
---| "gas"

---The mode of resource transfer between parts.
---@alias ResourceTransferModeInterop
---| "NONE"
---| "PUMP"

---The reference frame used when displaying vessel speed.
---@alias SpeedDisplayMode
---| "Orbit"
---| "Surface"
---| "Target"

---Display mode for the altimeter, indicating whether altitude is measured from sea level or ground level.
---@alias AltimeterDisplayMode
---| "SeaLevel"
---| "GroundLevel"

---The cause or context of a simulated vehicle explosion.
---@alias ExplosionType
---| "InAir"
---| "InSpace"
---| "OnOverheat"
---| "OnOverpressure"
---| "OnOverG"
---| "OnGround"
---| "OnWater"
---| "Default"

---The type of structural or fluid connection between two parts.
---@alias PartRelationshipType
---| "None"
---| "Strut"
---| "FuelLine"

---Part category classifications used in the Object Assembly Builder editor.
---@alias OABEditorCategory
---| "VAB"
---| "BAE"
---| "ALL"
---| "NONE"
---| "VAB_TERRESTRIAL"
---| "VAB_ORBITAL"
---| "BAE_TERRESTRIAL"
---| "BAE_ORBITAL"

---Size filter categories for a meta assembly in the object assembly building.
---@alias MetaAssemblySizeFilterTypeInterop
---| "Auto"
---| "XS"
---| "XSPLUS"
---| "S"
---| "SPLUS"
---| "M"
---| "MPLUS"
---| "L"
---| "LPLUS"
---| "XL"
---| "XLPLUS"
---| "XXL"
---| "XXXL"
---| "XXXXL"
---| "XXXXXL"
---| "XXXXXXL"
---| "XSMINUS"

---The staging role of a part in the Object Assembly Builder.
---@alias AssemblyPartStageTypeInterop
---| "None"
---| "LiquidEngine"
---| "SolidEngine"
---| "Parachute"
---| "Science"
---| "DecouplerHorizontal"
---| "DecouplerVertical"
---| "Fairing"

---The type of reference frame for a transform in the simulation.
---@alias TransformFrameType
---| "None"
---| "Body"
---| "Celestial"

---Roster status of a crew member.
---@alias RosterStatus
---| "Available"
---| "Assigned"
---| "Dead"
---| "Missing"

---The enabled state and heading mode of a vessel's autopilot.
---@class AutopilotStatus
---@field isEnabled boolean
---@field mode AutopilotMode

---The on/off state of every action group on a vessel, keyed by action-group name.
---@class ActionGroupStates
---@field states table<KSPActionGroup, boolean>

---The attachment rules for a part: how it may attach and what may attach to it.
---@class AttachRulesInterop
---@field stack boolean
---@field srfAttach boolean
---@field allowStack boolean
---@field allowSrfAttach boolean
---@field allowCollision boolean

---A screen resolution: pixel dimensions and refresh rate.
---@class Resolution
---@field width integer
---@field height integer
---@field refreshRate integer

---A 2-component single-precision vector, marshalled as a 2-element number array.
---@alias Vec2 [number, number]

---A 3-component single-precision vector, marshalled as a 3-element number array.
---@alias Vec3 [number, number, number]

---A 3-component double-precision vector, marshalled as a 3-element number array.
---@alias Vec3d [number, number, number]

---A 4-component single-precision vector, marshalled as a 4-element number array.
---@alias Vec4 [number, number, number, number]

---A 2-component double-precision vector, marshalled as a 2-element number array.
---@alias Vec2d [number, number]

---A 4-component double-precision vector, marshalled as a 4-element number array.
---@alias Vec4d [number, number, number, number]

---A single-precision quaternion, marshalled as a 4-element number array (x, y, z, w).
---@alias Quat3 [number, number, number, number]

---A double-precision quaternion, marshalled as a 4-element number array (x, y, z, w).
---@alias Quatd [number, number, number, number]

---A CLR type, marshalled as its registered interop type-nickname string.
---@alias Type string
