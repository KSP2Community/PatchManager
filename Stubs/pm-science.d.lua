---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/Science/UserData/DiscoverablesUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/UserData/ExperimentUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/UserData/ScienceRegionsUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/Converters/DiscoverablesConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/Converters/ExperimentConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/Converters/ScienceRegionsConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/ScienceLuaModule.cs
-- Source: Assets/Code/KSP/game/Science/ExperimentCore.cs
-- Source: Assets/Code/KSP/game/Science/ExperimentDefinition.cs
-- Source: Assets/Code/KSP/game/Science/FlavorDescription.cs
-- Source: Assets/Code/KSP/game/Science/ResearchLocation.cs
-- Source: Assets/Code/KSP/game/Science/CelestialBodyScienceRegionsData.cs
-- Source: Assets/Code/KSP/game/Science/CBSituationData.cs
-- Source: Assets/Code/KSP/game/Science/ScienceRegionDefinition.cs
-- Source: Assets/Code/KSP/game/Science/CelestialBodyBakedDiscoverables.cs
-- Source: Assets/Code/KSP/game/Science/CelestialBodyDiscoverablePosition.cs
-- Source: Assets/Code/KSP/game/Science/TechNodeData.cs
-- Source: Assets/Code/KSP/game/Science/TechRequiredResearchData.cs

---@class DiscoverablePositionUserData : _CelestialBodyDiscoverablePosition, JsonUserData

---Indexed-list wrapper for a science region's discoverables, keyed by each entry's `ScienceRegionId`, while
---preserving the full envelope for round-tripping.
---@class DiscoverablesUserData : IndexedListUserData<DiscoverablePositionUserData>
---@field BodyName string Gets the celestial body's name from the envelope's `BodyName` field.
---@field Layer string? Gets or sets the layer this copy of the body's discoverables belongs to, or `nil` for the default copy.

---Science experiment wrapper exposing the inner `data` subtree while preserving the full envelope for round-tripping.
---@class ExperimentUserData : _ExperimentDefinition, JsonUserData

---@class ScienceRegionUserData : _ScienceRegionDefinition, JsonUserData

---Indexed-list wrapper for a body's science regions, keyed by each region's `id`, while preserving the full
---envelope for round-tripping and exposing the body name and situation data as typed properties.
---@class ScienceRegionsUserData : IndexedListUserData<ScienceRegionUserData>
---@field BodyName string Gets the celestial body's name from the envelope's `BodyName` field.
---@field Layer string? Gets or sets the layer this copy of the body's science regions belongs to, or `nil` for the default copy.
---@field SituationData CBSituationData Gets or sets the situation data wrapping the envelope's `SituationData` field.

---@class TechNodeUserData : _TechNodeData, JsonUserData

---Lua submodule exposed as `PM.Science`, providing patches and creation helpers for science discoverables,
---experiments, regions, and tech nodes.
---@class ScienceLuaModule
local ScienceLuaModule = {}

---Registers a discoverables-list patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<DiscoverablesUserData, DiscoverablePositionUserData> patch The registered patch.
function ScienceLuaModule:PatchDiscoverables(name) end

---Registers a patch that copies each discoverables set it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the discoverables set to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: DiscoverablesUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<DiscoverablesUserData, DiscoverablePositionUserData> patch The registered patch.
function ScienceLuaModule:DuplicateDiscoverables(source, newName, patchMethod) end

---Registers a science-experiment patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<ExperimentUserData, any> patch The registered patch.
function ScienceLuaModule:PatchExperiments(name) end

---Registers a patch that copies each experiment it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the experiment to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: ExperimentUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<ExperimentUserData, any> patch The registered patch.
function ScienceLuaModule:DuplicateExperiments(source, newName, patchMethod) end

---Creates a new science experiment with the given name and runs callback against it for
---further configuration.
---@param name string The experiment name.
---@param callback fun(data: ExperimentUserData) Callback that receives the new experiment for further configuration.
function ScienceLuaModule:NewExperiment(name, callback) end

