---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PatchManagerCore.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/AudioLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/CampaignPacks/CampaignPacksLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Generic/GenericLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/MissionsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Parts/PartsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/PlanetsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/ResourcesLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Science/ScienceLuaModule.cs
-- Source: Assets/Modules/V-SwiFT/Runtime/VSwift/VSwiftLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/JsonModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/PatchDefinition.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/LuaAsset.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PatchManagerEnvContributor.cs

---The PatchManager Lua library, exposed to scripts as the global `PM`.
---@class PatchManagerCore
---@field InvalidatesOnChange string The cache-invalidation tag, exposed to scripts as `PM.InvalidatesOnChange` so a config entry can be tagged with `:Tag(PM.InvalidatesOnChange)`.
---@field Audio AudioLuaModule
---@field CampaignPacks CampaignPacksLuaModule
---@field JSON GenericLuaModule
---@field Missions MissionsLuaModule
---@field Parts PartsLuaModule
---@field Planets PlanetsLuaModule
---@field Resources ResourcesLuaModule
---@field Science ScienceLuaModule
---@field VSwift VSwiftLuaModule
PatchManagerCore = {}

---Registers a patch keyed by the given addressables label and namespaced patch name.
---@param converter string The name of the converter to use, as registered via ConverterAttribute.
---@param label string The addressables label whose assets to patch.
---@param name string The patch's local name. The host mod's ID is prepended to form the full namespaced name.
---@return PatchDefinition<JsonUserData, any> patch The registered patch, suitable for chaining (for example PatchDefinition.Do).
---@error Thrown when `converter` is not registered.
function PatchManagerCore:Patch(converter, label, name) end

---Registers a patch that copies each asset it matches under a new name in the same label, then runs patchMethod
---on the copy.
---The duplicate is a patch like any other: it takes a pass, ordering and requirements, and the copy is patched by
---everything ordered after it. Internal ID fields are left alone. The patch is named `Duplicate(source -> newName)`
---under the host mod.
---@param converter string The name of the converter to use, as registered via ConverterAttribute.
---@param label string The addressables label whose assets to copy, which the copies keep.
---@param source string The name of the asset to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source asset's name.
---@param patchMethod? fun(copy: JsonUserData): string? The callback to run on each copy, or nil to copy without changes.
---@return PatchDefinition<JsonUserData, any> patch The registered patch, suitable for chaining.
---@error Thrown when converter is not registered.
function PatchManagerCore:Duplicate(converter, label, source, newName, patchMethod) end

---Queues a brand-new asset for creation under the given label and address.
---@param converter string The name of the converter that will serialize `newObject` to JSON.
---@param label string The addressables label to tag the new asset with.
---@param name string The asset's addressables address (globally unique).
---@param newObject any The Lua-facing value for the new asset.
---@error Thrown when `converter` is not registered.
function PatchManagerCore:New(converter, label, name, newObject) end

---Returns whether the mod with the given ID is loaded.
---@param modId string The mod ID to test.
---@return boolean loaded True if the mod is loaded, false otherwise.
function PatchManagerCore:Loaded(modId) end

---Lua module exposed under the global `J` namespace, providing helpers for constructing JSON
---literals that MoonSharp's auto-conversion does not produce. Also installed as callable: invoking
---`J(value)` converts `value` to a JsonUserData.
---@class JsonModule
---@operator call(any): JsonUserData
local JsonModule = {}

---Returns an empty JSON array as a JsonUserData.
---@return JsonUserData value A JsonUserData wrapping an empty `JArray`.
function JsonModule.Empty() end

---Wraps the first argument as an integer JSON value, forcing the integer slot type.
---@param value number Call arguments; `args[0]` is cast to a number and truncated to a long.
---@return JsonUserData value A JsonUserData wrapping a `JValue` of integer type.
function JsonModule.Int(value) end

---A registered patch operation: which converter, which addressables target, what callback to run, and at which stage.
---@class PatchDefinition<T, V>
---@field Label string The addressables label whose assets this patch targets.
---@field Name string The patch's namespaced name, typically `modId:<supplied-name>`.
---@field PatchModId string The host mod's ID, used as the namespace for dependency-resolution lookups.
---@field Names fun(): string The asset names this patch targets, or empty to match every asset under Label.
---@field NeedsMods fun(): string The mod GUIDs this patch requires to run.
---@field ConflictsMods fun(): string The mod GUIDs this patch refuses to run alongside.
---@field NeedsPatches fun(): string The patch IDs that must also run for this patch to run.
---@field ConflictsPatches fun(): string The patch IDs this patch refuses to run alongside.
---@field AfterPatches fun(): string The patch IDs this patch runs after when they are present.
---@field AfterMods fun(): string The mod IDs whose patches this patch runs after when present.
---@field BeforePatches fun(): string The patch IDs this patch runs before when they are present.
---@field BeforeMods fun(): string The mod IDs whose patches this patch runs before when present.
local PatchDefinition = {}

