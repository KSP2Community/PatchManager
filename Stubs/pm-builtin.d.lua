---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PatchManagerCore.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/JsonModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/LuaPatchConfig.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/LuaPatch.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/LuaAsset.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Universe.cs

---The PatchManager Lua library, exposed to scripts as the global `PM`.
---@class PatchManagerCore
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
---@param name string The patch's local name; the host mod's ID is prepended to form the full namespaced name.
---@return LuaPatch<JsonUserData, any> patch The registered patch, suitable for chaining (for example LuaPatch.Do).
---@error Thrown when converter is not registered.
function PatchManagerCore:Patch(converter, label, name) end

---Queues a brand-new asset for creation under the given label and address.
---@param converter string The name of the converter that will serialize newObject to JSON.
---@param label string The addressables label to tag the new asset with.
---@param name string The asset's addressables address (globally unique).
---@param newObject any The Lua-facing value for the new asset.
---@error Thrown when converter is not registered.
function PatchManagerCore:New(converter, label, name, newObject) end

---Returns whether the mod with the given ID is loaded.
---@param modId string The mod ID to test.
---@return boolean loaded True if the mod is loaded, false otherwise.
function PatchManagerCore:Loaded(modId) end

---Lua module exposed under the global `J` namespace, providing helpers for constructing JSON literals
---that MoonSharp's auto-conversion does not produce. Also installed as callable: invoking `J(value)`
---converts value to a JsonUserData.
---@class JsonModule
---@operator call(any): JsonUserData
JsonModule = {}

---Returns an empty JSON array as a JsonUserData.
---@return JsonUserData value A JsonUserData wrapping an empty `JArray`.
function JsonModule.Empty() end

---Wraps the first argument as an integer JSON value, forcing the integer slot type.
---@param value number The value cast to a number and truncated to a long.
---@return JsonUserData value A JsonUserData wrapping a `JValue` of integer type.
function JsonModule.Int(value) end

---Redux configs exposed to patch manager. Bindings declared through this object are recorded in
---ConfigReplay so changes to their values invalidate the patch cache.
---@class LuaPatchConfig
LuaPatchConfig = {}

---Binds a boolean config for use in the patching engine. Any changes to the value will trigger an
---invalidation of the patch cache on the next launch.
---@param section string The section of the config file this will be bound in.
---@param name string The name of the value.
---@param defaultValue? boolean The default value, `false` if not passed.
---@param description? string The description of the config value.
---@return boolean value The current config value.
function LuaPatchConfig:Bool(section, name, defaultValue, description) end

---Binds a floating-point config for use in the patching engine. Any changes to the value will
---trigger an invalidation of the patch cache on the next launch.
---@param section string The section of the config file this will be bound in.
---@param name string The name of the value.
---@param defaultValue? number The default value, `0` if not passed.
---@param description? string The description of the config value.
---@return number value The current config value.
---@overload fun(self: LuaPatchConfig, section: string, name: string, defaultValue: number, min: number, max: number, description?: string): number
function LuaPatchConfig:Float(section, name, defaultValue, description) end

---Binds an integer config for use in the patching engine. Any changes to the value will trigger an
---invalidation of the patch cache on the next launch.
---@param section string The section of the config file this will be bound in.
---@param name string The name of the value.
---@param defaultValue? integer The default value, `0` if not passed.
---@param description? string The description of the config value.
---@return integer value The current config value.
---@overload fun(self: LuaPatchConfig, section: string, name: string, defaultValue: integer, min: integer, max: integer, description?: string): integer
function LuaPatchConfig:Integer(section, name, defaultValue, description) end

---Binds a string config for use in the patching engine. Any changes to the value will trigger an
---invalidation of the patch cache on the next launch.
---@param section string The section of the config file this will be bound in.
---@param name string The name of the value.
---@param defaultValue? string The default value, empty string if not passed.
---@param description? string The description of the config value.
---@return string value The current config value.
---@overload fun(self: LuaPatchConfig, section: string, name: string, defaultValue: string, acceptableValues: string[], description?: string): string
function LuaPatchConfig:String(section, name, defaultValue, description) end

---Binds a color config for use in the patching engine. Any changes to the value will trigger an
---invalidation of the patch cache on the next launch. Colors are passed and returned as Lua tables;
---either keyed (`{r=, g=, b=, a=}`) or indexed (`{1, 0.5, 0, 1}`) input is accepted, and output is keyed.
---@param section string The section of the config file this will be bound in.
---@param name string The name of the value.
---@param defaultValue? { r: number, g: number, b: number, a: number } The default value as a Lua table, transparent black if not passed.
---@param description? string The description of the config value.
---@return { r: number, g: number, b: number, a: number } value The current config value as a Lua table with `r`, `g`, `b`, `a` fields.
function LuaPatchConfig:Color(section, name, defaultValue, description) end

