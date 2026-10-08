---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Code/Root/PartCategories.cs
-- Source: Assets/Code/KSP/OAB/MetaAssemblySizeFilterType.cs
-- Source: Assets/Code/KSP/OAB/AssemblyPartStageType.cs
-- Source: Assets/Code/KSP/Sim/PartPhysicsModes.cs
-- Source: Assets/Code/KSP/OAB/OABEditorPartCategory.cs
-- Source: Assets/Code/KSP/OAB/AssemblyPartTypeFilter.cs
-- Source: Assets/Code/KSP/OAB/OABPartHideMode.cs
-- Source: Assets/Code/KSP/OAB/OABOrientation.cs
-- Source: Assets/Code/KSP/OAB/MirrorTechnique.cs
-- Source: Assets/Code/KSP/Sim/AttachNodeType.cs
-- Source: Assets/Code/KSP/Sim/Definitions/AttachNodeMethod.cs
-- Source: Assets/Code/KSP/Sim/Definitions/TransformDirAxis.cs
-- Source: Assets/Code/KSP/Modules/EngineType.cs
-- Source: Assets/Code/KSP/Modules/EngineState.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceFlowMode.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceTransferMode.cs
-- Source: Assets/Code/KSP/VolumeCloud/VolumeCloudConfiguration.cs
-- Source: Assets/Code/KSP/Sim/TransformFrameType.cs
-- Source: Assets/Code/KSP/game/Science/ScienceExperimentType.cs
-- Source: Assets/Code/KSP/game/Science/ScienceSitutation.cs
-- Source: Assets/Code/KSP/game/Science/ScienceReportType.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionProgressScope.cs

---Categories available for classifying parts in the vehicle assembly editor.
---@alias PartCategories
---| -1 # none - Sentinel value indicating no category is assigned.
---| 0 # Production - Parts related to resource production.
---| 1 # Control - Parts used to control vehicle attitude and flight.
---| 2 # Structural - Structural parts such as trusses, adapters, and decouplers.
---| 3 # Aero - Aerodynamic parts such as wings, fairings, and control surfaces.
---| 4 # Utility - General-purpose utility parts.
---| 5 # Science - Science instruments and experiment parts.
---| 6 # Pods - Command pods and crew cabins.
---| 7 # FuelTank - Fuel tank parts for storing propellant.
---| 8 # Engine - Engine parts providing thrust.
---| 9 # Communication - Communication and relay parts.
---| 10 # Electrical - Electrical parts such as batteries and solar panels.
---| 11 # Ground - Ground contact parts such as landing gear and wheels.
---| 12 # Thermal - Thermal management parts such as heat shields and radiators.
---| 13 # Payload - Payload parts intended to be delivered or deployed.
---| 14 # Coupling - Coupling and docking parts such as ports and clamps.
---| 15 # ColonyEssentials - Parts essential for establishing and sustaining colonies.
---| 16 # Favorites - Virtual category listing parts marked as favorites by the player.
---| 17 # SubAssemblies - Virtual category listing saved sub-assemblies.
---| 18 # Amenities - Colony amenity parts that provide comfort or morale to crew.
---| 19 # Storage - Storage parts for carrying cargo or resources.

---Size filter categories for a meta assembly in the object assembly building.
---@alias MetaAssemblySizeFilterType
---| 0 # Auto - Size category selected automatically based on part or assembly properties.
---| 1 # XS - Extra-small size category.
---| 2 # XSPLUS - Size category between extra-small and small.
---| 3 # S - Small size category.
---| 4 # SPLUS - Size category between small and medium.
---| 5 # M - Medium size category.
---| 6 # MPLUS - Size category between medium and large.
---| 7 # L - Large size category.
---| 8 # LPLUS - Size category between large and extra-large.
---| 9 # XL - Extra-large size category.
---| 10 # XLPLUS - Size category between extra-large and double extra-large.
---| 11 # XXL - Double extra-large size category.
---| 12 # XXXL - Triple extra-large size category.
---| 13 # XXXXL - Quadruple extra-large size category.
---| 14 # XXXXXL - Quintuple extra-large size category.
---| 15 # XXXXXXL - Sextuple extra-large size category.
---| 16 # XXS - Size category smaller than extra-small.

