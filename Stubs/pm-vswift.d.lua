---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/VSwiftLuaModule.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/PartSwitchUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantSetsUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantSetUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/Utilities/Adapters.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/Utilities/Transformers.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/Attributes/TransformerAdapter.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/AttachNodeAdderUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/EngineModeSwapperUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/NodesUserData.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Data/Data_PartSwitch.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Variants/VariantSet.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Variants/Variant.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/ITransformer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/Transformer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/AttachNodeAdder.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/AttachNodeMover.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/DefaultEngineModesVisualizer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/DefaultMassVisualizer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/DefaultScalarVisualizer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/EngineModeSwapper.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/MassModifier.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/MaterialSwapper.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/ModuleDefinitionTransformer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/PartScalarTransformer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/ResourceContainerAdder.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/ResourceContainerRemover.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/TextVisualizer.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/Modules/Transformers/TransformActivator.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/Utilities/SerializedDictionary.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/UserData/ModuleUserData.cs

---Lua module exposed under the global `PM.VSwift`, providing helpers for attaching V-SwiFT part-switching to parts.
---@class VSwiftLuaModule
local VSwiftLuaModule = {}

---Adds a PAM module-visuals override on the given part so Module_PartSwitch displays under a localized header in the parts manager.
---@param part PartUserData The part to add the PAM override to.
function VSwiftLuaModule:AddPAMOverride(part) end

---Adds Module_PartSwitch with a Data_PartSwitch entry to the given part, runs callback against the new data for further configuration, and applies the PAM module-visuals override.
---@param part PartUserData The part to receive the part-switch module.
---@param callback fun(data: PartSwitchUserData) Callback that receives the new Data_PartSwitch entry for further configuration.
function VSwiftLuaModule:AddPartSwitch(part, callback) end

---Lua wrapper for the Data_PartSwitch module-data adapter, exposing `VariantSets` as a typed VariantSetsUserData and `PredefinedDynamicNodes` as a typed NodesUserData.
---@class PartSwitchUserData : _Data_PartSwitch, ExtensibleJsonUserData
---@field VariantSets VariantSetsUserData
---@field PredefinedDynamicNodes NodesUserData
local PartSwitchUserData = {}

---Adds a new variant set with the given `VariantSetId` and runs callback against it for further configuration.
---@param name string The new variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData) Callback that receives the new variant set for further configuration.
function PartSwitchUserData:AddVariantSet(name, callback) end

---Patches the named variant set if it exists, otherwise adds it.
---@param name string The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData) Callback that receives the variant set for further configuration.
function PartSwitchUserData:EnsureVariantSet(name, callback) end

---Runs callback against the variant set with the given `VariantSetId`, doing nothing if absent.
---@param name string The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData) Callback that receives the existing variant set for further configuration.
function PartSwitchUserData:PatchVariantSet(name, callback) end

---Removes the named variant set from the part-switch.
---@param name string The variant set's `VariantSetId`.
function PartSwitchUserData:RemoveVariantSet(name) end

---Indexed-list wrapper for a Data_PartSwitch.`VariantSets` array, keyed by each entry's `VariantSetId`.
---@class VariantSetsUserData : IndexedListUserData<VariantSetUserData>
local VariantSetsUserData = {}

---Adds a new variant set with the given `VariantSetId` and runs callback against it for further configuration.
---@param name string The new variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData) Callback that receives the new variant set for further configuration.
function VariantSetsUserData:Add(name, callback) end

---Patches the named variant set if it exists, otherwise adds it.
---@param name string The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData) Callback that receives the variant set for further configuration.
function VariantSetsUserData:Ensure(name, callback) end

---@class _VariantSetUserDataVariantIndexer
---@field [string] VariantUserData

---Lua wrapper for a single VariantSet, exposing each contained variant as a virtual property keyed by its `VariantId`.
---@class VariantSetUserData : _VariantSet, _VariantSetUserDataVariantIndexer, ExtensibleJsonUserData
local VariantSetUserData = {}

