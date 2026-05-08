---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/VSwiftLuaModule.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/PartSwitchUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantSetsUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantSetUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/AttachNodeAdderUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/NodesUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/EngineModeSwapperUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/Modules/Data/Data_PartSwitch.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/Modules/Variants/VariantSet.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/Modules/Variants/Variant.cs

---The V-SwiFT submodule, exposed to scripts as `PM.VSwift`.
---@class VSwiftLuaModule
VSwiftLuaModule = {}

---Adds a PAM module-visuals override on the given part so Module_PartSwitch displays under
---a localized header in the parts manager.
---@param part PartUserData The part to add the PAM override to.
function VSwiftLuaModule:AddPAMOverride(part) end

---Adds a Module_PartSwitch to the given part with a Data_PartSwitch entry, runs callback
---against the new data for further configuration, and applies the PAM override.
---@param part PartUserData                        The part to receive the part-switch module.
---@param callback fun(data: PartSwitchUserData)   Callback that receives the new Data_PartSwitch entry for further configuration.
function VSwiftLuaModule:AddPartSwitch(part, callback) end

---`Data_PartSwitch` module-data adapter that exposes typed accessors for variant sets and
---predefined dynamic nodes alongside the rest of the JSON shape.
---Wraps the `Data_PartSwitch` module-data type (its JSON shape lives at `data["DataObject"]`).
---@class PartSwitchUserData : _Data_PartSwitch, ExtensibleJsonUserData
---@field VariantSets VariantSetsUserData               The part-switch's variant-set list, exposed as a typed `VariantSetsUserData`. Read-only.
---@field PredefinedDynamicNodes NodesUserData          The part-switch's predefined dynamic-attach-node list, exposed as a typed `NodesUserData`. Read-only.
local PartSwitchUserData = {}

---Adds a new variant set with the given `VariantSetId` and runs callback against it for further configuration.
---@param name string                                  The new variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData)        Callback that receives the new variant set for further configuration.
function PartSwitchUserData:AddVariantSet(name, callback) end

---Patches the named variant set if it exists, otherwise adds it.
---@param name string                                  The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData)        Callback that receives the variant set for further configuration.
function PartSwitchUserData:EnsureVariantSet(name, callback) end

---Runs callback against the existing variant set with the given `VariantSetId`, doing nothing if absent.
---@param name string                                  The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData)        Callback that receives the existing variant set for further configuration.
function PartSwitchUserData:PatchVariantSet(name, callback) end

---Removes the named variant set from the part-switch.
---@param name string The variant set's `VariantSetId`.
function PartSwitchUserData:RemoveVariantSet(name) end

---Indexed-list wrapper for a `Data_PartSwitch.VariantSets` array, keyed by each set's `VariantSetId`.
---@class VariantSetsUserData : IndexedListUserData<VariantSetUserData>
local VariantSetsUserData = {}

---Adds a new variant set with the given `VariantSetId` and runs callback against it for further configuration.
---@param name string                                  The new variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData)        Callback that receives the new variant set for further configuration.
function VariantSetsUserData:Add(name, callback) end

---Patches the named variant set if it exists, otherwise adds it.
---@param name string                                  The variant set's `VariantSetId`.
---@param callback fun(set: VariantSetUserData)        Callback that receives the variant set for further configuration.
function VariantSetsUserData:Ensure(name, callback) end

---Auxiliary indexer typing variant lookups on a `VariantSetUserData` (`set["VariantId"]` returns a typed `VariantUserData`).
---@class _VariantSetUserDataVariantIndexer
---@field [string] VariantUserData

---Synthetic per-element wrapper for entries of a `VariantSetsUserData`, exposing each contained
---variant as a virtual property keyed by its `VariantId`.
---@class VariantSetUserData : _VariantSet, _VariantSetUserDataVariantIndexer, ExtensibleJsonUserData
local VariantSetUserData = {}

---Adds a new variant with the given `VariantId` to this set and runs callback against it for further configuration.
---@param variantId string                             The new variant's `VariantId`.
---@param callback fun(variant: VariantUserData)       Callback that receives the new variant for further configuration.
function VariantSetUserData:AddVariant(variantId, callback) end

---Patches the named variant if it exists, otherwise adds it.
---@param variantId string                             The variant's `VariantId`.
---@param callback fun(variant: VariantUserData)       Callback that receives the variant for further configuration.
function VariantSetUserData:EnsureVariant(variantId, callback) end

---Runs callback against the existing variant with the given `VariantId`, doing nothing if absent.
---@param variantId string                             The variant's `VariantId`.
---@param callback fun(variant: VariantUserData)       Callback that receives the existing variant for further configuration.
function VariantSetUserData:PatchVariant(variantId, callback) end

---Removes the named variant from this set.
---@param variantId string The variant's `VariantId`.
function VariantSetUserData:RemoveVariant(variantId) end

---Returns whether this set contains a variant with the given `VariantId`.
---@param variantId string The variant's `VariantId`.
---@return boolean present True if the variant exists, false otherwise.
function VariantSetUserData:HasVariant(variantId) end

---Auxiliary indexer typing transformer lookups on a `VariantUserData` (`variant["TransformerName"]`
---returns the typed `[TransformerAdapter]` wrapper when one is registered, otherwise a raw `JsonUserData`).
---@class _VariantUserDataTransformerIndexer
---@field [string] AttachNodeAdderUserData | EngineModeSwapperUserData | JsonUserData

