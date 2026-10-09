---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/PartUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/ModuleUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/PartsUtilities.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/Attributes/ModuleDataAdapterAttribute.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Utility/IndexedListUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/ModesUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/EngineUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/ResourceContainersUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/PartsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/Converters/PartConverter.cs
-- Source: Assets/Code/KSP/Sim/Definitions/PartCore.cs
-- Source: Assets/Code/KSP/Sim/Definitions/PartData.cs
-- Source: Assets/Code/KSP/Sim/Definitions/PartResourceCostDefinition.cs
-- Source: Assets/Code/KSP/Sim/Definitions/AttachRules.cs
-- Source: Assets/Code/KSP/Sim/Definitions/AttachNodeDefinition.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ContainedResourceDefinition.cs
-- Source: Assets/Code/KSP/Sim/Definitions/SerializedPartModule.cs
-- Source: Assets/Code/KSP/Sim/Definitions/SerializedModuleData.cs
-- Source: Assets/Code/KSP/IO/SerializedModuleDataJsonConverter.cs
-- Source: Assets/Code/KSP/Sim/Definitions/SerializedResourceInfo.cs
-- Source: Assets/Code/KSP/game/PartsManagerCore.cs
-- Source: Assets/Code/KSP/Modules/Data_Engine.cs
-- Source: Assets/Code/Redux/Ecs/Modules/Engine.cs
-- Source: Assets/Code/KSP/Sim/Definitions/ModuleProperty.cs
-- Source: Assets/Code/KSP/Modules/PropellantDefinition.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeIngredientDefinitionOverride.cs
-- Source: Assets/Code/KSP/Modules/ThrustTransformGroup.cs

---@class _PartUserDataModuleIndexer
---@field [string] ModuleUserData

---Part definition wrapper exposing the inner `data` subtree, the typed `resourceContainers` array, and
---each part module as a virtual property keyed by the module's short name.
---@class PartUserData : _PartData, _PartUserDataModuleIndexer, ExtensibleJsonUserData
---@field resourceContainers ResourceContainersUserData
local PartUserData = {}

---Adds a new part module of the given type and runs callback against it for further configuration.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData) Callback that receives the new module for further configuration.
---@error Thrown when moduleType is not a known component module.
function PartUserData:AddModule(moduleType, callback) end

---Runs callback against the existing module of the given type, doing nothing if absent.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData) Callback that receives the existing module for further configuration.
function PartUserData:PatchModule(moduleType, callback) end

---Patches the named module if it exists, otherwise adds it.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData) Callback that receives the module for further configuration.
function PartUserData:EnsureModule(moduleType, callback) end

---Removes the named module from the part and rebuilds the module-index map.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
function PartUserData:RemoveModule(moduleType) end

---Returns whether the part has a module of the given type.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
---@return boolean present True if a module of that type is registered, false otherwise.
function PartUserData:HasModule(moduleType) end

---@class _ModuleUserDataDataIndexer
---@field [string] EngineUserData | PartSwitchUserData | JsonUserData
---@field [integer] EngineUserData | PartSwitchUserData | JsonUserData

---Wrapper for a part module's serialized JSON, exposing each `ModuleData` entry by name (and by 1-based
---position from Lua) with a typed adapter when one is registered for the data type. Backed by the module's
---`ModuleData` array as a name-indexed list.
---@class ModuleUserData : _ModuleUserDataDataIndexer, IndexedListUserData<EngineUserData | PartSwitchUserData | JsonUserData>
local ModuleUserData = {}

---Adds a new module-data entry of the given type and runs callback against it when supplied.
---@param type string The data module's short name as registered in `PartsUtilities.DataModules`.
---@param callback? fun(entry: EngineUserData | PartSwitchUserData | JsonUserData) Optional callback that receives the new entry for further configuration.
---@error Thrown when type is not a registered data module.
function ModuleUserData:AddData(type, callback) end

---Runs callback against the existing data entry of the given type, doing nothing if absent.
---@param type string The data module's short name.
---@param callback fun(entry: EngineUserData | PartSwitchUserData | JsonUserData) Callback that receives the existing entry for further configuration.
function ModuleUserData:PatchData(type, callback) end

---Patches the named data entry if it exists, otherwise adds it.
---@param type string The data module's short name.
---@param callback fun(entry: EngineUserData | PartSwitchUserData | JsonUserData) Callback that receives the entry for further configuration.
function ModuleUserData:EnsureData(type, callback) end

---Removes the data entry of the given type from the module, doing nothing if absent.
---@param type string The data module's short name.
function ModuleUserData:RemoveData(type) end

---Returns whether the module has a data entry of the given type.
---@param type string The data module's short name.
---@return boolean present True if an entry exists, false otherwise.
function ModuleUserData:HasData(type) end

---@class EngineModeUserData : _Data_Engine_EngineMode, JsonUserData

---Indexed-list wrapper for an engine's `engineModes` array, keyed by each mode's `engineID`.
---@class ModesUserData : IndexedListUserData<EngineModeUserData>
local ModesUserData = {}

---Adds a new engine mode with the given `engineID` and runs callback against it for further configuration.
---@param mode string The new mode's `engineID`.
---@param callback fun(mode: EngineModeUserData) Callback that receives the new mode for further configuration.
function ModesUserData:Add(mode, callback) end