---Sets the patch's apply callback.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param patchMethod fun(value: T): string? The supplied callback.
---@return PatchDefinition<T, V> self The patch instance for chaining.
---@error Thrown when a patch method has already been set.
function PatchDefinition:Do(patchMethod) end

---Makes the patch target the assets with these names.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The asset names to target.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Named(...) end

---Makes the patch reject the assets with these names.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The asset names to reject.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:NotNamed(...) end

---Makes the patch require these mod IDs to run.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The required mod IDs.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Needs(...) end

---Makes the patch refuse to run alongside these mod IDs.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The conflicting mod IDs.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Conflicts(...) end

---Makes the patch require these other patches to run.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The required patch IDs, namespaced to the host mod when they do not already carry a namespace.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:NeedsPatch(...) end

---Makes the patch refuse to run alongside these patches.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The conflicting patch IDs, namespaced to the host mod when they do not already carry a namespace.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:ConflictsPatch(...) end

---Makes this patch run after the given patches when they exist.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The patch IDs to run after, namespaced to the host mod when they do not already carry a namespace.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:AfterPatch(...) end

---Makes this patch run after every patch from the given mods.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The mod IDs whose patches this patch should run after.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:After(...) end

---Makes this patch run before the given patches when they exist.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The patch IDs to run before, namespaced to the host mod when they do not already carry a namespace.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:BeforePatch(...) end

---Makes this patch run before every patch from the given mods.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param ... string The mod IDs whose patches this patch should run before.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Before(...) end

---Adds a predicate that gates the patch and reports skips through the summary.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param predicate fun(value: T): boolean The predicate evaluated against each candidate asset.
---@param message? string Optional message logged when the predicate rejects an asset.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Requires(predicate, message) end

---Adds a constant gate that disables the patch entirely when `gate` is
---`false`. Each candidate asset is reported as skipped through the summary.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param gate boolean The constant value the predicate evaluates to.
---@param message? string Optional message logged when the gate is `false`.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Requires(gate, message) end

---Adds a requirement that the asset expose `key`, optionally with a predicate against the resolved value.
---@generic T, V
---@param self PatchDefinition<T, V>
---@param key string The key the asset must expose.
---@param predicate? fun(value: V): boolean Optional predicate evaluated against the value resolved at `key`, not the asset itself.
---@param message? string Optional assertion message logged when the key is present but the predicate fails.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Has(key, predicate, message) end

---Adds a requirement that the asset does not expose `key`
---@generic T, V
---@param self PatchDefinition<T, V>
---@param key string The key the asset must not expose.
---@param message? string Optional assertion message logged when the key is present.
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:HasNo(key, message) end

---Makes the patch run before every Default and Last patch in the same pass.
---@generic T, V
---@param self PatchDefinition<T, V>
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:First() end

---Makes the patch run after every First and Default patch in the same pass.
---@generic T, V
---@param self PatchDefinition<T, V>
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Last() end

---Makes the patch run in the Early pass.
---@generic T, V
---@param self PatchDefinition<T, V>
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Early() end

---Makes the patch run in the Late pass.
---@generic T, V
---@param self PatchDefinition<T, V>
---@return PatchDefinition<T, V> self The patch instance for chaining.
function PatchDefinition:Late() end

---A new asset queued for creation by a Lua patch script.
---@class LuaAsset
---@field CurrentValue any The asset's current Lua-facing value. Initialized at creation time and replaced by each patch that runs against this asset.
---@field Label string The addressables label the asset is tagged with for group-based loading.
---@field Name string The addressables address of the asset (globally unique).

---The PatchManager Lua library, exposed to scripts as the global `PM`.
---@type PatchManagerCore
PM = nil

---Lua module exposed under the global `J` namespace, providing helpers for constructing JSON
---literals that MoonSharp's auto-conversion does not produce. Also installed as callable: invoking
---`J(value)` converts `value` to a JsonUserData.
---@type JsonModule
J = nil