---Synthetic per-element wrapper for entries of a `VariantSetUserData`, exposing each contained
---transformer as a virtual property keyed by its `[Transformer]` short name. When an entry's
---`$type` resolves to a `[TransformerAdapter]`-decorated wrapper the entry is returned as that
---wrapper; otherwise it is returned as a raw `JsonUserData`.
---@class VariantUserData : _Variant, _VariantUserDataTransformerIndexer, ExtensibleJsonUserData
local VariantUserData = {}

---Adds a new transformer of the given type to this variant and runs callback against the new entry for further configuration.
---@param type string                                                                       The transformer's short name (the `[Transformer]` attribute argument).
---@param callback fun(entry: AttachNodeAdderUserData|EngineModeSwapperUserData|JsonUserData) Callback that receives the new entry for further configuration.
---@error Thrown when type is not a registered transformer.
function VariantUserData:AddTransformer(type, callback) end

---Patches the named transformer if it exists, otherwise adds it.
---@param type string                                                                       The transformer's short name.
---@param callback fun(entry: AttachNodeAdderUserData|EngineModeSwapperUserData|JsonUserData) Callback that receives the entry for further configuration.
function VariantUserData:EnsureTransformer(type, callback) end

---Runs callback against the existing transformer with the given short name, doing nothing if absent.
---@param type string                                                                       The transformer's short name.
---@param callback fun(entry: AttachNodeAdderUserData|EngineModeSwapperUserData|JsonUserData) Callback that receives the existing entry for further configuration.
function VariantUserData:PatchTransformer(type, callback) end

---Removes the named transformer from this variant.
---@param type string The transformer's short name.
function VariantUserData:RemoveTransformer(type) end

---Returns whether this variant has a transformer of the given type.
---@param type string The transformer's short name.
---@return boolean present True if the transformer exists, false otherwise.
function VariantUserData:HasTransformer(type) end

---`AttachNodeAdder` transformer adapter that exposes the transformer's `Nodes` array as a
---typed indexed-list of `AttachNodeDefinition` rather than a raw `JsonUserData`.
---@class AttachNodeAdderUserData : IndexedListUserData<AttachNodeDefinition>
local AttachNodeAdderUserData = {}

---`EngineModeSwapper` transformer adapter that exposes the transformer's `Modes` array as a
---typed `ModesUserData` rather than a raw `JsonUserData`.
---@class EngineModeSwapperUserData : ModesUserData

---Adds a new attach-node definition with the given `nodeID` and runs callback against the
---wrapped JSON for further configuration.
---@param id string                            The new node's `nodeID`.
---@param callback fun(node: JsonUserData)     Callback that receives the new attach-node JSON for further configuration.
function AttachNodeAdderUserData:Add(id, callback) end

---Indexed-list wrapper for an attach-node array (`predefinedDynamicNodes` on a part-switch),
---keyed by each entry's `nodeID`.
---@class NodesUserData : IndexedListUserData<AttachNodeDefinition>

---Data shape at `data["DataObject"]` for the `Data_PartSwitch` module-data type.
---@class _Data_PartSwitch : _JsonUserDataBase
---@field VariantSets JsonList<VariantSet>                       The part's variant sets.
---@field ActiveVariants JsonList<string>                        The currently-active variant ID per variant set, indexed positionally.
---@field PredefinedDynamicNodes JsonList<AttachNodeDefinition>  Attach nodes whose dynamic state can be toggled by transformers.
---@field MassModifier number                                    Additional mass contributed by the active variant configuration.

---@alias Data_PartSwitch _Data_PartSwitch | { VariantSets: JsonList<VariantSet>, ActiveVariants: JsonList<string>, PredefinedDynamicNodes: JsonList<AttachNodeDefinition>, MassModifier: number }

---On-disk JSON shape of a single variant set inside a `Data_PartSwitch`.
---@class _VariantSet : _JsonUserDataBase
---@field VariantSetId string                            The variant set's identifier; also used as the localization-key fallback.
---@field VariantSetLocalizationKey string               The variant set's localization key; defaults to VariantSetId when empty.
---@field IsPopout boolean                               Whether this set surfaces as a popout-window button rather than inline UI.
---@field Variants JsonList<Variant>                     The variants the player can pick from in this set.

---@alias VariantSet _VariantSet | { VariantSetId: string, VariantSetLocalizationKey: string, IsPopout: boolean, Variants: JsonList<Variant> }

---On-disk JSON shape of a single variant inside a `VariantSet`.
---@class _Variant : _JsonUserDataBase
---@field VariantId string                       The variant's identifier; also used as the localization-key fallback.
---@field VariantLocalizationKey string          The variant's localization key; defaults to VariantId when empty.
---@field VariantTechs JsonList<string>          Technology IDs that must be unlocked for this variant to be selectable.
---@field Transformers JsonList<table>           Polymorphic `$type`-discriminated transformer list. Typed access goes through `VariantUserData[<short name>]` keys.

---@alias Variant _Variant | { VariantId: string, VariantLocalizationKey: string, VariantTechs: JsonList<string>, Transformers: JsonList<table> }
