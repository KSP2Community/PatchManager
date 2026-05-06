---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/VSwiftLuaModule.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/PartSwitchUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantSetsUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/VariantUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/AttachNodeAdderUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/NodesUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/EngineModeSwapperUserData.cs
-- Source: ksp2redux/Assets/Modules/V-SwiFT/Runtime/VSwift/UserData/TransformersUserData.cs
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

---Indexed-list wrapper for a `Data_PartSwitch.VariantSets` array, keyed by each set's `VariantSetId`.
---@class VariantSetsUserData : IndexedListUserData<VariantUserData>

---Synthetic per-element wrapper for entries of a `VariantSetsUserData`, exposing the `Transformers`
---list as a typed `TransformersUserData` alongside the rest of the variant's JSON shape.
---@class VariantUserData : _Variant, ExtensibleJsonUserData
---@field Transformers TransformersUserData            The variant's transformer list, exposed as a typed `TransformersUserData`. Read-only.

---Indexed-list wrapper for a transformer list, keyed by each transformer's `$type` short name.
---When an entry's `$type` resolves to a `[TransformerAdapter]`-decorated wrapper the entry is
---returned as that wrapper; otherwise it is returned as a raw `JsonUserData`.
---@class TransformersUserData : IndexedListUserData<AttachNodeAdderUserData | EngineModeSwapperUserData | JsonUserData>
local TransformersUserData = {}

---Adds a new transformer of the given type and runs callback against the new entry for further configuration.
---@param type string                                                                       The transformer's short name (the `[Transformer]` attribute argument).
---@param callback fun(entry: AttachNodeAdderUserData|EngineModeSwapperUserData|JsonUserData) Callback that receives the new entry for further configuration.
---@error Thrown when type is not a registered transformer.
function TransformersUserData:Add(type, callback) end

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
---@field Transformers JsonList<table>           Polymorphic `$type`-discriminated transformer list. Typed access goes through `VariantUserData.Transformers`.

---@alias Variant _Variant | { VariantId: string, VariantLocalizationKey: string, VariantTechs: JsonList<string>, Transformers: JsonList<table> }