---Registers a science-region patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<ScienceRegionsUserData, ScienceRegionUserData> patch The registered patch.
function ScienceLuaModule:PatchRegions(name) end

---Registers a patch that copies each science region set it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the science region set to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: ScienceRegionsUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<ScienceRegionsUserData, ScienceRegionUserData> patch The registered patch.
function ScienceLuaModule:DuplicateRegions(source, newName, patchMethod) end

---Registers a tech-tree-node patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<TechNodeUserData, any> patch The registered patch.
function ScienceLuaModule:PatchTechNodes(name) end

---Registers a patch that copies each tech node it matches under a new name, then runs the chained `:Do` callback on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the tech node to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: TechNodeUserData): string? Shorthand for chaining `:Do(callback)`, which is the canonical form. Leave both off to copy without changes.
---@return PatchDefinition<TechNodeUserData, any> patch The registered patch.
function ScienceLuaModule:DuplicateTechNodes(source, newName, patchMethod) end

---Adds the given part IDs to the tech node named nodeName's `UnlockedPartIds` list.
---@param nodeName string The tech node name.
---@param ... string The part IDs to append.
function ScienceLuaModule:AddPartsToTechNode(nodeName, ...) end

---Represents the serializable core data for a science experiment, including a version stamp and an ExperimentDefinition.
---@class _ExperimentCore : _JsonUserDataBase
---@field version number The serialization format version recorded when this experiment instance was saved.
---@field data ExperimentDefinition The experiment definition containing the configuration and metadata for this experiment.

---@alias ExperimentCore _ExperimentCore | { version: number, data: ExperimentDefinition }

---Represents the definition of a science experiment, including its valid research locations, type, and associated data and sample values.
---@class _ExperimentDefinition : _JsonUserDataBase
---@field ExperimentID string The unique identifier string for this experiment.
---@field DisplayName string The human-readable display name for this experiment.
---@field DisplayRequirements string The human-readable description of the requirements to run this experiment.
---@field ExperimentType ScienceExperimentType The type of result produced by this experiment, indicating whether it yields data, a sample, or both.
---@field DataFlavorDescriptions JsonList<FlavorDescription> The list of flavor text descriptions associated with the data result of this experiment.
---@field SampleFlavorDescriptions JsonList<FlavorDescription> The list of flavor text descriptions associated with the sample result of this experiment.
---@field DataReportDisplayName string The display name used when presenting a data report produced by this experiment.
---@field SampleReportDisplayName string The display name used when presenting a sample report produced by this experiment.
---@field ValidLocations JsonList<ResearchLocation> The list of research locations at which this experiment is valid.
---@field DataValue number The science value awarded for collecting data from this experiment.
---@field SampleValue number The science value awarded for collecting a sample from this experiment.
---@field TransmissionSize number The data transmission size, in abstract units, for sending results from this experiment.
---@field RequiresEVA boolean Indicates whether this experiment requires the kerbal to be on EVA to perform it.

---@alias ExperimentDefinition _ExperimentDefinition | { ExperimentID: string, DisplayName: string, DisplayRequirements: string, ExperimentType: ScienceExperimentType, DataFlavorDescriptions: JsonList<FlavorDescription>, SampleFlavorDescriptions: JsonList<FlavorDescription>, DataReportDisplayName: string, SampleReportDisplayName: string, ValidLocations: JsonList<ResearchLocation>, DataValue: number, SampleValue: number, TransmissionSize: number, RequiresEVA: boolean }

---Represents a flavor text description associated with a specific science research location.
---@class _FlavorDescription : _JsonUserDataBase
---@field ResearchLocationID string The identifier of the research location this flavor description applies to.
---@field LocalizationTag string The localization tag used to retrieve the localized flavor text string.

---@alias FlavorDescription _FlavorDescription | { ResearchLocationID: string, LocalizationTag: string }