---`Data_Engine` module-data adapter that exposes the engine's `engineModes` array as a typed
---ModesUserData rather than a raw JsonUserData.
---@class EngineUserData : _Data_Engine, ExtensibleJsonUserData
---@field engineModes ModesUserData

---@class ResourceContainerUserData : _ContainedResourceDefinition, JsonUserData

---Indexed-list wrapper for a part's `resourceContainers` array, keyed by each container's `name`.
---@class ResourceContainersUserData : IndexedListUserData<ResourceContainerUserData>
local ResourceContainersUserData = {}

---Adds a new resource container with the given resource type, capacity, and optional initial fill.
---@param type string The resource name to store.
---@param capacity number The container's capacity in units.
---@param initial? number The initial fill in units; defaults to 0.
---@param nonStageable? boolean Whether the container is exempt from staging; defaults to false.
function ResourceContainersUserData:Add(type, capacity, initial, nonStageable) end

---Returns whether the part contains a resource container of the given type.
---@param type string The resource name to look up.
---@return boolean present True if such a container exists, false otherwise.
function ResourceContainersUserData:Has(type) end

---Lua submodule exposed as `PM.Parts`, providing patches for part definitions.
---@class PartsLuaModule
local PartsLuaModule = {}

---Registers a part patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<PartUserData, ModuleUserData> patch The registered patch.
function PartsLuaModule:Patch(name) end

---Registers a patch that copies each part it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the part to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: PartUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<PartUserData, ModuleUserData> patch The registered patch.
function PartsLuaModule:Duplicate(source, newName, patchMethod) end

---Represents the top-level container for a serialized part definition, holding part and module data with versioned JSON deserialization support.
---@class _PartCore : _JsonUserDataBase
---@field version number The serialization version recorded in this part definition instance.
---@field useExternal boolean A value indicating whether this part definition uses an external data source rather than inline data.
---@field data PartData The core part data payload holding physical properties, attachment rules, and editor settings for the part.
---@field modules JsonList<ModuleDataPayload> The list of module data definitions associated with this part.
---@field legacyModules string Serialized module data from legacy format versions that have not yet been migrated.

---@alias PartCore _PartCore | { version: number, useExternal: boolean, data: PartData, modules: JsonList<ModuleDataPayload>, legacyModules: string }

