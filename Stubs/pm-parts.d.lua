---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/PartsLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/PartsUtilities.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/Converters/PartConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/Attributes/ModuleDataAdapterAttribute.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/UserData/PartUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/UserData/ModuleUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/UserData/ModesUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/UserData/EngineUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Parts/UserData/ResourceContainersUserData.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/PartCore.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/PartData.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/SerializedPartModule.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/SerializedModuleData.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/SerializedResourceInfo.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/PartResourceCostDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/AttachRules.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/AttachNodeDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/ResourceSystem/ContainedResourceDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeIngredientDefinitionOverride.cs
-- Source: ksp2redux/Assets/Code/KSP/Modules/Data_Engine.cs
-- Source: ksp2redux/Assets/Code/KSP/Modules/PropellantDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Modules/ThrustTransformGroup.cs
-- Source: ksp2redux/Assets/Code/KSP/game/PartsManagerCore.cs

-- ============================================================================
-- Wrapper UserData
-- ============================================================================

---@class _PartUserDataModuleIndexer
---@field [string] ModuleUserData

---Part definition wrapper exposing the inner `data` subtree, the typed `resourceContainers` array, and
---each part module as a virtual property keyed by the module's short name.
---@class PartUserData : _PartData, _PartUserDataModuleIndexer, ExtensibleJsonUserData
---@field resourceContainers ResourceContainersUserData The typed wrapper around the part's `resourceContainers` array.
local PartUserData = {}

---Adds a new part module of the given type and runs callback against it for further configuration.
---@param moduleType string                       The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData)    Callback that receives the new module for further configuration.
---@error Thrown when moduleType is not a known component module.
function PartUserData:AddModule(moduleType, callback) end

---Runs callback against the existing module of the given type, doing nothing if absent.
---@param moduleType string                       The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData)    Callback that receives the existing module for further configuration.
function PartUserData:PatchModule(moduleType, callback) end

---Patches the named module if it exists, otherwise adds it.
---@param moduleType string                       The module's short name (with or without the `PartComponent` prefix).
---@param callback fun(module: ModuleUserData)    Callback that receives the module for further configuration.
function PartUserData:EnsureModule(moduleType, callback) end

---Removes the named module from the part and rebuilds the module-index map.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
function PartUserData:RemoveModule(moduleType) end

---Returns whether the part has a module of the given type.
---@param moduleType string The module's short name (with or without the `PartComponent` prefix).
---@return boolean present True if a module of that type is registered, false otherwise.
function PartUserData:HasModule(moduleType) end

---@class _ModuleUserDataDataIndexer
---@field [string] EngineUserData | JsonUserData
---@field [integer] EngineUserData | JsonUserData

---Standalone wrapper for a part module's serialized JSON, exposing each `ModuleData` entry by name with a
---typed adapter when one is registered for the data type.
---Iterable with `pairs()`; each iteration yields `(name, value)` pairs in declaration order.
---@class ModuleUserData : _SerializedPartModule, _ModuleUserDataDataIndexer, JsonUserData
---@field Count integer Gets the number of data entries on this module.
local ModuleUserData = {}

---Refreshes the data-name and conversion caches from the current state of the module's `ModuleData` array.
function ModuleUserData:RefreshData() end

---Adds a new module-data entry of the given type and runs callback against it when supplied.
---@param type string                                       The data module's short name as registered in `PartsUtilities.DataModules`.
---@param callback? fun(entry: EngineUserData|JsonUserData) Optional callback that receives the new entry for further configuration.
---@error Thrown when type is not a registered data module.
function ModuleUserData:AddData(type, callback) end

---Runs callback against the existing data entry of the given type, doing nothing if absent.
---@param type string                                      The data module's short name.
---@param callback fun(entry: EngineUserData|JsonUserData) Callback that receives the existing entry for further configuration.
function ModuleUserData:PatchData(type, callback) end

---Patches the named data entry if it exists, otherwise adds it.
---@param type string                                      The data module's short name.
---@param callback fun(entry: EngineUserData|JsonUserData) Callback that receives the entry for further configuration.
function ModuleUserData:EnsureData(type, callback) end

---Removes the data entry of the given type from the module.
---@param type string The data module's short name.
function ModuleUserData:RemoveData(type) end

---Returns whether the module has a data entry of the given type.
---@param type string The data module's short name.
---@return boolean present True if an entry exists, false otherwise.
function ModuleUserData:HasData(type) end

