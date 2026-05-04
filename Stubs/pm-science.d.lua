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

-- ============================================================
-- UserData wrappers
-- ============================================================

---Synthetic per-element wrapper for entries of a `DiscoverablesUserData`.
---@class DiscoverablePositionUserData : JsonUserData, CelestialBodyDiscoverablePosition

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
---@class ExperimentUserData : JsonUserData, ExperimentDefinition

---Synthetic per-element wrapper for entries of a `ScienceRegionsUserData`.
---@class ScienceRegionUserData : JsonUserData, ScienceRegionDefinition

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
---@class TechNodeUserData : JsonUserData, TechNodeData

-- ============================================================
-- Submodule
-- ============================================================

---Lua submodule exposed as `PM.Science`, providing patches and creation helpers for science discoverables,
---experiments, regions, and tech nodes.
---@class ScienceLuaModule
local ScienceLuaModule = {}

---Registers a patch that runs against every science region's discoverables list.
---@param callback fun(data: DiscoverablesUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                          The registered patch.
function ScienceLuaModule:PatchAllDiscoverables(callback) end

---Registers a patch that runs against the discoverables list matching name.
---@param name string                                        The discoverables-asset name pattern (supports `*` and `?` wildcards).
---@param callback fun(data: DiscoverablesUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                          The registered patch.
function ScienceLuaModule:PatchDiscoverables(name, callback) end

---Registers a patch that runs against every science experiment.
---@param callback fun(data: ExperimentUserData): string? The patch callback. Returns `"remove"` to delete the experiment, `nil` to keep it.
---@return LuaPatch                                       The registered patch.
function ScienceLuaModule:PatchAllExperiments(callback) end

---Registers a patch that runs against the science experiment matching name.
---@param name string                                     The experiment name pattern (supports `*` and `?` wildcards).
---@param callback fun(data: ExperimentUserData): string? The patch callback. Returns `"remove"` to delete the experiment, `nil` to keep it.
---@return LuaPatch                                       The registered patch.
function ScienceLuaModule:PatchExperiment(name, callback) end

---Creates a new science experiment with the given name and runs callback against it for
---further configuration.
---@param name string                              The experiment name.
---@param callback fun(data: ExperimentUserData)   Callback that receives the new experiment for further configuration.
function ScienceLuaModule:NewExperiment(name, callback) end

---Registers a patch that runs against every science-region asset.
---@param callback fun(data: ScienceRegionsUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                           The registered patch.
function ScienceLuaModule:PatchAllRegions(callback) end

---Registers a patch that runs against the science-region asset matching name.
---@param name string                                         The region-asset name pattern (supports `*` and `?` wildcards).
---@param callback fun(data: ScienceRegionsUserData): string? The patch callback. Returns `"remove"` to delete the asset, `nil` to keep it.
---@return LuaPatch                                           The registered patch.
function ScienceLuaModule:PatchRegions(name, callback) end

---Registers a JSON patch that runs against every tech-tree node.
---@param callback fun(data: TechNodeUserData): string? The patch callback. Returns `"remove"` to delete the node, `nil` to keep it.
---@return LuaPatch                                    The registered patch.
function ScienceLuaModule:PatchAllTechNodes(callback) end

---Registers a JSON patch that runs against the tech-tree node matching name.
---@param name string                                  The tech node name pattern (supports `*` and `?` wildcards).
---@param callback fun(data: TechNodeUserData): string? The patch callback. Returns `"remove"` to delete the node, `nil` to keep it.
---@return LuaPatch                                    The registered patch.
function ScienceLuaModule:PatchTechNode(name, callback) end

---Adds the given part IDs to the tech node named nodeName's `UnlockedPartIds` list.
---@param nodeName string The tech node name.
---@param ... string      The part IDs to append.
function ScienceLuaModule:AddPartsToTechNode(nodeName, ...) end

-- ============================================================
-- Asset JSON schemas
-- ============================================================

---Represents the serializable core data for a science experiment, including a version stamp and an ExperimentDefinition.
---Top-level envelope at addressables label `scienceExperiment`.
---@class ExperimentCore : JsonUserData
---@field version number
---@field data ExperimentDefinition

---Represents the definition of a science experiment, including its valid research locations, type, and associated data and sample values.
---This is the shape of the inner `data` subtree (what ExperimentUserData wraps).
---@class ExperimentDefinition : JsonUserData
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

---Represents a flavor text description associated with a specific science research location.
---@class FlavorDescription : JsonUserData
---@field ResearchLocationID string
---@field LocalizationTag string

---Represents a science research location defined by a celestial body, science situation, and optional science region.
---@class ResearchLocation : JsonUserData
---@field RequiresRegion boolean
---@field BodyName string
---@field ScienceSituation ScienceSitutation
---@field ScienceRegion string
---@field ResearchLocationId string

---Represents the science regions data for a celestial body, including situation data and region definitions.
---Top-level envelope at addressables label `science_region`.
---@class CelestialBodyScienceRegionsData : JsonUserData
---@field Version string
---@field BodyName string
---@field SituationData CBSituationData
---@field Regions JsonList<ScienceRegionDefinition>

---Represents altitude boundaries and science scalar multipliers for the science situations of a celestial body.
---@class CBSituationData : JsonUserData
---@field HighOrbitMaxAltitude number
---@field LowOrbitMaxAltutude number
---@field AtmosphereMaxAltutude number
---@field CelestialBodyScalar number
---@field HighOrbitScalar number
---@field LowOrbitScalar number
---@field AtmosphereScalar number
---@field SplashedScalar number
---@field LandedScalar number

---Represents a science region definition, specifying situation scalars for atmosphere, splashed, and landed states.
---@class ScienceRegionDefinition : JsonUserData
---@field Id string
---@field AtmosphereScalar number
---@field SplashedScalar number
---@field LandedScalar number
---@field MapId integer

---Represents the baked set of discoverable positions associated with a named celestial body.
---Top-level envelope at addressables label `science_region_discoverables`.
---@class CelestialBodyBakedDiscoverables : JsonUserData
---@field Version string
---@field BodyName string
---@field Discoverables JsonList<CelestialBodyDiscoverablePosition>

---Represents a discoverable position on a celestial body, defined by a science region, a spatial location, and a detection radius.
---@class CelestialBodyDiscoverablePosition : JsonUserData
---@field ScienceRegionId string
---@field Position Vector3d
---@field Radius number

---Represents the serialized data for a single tech tree node, including its display info, science point cost, unlock requirements, and position.
---Top-level shape at addressables label `techNodeData`.
---@class TechNodeData : JsonUserData
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

---Represents research conditions required to unlock a technology node, including experiment, location, and report type.
---@class TechRequiredResearchData : JsonUserData
---@field ExperimentID string
---@field ResearchLocationID? string
---@field ResearchReportType? ScienceReportType