---Represents the serializable data definition for a spacecraft part, including physical properties, attachment rules, resource containers, part modules, and OAB editor settings.
---@class _PartData : _JsonUserDataBase
---@field partName string The unique identifier key for the part, used in save files and internal references.
---@field Layer? string The part layer this copy of the part belongs to, or nil for the default copy.
---@field author string The name of the author or authors who created the part.
---@field category PartCategories The category classification used to group the part in the parts list.
---@field family string The family identifier grouping related parts together.
---@field childStageOffset integer The staging index offset applied to child parts relative to this part.
---@field cost integer The base currency cost of the part.
---@field crewCapacity integer The number of crew members this part can accommodate.
---@field stageOffset integer The staging index offset applied to this part.
---@field isCompound boolean A value indicating whether the part is a compound assembly part.
---@field sizeKey string The string key identifying the size classification of the part.
---@field sizeCategory MetaAssemblySizeFilterType The size category used for meta-assembly size filtering.
---@field stageType AssemblyPartStageType The staging type that determines how the part participates in stage sequencing.
---@field resourceCosts JsonList<PartResourceCostDefinition> The list of resource costs required to build or purchase this part.
---@field tags string The space-separated search tags associated with this part.
---@field stagingIconAssetAddress string The addressable asset address for the staging icon sprite used by this part.
---@field PartSizeDiameter number The diameter in meters used to classify the physical size of the part.
---@field angularDrag number The angular drag coefficient applied to the part during physics simulation.
---@field breakingForce number The maximum force in kilonewtons the part can withstand before breaking.
---@field breakingTorque number The maximum torque in kilonewton-meters the part can withstand before breaking.
---@field buoyancy number The buoyancy factor controlling how much upward force the part receives when submerged.
---@field buoyancyUseSine boolean A value indicating whether buoyancy force is modulated using a sine curve.
---@field coLiftOffset Vector3 The local-space offset of the center of lift from the part origin.
---@field coMassOffset Vector3 The local-space offset of the center of mass from the part origin.
---@field coPressureOffset Vector3 The local-space offset of the center of pressure from the part origin.
---@field coBuoyancy Vector3 The local-space position of the center of buoyancy.
---@field coDisplacement Vector3 The local-space position of the center of displacement volume.
---@field crashTolerance number The impact speed in meters per second the part can survive without taking damage.
---@field explosionPotential number The relative explosion yield of the part when it is destroyed.
---@field fuelCrossFeed boolean A value indicating whether fuel can flow across this part to connected parts.
---@field heatConductivity number The thermal conductivity coefficient controlling how quickly heat transfers through the part.
---@field mass number The dry mass of the part in metric tons.
---@field maxTemp number The maximum core temperature in Kelvin the part can reach before being destroyed.
---@field attachRules AttachRules The attachment rules that govern how the part can be connected to other parts.
---@field attachNodes JsonList<AttachNodeDefinition> The list of attach node definitions that describe connection points on the part.
---@field resourceContainers JsonList<ContainedResourceDefinition> The list of resource containers defining the resources stored in this part.
---@field AllowKinematicPhysicsIfIntersectTerrain boolean A value indicating whether the part is allowed to use kinematic physics when it intersects terrain.
---@field serializedPartModules JsonList<SerializedPartModule> The serialized list of part module data attached to this part.
---@field resourceSummary SerializedResourceInfo The cached summary of resource totals for this part.
---@field PAMModuleSortOverride JsonList<SerializedPartModuleDisplayOrder> The list of per-module sort order overrides used by the Parts Action Manager.
---@field PAMModuleVisualsOverride JsonList<SerializedPartModuleDisplayVisuals> The list of per-module visual display overrides used by the Parts Action Manager.
---@field hideFromFlightPartsManager boolean A value indicating whether this part is hidden from the in-flight Parts Manager.
---@field hideFromOABPartsManager boolean A value indicating whether this part is hidden from the OAB Parts Manager.
---@field collisionVolumeBoundsScale Vector3 The scale factor applied to the collision volume bounds of the part in the editor.
---@field emissiveConstant number The emissive constant controlling the ratio of thermal energy radiated by the part.
---@field maximumDrag number The maximum aerodynamic drag coefficient applied to the part.
---@field minimumDrag number The minimum aerodynamic drag coefficient applied to the part.
---@field physicsMode PartPhysicsModes The physics simulation mode that determines how the part is handled by the physics engine.
---@field inverseStageCarryover boolean A value indicating whether staging information carries over in reverse staging order.
---@field skinMassPerArea number The mass per unit surface area of the part skin in kilograms per square meter.
---@field bodyLiftOnlyUnattachedLift boolean A value indicating whether body lift is applied only when no attached lift surfaces are present.
---@field bodyLiftOnlyAttachName string The name of the attach node used to restrict body lift to unattached lift conditions.
---@field maxLength integer The maximum segment length for extendable parts, or -1 if no limit applies.
---@field radiatorHeadroom number The headroom fraction reserved above the radiator cooling capacity.
---@field radiatorMax number The maximum fraction of excess heat a radiator on this part can dissipate.
---@field skinMaxTemp number The maximum temperature in Kelvin the outer skin of the part can reach before being destroyed.
---@field skinInternalConductionMult number The multiplier applied to heat conduction between the part skin and its internal structure.
---@field thermalMassModifier number The multiplier applied to the thermal mass of the part.
---@field buoyancyUseCubeNamed string The name of the buoyancy cube used for directed buoyancy calculations, if any.
---@field HasReportStorage boolean A value indicating whether this part can store science reports.
---@field oabEditorCategory OABEditorPartCategory The OAB editor category that determines which tab this part appears under.
---@field partType AssemblyPartTypeFilter The assembly part type filter classifying what kind of part this is in the editor.
---@field partHideMode OABPartHideMode The hide mode controlling whether the part is visible or hidden in the OAB parts list.
---@field PreferredOrientation OABOrientation The preferred orientation the OAB uses when placing this part, matching its prefab orientation.
---@field MirrorTechnique MirrorTechnique The mirror technique that determines how this part mirrors and interacts with mirroring in the OAB.
---@field CanSuggestOrientation boolean A value indicating whether the OAB may suggest a placement rotation for this part when no user rotation is provided.
---@field PickUpPointOffset Vector3 The local-space offset at which the player cursor grabs the part when placing it in the OAB.
---@field PickupRotationPointOffset Vector3 The local-space offset used as the rotation pivot when the player grabs and rotates the part in the OAB.

---@alias PartData _PartData | { partName: string, Layer?: string, author: string, category: PartCategories, family: string, childStageOffset: integer, cost: integer, crewCapacity: integer, stageOffset: integer, isCompound: boolean, sizeKey: string, sizeCategory: MetaAssemblySizeFilterType, stageType: AssemblyPartStageType, resourceCosts: JsonList<PartResourceCostDefinition>, tags: string, stagingIconAssetAddress: string, PartSizeDiameter: number, angularDrag: number, breakingForce: number, breakingTorque: number, buoyancy: number, buoyancyUseSine: boolean, coLiftOffset: Vector3, coMassOffset: Vector3, coPressureOffset: Vector3, coBuoyancy: Vector3, coDisplacement: Vector3, crashTolerance: number, explosionPotential: number, fuelCrossFeed: boolean, heatConductivity: number, mass: number, maxTemp: number, attachRules: AttachRules, attachNodes: JsonList<AttachNodeDefinition>, resourceContainers: JsonList<ContainedResourceDefinition>, AllowKinematicPhysicsIfIntersectTerrain: boolean, serializedPartModules: JsonList<SerializedPartModule>, resourceSummary: SerializedResourceInfo, PAMModuleSortOverride: JsonList<SerializedPartModuleDisplayOrder>, PAMModuleVisualsOverride: JsonList<SerializedPartModuleDisplayVisuals>, hideFromFlightPartsManager: boolean, hideFromOABPartsManager: boolean, collisionVolumeBoundsScale: Vector3, emissiveConstant: number, maximumDrag: number, minimumDrag: number, physicsMode: PartPhysicsModes, inverseStageCarryover: boolean, skinMassPerArea: number, bodyLiftOnlyUnattachedLift: boolean, bodyLiftOnlyAttachName: string, maxLength: integer, radiatorHeadroom: number, radiatorMax: number, skinMaxTemp: number, skinInternalConductionMult: number, thermalMassModifier: number, buoyancyUseCubeNamed: string, HasReportStorage: boolean, oabEditorCategory: OABEditorPartCategory, partType: AssemblyPartTypeFilter, partHideMode: OABPartHideMode, PreferredOrientation: OABOrientation, MirrorTechnique: MirrorTechnique, CanSuggestOrientation: boolean, PickUpPointOffset: Vector3, PickupRotationPointOffset: Vector3 }