---Adds a new variant with the given `VariantId` to this set and runs callback against it for further configuration.
---@param variantId string The new variant's `VariantId`.
---@param callback fun(variant: VariantUserData) Callback that receives the new variant for further configuration.
function VariantSetUserData:AddVariant(variantId, callback) end

---Patches the named variant if it exists, otherwise adds it.
---@param variantId string The variant's `VariantId`.
---@param callback fun(variant: VariantUserData) Callback that receives the variant for further configuration.
function VariantSetUserData:EnsureVariant(variantId, callback) end

---Runs callback against the variant with the given `VariantId`, doing nothing if absent.
---@param variantId string The variant's `VariantId`.
---@param callback fun(variant: VariantUserData) Callback that receives the existing variant for further configuration.
function VariantSetUserData:PatchVariant(variantId, callback) end

---Removes the named variant from this set.
---@param variantId string The variant's `VariantId`.
function VariantSetUserData:RemoveVariant(variantId) end

---Returns whether this set contains a variant with the given `VariantId`.
---@param variantId string The variant's `VariantId`.
---@return boolean exists True if the variant exists, false otherwise.
function VariantSetUserData:HasVariant(variantId) end

---@alias VariantTransformerUserData AttachNodeAdderUserData | AttachNodeMoverUserData | DefaultEngineModesVisualizerUserData | DefaultMassVisualizerUserData | DefaultScalarVisualizerUserData | EngineModeSwapperUserData | MassModifierUserData | MaterialSwapperUserData | ModuleDefinitionTransformerUserData | PartScalarTransformerUserData | ResourceContainerAdderUserData | ResourceContainerRemoverUserData | TextVisualizerUserData | TransformActivatorUserData | JsonUserData

---@class _VariantUserDataTransformerIndexer
---@field AttachNodeAdder? AttachNodeAdderUserData
---@field AttachNodeMover? AttachNodeMoverUserData
---@field DefaultEngineModesVisualizer? DefaultEngineModesVisualizerUserData
---@field DefaultMassVisualizer? DefaultMassVisualizerUserData
---@field DefaultScalarVisualizer? DefaultScalarVisualizerUserData
---@field EngineModeSwapper? EngineModeSwapperUserData
---@field MassModifier? MassModifierUserData
---@field MaterialSwapper? MaterialSwapperUserData
---@field ModuleDefinitionTransformer? ModuleDefinitionTransformerUserData
---@field PartScalarTransformer? PartScalarTransformerUserData
---@field ResourceContainerAdder? ResourceContainerAdderUserData
---@field ResourceContainerRemover? ResourceContainerRemoverUserData
---@field TextVisualizer? TextVisualizerUserData
---@field TransformActivator? TransformActivatorUserData
---@field [string] VariantTransformerUserData

---Lua wrapper for a single Variant, exposing each contained transformer as a virtual property keyed by its TransformerName short name.
---@class VariantUserData : _Variant, _VariantUserDataTransformerIndexer, ExtensibleJsonUserData
local VariantUserData = {}

---Adds a new transformer of the given type to this variant and runs callback against the new entry for further configuration.
---@param type string The transformer's short name (the Transformer attribute argument).
---@param callback fun(entry: VariantTransformerUserData) Callback that receives the new entry for further configuration.
---@error Thrown when type is not a registered transformer.
function VariantUserData:AddTransformer(type, callback) end

---Patches the named transformer if it exists, otherwise adds it.
---@param type string The transformer's short name.
---@param callback fun(entry: VariantTransformerUserData) Callback that receives the entry for further configuration.
function VariantUserData:EnsureTransformer(type, callback) end

---Runs callback against the transformer with the given short name, doing nothing if absent.
---@param type string The transformer's short name.
---@param callback fun(entry: VariantTransformerUserData) Callback that receives the existing entry for further configuration.
function VariantUserData:PatchTransformer(type, callback) end

---Removes the named transformer from this variant.
---@param type string The transformer's short name.
function VariantUserData:RemoveTransformer(type) end