---Synthetic per-element wrapper for entries of a `ModesUserData`.
---@class EngineModeUserData : _Data_Engine_EngineMode, JsonUserData

---Indexed-list wrapper for an engine's `engineModes` array, keyed by each mode's `engineID`.
---@class ModesUserData : IndexedListUserData<EngineModeUserData>
local ModesUserData = {}

---Adds a new engine mode with the given `engineID` and runs callback against it for further configuration.
---@param mode string                              The new mode's `engineID`.
---@param callback fun(mode: EngineModeUserData)   Callback that receives the new mode for further configuration.
function ModesUserData:Add(mode, callback) end

---`Data_Engine` module-data adapter that exposes the engine's `engineModes` array as a typed
---ModesUserData rather than a raw JsonUserData.
---Wraps the `Data_Engine` module-data type (its JSON shape lives at `data["DataObject"]`).
---@class EngineUserData : _Data_Engine, ExtensibleJsonUserData
---@field engineModes ModesUserData The engine's `engineModes` array, exposed as a typed `ModesUserData`.

---Synthetic per-element wrapper for entries of a `ResourceContainersUserData`.
---@class ResourceContainerUserData : _ContainedResourceDefinition, JsonUserData

---Indexed-list wrapper for a part's `resourceContainers` array, keyed by each container's `name`.
---@class ResourceContainersUserData : IndexedListUserData<ResourceContainerUserData>
local ResourceContainersUserData = {}

---Adds a new resource container with the given resource type, capacity, and optional initial fill.
---@param type string             The resource name to store.
---@param capacity number         The container's capacity in units.
---@param initial? number         The initial fill in units; defaults to 0.
---@param nonStageable? boolean   Whether the container is exempt from staging; defaults to false.
function ResourceContainersUserData:Add(type, capacity, initial, nonStageable) end

---Returns whether the part contains a resource container of the given type.
---@param type string The resource name to look up.
---@return boolean present True if such a container exists, false otherwise.
function ResourceContainersUserData:Has(type) end

-- ============================================================================
-- Submodule
-- ============================================================================

---Lua submodule exposed as `PM.Parts`, providing patches for part definitions.
---@class PartsLuaModule
local PartsLuaModule = {}

---Registers a part patch with the given namespaced patch name.
---The patch matches every part by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                                  The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<PartUserData, ModuleUserData> patch The registered patch.
function PartsLuaModule:Patch(name) end

-- ============================================================================
-- Asset JSON schemas (the on-disk PartCore tree)
-- ============================================================================

---Represents the top-level container for a serialized part definition, holding part and module data with versioned JSON deserialization support.
---@class _PartCore : _JsonUserDataBase
---@field version number
---@field useExternal boolean
---@field data PartData
---@field modules JsonList<ModuleDataPayload>
---@field legacyModules string

---@alias PartCore _PartCore | { version: number, useExternal: boolean, data: PartData, modules: JsonList<ModuleDataPayload>, legacyModules: string }