---Represents the resource cost of a part as a named resource and unit quantity.
---@class _PartResourceCostDefinition : _JsonUserDataBase
---@field name string The name of the resource associated with this cost entry.
---@field resourceUnits number The number of resource units required as the cost.

---@alias PartResourceCostDefinition _PartResourceCostDefinition | { name: string, resourceUnits: number }

---Represents the attachment rules for a part, defining which attachment modes and behaviors are permitted.
---@class _AttachRules : _JsonUserDataBase
---@field stack boolean A flag indicating whether the part uses stack attachment.
---@field srfAttach boolean A flag indicating whether the part uses surface attachment.
---@field allowStack boolean A flag indicating whether other parts are permitted to attach to this part's stack nodes.
---@field allowSrfAttach boolean A flag indicating whether other parts are permitted to surface-attach to this part.
---@field allowCollision boolean A flag indicating whether collisions between this part and adjacent attached parts are permitted.
---@field allowDock boolean A flag indicating whether this part is permitted to dock with other parts.
---@field allowRotate boolean A flag indicating whether this part is permitted to be rotated in the editor.
---@field allowRoot boolean A flag indicating whether this part is permitted to be set as the root part of a vessel.

---@alias AttachRules _AttachRules | { stack: boolean, srfAttach: boolean, allowStack: boolean, allowSrfAttach: boolean, allowCollision: boolean, allowDock: boolean, allowRotate: boolean, allowRoot: boolean }

---Represents the definition of a part attachment node, including its position, orientation, type, and joint configuration.
---@class _AttachNodeDefinition : _JsonUserDataBase
---@field nodeID string The unique identifier for this attachment node.
---@field NodeSymmetryGroupID string An optional group identifier used to associate multiple nodes together, such as two downward-facing nodes sharing a bottom group. An empty string indicates no group.
---@field nodeType AttachNodeType The type of this attachment node, controlling how it participates in part attachment.
---@field attachMethod AttachNodeMethod The method used to attach a part at this node.
---@field IsMultiJoint boolean A value indicating whether this attach node creates multiple PhysX joints.
---@field MultiJointMaxJoint integer The maximum number of PhysX joints to create when IsMultiJoint is true.
---@field MultiJointRadiusOffset number The radial offset used when positioning the additional PhysX joints, only applied when IsMultiJoint is true.
---@field MultiJointOnSingleAxis boolean A value indicating whether the multi-joints are placed along a single axis instead of arranged in a circle, only applied when IsMultiJoint is true.
---@field SingleJointAxis TransformDirAxis The axis along which multi-joints are arranged when MultiJointOnSingleAxis is true.
---@field MultiJointFullBreakStrength boolean A value indicating whether all multi-joints must break simultaneously at full strength rather than individually, only applied when IsMultiJoint is true.
---@field position Vector3d The local position of this attachment node relative to its part.
---@field orientation Vector3d The local orientation direction of this attachment node relative to its part.
---@field size integer The integer size category of this attachment node, used for compatibility checks between nodes.
---@field sizeKey string The string key identifying the size category of this attachment node.
---@field visualSize number The visual scale of this attachment node as rendered in the editor.
---@field isResourceCrossfeed boolean A value indicating whether resources can flow across this attachment node to connected parts.
---@field isRigid boolean A value indicating whether this attachment node creates a rigid joint that resists flex.
---@field angularStrengthMultiplier number A multiplier applied to the angular break strength of the joint at this attachment node.
---@field contactArea number The contact surface area of this attachment node, used in joint strength calculations.
---@field overrideDragArea number An override value for the drag area contributed by this attachment node. A value of -1 disables the override.
---@field isCompoundJoint boolean A value indicating whether the joint at this attachment node is a compound joint.

---@alias AttachNodeDefinition _AttachNodeDefinition | { nodeID: string, NodeSymmetryGroupID: string, nodeType: AttachNodeType, attachMethod: AttachNodeMethod, IsMultiJoint: boolean, MultiJointMaxJoint: integer, MultiJointRadiusOffset: number, MultiJointOnSingleAxis: boolean, SingleJointAxis: TransformDirAxis, MultiJointFullBreakStrength: boolean, position: Vector3d, orientation: Vector3d, size: integer, sizeKey: string, visualSize: number, isResourceCrossfeed: boolean, isRigid: boolean, angularStrengthMultiplier: number, contactArea: number, overrideDragArea: number, isCompoundJoint: boolean }

---Represents the definition of a resource contained within a part, including capacity, initial amount, and staging configuration.
---@class _ContainedResourceDefinition : _JsonUserDataBase
---@field name string The name of the resource type contained in this definition.
---@field capacityUnits number The maximum capacity of the resource container in units.
---@field initialUnits number The amount of this resource present when the container is first loaded.
---@field NonStageable boolean A flag indicating whether this resource is excluded from staging.

---@alias ContainedResourceDefinition _ContainedResourceDefinition | { name: string, capacityUnits: number, initialUnits: number, NonStageable: boolean }

