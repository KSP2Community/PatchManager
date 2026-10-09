---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/ResourcesLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/ResourceConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/ResourceUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/RecipeUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/PatchManager.Resources/UserData/IngredientsUserData.cs
-- Source: Assets/Code/KSP/Sim/Definitions/ResourceCore.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceDefinition.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeDefinition.cs
-- Source: Assets/Code/KSP/Sim/ResourceSystem/ResourceRecipeIngredientDefinition.cs

---Lua submodule exposed as `PM.Resources`, providing patches and creation helpers for resource and recipe definitions.
---@class ResourcesLuaModule
local ResourcesLuaModule = {}

---Registers a resource patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<ResourceUserData | RecipeUserData, any> patch The registered patch.
function ResourcesLuaModule:Patch(name) end

---Registers a patch that copies each resource it matches under a new name, then runs patchMethod on the copy.
---Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
---@param source string The name of the resource to copy. Supports `*` and `?` wildcards.
---@param newName string The name of the copy, with `{name}` standing in for the source's name.
---@param patchMethod? fun(copy: ResourceUserData | RecipeUserData): string? The callback to run on each copy, or nil to copy without changes.
---@return PatchDefinition<ResourceUserData | RecipeUserData, any> patch The registered patch.
function ResourcesLuaModule:Duplicate(source, newName, patchMethod) end

---Creates a new recipe-style resource definition with the given name and runs callback
---against it for further configuration.
---@param name string The recipe's resource name.
---@param callback fun(recipe: RecipeUserData) Callback that receives the new recipe for further configuration.
function ResourcesLuaModule:NewRecipe(name, callback) end

---Creates a new plain resource definition with the given name and runs callback against
---it for further configuration.
---@param name string The resource name.
---@param callback fun(resource: ResourceUserData) Callback that receives the new resource for further configuration.
function ResourcesLuaModule:NewResource(name, callback) end

---Registers a JSON resource-units definition, queueing it under the `resource_units` addressables label
---with a sequential synthetic name.
---@param value any The JSON value describing the resource units, typically a JsonUserData wrapping a `JObject`.
function ResourcesLuaModule:RegisterUnits(value) end

---Plain resource definition wrapper exposing the inner `data` subtree while preserving the full envelope for round-tripping.
---@class ResourceUserData : _ResourceDefinition, JsonUserData

---Recipe-style resource definition wrapper exposing the inner `recipeData` subtree while preserving the full
---envelope, and exposing the `ingredients` array as a typed IngredientsUserData.
---@class RecipeUserData : _ResourceRecipeDefinition, ExtensibleJsonUserData
---@field ingredients IngredientsUserData

---@class IngredientUserData : _ResourceRecipeIngredientDefinition, JsonUserData

---Indexed-list wrapper for a recipe's `ingredients` array, keyed by each ingredient's `name`.
---@class IngredientsUserData : IndexedListUserData<IngredientUserData>
local IngredientsUserData = {}

---Adds a new ingredient with the given name and units-per-recipe-unit ratio.
---@param name string The ingredient's resource name.
---@param unitsPerRecipeUnit number How many units of the ingredient are consumed per unit of recipe output.
function IngredientsUserData:Add(name, unitsPerRecipeUnit) end

---Represents a serializable resource definition, holding a ResourceDefinition or a ResourceRecipeDefinition along with versioning metadata.
---@class _ResourceCore : _JsonUserDataBase
---@field version string The serialization version of this resource instance.
---@field useExternal boolean Indicates whether this resource uses an external resource source.
---@field isRecipe boolean Indicates whether this resource core represents a recipe rather than a direct resource definition.
---@field data ResourceDefinition The ResourceDefinition data for this resource when it is not a recipe.
---@field recipeData ResourceRecipeDefinition The ResourceRecipeDefinition data for this resource when it is a recipe.