---Represents a science research location defined by a celestial body, science situation, and optional science region.
---@class _ResearchLocation : _JsonUserDataBase
---@field RequiresRegion boolean Indicates whether this research location requires a science region to be specified.
---@field BodyName string Gets the name of the celestial body for this research location.
---@field ScienceSituation ScienceSitutation Gets the science situation for this research location.
---@field ScienceRegion string Gets the science region for this research location.
---@field ResearchLocationId string Gets the composite identifier string for this research location, computed lazily from the body name, science situation, and optional region.

---@alias ResearchLocation _ResearchLocation | { RequiresRegion: boolean, BodyName: string, ScienceSituation: ScienceSitutation, ScienceRegion: string, ResearchLocationId: string }

---Represents the science regions data for a celestial body, including situation data and region definitions.
---@class _CelestialBodyScienceRegionsData : _JsonUserDataBase
---@field Version string The version identifier for this science regions data record.
---@field BodyName string The name of the celestial body this science regions data describes.
---@field Layer? string The layer this copy of the body's science regions belongs to, or nil for the default copy.
---@field SituationData CBSituationData The situation data associated with this celestial body.
---@field Regions JsonList<ScienceRegionDefinition> The array of science region definitions for this celestial body.

---@alias CelestialBodyScienceRegionsData _CelestialBodyScienceRegionsData | { Version: string, BodyName: string, Layer?: string, SituationData: CBSituationData, Regions: JsonList<ScienceRegionDefinition> }

---Represents altitude boundaries and science scalar multipliers for the science situations of a celestial body.
---@class _CBSituationData : _JsonUserDataBase
---@field HighOrbitMaxAltitude number The maximum altitude in meters defining the upper boundary of the high orbit science situation for this celestial body.
---@field LowOrbitMaxAltutude number The maximum altitude in meters defining the upper boundary of the low orbit science situation for this celestial body.
---@field AtmosphereMaxAltutude number The maximum altitude in meters defining the upper boundary of the atmosphere science situation for this celestial body.
---@field CelestialBodyScalar number A global science reward scalar multiplier applied to all science situations on this celestial body.
---@field HighOrbitScalar number A science reward scalar multiplier applied to experiments conducted in high orbit around this celestial body.
---@field LowOrbitScalar number A science reward scalar multiplier applied to experiments conducted in low orbit around this celestial body.
---@field AtmosphereScalar number A science reward scalar multiplier applied to experiments conducted within the atmosphere of this celestial body.
---@field SplashedScalar number A science reward scalar multiplier applied to experiments conducted while splashed down on this celestial body.
---@field LandedScalar number A science reward scalar multiplier applied to experiments conducted while landed on this celestial body.

---@alias CBSituationData _CBSituationData | { HighOrbitMaxAltitude: number, LowOrbitMaxAltutude: number, AtmosphereMaxAltutude: number, CelestialBodyScalar: number, HighOrbitScalar: number, LowOrbitScalar: number, AtmosphereScalar: number, SplashedScalar: number, LandedScalar: number }

---Represents a science region definition, specifying situation scalars for atmosphere, splashed, and landed states.
---@class _ScienceRegionDefinition : _JsonUserDataBase
---@field Id string The unique identifier for this science region.
---@field AtmosphereScalar number The science point scalar applied when conducting experiments in atmosphere.
---@field SplashedScalar number The science point scalar applied when conducting experiments in a splashed state.
---@field LandedScalar number The science point scalar applied when conducting experiments in a landed state.
---@field MapId integer The map identifier used to look up this region's boundaries.

---@alias ScienceRegionDefinition _ScienceRegionDefinition | { Id: string, AtmosphereScalar: number, SplashedScalar: number, LandedScalar: number, MapId: integer }

---Represents the baked set of discoverable positions associated with a named celestial body.
---@class _CelestialBodyBakedDiscoverables : _JsonUserDataBase
---@field Version string The format version of this baked discoverable data set.
---@field BodyName string The name of the celestial body this data set belongs to.
---@field Layer? string The layer this copy of the body's discoverables belongs to, or nil for the default copy.
---@field Discoverables JsonList<CelestialBodyDiscoverablePosition> The baked discoverable positions associated with this celestial body.