---Represents a serialized snapshot of a part module, storing its name, component type, behaviour type, and associated module data.
---@class _SerializedPartModule : _JsonUserDataBase
---@field Name string The name of the part component module type, used as the lookup key when matching this serialized entry to a live module.
---@field ComponentType string The runtime Type of the part component module that owns the serialized module data.
---@field BehaviourType string The runtime Type of the part behaviour module that hosts the component side in the Unity scene.
---@field ModuleData JsonList<SerializedModuleData> The serialized module data blocks attached to the module, one entry per data module captured from the source instance.

---@alias SerializedPartModule _SerializedPartModule | { Name: string, ComponentType: string, BehaviourType: string, ModuleData: JsonList<SerializedModuleData> }

---Represents the serialized form of a ModuleData object for persistent storage.
---@class _SerializedModuleData : _JsonUserDataBase
---@field Name string The name of the data type for this module entry.
---@field ModuleType string The module type, retained for converting persistent data from versions prior to 0.3.
---@field DataType string The runtime type of the serialized module data object.
---@field Data string? The raw serialized data string for this module entry, superseded by StateJson.
---@field DataObject ModuleDataPayload The module payload as parsed JSON, serialized under the "DataObject" name. Populated when this entry is deserialized from disk. The JSON is applied to a live module only at entity bind time, never to an unbound instance.

---@alias SerializedModuleData _SerializedModuleData | { Name: string, ModuleType: string, DataType: string, Data: string?, DataObject: ModuleDataPayload }

---Represents the serialized resource relationship data for a part, describing the resources it consumes, generates, and contains.
---@class _SerializedResourceInfo : _JsonUserDataBase
---@field Consumes JsonList<string> Names of resources consumed by this part.
---@field Generates JsonList<string> Names of resources generated by this part.
---@field Contains JsonList<string> Names of resources stored in this part.

---@alias SerializedResourceInfo _SerializedResourceInfo | { Consumes: JsonList<string>, Generates: JsonList<string>, Contains: JsonList<string> }

---Serializable sort order entry that maps a part component module name to its display sort index in the parts manager.
---@class _SerializedPartModuleDisplayOrder : _JsonUserDataBase
---@field PartComponentModuleName string The fully-qualified name of the part component module type this entry describes.
---@field sortIndex integer The display sort index used to order this module in the parts manager list.

---@alias SerializedPartModuleDisplayOrder _SerializedPartModuleDisplayOrder | { PartComponentModuleName: string, sortIndex: integer }

---Serializable visual configuration for a part component module's display in the parts manager, including display name and header and footer visibility.
---@class _SerializedPartModuleDisplayVisuals : _JsonUserDataBase
---@field PartComponentModuleName string The fully-qualified name of the part component module type this entry describes.
---@field ModuleDisplayName string The localized display name shown for the module in the parts manager UI.
---@field ShowHeader boolean A value indicating whether the module header row is visible.
---@field ShowFooter boolean A value indicating whether the module footer row is visible.

---@alias SerializedPartModuleDisplayVisuals _SerializedPartModuleDisplayVisuals | { PartComponentModuleName: string, ModuleDisplayName: string, ShowHeader: boolean, ShowFooter: boolean }

---Serializable data definition for Module_Engine.
---@class _Data_Engine : _JsonUserDataBase
---@field ["$type"] "KSP.Modules.Data_Engine, Assembly-CSharp"
---@field engineModes JsonList<Data_Engine_EngineMode> Engine mode definitions supported by this engine.
---@field UseEmissive boolean Flag indicating whether emissive visual effects are enabled for this engine.
---@field EmissiveMaterialNames JsonList<string> Names of the materials on the engine mesh that receive emissive coloring.
---@field EmissiveTemperatureCurve FloatCurve Curve that maps engine temperature to emissive intensity.
---@field EmissiveLerpRateUp number Interpolation rate applied when the emissive intensity is increasing.
---@field EmissiveLerpRateDown number Interpolation rate applied when the emissive intensity is decreasing.
---@field DeployedModeAnimationStateShortName string Short name of the animation state used when the engine enters deployed mode.
---@field IndependentThrottle ModuleProperty<boolean>
---@field IndependentThrottlePercentage ModuleProperty<number>
---@field EngineAutoSwitchMode ModuleProperty<boolean>
---@field thrustPercentage ModuleProperty<number>
---@field stagingOn ModuleProperty<boolean>
---@field activeEngineMode ModuleProperty<string> Gets or sets the identifier of the currently active engine mode.
---@field EngineStatePriorChangeMode EngineState The engine state recorded immediately before a mode change was initiated.
---@field EngineChangingToMode integer The index of the engine mode being transitioned to during a mode change.
---@field FinalThrustValue number The actual thrust output produced on the last simulation step, in kilonewtons.
---@field RealISPValue number The specific impulse value computed on the last simulation step, in seconds.
---@field staged boolean A flag indicating whether the engine has been activated via staging.
---@field Flameout boolean Gets a value indicating whether the engine is in a flameout state.
---@field EngineIgnited boolean A flag indicating whether the engine is currently ignited.
---@field EngineShutdown boolean Gets a value indicating whether the engine has been shut down.
---@field currentThrottle number The current effective throttle level of the engine, ranging from 0 to 1.
---@field thrustCurveDisplay number The displayed thrust curve output value, updated each simulation step for UI presentation.
---@field thrustCurveRatio number The thrust curve ratio applied to the throttle level to produce the effective thrust output.
---@field EngineSpool number The current turbine spool fraction, ranging from 0 (idle) to 1 (full spool).
---@field ThrustDirRelativePartWorldSpace Vector3 The net thrust direction vector in world space, expressed relative to the part origin.
---@field currentEngineModeIndex integer The zero-based index of the currently active engine mode within the engine modes list.