---A registered patch operation: which converter, which addressables target, what callback to run, and at which stage.
---@class LuaPatch<T, V>
---@field Label string The addressables label whose assets this patch targets.
---@field Name string The patch's namespaced name, typically `modId:<supplied-name>`.
---@field PatchModId string The host mod's ID, used as the namespace for dependency-resolution lookups.
---@field Names table<string, true> The asset names this patch targets, or empty to match every asset under Label.
---@field NeedsMods table<string, true> The mod GUIDs this patch requires to run.
---@field ConflictsMods table<string, true> The mod GUIDs this patch refuses to run alongside.
---@field NeedsPatches table<string, true> The patch IDs that must also run for this patch to run.
---@field ConflictsPatches table<string, true> The patch IDs this patch refuses to run alongside.
---@field AfterPatches table<string, true> The patch IDs this patch runs after when they are present.
---@field AfterMods table<string, true> The mod IDs whose patches this patch runs after when present.
---@field BeforePatches table<string, true> The patch IDs this patch runs before when they are present.
---@field BeforeMods table<string, true> The mod IDs whose patches this patch runs before when present.
---@field Pass PatchPass The pass the patch runs in.
---@field Ordering PatchOrdering The ordering bucket the patch belongs to within its pass.
LuaPatch = {}

---Sets the patch's apply callback.
---@generic T, V
---@param self LuaPatch<T, V>
---@param patchMethod fun(value: T): string? The supplied callback.
---@return LuaPatch<T, V> self The patch instance for chaining.
---@error Thrown when a patch method has already been set.
function LuaPatch:Do(patchMethod) end

---Makes the patch target the assets with these names.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The asset names to target.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Named(...) end

---Makes the patch reject the assets with these names.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The asset names to reject.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:NotNamed(...) end

---Makes the patch require these mod IDs to run.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The required mod IDs.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Needs(...) end

---Makes the patch refuse to run alongside these mod IDs.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The conflicting mod IDs.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Conflicts(...) end

---Makes the patch require these other patches to run.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The required patch IDs; namespaced to the host mod when they do not already carry a namespace.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:NeedsPatch(...) end

---Makes the patch refuse to run alongside these patches.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The conflicting patch IDs; namespaced to the host mod when they do not already carry a namespace.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:ConflictsPatch(...) end

---Makes this patch run after the given patches when they exist.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The patch IDs to run after; namespaced to the host mod when they do not already carry a namespace.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:AfterPatch(...) end

---Makes this patch run after every patch from the given mods.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The mod IDs whose patches this patch should run after.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:After(...) end

---Makes this patch run before the given patches when they exist.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The patch IDs to run before; namespaced to the host mod when they do not already carry a namespace.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:BeforePatch(...) end

---Makes this patch run before every patch from the given mods.
---@generic T, V
---@param self LuaPatch<T, V>
---@param ... string The mod IDs whose patches this patch should run before.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Before(...) end

---Adds a predicate that gates the patch and reports skips through the summary. Pass a function for
---per-asset evaluation, or a constant boolean to gate the whole patch (for example to feed the
---result of a Config check through).
---@generic T, V
---@param self LuaPatch<T, V>
---@param predicate (fun(value: T): boolean) | boolean The predicate evaluated against each candidate asset, or a constant gate.
---@param message? string Optional message logged when the predicate rejects an asset.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Requires(predicate, message) end

---Adds a requirement that the asset expose key, optionally with a predicate against the resolved value.
---@generic T, V
---@param self LuaPatch<T, V>
---@param key string The key the asset must expose.
---@param predicate? fun(value: V): boolean Optional predicate evaluated against the value resolved at key, not the asset itself.
---@param message? string Optional assertion message logged when the key is present but the predicate fails.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Has(key, predicate, message) end

---Adds a requirement that the asset does not expose key.
---@generic T, V
---@param self LuaPatch<T, V>
---@param key string The key the asset must not expose.
---@param message? string Optional assertion message logged when the key is present.
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:HasNo(key, message) end

---Makes the patch run before every Default and Last patch in the same pass.
---@generic T, V
---@param self LuaPatch<T, V>
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:First() end

---Makes the patch run after every First and Default patch in the same pass.
---@generic T, V
---@param self LuaPatch<T, V>
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Last() end

---Makes the patch run in the Early pass.
---@generic T, V
---@param self LuaPatch<T, V>
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Early() end

---Makes the patch run in the Late pass.
---@generic T, V
---@param self LuaPatch<T, V>
---@return LuaPatch<T, V> self The patch instance for chaining.
function LuaPatch:Late() end

---A new asset queued for creation by a Lua patch script.
---@class LuaAsset
---@field CurrentValue any The asset's current Lua-facing value. Initialized at creation time and replaced by each patch that runs against this asset.
---@field Label string The addressables label the asset is tagged with for group-based loading.
---@field Name string The addressables address of the asset (globally unique).
LuaAsset = {}

---The PatchManager Lua library, exposed to scripts as the global `PM`.
---@type PatchManagerCore
PM = nil

---Lua module exposed under the global `J` namespace, providing helpers for constructing JSON literals
---that MoonSharp's auto-conversion does not produce. Also callable: `J(value)` converts value to a JsonUserData.
---@type JsonModule
J = nil

---Per-script Redux config object. Bindings made through this object are recorded in ConfigReplay so
---changes to their values invalidate the patch cache.
---@type LuaPatchConfig
Config = nil

---The mod ID; exposed to scripts as the `ModId` global.
---@type string
ModId = nil

---The directory the patch was discovered in; exposed to the script as the `Location` global. Nil for patches loaded from a TextAsset.
---@type string?
Location = nil