---The staging role of a part in the Object Assembly Builder.
---@alias AssemblyPartStageType
---| 0 # None - No staging role assigned to the part.
---| 1 # LiquidEngine - Part is a liquid-fueled engine that participates in staging for ignition or shutdown.
---| 2 # SolidEngine - Part is a solid rocket motor that participates in staging for ignition.
---| 3 # Parachute - Part is a parachute that can be deployed via staging.
---| 4 # Science - Part is a science instrument that can be activated via staging.
---| 5 # DecouplerHorizontal - Part is a horizontal decoupler that separates parts along a lateral axis when staged.
---| 6 # DecouplerVertical - Part is a vertical decoupler that separates parts along the stack axis when staged.
---| 7 # Fairing - Part is a fairing that can be jettisoned via staging.

---The physics simulation modes available for a part.
---@alias PartPhysicsModes
---| 0 # Full - Full physics simulation mode, in which the part participates in the physics simulation.
---| 1 # None - No physics simulation mode, in which the part is excluded from the physics simulation.

---Part category classifications used in the Object Assembly Builder editor.
---@alias OABEditorPartCategory
---| 0 # VAB - Vehicle Assembly Building part category.
---| 1 # BAE - Base Assembly Editor part category.
---| 2 # ALL - All part categories combined.
---| 3 # NONE - No part category assigned.
---| 11 # VAB_TERRESTRIAL - Vehicle Assembly Building terrestrial part subcategory.
---| 10 # VAB_ORBITAL - Vehicle Assembly Building orbital part subcategory.
---| 21 # BAE_TERRESTRIAL - Base Assembly Editor terrestrial part subcategory.
---| 20 # BAE_ORBITAL - Base Assembly Editor orbital part subcategory.

---Vehicle type categories for filtering assembly parts in the Object Assembly Building.
---@alias AssemblyPartTypeFilter
---| 0 # Rocket - The rocket vehicle category.
---| 1 # Airplane - The airplane vehicle category.
---| 2 # Spaceplane - The spaceplane vehicle category.
---| 3 # Rover - The rover vehicle category.

---The hide mode for a part entry in the Object Assembly Building.
---@alias OABPartHideMode
---| 0 # Shown - The part is always visible in the OAB parts list.
---| 1 # ShownByDefault - The part is visible in the OAB parts list by default.
---| 2 # Hidden - The part is always hidden from the OAB parts list.
---| 3 # HiddenByDefault - The part is hidden from the OAB parts list by default.

---Represents the build orientation mode for the Object Assembly Building.
---@alias OABOrientation
---| 0 # NONE - No orientation. Used when the build orientation is unset or not applicable.
---| 1 # VAB - Vertical Assembly Building orientation, used for assembling rockets upright.
---| 2 # AIRPLANE - Airplane orientation, used for assembling aircraft in a horizontal configuration.

---The technique used to mirror a part in the Object Assembly Builder.
---@alias MirrorTechnique
---| 0 # Auto - Selects the mirroring technique automatically based on the part's configuration.
---| 1 # Scale - Mirrors the part by negating its scale along the mirror axis.
---| 2 # Rotation - Mirrors the part using a 180-degree rotation about the mirror axis.
---| 3 # RotationYZ - Mirrors the part using a rotation in the YZ plane.
---| 4 # RotationXZ - Mirrors the part using a rotation in the XZ plane.
---| 5 # RotationXY - Mirrors the part using a rotation in the XY plane.

---The type of attachment node used to connect spacecraft parts.
---@alias AttachNodeType
---| 0 # Stack - A rigid axial attachment node that connects parts end-to-end along a stack axis.
---| 1 # Surface - An attachment node that connects a part to the surface of another part.
---| 2 # Dock - An attachment node used for docking port connections between vessels.

---The physics attachment method used for a part attach node.
---@alias AttachNodeMethod
---| 0 # FIXED_JOINT - Attaches the node using a fixed joint that prevents all relative movement between parts.
---| 1 # HINGE_JOINT - Attaches the node using a hinge joint that permits rotation around a single axis.
---| 2 # LOCKED_JOINT - Attaches the node using a locked joint that constrains all degrees of freedom.
---| 3 # MERGED_PHYSICS - Attaches the node by merging the part's physics simulation with that of its parent.
---| 4 # NO_PHYSICS - Attaches the node without applying any physics simulation to the part.
---| 5 # NONE - No attachment method. The node carries no physics connection.

---The directional axis of a transform, representing the X, Y, or Z axis.
---@alias TransformDirAxis
---| 0 # X - The X axis of the transform.
---| 1 # Y - The Y axis of the transform.
---| 2 # Z - The Z axis of the transform.