---Returns whether this variant has a transformer of the given type.
---@param type string The transformer's short name.
---@return boolean exists True if the transformer exists, false otherwise.
function VariantUserData:HasTransformer(type) end

---@class AttachNodeUserData : _AttachNodeDefinition, JsonUserData

---Transformer-adapter wrapper for AttachNodeAdder, exposing the transformer's `Nodes` array as a typed indexed-list keyed by each entry's `nodeID`.
---@class AttachNodeAdderUserData : IndexedListUserData<AttachNodeUserData>
local AttachNodeAdderUserData = {}

---Adds a new attach-node definition with the given `nodeID` and runs callback against the wrapped JSON for further configuration.
---@param id string The new node's `nodeID`.
---@param callback fun(node: AttachNodeUserData) Callback that receives the new attach-node JSON for further configuration.
function AttachNodeAdderUserData:Add(id, callback) end

---Transformer-adapter wrapper for EngineModeSwapper, exposing the transformer's `Modes` array as a typed ModesUserData.
---@class EngineModeSwapperUserData : ModesUserData

---Indexed-list wrapper for an attach-node array (such as a part-switch's `PredefinedDynamicNodes`), keyed by each entry's `nodeID`.
---@class NodesUserData : IndexedListUserData<AttachNodeUserData>

---@class AttachNodeMoverUserData : _AttachNodeMover, JsonUserData

---@class DefaultEngineModesVisualizerUserData : _DefaultEngineModesVisualizer, JsonUserData

---@class DefaultMassVisualizerUserData : _DefaultMassVisualizer, JsonUserData

---@class DefaultScalarVisualizerUserData : _DefaultScalarVisualizer, JsonUserData

---@class MassModifierUserData : _MassModifier, JsonUserData

---@class MaterialSwapperUserData : _MaterialSwapper, JsonUserData

---@class ModuleDefinitionTransformerUserData : _ModuleDefinitionTransformer, JsonUserData

---@class PartScalarTransformerUserData : _PartScalarTransformer, JsonUserData

---@class ResourceContainerAdderUserData : _ResourceContainerAdder, JsonUserData

---@class ResourceContainerRemoverUserData : _ResourceContainerRemover, JsonUserData

---@class TextVisualizerUserData : _TextVisualizer, JsonUserData

---@class TransformActivatorUserData : _TransformActivator, JsonUserData

---Module-data attached to a part-switch module, carrying the variant sets, the per-set active variant, and the predefined dynamic attach nodes.
---@class _Data_PartSwitch : _JsonUserDataBase
---@field DataType "VSwift.Modules.Data.Data_PartSwitch, Assembly-CSharp"
---@field VariantSets JsonList<VariantSet> The variant sets this module data carries.
---@field ActiveVariants JsonList<string> The currently-active variant ID per variant set, indexed positionally.
---@field DefaultActiveVariants JsonList<string> The definition-side default variant ID per variant set, indexed positionally.
---@field PredefinedDynamicNodes JsonList<AttachNodeDefinition> Attach nodes whose dynamic state can be toggled by transformers.
---@field MassModifier number Gets or sets the additional mass contributed by the active variant configuration.

---@alias Data_PartSwitch _Data_PartSwitch | { DataType: "VSwift.Modules.Data.Data_PartSwitch, Assembly-CSharp", VariantSets: JsonList<VariantSet>, ActiveVariants: JsonList<string>, DefaultActiveVariants: JsonList<string>, PredefinedDynamicNodes: JsonList<AttachNodeDefinition>, MassModifier: number }

---A switchable axis on a part-switch module, listing the variants the player can pick from.
---@class _VariantSet : _JsonUserDataBase
---@field VariantSetId string Gets the variant set's identifier. Also used as the localization-key fallback.
---@field VariantSetLocalizationKey string Gets the variant set's localization key. Defaults to `VariantSetId` when empty.
---@field IsPopout boolean Gets whether this set surfaces as a popout-window button rather than an inline dropdown.
---@field Variants JsonList<Variant> Gets the variants the player can pick from in this set.

---@alias VariantSet _VariantSet | { VariantSetId: string, VariantSetLocalizationKey: string, IsPopout: boolean, Variants: JsonList<Variant> }

---A single variant within a VariantSet, applying its Transformers when active.
---@class _Variant : _JsonUserDataBase
---@field VariantId string The variant's identifier. Also used as the localization-key fallback.
---@field VariantLocalizationKey string The variant's localization key. Defaults to `VariantId` when empty.
---@field VariantTechs JsonList<string> Technology IDs that must be unlocked for this variant to be selectable.
---@field Transformers JsonList<ITransformer> The transformers applied to the part when this variant is active.

---@alias Variant _Variant | { VariantId: string, VariantLocalizationKey: string, VariantTechs: JsonList<string>, Transformers: JsonList<ITransformer> }

---@alias ITransformer table | AttachNodeAdder | AttachNodeMover | DefaultEngineModesVisualizer | DefaultMassVisualizer | DefaultScalarVisualizer | EngineModeSwapper | MassModifier | MaterialSwapper | ModuleDefinitionTransformer | PartScalarTransformer | ResourceContainerAdder | ResourceContainerRemover | TextVisualizer | TransformActivator

---Adds attach nodes to the part (or repositions existing nodes whose tag matches a configured node) when active.
---@class _AttachNodeAdder : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.AttachNodeAdder, Assembly-CSharp"
---@field Nodes JsonList<AttachNodeDefinition> The attach-node definitions to add or reposition.

---@alias AttachNodeAdder _AttachNodeAdder | { ["$type"]: "VSwift.Modules.Transformers.AttachNodeAdder, Assembly-CSharp", Nodes: JsonList<AttachNodeDefinition> }

---Repositions existing attach nodes on the part to the configured local positions when active.
---@class _AttachNodeMover : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.AttachNodeMover, Assembly-CSharp"
---@field MovedNodes JsonTable<Vector3d> Map of node ID to the new local position to move it to.

---@alias AttachNodeMover _AttachNodeMover | { ["$type"]: "VSwift.Modules.Transformers.AttachNodeMover, Assembly-CSharp", MovedNodes: JsonTable<Vector3d> }

---Renders the part's original engine-mode stat blocks (propellant, thrust, ISP) in the variant-info popout without modifying the part.
---@class _DefaultEngineModesVisualizer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.DefaultEngineModesVisualizer, Assembly-CSharp"

---@alias DefaultEngineModesVisualizer _DefaultEngineModesVisualizer | { ["$type"]: "VSwift.Modules.Transformers.DefaultEngineModesVisualizer, Assembly-CSharp" }

---Renders the part's original mass as a stat block in the variant-info popout without modifying the part.
---@class _DefaultMassVisualizer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.DefaultMassVisualizer, Assembly-CSharp"

---@alias DefaultMassVisualizer _DefaultMassVisualizer | { ["$type"]: "VSwift.Modules.Transformers.DefaultMassVisualizer, Assembly-CSharp" }

---Renders the value at the configured key on the part data as a stat block in the variant-info popout without modifying the part.
---@class _DefaultScalarVisualizer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.DefaultScalarVisualizer, Assembly-CSharp"
---@field Key string The key to read from the part data.

---@alias DefaultScalarVisualizer _DefaultScalarVisualizer | { ["$type"]: "VSwift.Modules.Transformers.DefaultScalarVisualizer, Assembly-CSharp", Key: string }

---Replaces matching engine modes on the part's `Module_Engine` with the configured modes when active, persists them across saves, and renders their stats in the variant-info popout.
---@class _EngineModeSwapper : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.EngineModeSwapper, Assembly-CSharp"
---@field Modes JsonList<Data_Engine_EngineMode> The engine modes to swap in. Each entry replaces the existing mode whose `engineID` matches.
---@field Name string The transformer instance name. Defaults to the `EngineModeSwapper` short name.

---@alias EngineModeSwapper _EngineModeSwapper | { ["$type"]: "VSwift.Modules.Transformers.EngineModeSwapper, Assembly-CSharp", Modes: JsonList<Data_Engine_EngineMode>, Name: string }

---Adds a fixed value to the part's MassModifier when active.
---@class _MassModifier : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.MassModifier, Assembly-CSharp"
---@field Modifier number The mass delta added to the part when this transformer is active.

---@alias MassModifier _MassModifier | { ["$type"]: "VSwift.Modules.Transformers.MassModifier, Assembly-CSharp", Modifier: number }

---Swaps materials on the part by mapping each source material name to a replacement loaded from addressables.
---@class _MaterialSwapper : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.MaterialSwapper, Assembly-CSharp"
---@field Swaps JsonTable<string> Map of source material name to the addressables address of the replacement material.

---@alias MaterialSwapper _MaterialSwapper | { ["$type"]: "VSwift.Modules.Transformers.MaterialSwapper, Assembly-CSharp", Swaps: JsonTable<string> }

---Replaces a part module's data with a configured replacement when active, persisted across saves.
---@class _ModuleDefinitionTransformer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.ModuleDefinitionTransformer, Assembly-CSharp"
---@field BehaviourType string The short name of the part-behaviour-module type whose data this transformer replaces.
---@field DataType string The short name of the module-data type whose value at Key is replaced.
---@field Key string The field name on the module-data type to replace.
---@field Value any The replacement value to deserialize into the field at Key.

---@alias ModuleDefinitionTransformer _ModuleDefinitionTransformer | { ["$type"]: "VSwift.Modules.Transformers.ModuleDefinitionTransformer, Assembly-CSharp", BehaviourType: string, DataType: string, Key: string, Value: any }

---Sets a scalar value at the configured key path on the part data when active, persisted across saves.
---@class _PartScalarTransformer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.PartScalarTransformer, Assembly-CSharp"
---@field Key string The key path on the part data to set.
---@field Value any The value to set at Key.

---@alias PartScalarTransformer _PartScalarTransformer | { ["$type"]: "VSwift.Modules.Transformers.PartScalarTransformer, Assembly-CSharp", Key: string, Value: any }

---Adds resource containers to the part when active, persisted across saves and rendered in the variant-info popout.
---@class _ResourceContainerAdder : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.ResourceContainerAdder, Assembly-CSharp"
---@field Containers JsonList<ContainedResourceDefinition> The resource-container definitions to add.

---@alias ResourceContainerAdder _ResourceContainerAdder | { ["$type"]: "VSwift.Modules.Transformers.ResourceContainerAdder, Assembly-CSharp", Containers: JsonList<ContainedResourceDefinition> }

---Removes resource containers by name from the part when active, persisted across saves.
---@class _ResourceContainerRemover : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.ResourceContainerRemover, Assembly-CSharp"
---@field Containers JsonList<string> The resource-container names to remove.

---@alias ResourceContainerRemover _ResourceContainerRemover | { ["$type"]: "VSwift.Modules.Transformers.ResourceContainerRemover, Assembly-CSharp", Containers: JsonList<string> }

---Renders a localized title and description as a stat block in the variant-info popout without modifying the part.
---@class _TextVisualizer : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.TextVisualizer, Assembly-CSharp"
---@field TitleKey string Localization key for the stat block's title.
---@field DescriptionKey string Localization key for the stat block's description.

---@alias TextVisualizer _TextVisualizer | { ["$type"]: "VSwift.Modules.Transformers.TextVisualizer, Assembly-CSharp", TitleKey: string, DescriptionKey: string }

---Activates child GameObject transforms on the part by name when active.
---@class _TransformActivator : _JsonUserDataBase
---@field ["$type"] "VSwift.Modules.Transformers.TransformActivator, Assembly-CSharp"
---@field Transforms JsonList<string> The names of the child transforms to activate.

---@alias TransformActivator _TransformActivator | { ["$type"]: "VSwift.Modules.Transformers.TransformActivator, Assembly-CSharp", Transforms: JsonList<string> }
