---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PatchManager.Resources/ResourcesLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PatchManager.Resources/ResourceConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/ResourceUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/RecipeUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/IngredientsUserData.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/ResourceSystem/ResourceDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeIngredientDefinition.cs

---Lua submodule exposed as `PM.Resources`, providing patches and creation helpers for resource and recipe definitions.
---@class ResourcesLuaModule
local ResourcesLuaModule = {}

---Registers a resource patch with the given namespaced patch name.
---The patch matches every resource by default; restrict it via `LuaPatch.Named`, which supports `*` and `?` wildcards.
---@param name string                                              The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<ResourceUserData | RecipeUserData, any> patch  The registered patch.
function ResourcesLuaModule:Patch(name) end

---Creates a new recipe-style resource definition with the given name and runs callback
---against it for further configuration.
---@param name string                            The recipe's resource name.
---@param callback fun(recipe: RecipeUserData)   Callback that receives the new recipe for further configuration.
function ResourcesLuaModule:NewRecipe(name, callback) end

---Creates a new plain resource definition with the given name and runs callback against
---it for further configuration.
---@param name string                                The resource name.
---@param callback fun(resource: ResourceUserData)   Callback that receives the new resource for further configuration.
function ResourcesLuaModule:NewResource(name, callback) end

---Registers a JSON resource-units definition, queueing it under the `resource_units` addressables label
---with a sequential synthetic name.
---@param value any The JSON value describing the resource units, typically a JsonUserData wrapping a `JObject`.
function ResourcesLuaModule:RegisterUnits(value) end

---Plain resource definition wrapper exposing the inner `data` subtree while preserving the full envelope for round-tripping.
---@class ResourceUserData : _ResourceDefinition, JsonUserData
local ResourceUserData = {}

---Recipe-style resource definition wrapper exposing the inner `recipeData` subtree while preserving the full
---envelope, and exposing the `ingredients` array as a typed IngredientsUserData.
---@class RecipeUserData : _ResourceRecipeDefinition, ExtensibleJsonUserData
---@field ingredients IngredientsUserData The recipe's ingredients array, surfaced as a typed wrapper.
local RecipeUserData = {}

---Synthetic per-element wrapper for entries of an `IngredientsUserData`.
---@class IngredientUserData : _ResourceRecipeIngredientDefinition, JsonUserData

---Indexed-list wrapper for a recipe's `ingredients` array, keyed by each ingredient's `name`.
---@class IngredientsUserData : IndexedListUserData<IngredientUserData>
local IngredientsUserData = {}

---Adds a new ingredient with the given name and units-per-recipe-unit ratio.
---@param name string                The ingredient's resource name.
---@param unitsPerRecipeUnit number  How many units of the ingredient are consumed per unit of recipe output.
function IngredientsUserData:Add(name, unitsPerRecipeUnit) end

---Resource asset JSON envelope wrapping a plain `ResourceDefinition` under the `data` field.
---@class _ResourceDefinitionEnvelope : _JsonUserDataBase
---@field version number               Asset schema version.
---@field useExternal boolean          Whether the asset is loaded from an external source.
---@field isRecipe? boolean            When true, the envelope carries `recipeData` instead of `data`; routes to RecipeDefinitionEnvelope.
---@field data ResourceDefinition      The inner resource definition payload.

---@alias ResourceDefinitionEnvelope _ResourceDefinitionEnvelope | { version: number, useExternal: boolean, isRecipe: boolean?, data: ResourceDefinition }

---Recipe asset JSON envelope wrapping a `ResourceRecipeDefinition` under the `recipeData` field.
---@class _RecipeDefinitionEnvelope : _JsonUserDataBase
---@field version number                       Asset schema version.
---@field useExternal boolean                  Whether the asset is loaded from an external source.
---@field isRecipe boolean                     Always true for recipe envelopes.
---@field recipeData ResourceRecipeDefinition  The inner recipe definition payload.

---@alias RecipeDefinitionEnvelope _RecipeDefinitionEnvelope | { version: number, useExternal: boolean, isRecipe: boolean, recipeData: ResourceRecipeDefinition }

---Represents the definition data for a resource in the simulation resource system.
---@class _ResourceDefinition : _JsonUserDataBase
---@field name string
---@field displayNameKey string
---@field abbreviationKey string
---@field mapOverlayColor Color
---@field isTweakable boolean
---@field isVisible boolean
---@field massPerUnit number
---@field volumePerUnit number
---@field specificHeatCapacityPerUnit number
---@field flowMode ResourceFlowMode
---@field transferMode ResourceTransferMode
---@field costPerUnit number
---@field ignoreForIsp boolean
---@field NonStageable boolean
---@field resourceIconAssetAddress string
---@field vfxFuelType string

---@alias ResourceDefinition _ResourceDefinition | { name: string, displayNameKey: string, abbreviationKey: string, mapOverlayColor: Color, isTweakable: boolean, isVisible: boolean, massPerUnit: number, volumePerUnit: number, specificHeatCapacityPerUnit: number, flowMode: ResourceFlowMode, transferMode: ResourceTransferMode, costPerUnit: number, ignoreForIsp: boolean, NonStageable: boolean, resourceIconAssetAddress: string, vfxFuelType: string }

---Represents the definition of a resource recipe, including its display name, icon, ingredients, and VFX fuel type.
---@class _ResourceRecipeDefinition : _JsonUserDataBase
---@field name string
---@field displayNameKey string
---@field abbreviationKey string
---@field resourceIconAssetAddress string
---@field ingredients JsonList<ResourceRecipeIngredientDefinition>
---@field vfxFuelType string

---@alias ResourceRecipeDefinition _ResourceRecipeDefinition | { name: string, displayNameKey: string, abbreviationKey: string, resourceIconAssetAddress: string, ingredients: JsonList<ResourceRecipeIngredientDefinition>, vfxFuelType: string }

---Represents a single ingredient in a resource recipe, specifying the resource name and its quantity per recipe unit.
---@class _ResourceRecipeIngredientDefinition : _JsonUserDataBase
---@field name string
---@field unitsPerRecipeUnit number

---@alias ResourceRecipeIngredientDefinition _ResourceRecipeIngredientDefinition | { name: string, unitsPerRecipeUnit: number }