---The category of propulsion technology for an engine module.
---@alias EngineType
---| 0 # Generic - A generic or unspecified engine type.
---| 1 # SolidBooster - A solid-propellant rocket booster.
---| 2 # Methalox - A methane-liquid oxygen (methalox) bipropellant engine.
---| 3 # Piston - A piston-driven internal combustion engine.
---| 4 # Turbine - A turbine-based air-breathing jet engine.
---| 5 # ScramJet - A supersonic-combustion ramjet (scramjet) engine.
---| 6 # Electric - An electrically powered propulsion engine.
---| 7 # Nuclear - A nuclear thermal propulsion engine.
---| 8 # MonoProp - A monopropellant thruster.
---| 9 # Helium3 - A helium-3 fusion propulsion engine.
---| 10 # MetallicHydrogen - A metallic hydrogen propulsion engine.
---| 11 # NuclearSaltwater - A nuclear saltwater rocket engine.
---| 12 # Antimatter - An antimatter annihilation propulsion engine.

---Represents the operational state of a rocket engine.
---@alias EngineState
---| 0 # Off - The engine is off and not producing thrust.
---| 1 # Running - The engine is running at a steady throttle level.
---| 2 # RunningIncreasing - The engine is running and its thrust output is increasing.
---| 3 # RunningDecreasing - The engine is running and its thrust output is decreasing.
---| 4 # Starved - The engine is running but is starved of propellant.
---| 5 # ChangingMode - The engine is in the process of switching to a different operating mode.
---| 6 # Starting - The engine is in the ignition and startup sequence.
---| 7 # Stopping - The engine is in the shutdown sequence.

---Flow mode governing how resources are distributed between parts in a vessel.
---@alias ResourceFlowMode
---| 0 # NULL - Uninitialized or unset flow mode sentinel.
---| 1 # NO_FLOW - Resource does not flow. Consumption is limited to the part that contains it.
---| 2 # ALL_VESSEL - Resource flows freely across all parts in the vessel.
---| 3 # STAGE_PRIORITY_FLOW - Resource flows from parts in order of staging priority, draining lower-stage parts first.
---| 4 # STACK_PRIORITY_SEARCH - Resource is consumed from the highest-priority stack found by a depth-first search.
---| 5 # STAGE_STACK_FLOW_BALANCE - Resource is drawn in balanced amounts from all stacks within the current stage.

---The mode of resource transfer between parts.
---@alias ResourceTransferMode
---| 0 # NONE - No resource transfer. Resources do not flow between parts.
---| 1 # PUMP - Pump transfer mode. Resources are actively pumped between connected parts.

---The type of a volumetric cloud layer.
---@alias CloudsLayerType
---| 1 # Cumulus - Cumulus cloud layer type.
---| 2 # Stratus - Stratus cloud layer type.
---| 3 # Box - Box cloud layer type.

---The type of reference frame for a transform in the simulation.
---@alias TransformFrameType
---| 0 # None - No transform frame type assigned.
---| 1 # Body - A reference frame attached to a physical simulation body.
---| 2 # Celestial - A reference frame attached to a celestial body.

---The type of result produced by a science experiment.
---@alias ScienceExperimentType
---| 1 # DataType - Experiment produces a data report.
---| 2 # SampleType - Experiment produces a physical sample.
---| 3 # Both - Experiment produces both a data report and a physical sample.

---Science situations that classify a vessel's current orbital or surface environment for science collection.
---@alias ScienceSitutation
---| 0 # None - No science situation.
---| 1 # HighOrbit - Vessel is in high orbit around a body.
---| 2 # LowOrbit - Vessel is in low orbit around a body.
---| 3 # Atmosphere - Vessel is flying within a body's atmosphere.
---| 4 # Splashed - Vessel is splashed down in a body of liquid.
---| 5 # Landed - Vessel is landed on the surface of a body.

---Kinds of science report, distinguishing digital data from physical samples.
---@alias ScienceReportType
---| 1 # DataType - A science report type representing digitally recorded sensor data.
---| 2 # SampleType - A science report type representing a collected physical sample.

---Whether a mission stage is progressed by the campaign as a whole or by an individual vessel.
---@alias MissionProgressScope
---| "Campaign" # The stage is progressed by the campaign, and any vessel or none at all can satisfy it.
---| "Vessel" # The stage is progressed by whichever vessel is active, and each vessel holds its own progress through it.

---@alias WrapMode
---| 0 # Default
---| 1 # Once
---| 1 # Clamp
---| 2 # Loop
---| 4 # PingPong
---| 8 # ClampForever

---@alias WeightedMode
---| 0 # None
---| 1 # In
---| 2 # Out
---| 3 # Both