---@alias Data_Engine _Data_Engine | { ["$type"]: "KSP.Modules.Data_Engine, Assembly-CSharp", engineModes: JsonList<Data_Engine_EngineMode>, UseEmissive: boolean, EmissiveMaterialNames: JsonList<string>, EmissiveTemperatureCurve: FloatCurve, EmissiveLerpRateUp: number, EmissiveLerpRateDown: number, DeployedModeAnimationStateShortName: string, IndependentThrottle: ModuleProperty<boolean>, IndependentThrottlePercentage: ModuleProperty<number>, EngineAutoSwitchMode: ModuleProperty<boolean>, thrustPercentage: ModuleProperty<number>, stagingOn: ModuleProperty<boolean>, activeEngineMode: ModuleProperty<string>, EngineStatePriorChangeMode: EngineState, EngineChangingToMode: integer, FinalThrustValue: number, RealISPValue: number, staged: boolean, Flameout: boolean, EngineIgnited: boolean, EngineShutdown: boolean, currentThrottle: number, thrustCurveDisplay: number, thrustCurveRatio: number, EngineSpool: number, ThrustDirRelativePartWorldSpace: Vector3, currentEngineModeIndex: integer }

---Represents the configuration for a single engine mode, including thrust, propellant, atmosphere, exhaust damage, and throttle parameters.
---@class _Data_Engine_EngineMode : _JsonUserDataBase
---@field engineID string The identifier string for this engine mode. Expected to be in English.
---@field EngineDisplayName string The localization tag used to display the engine mode name in the UI. Appears only for multi-mode engines.
---@field thrustVectorTransformName string The name of the thrust vector transform in the model for this engine mode. Used only when ThrustTransformNamesMultipliers is empty.
---@field ThrustTransformNamesMultipliers JsonList<ThrustTransformGroup> The thrust transform names and their thrust multipliers for this engine mode. Overrides thrustVectorTransformName when set.
---@field throttleLocked boolean Gets a value indicating whether the throttle is locked for this engine mode, as with a solid rocket booster.
---@field ignitionThreshold number The minimum propellant flow fraction below which the engine flames out. Defaults to 10%.
---@field clampPropReceived boolean Gets a value indicating whether the received propellant fraction is clamped to the minimum ratio rather than always requesting the full amount.
---@field clampPropReceivedMinLowerAmount number The lower bound fraction applied when clampPropReceived is active, below which the received propellant ratio is not further clamped.
---@field allowRestart boolean Gets a value indicating whether the engine can be restarted after shutdown in this mode.
---@field allowShutdown boolean Gets a value indicating whether the engine can be shut down while running in this mode.
---@field shieldedCanActivate boolean Gets a value indicating whether the engine can be activated while shielded from the airstream, such as inside a fairing.
---@field atmosphereCurve FloatCurve A curve mapping atmospheric pressure in atm to an ISP multiplier, used to interpolate thrust between sea-level and vacuum values.
---@field useThrustCurve boolean Gets a value indicating whether a thrust curve based on remaining resource fraction is applied.
---@field thrustCurve FloatCurve The curve applied to scale thrust based on remaining propellant fraction when useThrustCurve is true.
---@field disableUnderwater boolean Gets a value indicating whether this engine is disabled when the part is submerged.
---@field nonThrustMotor boolean Gets a value indicating whether this engine mode is excluded from delta-V calculations.
---@field minThrust number The minimum thrust output in kilonewtons produced at zero throttle.
---@field maxThrust number The maximum thrust output in kilonewtons produced at full throttle.
---@field engineType EngineType The classification of this engine, such as liquid, solid, or jet.
---@field propellant PropellantDefinition The propellant definition describing the fuel and oxidizer consumed by this engine mode.
---@field useEngineResponseTime boolean Gets a value indicating whether engine acceleration and deceleration response time variables are applied.
---@field engineAccelerationSpeed number The rate at which the engine increases thrust output, expressed as a fraction of maximum thrust per second.
---@field engineDecelerationSpeed number The rate at which the engine decreases thrust output, expressed as a fraction of maximum thrust per second.
---@field GenerateHeat boolean Gets a value indicating whether this engine mode generates heat.
---@field HeatAtmosphereCurve FloatCurve A curve mapping atmospheric pressure in atm to a heat production multiplier.
---@field NormalizeHeatForFlow boolean Gets a value indicating whether the heat produced is divided by the flow multiplier so that a given throttle setting always yields the same heat flux.
---@field exhaustDamage boolean Gets a value indicating whether the engine heats up and applies force to parts in its exhaust path.
---@field exhaustDamageRadiusMultiplier number A multiplier applied to the exhaust damage radius, which is derived from the part size category.
---@field ExhaustDamageValue number The heat added to a part by exhaust contact, in kilowatts.
---@field exhaustDamageLogEvent boolean Gets a value indicating whether exhaust damage events are written to the debug log.
---@field exhaustSplashbackDamage boolean Gets a value indicating whether the engine itself receives heating from exhaust splashback.
---@field exhaustDamageFalloffPower number The exponent controlling how exhaust damage falls off with distance.
---@field exhaustDamageSplashbackFallofPower number The exponent controlling how exhaust splashback damage falls off with distance.
---@field exhaustDamageSplashbackMult number A multiplier scaling splashback damage per newton of thrust produced.
---@field exhaustDamageSplashbackMaxMutliplier number The upper bound on the splashback damage multiplier.
---@field exhaustDamageDistanceOffset number The distance from the thrust transform at which exhaust damage begins, in meters.
---@field exhaustDamageMaxRange number The maximum range in meters over which exhaust damage is applied.
---@field exhaustDamageMaxMutliplier number The upper bound on the combined exhaust damage multiplier.
---@field exhaustShockwave boolean Gets a value indicating whether this engine mode produces a ground shockwave.
---@field exhaustShockwaveLogEvent boolean Gets a value indicating whether shockwave damage events are written to the debug log.
---@field exhaustShockwaveInterval number The time interval in seconds between shockwave pulses. A value of -1 causes the shockwave to occur continuously.
---@field exhaustShockwaveMultiplier number A multiplier scaling the shockwave force in newtons used for damage calculations.
---@field exhaustShockwaveFalloffPower number The exponent controlling how shockwave damage falls off with distance.
---@field exhaustShockwaveDistanceOffset number The distance from the thrust transform at which shockwave damage begins, in meters.
---@field exhaustShockwaveMaxRange number The maximum range in meters over which shockwave damage is applied.
---@field exhaustShockwaveMaxMultiplier number The upper bound on the combined shockwave damage multiplier.
---@field throttleUseAlternate boolean Gets a value indicating whether this engine mode uses the alternate throttle response system.
---@field throttleResponseRate number The rate at which the alternate throttle system responds to throttle input changes, -1 uses the engine default.
---@field throttleIgniteLevelMult number A multiplier applied to the throttle level while the engine is in the ignition phase.
---@field throttleStartupMult number A multiplier applied to the throttle level while the engine is starting up.
---@field throttleStartedMult number A multiplier applied to the throttle level while the engine is fully running.
---@field throttleInstantShutdown boolean A flag indicating whether the engine shuts down instantly when the throttle drops to zero, bypassing the shutdown ramp.
---@field throttleShutdownMult number A multiplier controlling how quickly the throttle ramps down when the engine shuts down.
---@field throttleInstant boolean A flag indicating whether throttle level changes take effect instantly without ramping.
---@field throttlingBaseRate number The base rate used in the alternate throttle ramping calculation.
---@field throttlingBaseClamp number The upper clamp value applied in the alternate throttle ramping calculation.
---@field throttlingBaseDivisor number The divisor applied in the alternate throttle ramping calculation.
---@field atmChangeFlow boolean Gets a value indicating whether atmospheric density changes the fuel flow rate and thus thrust.
---@field atmCurve FloatCurve A curve mapping normalized atmospheric density to a thrust multiplier. Used when useAtmCurve is true.
---@field useAtmCurve boolean Gets a value indicating whether atmCurve is used to scale thrust. When false and atmChangeFlow is true, atmospheric density scales thrust linearly.
---@field velCurve FloatCurve A curve mapping Mach number to a thrust multiplier. Used when useVelCurve is true.
---@field useVelCurve boolean Gets a value indicating whether velCurve is used to scale thrust by Mach number.
---@field CLAMP number The minimum value the flow multiplier is clamped to before thrust is computed.
---@field atmCurveIsp FloatCurve A curve mapping normalized atmospheric density to an ISP multiplier. Used when useAtmCurveIsp is true.
---@field useAtmCurveIsp boolean Gets a value indicating whether atmCurveIsp is used to modify ISP by atmospheric density.
---@field velCurveIsp FloatCurve A curve mapping Mach number to an ISP multiplier. Used when useVelCurveIsp is true.
---@field useVelCurveIsp boolean Gets a value indicating whether velCurveIsp is used to modify ISP by Mach number.
---@field flameoutBar number The flow multiplier threshold below which the engine flames out.
---@field flowMultCap number The flow multiplier value beyond which further increases are tapered rather than applied linearly.
---@field flowMultCapSharpness number Controls how sharply the tapering is applied when the flow multiplier exceeds flowMultCap.
---@field multFlow number A final multiplier applied to the computed fuel flow.
---@field multIsp number A final multiplier applied to the computed specific impulse.
---@field engineSpoolTime number The time in seconds for the turbine to spool up, used to drive engine spool FX.
---@field engineSpoolIdle number The normalized turbine spool fraction maintained while the engine is idling.
---@field ModeExitWaitTime number The time in seconds to wait when exiting this engine mode.
---@field ModeExitRunningWaitTime number The time in seconds to wait when exiting the running state within this engine mode.
---@field ModeEnterWaitTime number The time in seconds to wait when entering this engine mode.
---@field ModeEnterRunningWaitTime number The time in seconds to wait when entering the running state within this engine mode.
---@field DeactivateEngineWaitTime number The time in seconds to wait when deactivating this engine mode.
---@field ActivateEngineWaitTime number The time in seconds to wait when activating this engine mode.
---@field RunAnimationOnActivateDeactivate boolean Gets a value indicating whether the deploy or retract animation is played on engine activation and deactivation.
---@field useThrottleIspCurve boolean Gets a value indicating whether the throttle ISP curve is applied.
---@field throttleIspCurveAtmStrength FloatCurve A curve mapping atmospheric pressure in atm to the blend weight used when applying throttleIspCurve.
---@field throttleIspCurve FloatCurve A curve mapping throttle fraction to an ISP multiplier, blended by throttleIspCurveAtmStrength.

