---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/ScienceLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/Converters/DiscoverablesConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/Converters/ExperimentConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/Converters/ScienceRegionsConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/UserData/DiscoverablesUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/UserData/ExperimentUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Science/UserData/ScienceRegionsUserData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ExperimentCore.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ExperimentDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/FlavorDescription.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ResearchLocation.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ScienceExperimentType.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ScienceSitutation.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/CelestialBodyScienceRegionsData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/CBSituationData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ScienceRegionDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/CelestialBodyBakedDiscoverables.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/CelestialBodyDiscoverablePosition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/TechNodeData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/TechRequiredResearchData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Science/ScienceReportType.cs

-- ============================================================
-- UserData wrappers
-- ============================================================

---Synthetic per-element wrapper for entries of a `DiscoverablesUserData`. The underlying converter does not
---override `Convert`, so each element is exposed as a generic `JsonUserData` carrying the
---`CelestialBodyDiscoverablePosition` schema shape.
---@class DiscoverablePositionUserData : _CelestialBodyDiscoverablePosition, JsonUserData

---Indexed-list wrapper for a science region's discoverables, keyed by each entry's `ScienceRegionId`, while
---preserving the full envelope for round-tripping.
---@class DiscoverablesUserData : IndexedListUserData<DiscoverablePositionUserData>
---@field BodyName string Gets the celestial body's name from the envelope's `BodyName` field.
local DiscoverablesUserData = {}

---Returns the lookup name for the given item.
---@param source any The item to extract the name from.
---@return string name The lookup name for the item.
function DiscoverablesUserData:Name(source) end

---Science experiment wrapper exposing the inner `Data` subtree while preserving the full envelope for round-tripping.
---@class ExperimentUserData : _ExperimentDefinition, JsonUserData

---Synthetic per-element wrapper for entries of a `ScienceRegionsUserData`. The underlying converter does not
---override `Convert`, so each element is exposed as a generic `JsonUserData` carrying the
---`ScienceRegionDefinition` schema shape.
---@class ScienceRegionUserData : _ScienceRegionDefinition, JsonUserData

---Indexed-list wrapper for a body's science regions, keyed by each region's `id`, while preserving the full
---envelope for round-tripping and exposing the body name and situation data as typed properties.
---@class ScienceRegionsUserData : IndexedListUserData<ScienceRegionUserData>
---@field BodyName string Gets the celestial body's name from the envelope's `BodyName` field.
---@field SituationData CBSituationData Gets or sets the situation data wrapping the envelope's `SituationData` field.
local ScienceRegionsUserData = {}

---Returns the lookup name for the given item.
---@param source any The item to extract the name from.
---@return string name The lookup name for the item.
function ScienceRegionsUserData:Name(source) end

---Synthetic wrapper around a tech-tree node's JSON. Tech nodes are patched through the generic JSON converter,
---so there is no purpose-built C# wrapper class -- this stub gives Lua scripts the typed `TechNodeData`
---field surface plus the inherited `JsonUserData` methods.
---@class TechNodeUserData : JsonUserData, _TechNodeData

-- ============================================================
-- Submodule
-- ============================================================

---Lua submodule exposed as `PM.Science`, providing patches and creation helpers for science discoverables,
---experiments, regions, and tech nodes.
---@class ScienceLuaModule
local ScienceLuaModule = {}

---Registers a discoverables-list patch with the given namespaced patch name.
---The patch matches every discoverables asset by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                                                          The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<DiscoverablesUserData, DiscoverablePositionUserData> patch The registered patch.
function ScienceLuaModule:PatchDiscoverables(name) end

---Registers a science-experiment patch with the given namespaced patch name.
---The patch matches every experiment by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                                  The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<ExperimentUserData, any> patch     The registered patch.
function ScienceLuaModule:PatchExperiments(name) end