---Represents the serializable data definition for a spacecraft part, including physical properties, attachment rules, resource containers, part modules, and OAB editor settings.
---@class _PartData : _JsonUserDataBase
---@field partName string A unique key for the part - appears in save files, not user facing.
---@field author string The name of the person or people working on the part.
---@field category PartCategories
---@field family string
---@field childStageOffset integer
---@field cost integer
---@field crewCapacity integer
---@field stageOffset integer
---@field isCompound boolean
---@field sizeCategory MetaAssemblySizeFilterType
---@field stageType AssemblyPartStageType
---@field resourceCosts JsonList<PartResourceCostDefinition>
---@field tags string
---@field stagingIconAssetAddress string
---@field PartSizeDiameter number
---@field angularDrag number
---@field breakingForce number
---@field breakingTorque number
---@field buoyancy number
---@field buoyancyUseSine boolean
---@field coLiftOffset Vector3
---@field coMassOffset Vector3
---@field coPressureOffset Vector3
---@field coBuoyancy Vector3
---@field coDisplacement Vector3
---@field crashTolerance number
---@field explosionPotential number
---@field fuelCrossFeed boolean
---@field heatConductivity number
---@field mass number
---@field maxTemp number
---@field attachRules AttachRules
---@field attachNodes JsonList<AttachNodeDefinition>
---@field resourceContainers JsonList<ContainedResourceDefinition>
---@field AllowKinematicPhysicsIfIntersectTerrain boolean
---@field serializedPartModules JsonList<SerializedPartModule>
---@field resourceSummary SerializedResourceInfo
---@field PAMModuleSortOverride JsonList<SerializedPartModuleDisplayOrder> Parts Manager module sort overrides.
---@field PAMModuleVisualsOverride JsonList<SerializedPartModuleDisplayVisuals> Parts Manager module visual overrides.
---@field collisionVolumeBoundsScale Vector3
---@field emissiveConstant number
---@field maximumDrag number
---@field minimumDrag number
---@field physicsMode PartPhysicsModes
---@field inverseStageCarryover boolean
---@field skinMassPerArea number
---@field bodyLiftOnlyUnattachedLift boolean
---@field bodyLiftOnlyAttachName string
---@field maxLength integer
---@field radiatorHeadroom number
---@field radiatorMax number
---@field skinMaxTemp number
---@field skinInternalConductionMult number
---@field thermalMassModifier number
---@field buoyancyUseCubeNamed string
---@field HasReportStorage boolean
---@field oabEditorCategory OABEditorPartCategory
---@field partType AssemblyPartTypeFilter
---@field partHideMode OABPartHideMode Defines whether or not a part is hidden in the OAB, and if it is toggleable.
---@field PreferredOrientation OABOrientation Allows a part to specify what orientation it considers its primary. The mode specified will see the part oriented similarly to how it is in the prefab by default. The part will try to maintain this orientation regardless of VAB mode.
---@field MirrorTechnique MirrorTechnique Allows specifying what type of mirror logic is used for this part. May impact how other parts interact with it. The Auto setting is configurable on OAB.prefab.
---@field CanSuggestOrientation boolean Based on all of the other settings, from orientation preference to mirror technique and category, suggest a rotation for this part only if no use rotation is provided. By default, no suggestions are made.
---@field PickUpPointOffset Vector3 Offset at which the player cursor "grabs" the part when in OAB.
---@field PickupRotationPointOffset Vector3 Offset at which the part is rotated in the OAB when the player is grabbing a part.

---@alias PartData _PartData | { partName: string, author: string, category: PartCategories, family: string, childStageOffset: integer, cost: integer, crewCapacity: integer, stageOffset: integer, isCompound: boolean, sizeCategory: MetaAssemblySizeFilterType, stageType: AssemblyPartStageType, resourceCosts: JsonList<PartResourceCostDefinition>, tags: string, stagingIconAssetAddress: string, PartSizeDiameter: number, angularDrag: number, breakingForce: number, breakingTorque: number, buoyancy: number, buoyancyUseSine: boolean, coLiftOffset: Vector3, coMassOffset: Vector3, coPressureOffset: Vector3, coBuoyancy: Vector3, coDisplacement: Vector3, crashTolerance: number, explosionPotential: number, fuelCrossFeed: boolean, heatConductivity: number, mass: number, maxTemp: number, attachRules: AttachRules, attachNodes: JsonList<AttachNodeDefinition>, resourceContainers: JsonList<ContainedResourceDefinition>, AllowKinematicPhysicsIfIntersectTerrain: boolean, serializedPartModules: JsonList<SerializedPartModule>, resourceSummary: SerializedResourceInfo, PAMModuleSortOverride: JsonList<SerializedPartModuleDisplayOrder>, PAMModuleVisualsOverride: JsonList<SerializedPartModuleDisplayVisuals>, collisionVolumeBoundsScale: Vector3, emissiveConstant: number, maximumDrag: number, minimumDrag: number, physicsMode: PartPhysicsModes, inverseStageCarryover: boolean, skinMassPerArea: number, bodyLiftOnlyUnattachedLift: boolean, bodyLiftOnlyAttachName: string, maxLength: integer, radiatorHeadroom: number, radiatorMax: number, skinMaxTemp: number, skinInternalConductionMult: number, thermalMassModifier: number, buoyancyUseCubeNamed: string, HasReportStorage: boolean, oabEditorCategory: OABEditorPartCategory, partType: AssemblyPartTypeFilter, partHideMode: OABPartHideMode, PreferredOrientation: OABOrientation, MirrorTechnique: MirrorTechnique, CanSuggestOrientation: boolean, PickUpPointOffset: Vector3, PickupRotationPointOffset: Vector3 }

---Represents the resource cost of a part as a named resource and unit quantity.
---@class _PartResourceCostDefinition : _JsonUserDataBase
---@field name string
---@field resourceUnits number