---@alias Data_Engine_EngineMode _Data_Engine_EngineMode | { engineID: string, EngineDisplayName: string, thrustVectorTransformName: string, ThrustTransformNamesMultipliers: JsonList<ThrustTransformGroup>, throttleLocked: boolean, ignitionThreshold: number, clampPropReceived: boolean, clampPropReceivedMinLowerAmount: number, allowRestart: boolean, allowShutdown: boolean, shieldedCanActivate: boolean, atmosphereCurve: FloatCurve, useThrustCurve: boolean, thrustCurve: FloatCurve, disableUnderwater: boolean, nonThrustMotor: boolean, minThrust: number, maxThrust: number, engineType: EngineType, propellant: PropellantDefinition, useEngineResponseTime: boolean, engineAccelerationSpeed: number, engineDecelerationSpeed: number, GenerateHeat: boolean, HeatAtmosphereCurve: FloatCurve, NormalizeHeatForFlow: boolean, exhaustDamage: boolean, exhaustDamageRadiusMultiplier: number, ExhaustDamageValue: number, exhaustDamageLogEvent: boolean, exhaustSplashbackDamage: boolean, exhaustDamageFalloffPower: number, exhaustDamageSplashbackFallofPower: number, exhaustDamageSplashbackMult: number, exhaustDamageSplashbackMaxMutliplier: number, exhaustDamageDistanceOffset: number, exhaustDamageMaxRange: number, exhaustDamageMaxMutliplier: number, exhaustShockwave: boolean, exhaustShockwaveLogEvent: boolean, exhaustShockwaveInterval: number, exhaustShockwaveMultiplier: number, exhaustShockwaveFalloffPower: number, exhaustShockwaveDistanceOffset: number, exhaustShockwaveMaxRange: number, exhaustShockwaveMaxMultiplier: number, throttleUseAlternate: boolean, throttleResponseRate: number, throttleIgniteLevelMult: number, throttleStartupMult: number, throttleStartedMult: number, throttleInstantShutdown: boolean, throttleShutdownMult: number, throttleInstant: boolean, throttlingBaseRate: number, throttlingBaseClamp: number, throttlingBaseDivisor: number, atmChangeFlow: boolean, atmCurve: FloatCurve, useAtmCurve: boolean, velCurve: FloatCurve, useVelCurve: boolean, CLAMP: number, atmCurveIsp: FloatCurve, useAtmCurveIsp: boolean, velCurveIsp: FloatCurve, useVelCurveIsp: boolean, flameoutBar: number, flowMultCap: number, flowMultCapSharpness: number, multFlow: number, multIsp: number, engineSpoolTime: number, engineSpoolIdle: number, ModeExitWaitTime: number, ModeExitRunningWaitTime: number, ModeEnterWaitTime: number, ModeEnterRunningWaitTime: number, DeactivateEngineWaitTime: number, ActivateEngineWaitTime: number, RunAnimationOnActivateDeactivate: boolean, useThrottleIspCurve: boolean, throttleIspCurveAtmStrength: FloatCurve, throttleIspCurve: FloatCurve }