---Creates a new science experiment with the given name and runs callback against it for further configuration.
---@param name string                              The experiment name.
---@param callback fun(data: ExperimentUserData)   Callback that receives the new experiment for further configuration.
function ScienceLuaModule:NewExperiment(name, callback) end

---Registers a science-region patch with the given namespaced patch name.
---The patch matches every region asset by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                                                      The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<ScienceRegionsUserData, ScienceRegionUserData> patch   The registered patch.
function ScienceLuaModule:PatchRegions(name) end

---Registers a tech-tree-node patch with the given namespaced patch name.
---The patch matches every tech-tree node by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                              The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<TechNodeUserData, any> patch   The registered patch.
function ScienceLuaModule:PatchTechNodes(name) end

---Adds the given part IDs to the tech node named nodeName's `UnlockedPartIds` list.
---@param nodeName string The tech node name.
---@param ... string      The part IDs to append.
function ScienceLuaModule:AddPartsToTechNode(nodeName, ...) end

-- ============================================================
-- Asset JSON schemas
-- ============================================================

---Represents the serializable core data for a science experiment, including a version stamp and an ExperimentDefinition.
---Top-level envelope at addressables label `scienceExperiment`.
---@class _ExperimentCore : _JsonUserDataBase
---@field version number
---@field data ExperimentDefinition

---@alias ExperimentCore _ExperimentCore | { version: number, data: ExperimentDefinition }

---Represents the definition of a science experiment, including its valid research locations, type, and associated data and sample values.
---This is the shape of the inner `data` subtree (what ExperimentUserData wraps).
---@class _ExperimentDefinition : _JsonUserDataBase
---@field ExperimentID string
---@field DisplayName string
---@field DisplayRequirements string
---@field ExperimentType ScienceExperimentType
---@field DataFlavorDescriptions JsonList<FlavorDescription>
---@field SampleFlavorDescriptions JsonList<FlavorDescription>
---@field DataReportDisplayName string
---@field SampleReportDisplayName string
---@field ValidLocations JsonList<ResearchLocation>
---@field DataValue number
---@field SampleValue number
---@field TransmissionSize number
---@field RequiresEVA boolean

---@alias ExperimentDefinition _ExperimentDefinition | { ExperimentID: string, DisplayName: string, DisplayRequirements: string, ExperimentType: ScienceExperimentType, DataFlavorDescriptions: JsonList<FlavorDescription>, SampleFlavorDescriptions: JsonList<FlavorDescription>, DataReportDisplayName: string, SampleReportDisplayName: string, ValidLocations: JsonList<ResearchLocation>, DataValue: number, SampleValue: number, TransmissionSize: number, RequiresEVA: boolean }

---Represents a flavor text description associated with a specific science research location.
---@class _FlavorDescription : _JsonUserDataBase
---@field ResearchLocationID string
---@field LocalizationTag string

---@alias FlavorDescription _FlavorDescription | { ResearchLocationID: string, LocalizationTag: string }

---Represents a science research location defined by a celestial body, science situation, and optional science region.
---@class _ResearchLocation : _JsonUserDataBase
---@field RequiresRegion boolean
---@field BodyName string
---@field ScienceSituation ScienceSitutation
---@field ScienceRegion string
---@field ResearchLocationId string

---@alias ResearchLocation _ResearchLocation | { RequiresRegion: boolean, BodyName: string, ScienceSituation: ScienceSitutation, ScienceRegion: string, ResearchLocationId: string }

---Represents the science regions data for a celestial body, including situation data and region definitions.
---Top-level envelope at addressables label `science_region`.
---@class _CelestialBodyScienceRegionsData : _JsonUserDataBase
---@field Version string
---@field BodyName string
---@field SituationData CBSituationData
---@field Regions JsonList<ScienceRegionDefinition>

---@alias CelestialBodyScienceRegionsData _CelestialBodyScienceRegionsData | { Version: string, BodyName: string, SituationData: CBSituationData, Regions: JsonList<ScienceRegionDefinition> }