---@alias PartResourceCostDefinition _PartResourceCostDefinition | { name: string, resourceUnits: number }

---Represents the attachment rules for a part, defining which attachment modes and behaviors are permitted.
---@class _AttachRules : _JsonUserDataBase
---@field stack boolean
---@field srfAttach boolean
---@field allowStack boolean
---@field allowSrfAttach boolean
---@field allowCollision boolean
---@field allowDock boolean
---@field allowRotate boolean
---@field allowRoot boolean

---@alias AttachRules _AttachRules | { stack: boolean, srfAttach: boolean, allowStack: boolean, allowSrfAttach: boolean, allowCollision: boolean, allowDock: boolean, allowRotate: boolean, allowRoot: boolean }

---Represents the definition of a part attachment node, including its position, orientation, type, and joint configuration.
---@class _AttachNodeDefinition : _JsonUserDataBase
---@field nodeID string Required. Must be unique for this node.
---@field NodeSymmetryGroupID string Optional field that can be used to group nodes together, eg. 2 downward facing nodes grouped into a 'bottom' group. The group ID would be the same on both nodes. Empty means no group, which is default behavior.
---@field nodeType AttachNodeType
---@field attachMethod AttachNodeMethod
---@field IsMultiJoint boolean Set true this attach node will create multiple PhysX joints.
---@field MultiJointMaxJoint integer The number of PhysX joints, only used if IsMultiJoint is true.
---@field MultiJointRadiusOffset number The radius to use when creating the PhysX joints, only used if IsMultiJoint is true.
---@field MultiJointOnSingleAxis boolean If True multi joints will be created along one axis instead of in a circle, only used if IsMultiJoint is true.
---@field SingleJointAxis TransformDirAxis The axis the joints will be credated along, only used if IsMultiJoint and MultiJointOnSingleAxis are true.
---@field MultiJointFullBreakStrength boolean The number of PhysX joints, only used if IsMultiJoint is true.
---@field position Vector3d
---@field orientation Vector3d
---@field size integer
---@field visualSize number
---@field isResourceCrossfeed boolean
---@field isRigid boolean
---@field angularStrengthMultiplier number
---@field contactArea number
---@field overrideDragArea number
---@field isCompoundJoint boolean

---@alias AttachNodeDefinition _AttachNodeDefinition | { nodeID: string, NodeSymmetryGroupID: string, nodeType: AttachNodeType, attachMethod: AttachNodeMethod, IsMultiJoint: boolean, MultiJointMaxJoint: integer, MultiJointRadiusOffset: number, MultiJointOnSingleAxis: boolean, SingleJointAxis: TransformDirAxis, MultiJointFullBreakStrength: boolean, position: Vector3d, orientation: Vector3d, size: integer, visualSize: number, isResourceCrossfeed: boolean, isRigid: boolean, angularStrengthMultiplier: number, contactArea: number, overrideDragArea: number, isCompoundJoint: boolean }

---Represents the definition of a resource contained within a part, including capacity, initial amount, and staging configuration.
---@class _ContainedResourceDefinition : _JsonUserDataBase
---@field name string
---@field capacityUnits number
---@field initialUnits number
---@field NonStageable boolean

---@alias ContainedResourceDefinition _ContainedResourceDefinition | { name: string, capacityUnits: number, initialUnits: number, NonStageable: boolean }

---Represents a serialized snapshot of a part module, storing its name, component type, behaviour type, and associated module data.
---@class _SerializedPartModule : _JsonUserDataBase
---@field Name string
---@field ComponentType string
---@field BehaviourType string
---@field ModuleData JsonList<ModuleDataPayload>

---@alias SerializedPartModule _SerializedPartModule | { Name: string, ComponentType: string, BehaviourType: string, ModuleData: JsonList<ModuleDataPayload> }

---Represents the serialized form of a `ModuleData` object for persistent storage.
---@class _SerializedModuleData : _JsonUserDataBase
---@field Name string
---@field DataType string
---@field DataObject ModuleDataPayload

---@alias SerializedModuleData _SerializedModuleData | { Name: string, DataType: string, DataObject: ModuleDataPayload }

---Represents the serialized resource relationship data for a part, describing the resources it consumes, generates, and contains.
---@class _SerializedResourceInfo : _JsonUserDataBase
---@field Consumes JsonList<string>
---@field Generates JsonList<string>
---@field Contains JsonList<string>