---Represents a propellant configuration defining a fuel mixture name, multiplier, and ingredient overrides for a module.
---@class _PropellantDefinition : _JsonUserDataBase
---@field mixtureName string The name of the fuel mixture used by this propellant configuration.
---@field mixtureMultiplier number The scaling multiplier applied to the fuel mixture ratio for this propellant.
---@field ignoreForThrustCurve boolean Indicates whether this propellant is excluded from thrust curve calculations.
---@field ingredientOverrides JsonList<ResourceRecipeIngredientDefinitionOverride> Per-ingredient overrides that customize resource recipe ingredient definitions for this propellant.

---@alias PropellantDefinition _PropellantDefinition | { mixtureName: string, mixtureMultiplier: number, ignoreForThrustCurve: boolean, ingredientOverrides: JsonList<ResourceRecipeIngredientDefinitionOverride> }

---Represents a serializable override for a resource recipe ingredient definition, specifying name, units per recipe unit, and flow mode.
---@class _ResourceRecipeIngredientDefinitionOverride : _JsonUserDataBase
---@field name string The name of the resource ingredient.
---@field unitsPerRecipeUnit number The number of resource units consumed or produced per recipe unit.
---@field flowMode ResourceFlowMode The resource flow mode used for this ingredient.

---@alias ResourceRecipeIngredientDefinitionOverride _ResourceRecipeIngredientDefinitionOverride | { name: string, unitsPerRecipeUnit: number, flowMode: ResourceFlowMode }

---Represents a named thrust transform and its proportional contribution multiplier.
---@class _ThrustTransformGroup : _JsonUserDataBase
---@field ThrustTransformName string The name of the thrust transform in the model associated with this group.
---@field ThrustTransformMultiplier number The proportional contribution of this thrust transform, where the sum of all group multipliers must equal exactly 1.

---@alias ThrustTransformGroup _ThrustTransformGroup | { ThrustTransformName: string, ThrustTransformMultiplier: number }

---@alias ModuleDataPayload table | Data_Engine | Data_PartSwitch