---@alias ResourceCore _ResourceCore | { version: string, useExternal: boolean, isRecipe: boolean, data: ResourceDefinition, recipeData: ResourceRecipeDefinition }

---Represents the definition data for a resource in the simulation resource system.
---@class _ResourceDefinition : _JsonUserDataBase
---@field name string The canonical identifier name of this resource.
---@field displayNameKey string The localization key used to look up the display name of this resource.
---@field abbreviationKey string The localization key used to look up the abbreviated name of this resource.
---@field mapOverlayColor Color The color used to represent this resource in map overlay views.
---@field isTweakable boolean A value indicating whether the resource amount can be adjusted by the player in the editor.
---@field isVisible boolean A value indicating whether this resource is shown in the UI.
---@field massPerUnit number The mass in tonnes contributed by one unit of this resource.
---@field volumePerUnit number The volume in liters occupied by one unit of this resource.
---@field specificHeatCapacityPerUnit number The specific heat capacity per unit of this resource.
---@field flowMode ResourceFlowMode The ResourceFlowMode governing how this resource is distributed between parts.
---@field transferMode ResourceTransferMode The ResourceTransferMode governing how this resource can be moved between parts.
---@field costPerUnit number The cost in funds for one unit of this resource.
---@field ignoreForIsp boolean A value indicating whether this resource is excluded from specific impulse calculations.
---@field NonStageable boolean A value indicating whether this resource cannot be staged away when depleted.
---@field IsCryogenic boolean Whether this resource cools fuel tank walls enough to produce cosmetic frost and condensation.
---@field resourceIconAssetAddress string The Addressables asset address for the icon representing this resource.
---@field vfxFuelType string The VFX fuel type identifier used to select visual effects associated with this resource.

---@alias ResourceDefinition _ResourceDefinition | { name: string, displayNameKey: string, abbreviationKey: string, mapOverlayColor: Color, isTweakable: boolean, isVisible: boolean, massPerUnit: number, volumePerUnit: number, specificHeatCapacityPerUnit: number, flowMode: ResourceFlowMode, transferMode: ResourceTransferMode, costPerUnit: number, ignoreForIsp: boolean, NonStageable: boolean, IsCryogenic: boolean, resourceIconAssetAddress: string, vfxFuelType: string }

---Represents the definition of a resource recipe, including its display name, icon, ingredients, and VFX fuel type.
---@class _ResourceRecipeDefinition : _JsonUserDataBase
---@field name string The internal name identifier of the resource recipe.
---@field displayNameKey string The localization key for the human-readable display name of the resource recipe.
---@field abbreviationKey string The localization key for the abbreviated name of the resource recipe.
---@field resourceIconAssetAddress string The Addressable asset address of the icon representing this resource recipe.
---@field ingredients JsonList<ResourceRecipeIngredientDefinition> The array of ResourceRecipeIngredientDefinition entries that make up this recipe, each specifying a resource name and quantity per recipe unit.
---@field vfxFuelType string The VFX fuel type identifier used to select visual effects associated with this resource recipe.
---@field IsCryogenic boolean Whether tanks configured with this recipe produce cosmetic cryogenic effects, regardless of ingredient flags.

---@alias ResourceRecipeDefinition _ResourceRecipeDefinition | { name: string, displayNameKey: string, abbreviationKey: string, resourceIconAssetAddress: string, ingredients: JsonList<ResourceRecipeIngredientDefinition>, vfxFuelType: string, IsCryogenic: boolean }

---Represents a single ingredient in a resource recipe, specifying the resource name and its quantity per recipe unit.
---@class _ResourceRecipeIngredientDefinition : _JsonUserDataBase
---@field name string The name of the resource ingredient.
---@field unitsPerRecipeUnit number The quantity of this resource consumed or produced per recipe unit.

---@alias ResourceRecipeIngredientDefinition _ResourceRecipeIngredientDefinition | { name: string, unitsPerRecipeUnit: number }