---@alias SerializedResourceInfo _SerializedResourceInfo | { Consumes: JsonList<string>, Generates: JsonList<string>, Contains: JsonList<string> }

---Serializable sort order entry that maps a part component module name to its display sort index in the parts manager.
---@class _SerializedPartModuleDisplayOrder : _JsonUserDataBase
---@field PartComponentModuleName string
---@field sortIndex integer

---@alias SerializedPartModuleDisplayOrder _SerializedPartModuleDisplayOrder | { PartComponentModuleName: string, sortIndex: integer }

---Serializable visual configuration for a part component module's display in the parts manager, including display name and header and footer visibility.
---@class _SerializedPartModuleDisplayVisuals : _JsonUserDataBase
---@field PartComponentModuleName string
---@field ModuleDisplayName string
---@field ShowHeader boolean
---@field ShowFooter boolean

---@alias SerializedPartModuleDisplayVisuals _SerializedPartModuleDisplayVisuals | { PartComponentModuleName: string, ModuleDisplayName: string, ShowHeader: boolean, ShowFooter: boolean }

-- ============================================================================
-- ModuleData payload schemas (each describes the `DataObject` shape)
-- ============================================================================

---Serializable data definition for `Module_Engine`. Schema for the `data["DataObject"]` sub-object of a `Data_Engine` module-data entry.
---@class _Data_Engine : _JsonUserDataBase
---@field DataType "KSP.Sim.Definitions.Data_Engine, Assembly-CSharp"
---@field IndependentThrottle boolean
---@field IndependentThrottlePercentage number
---@field activeEngineMode string
---@field EngineStatePriorChangeMode EngineState
---@field EngineChangingToMode integer
---@field EngineAutoSwitchMode boolean
---@field thrustPercentage number
---@field FinalThrustValue number
---@field RealISPValue number
---@field stagingOn boolean
---@field staged boolean
---@field Flameout boolean
---@field EngineIgnited boolean
---@field EngineShutdown boolean
---@field currentThrottle number
---@field thrustCurveDisplay number
---@field thrustCurveRatio number
---@field EngineSpool number
---@field ThrustDirRelativePartWorldSpace Vector3
---@field currentEngineModeIndex integer
---@field engineModes JsonList<Data_Engine_EngineMode>
---@field UseEmissive boolean
---@field EmissiveMaterialNames JsonList<string>
---@field EmissiveTemperatureCurve FloatCurve
---@field EmissiveLerpRateUp number
---@field EmissiveLerpRateDown number
---@field DeployedModeAnimationStateShortName string

---@alias Data_Engine _Data_Engine | { DataType: "KSP.Sim.Definitions.Data_Engine, Assembly-CSharp", IndependentThrottle: boolean, IndependentThrottlePercentage: number, activeEngineMode: string, EngineStatePriorChangeMode: EngineState, EngineChangingToMode: integer, EngineAutoSwitchMode: boolean, thrustPercentage: number, FinalThrustValue: number, RealISPValue: number, stagingOn: boolean, staged: boolean, Flameout: boolean, EngineIgnited: boolean, EngineShutdown: boolean, currentThrottle: number, thrustCurveDisplay: number, thrustCurveRatio: number, EngineSpool: number, ThrustDirRelativePartWorldSpace: Vector3, currentEngineModeIndex: integer, engineModes: JsonList<Data_Engine_EngineMode>, UseEmissive: boolean, EmissiveMaterialNames: JsonList<string>, EmissiveTemperatureCurve: FloatCurve, EmissiveLerpRateUp: number, EmissiveLerpRateDown: number, DeployedModeAnimationStateShortName: string }