---@alias CelestialBodyBakedDiscoverables _CelestialBodyBakedDiscoverables | { Version: string, BodyName: string, Layer?: string, Discoverables: JsonList<CelestialBodyDiscoverablePosition> }

---Represents a discoverable position on a celestial body, defined by a science region, a spatial location, and a detection radius.
---@class _CelestialBodyDiscoverablePosition : _JsonUserDataBase
---@field ScienceRegionId string The identifier of the science region associated with this discoverable position.
---@field Position Vector3d The three-dimensional world-space position of this discoverable location.
---@field Radius number The radius within which this position is considered discoverable.

---@alias CelestialBodyDiscoverablePosition _CelestialBodyDiscoverablePosition | { ScienceRegionId: string, Position: Vector3d, Radius: number }

---Represents the serialized data for a single tech tree node, including its display info, science point cost, unlock requirements, and position.
---@class _TechNodeData : _JsonUserDataBase
---@field Version integer The schema version of the serialized tech node data.
---@field ID string The unique string identifier of this tech node.
---@field NameLocKey string The localization key for the display name of this tech node.
---@field IconID string The identifier of the icon used to represent this tech node in the tech tree UI.
---@field CategoryID string The identifier of the category this tech node belongs to.
---@field HiddenByNodeID string The identifier of a tech node that hides this node from the tech tree while it remains locked.
---@field DescriptionLocKey string The localization key for the description text of this tech node.
---@field RequiredSciencePoints integer The number of science points required to unlock this tech node.
---@field UnlockedPartsIDs JsonList<string> The identifiers of the parts that become available when this tech node is unlocked.
---@field RequiredTechNodeIDs JsonList<string> The identifiers of tech nodes that must be unlocked before this node can be researched.
---@field RequiredMissionIDs JsonList<string> The identifiers of missions that must be completed before this tech node can be unlocked.
---@field RequiredResearchData JsonList<TechRequiredResearchData> The research conditions that must be satisfied to unlock this tech node.
---@field TierToUnlock integer The tech tree tier at which this node becomes available to unlock.
---@field TechTreePosition Vector2 The 2D position of this tech node within the tech tree layout.
---@field MysteryTech boolean When true, the node name and description are hidden until all prerequisites are fulfilled.
---@field Layer string The tech tree layer this node belongs to, which a campaign pack selects nodes by.

---@alias TechNodeData _TechNodeData | { Version: integer, ID: string, NameLocKey: string, IconID: string, CategoryID: string, HiddenByNodeID: string, DescriptionLocKey: string, RequiredSciencePoints: integer, UnlockedPartsIDs: JsonList<string>, RequiredTechNodeIDs: JsonList<string>, RequiredMissionIDs: JsonList<string>, RequiredResearchData: JsonList<TechRequiredResearchData>, TierToUnlock: integer, TechTreePosition: Vector2, MysteryTech: boolean, Layer: string }

---Represents research conditions required to unlock a technology node, including experiment, location, and report type.
---Leave ExperimentID nil or empty to match any experiment.
---@class _TechRequiredResearchData : _JsonUserDataBase
---@field ExperimentID string The identifier of the experiment associated with this research requirement, or nil or empty to match any experiment.
---@field ResearchLocationID? string The identifier of the research location, or nil if no specific location is required.
---@field ResearchReportType? ScienceReportType The optional ScienceReportType filter for this research entry. Nil if any report type satisfies the requirement.
---@field Coordinates JsonList<number> The optional geographic position of the research location, in decimal degrees, as a two element array ordered latitude then longitude. Longitude is east positive, matching the rest of the project. Leave the array out or empty when the location needs no coordinates.

---@alias TechRequiredResearchData _TechRequiredResearchData | { ExperimentID: string, ResearchLocationID: string?, ResearchReportType: ScienceReportType?, Coordinates: JsonList<number> }