---Represents altitude boundaries and science scalar multipliers for the science situations of a celestial body.
---@class _CBSituationData : _JsonUserDataBase
---@field HighOrbitMaxAltitude number
---@field LowOrbitMaxAltutude number
---@field AtmosphereMaxAltutude number
---@field CelestialBodyScalar number
---@field HighOrbitScalar number
---@field LowOrbitScalar number
---@field AtmosphereScalar number
---@field SplashedScalar number
---@field LandedScalar number

---@alias CBSituationData _CBSituationData | { HighOrbitMaxAltitude: number, LowOrbitMaxAltutude: number, AtmosphereMaxAltutude: number, CelestialBodyScalar: number, HighOrbitScalar: number, LowOrbitScalar: number, AtmosphereScalar: number, SplashedScalar: number, LandedScalar: number }

---Represents a science region definition, specifying situation scalars for atmosphere, splashed, and landed states.
---@class _ScienceRegionDefinition : _JsonUserDataBase
---@field Id string
---@field AtmosphereScalar number
---@field SplashedScalar number
---@field LandedScalar number
---@field MapId integer

---@alias ScienceRegionDefinition _ScienceRegionDefinition | { Id: string, AtmosphereScalar: number, SplashedScalar: number, LandedScalar: number, MapId: integer }

---Represents the baked set of discoverable positions associated with a named celestial body.
---Top-level envelope at addressables label `science_region_discoverables`.
---@class _CelestialBodyBakedDiscoverables : _JsonUserDataBase
---@field Version string
---@field BodyName string
---@field Discoverables JsonList<CelestialBodyDiscoverablePosition>

---@alias CelestialBodyBakedDiscoverables _CelestialBodyBakedDiscoverables | { Version: string, BodyName: string, Discoverables: JsonList<CelestialBodyDiscoverablePosition> }

---Represents a discoverable position on a celestial body, defined by a science region, a spatial location, and a detection radius.
---@class _CelestialBodyDiscoverablePosition : _JsonUserDataBase
---@field ScienceRegionId string
---@field Position Vector3d
---@field Radius number

---@alias CelestialBodyDiscoverablePosition _CelestialBodyDiscoverablePosition | { ScienceRegionId: string, Position: Vector3d, Radius: number }

---Represents the serialized data for a single tech tree node, including its display info, science point cost, unlock requirements, and position.
---Top-level shape at addressables label `techNodeData`.
---@class _TechNodeData : _JsonUserDataBase
---@field Version integer
---@field ID string
---@field NameLocKey string
---@field IconID string
---@field CategoryID string
---@field HiddenByNodeID string
---@field DescriptionLocKey string
---@field RequiredSciencePoints integer
---@field UnlockedPartsIDs JsonList<string>
---@field RequiredTechNodeIDs JsonList<string>
---@field RequiredMissionIDs JsonList<string>
---@field RequiredResearchData JsonList<TechRequiredResearchData>
---@field TierToUnlock integer
---@field TechTreePosition Vector2

---@alias TechNodeData _TechNodeData | { Version: integer, ID: string, NameLocKey: string, IconID: string, CategoryID: string, HiddenByNodeID: string, DescriptionLocKey: string, RequiredSciencePoints: integer, UnlockedPartsIDs: JsonList<string>, RequiredTechNodeIDs: JsonList<string>, RequiredMissionIDs: JsonList<string>, RequiredResearchData: JsonList<TechRequiredResearchData>, TierToUnlock: integer, TechTreePosition: Vector2 }

---Represents research conditions required to unlock a technology node, including experiment, location, and report type.
---@class _TechRequiredResearchData : _JsonUserDataBase
---@field ExperimentID string
---@field ResearchLocationID? string
---@field ResearchReportType? ScienceReportType

---@alias TechRequiredResearchData _TechRequiredResearchData | { ExperimentID: string, ResearchLocationID: string?, ResearchReportType: ScienceReportType? }