---Represents the configuration for a single engine mode, including thrust, propellant, atmosphere, exhaust damage, and throttle parameters.
---@class _Data_Engine_EngineMode : _JsonUserDataBase
---@field engineID string The Engine ID. should be in English.
---@field EngineDisplayName string The Engine ID display name, should be a localization tag to get localized engine mode in the UI. This only appears in the UI for multi-mode engines.
---@field thrustVectorTransformName string The Thrust Transform name in the model for this engine mode. This is only used to find the thrust transforms if ThrustTransformNamesMultipliers list below is left empty.
---@field ThrustTransformNamesMultipliers JsonList<ThrustTransformGroup> The Thrust Transform names and Thrust multipliers in the model for this engine mode. This will override the thrustVectorTransformName field above.
---@field throttleLocked boolean Is the throttle locked in this engine mode? EG: SRB or can be changed by the player.
---@field ignitionThreshold number When will the engine fail from lack of propellants? Default = 0.1 or at 10% of required fuel or less the engine flames out.
---@field clampPropReceived boolean Do we clamp the return percent to the min ratio (and never request more on followups) or do we request all always, and average?
---@field clampPropReceivedMinLowerAmount number
---@field allowRestart boolean Can the engine be restarted in this mode? eg: SRB would be false.
---@field allowShutdown boolean Can the engine be shut down in this mode? eg: SRB would be false.
---@field shieldedCanActivate boolean Can the engine be be activated when shielded from airstream? ie: inside a fairing?
---@field atmosphereCurve FloatCurve A curve to determine loss or gain of thrust due to changes in atmosphere vs vacuum values are based on ISP to ATM Pressure.
---@field useThrustCurve boolean should we use a thrust curve (based on resource remaining)?
---@field thrustCurve FloatCurve The thrust curve to use if useThrustCurve is true.
---@field disableUnderwater boolean Is this engine disabled when under water?
---@field nonThrustMotor boolean If set to true this engine mode will not be included in Delta-V calculations.
---@field minThrust number Minimum Thrust in kN this engine produces at 0% throttle.
---@field maxThrust number Maximum Thrust in kN this engine produces at 100% throttle.
---@field engineType EngineType What type of engine is this?
---@field propellant PropellantDefinition
---@field useEngineResponseTime boolean Whether to apply the engine acceleration and deceleration speed variables.
---@field engineAccelerationSpeed number How quickly the engine can increase its thrust production, as a fraction of maximum/second.
---@field engineDecelerationSpeed number How quickly the engine can decrease its thrust production, as a fraction of maximum/second.
---@field GenerateHeat boolean Does this engine generate heat at all?
---@field HeatAtmosphereCurve FloatCurve Curve to adjust heat produced based on atmosphere pressure key (coordinate). X: Atmospheric Pressure. 1 = Kerbin Atmosphere at sea level. Y: Defines the heat production at the given atmosphere of pressure.
---@field NormalizeHeatForFlow boolean Do we divide the heat produced by the flow multiplier to get the final flux? i.e. do we always produce the same heat for the same throttle setting?
---@field exhaustDamage boolean Determines whether the engine heats up and pushes on parts that are arranged in its exhaust path.
---@field exhaustDamageRadiusMultiplier number A multiplier to the exhaust damage radius. The radius is calculated from the Part Size category * this multiplier.
---@field ExhaustDamageValue number The amount of heat added from exhaust to a part, in kW.
---@field exhaustDamageLogEvent boolean Whether damage from the engine exhaust is logged for debugging.
---@field exhaustSplashbackDamage boolean Whether the engine will receive heating from the exhaust splashing back.
---@field exhaustDamageFalloffPower number Adjusts the exponent of the exhaust damage distance falloff curve.
---@field exhaustDamageSplashbackFallofPower number Adjusts the exponent of the exhaust splashback damage distance falloff curve.
---@field exhaustDamageSplashbackMult number Adjusts the splashback damage multiplier per Newton of force produced.
---@field exhaustDamageSplashbackMaxMutliplier number The maximum amount of splashback damage that can occur.
---@field exhaustDamageDistanceOffset number Distance from the thrust transform where exhaust damage starts to occur.
---@field exhaustDamageMaxRange number Maximum range in meters that the exhaust damage is applied.
---@field exhaustDamageMaxMutliplier number Cap on the maximum multiplier to above factors that the exhaust damage can be at.
---@field exhaustShockwave boolean Whether this engine creates a shockwave.
---@field exhaustShockwaveLogEvent boolean Whether damage from shockwave events are logged for debugging.
---@field exhaustShockwaveInterval number Period of time between shockwaves. A value of -1 means this shockwave always occurs.
---@field exhaustShockwaveMultiplier number Adjusts the force in Newtons a shockwave produces for damage purposes.
---@field exhaustShockwaveFalloffPower number Adjusts the exponent of the shockwave damage distance falloff curve.
---@field exhaustShockwaveDistanceOffset number Distance from the thrust transform that the shockwave starts.
---@field exhaustShockwaveMaxRange number Maximum range in meters that shockwave damage is applied.
---@field exhaustShockwaveMaxMultiplier number Cap on the maximum multiplier that shockwave damage can be at.
---@field throttleUseAlternate boolean
---@field throttleResponseRate number
---@field throttleIgniteLevelMult number
---@field throttleStartupMult number
---@field throttleStartedMult number
---@field throttleInstantShutdown boolean
---@field throttleShutdownMult number
---@field throttleInstant boolean
---@field throttlingBaseRate number
---@field throttlingBaseClamp number
---@field throttlingBaseDivisor number
---@field atmChangeFlow boolean Atmospheric density will change fuel flow (and thus thrust).
---@field atmCurve FloatCurve Normally thrust is proportional to density, but we allow tuning. Tuning is especially needed because there's no stratosphere, so temperature keeps decreasing and thus speed of sound keeps decreasing.
---@field useAtmCurve boolean Do we use the atm curve? If not, and atmChangeFlow is true, just use atm linearly.
---@field velCurve FloatCurve replacement for the existing module's velocityCurve. Note that its x value is Mach, not m/s velocity. High-bypass turbofans will see thrust decrease steadily from static. Low-bypass turbofans and turbojets will see thrust decrease slightly up to about 0.2 Mach then increase steadily until the limit is reached (both in terms of heat, and incoming compression vs compressor compression). Ramjets have 0 static thrust, and do not light until 0.3 Mach or so, but once lit have steadily increasing thrust until Mach 5, when the incoming air can no longer be slowed to subsonic (combustion must be subsonic for ramjets). Thermal limits also apply, of course.
---@field useVelCurve boolean If false, we don't use the new velCurve above.
---@field CLAMP number tunable clamp. The flow multiplier will never go below this.
---@field atmCurveIsp FloatCurve Same as atmCurve, but changes Isp not flow.
---@field useAtmCurveIsp boolean Whether to use the atmCurveIsp curve above.
---@field velCurveIsp FloatCurve Same as velCurve but changes Isp not flow.
---@field useVelCurveIsp boolean Whether to use the velCurveIsp curve above.
---@field flameoutBar number When the flow multiplier goes below this, we Flameout the engine. NOTE: THIS FIXES ASYMMETRIC FLAMEOUTS.
---@field flowMultCap number cap beyond which increases to flow multiplier aren't fully felt (start to taper off).
---@field flowMultCapSharpness number Sharpness of the tapering off of flow increase beyond cap.
---@field multFlow number Multiplier to final flow as calculated.
---@field multIsp number Multiplier to final Isp as calculated.
---@field engineSpoolTime number This is the Turbine Spool Up time used for Spool Up Engine FX.
---@field engineSpoolIdle number
---@field ModeExitWaitTime number The time to wait when exiting this engine mode in seconds.
---@field ModeExitRunningWaitTime number The time to wait when exiting running state in this engine mode in seconds.
---@field ModeEnterWaitTime number The time to wait when entering this engine mode in seconds.
---@field ModeEnterRunningWaitTime number The time to wait when entering running state in this engine mode in seconds.
---@field DeactivateEngineWaitTime number The time to wait when deactivating this engine mode in seconds.
---@field ActivateEngineWaitTime number The time to wait when activating this engine mode in seconds.
---@field RunAnimationOnActivateDeactivate boolean Set this to true will run the Deploy/Retract animation on Activation and Deactivation of the engine.
---@field useThrottleIspCurve boolean Should we use the Throttle ISP curve?
---@field throttleIspCurveAtmStrength FloatCurve Modifies Isp based on throttle. Time is pressure in atm, value is how much throttling affects Isp (i.e. Isp = input * Lerp(1, throttleIspCurve, throttleIspCurveAtmStrength).
---@field throttleIspCurve FloatCurve Modifies Isp based on throttle. time is throttle, value is multiplier to Isp.

---@alias Data_Engine_EngineMode _Data_Engine_EngineMode | { engineID: string, EngineDisplayName: string, thrustVectorTransformName: string, ThrustTransformNamesMultipliers: JsonList<ThrustTransformGroup>, throttleLocked: boolean, ignitionThreshold: number, clampPropReceived: boolean, clampPropReceivedMinLowerAmount: number, allowRestart: boolean, allowShutdown: boolean, shieldedCanActivate: boolean, atmosphereCurve: FloatCurve, useThrustCurve: boolean, thrustCurve: FloatCurve, disableUnderwater: boolean, nonThrustMotor: boolean, minThrust: number, maxThrust: number, engineType: EngineType, propellant: PropellantDefinition, useEngineResponseTime: boolean, engineAccelerationSpeed: number, engineDecelerationSpeed: number, GenerateHeat: boolean, HeatAtmosphereCurve: FloatCurve, NormalizeHeatForFlow: boolean, exhaustDamage: boolean, exhaustDamageRadiusMultiplier: number, ExhaustDamageValue: number, exhaustDamageLogEvent: boolean, exhaustSplashbackDamage: boolean, exhaustDamageFalloffPower: number, exhaustDamageSplashbackFallofPower: number, exhaustDamageSplashbackMult: number, exhaustDamageSplashbackMaxMutliplier: number, exhaustDamageDistanceOffset: number, exhaustDamageMaxRange: number, exhaustDamageMaxMutliplier: number, exhaustShockwave: boolean, exhaustShockwaveLogEvent: boolean, exhaustShockwaveInterval: number, exhaustShockwaveMultiplier: number, exhaustShockwaveFalloffPower: number, exhaustShockwaveDistanceOffset: number, exhaustShockwaveMaxRange: number, exhaustShockwaveMaxMultiplier: number, throttleUseAlternate: boolean, throttleResponseRate: number, throttleIgniteLevelMult: number, throttleStartupMult: number, throttleStartedMult: number, throttleInstantShutdown: boolean, throttleShutdownMult: number, throttleInstant: boolean, throttlingBaseRate: number, throttlingBaseClamp: number, throttlingBaseDivisor: number, atmChangeFlow: boolean, atmCurve: FloatCurve, useAtmCurve: boolean, velCurve: FloatCurve, useVelCurve: boolean, CLAMP: number, atmCurveIsp: FloatCurve, useAtmCurveIsp: boolean, velCurveIsp: FloatCurve, useVelCurveIsp: boolean, flameoutBar: number, flowMultCap: number, flowMultCapSharpness: number, multFlow: number, multIsp: number, engineSpoolTime: number, engineSpoolIdle: number, ModeExitWaitTime: number, ModeExitRunningWaitTime: number, ModeEnterWaitTime: number, ModeEnterRunningWaitTime: number, DeactivateEngineWaitTime: number, ActivateEngineWaitTime: number, RunAnimationOnActivateDeactivate: boolean, useThrottleIspCurve: boolean, throttleIspCurveAtmStrength: FloatCurve, throttleIspCurve: FloatCurve }

---Represents a propellant configuration defining a fuel mixture name, multiplier, and ingredient overrides for a module.
---@class _PropellantDefinition : _JsonUserDataBase
---@field mixtureName string
---@field mixtureMultiplier number
---@field ignoreForThrustCurve boolean
---@field ingredientOverrides JsonList<ResourceRecipeIngredientDefinitionOverride>

---@alias PropellantDefinition _PropellantDefinition | { mixtureName: string, mixtureMultiplier: number, ignoreForThrustCurve: boolean, ingredientOverrides: JsonList<ResourceRecipeIngredientDefinitionOverride> }

---Represents a serializable override for a resource recipe ingredient definition, specifying name, units per recipe unit, and flow mode.
---@class _ResourceRecipeIngredientDefinitionOverride : _JsonUserDataBase
---@field name string
---@field unitsPerRecipeUnit number
---@field flowMode ResourceFlowMode

---@alias ResourceRecipeIngredientDefinitionOverride _ResourceRecipeIngredientDefinitionOverride | { name: string, unitsPerRecipeUnit: number, flowMode: ResourceFlowMode }

---Represents a named thrust transform and its proportional contribution multiplier.
---@class _ThrustTransformGroup : _JsonUserDataBase
---@field ThrustTransformName string The Thrust Transform name in the model for this ThrustTransform - Multiplier combo.
---@field ThrustTransformMultiplier number Proportional contribution of this Thrust Transform. The sum of all values must be equal to a value of exactly 1 (which represents 100%).

---@alias ThrustTransformGroup _ThrustTransformGroup | { ThrustTransformName: string, ThrustTransformMultiplier: number }

-- ============================================================================
-- Polymorphism: ModuleData payload union
-- ============================================================================

---Discriminator-driven union of every registered ModuleData payload type. Each member is the JSON
---shape of the `data["DataObject"]` sub-object for one ModuleData kind, narrowable by its literal
---`DataType` field. Grows as new ModuleData adapters get registered. The `table` member at the head
---of the union is the catch-all for unregistered ModuleData types (any module data whose C# type has
---no adapter stubbed); LuaLS falls back to it when none of the typed alternatives match.
---@alias ModuleDataPayload table | Data_Engine
