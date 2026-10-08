---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/PrefabPatching/PrefabPatchModel.cs
-- Source: Assets/Modules/PatchManager/Runtime/PrefabPatching/PrefabPatchJson.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PrefabPatchLuaBuilder.cs
-- Source: Assets/Modules/PatchManager/Runtime/PrefabPatching/PrefabPatchBuilder.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PatchManagerCore.cs

---Runtime traversal data. Visual compilation uses sibling indices backed by
---canonical source identity; key-first C#/Lua authoring uses a hierarchy path.
---@class PrefabPatchRuntimeLocator
---@field siblingIndices? integer[] Sibling indices from the prefab root to the target.
---@field hierarchyPath? string An optional slash-delimited hierarchy path for imperative patches.
---@field targetKind "GameObject" | "Component" The Unity object kind produced by the locator.
---@field componentType? string The assembly-qualified component type when targeting a component.
---@field componentOrdinal? integer The zero-based component occurrence on the located GameObject.
---@field displayPath? string A diagnostic-only human-readable authoring path.

---An object or component inherited from the stock prefab.
---@class PrefabPatchStockTarget
---@field kind "Stock" The source domain of the target.
---@field objectType string The expected assembly-qualified Unity object type.
---@field runtimeLocator PrefabPatchRuntimeLocator The traversal information for stock targets.

---A GameObject introduced by a patch operation.
---@class PrefabPatchOwnedTarget
---@field kind "PatchOwned" The source domain of the target.
---@field ownerPatchId string The namespaced patch ID that introduced a patch-owned target.
---@field objectId string The stable local ID of a patch-owned GameObject.
---@field objectType? string The expected assembly-qualified Unity object type.
---@field runtimeLocator? PrefabPatchRuntimeLocator The traversal information for stock targets.

---A component introduced by a patch operation.
---@class PrefabPatchOwnedComponentTarget
---@field kind "PatchComponent" The source domain of the target.
---@field ownerPatchId string The namespaced patch ID that introduced a patch-owned target.
---@field componentId string The stable local ID of a patch-owned component.
---@field objectType? string The expected assembly-qualified Unity object type.
---@field runtimeLocator? PrefabPatchRuntimeLocator The traversal information for stock targets.

---An inherited source object or an object introduced by a named patch.
---@alias PrefabPatchTarget
---| PrefabPatchStockTarget
---| PrefabPatchOwnedTarget
---| PrefabPatchOwnedComponentTarget

---Resolve the object from an Addressables key.
---@class PrefabPatchAddressableReference
---@field kind "Addressable" The resolution strategy.
---@field address string The Addressables key for an external reference.
---@field expectedType? string The optional assembly-qualified type expected after resolution.

---Resolve the object from the effective prefab hierarchy.
---@class PrefabPatchTargetReference
---@field kind "Target" The resolution strategy.
---@field target PrefabPatchTarget The effective-prefab target for a local reference.
---@field expectedType? string The optional assembly-qualified type expected after resolution.

---Addressable or target-local Unity object reference used by
---SetObjectReference or a component fragment.
---@alias PrefabPatchReference
---| PrefabPatchAddressableReference
---| PrefabPatchTargetReference

---Identifies the payload stored in a PrefabPatchValue.
---@alias PrefabPatchValueKind
---| "Boolean" # A Boolean value.
---| "Integer" # A signed integer value.
---| "Float" # A floating-point value.
---| "String" # A string value.
---| "Vector2" # A two-component vector.
---| "Vector3" # A three-component vector.
---| "Vector4" # A four-component vector.
---| "Quaternion" # A quaternion.
---| "Color" # An RGBA color.
---| "ArraySize" # A serialized array or list size.
---| "ManagedReference" # A managed-reference payload with an explicit CLR type.
---| "Json" # An arbitrary JSON payload interpreted by the target property.

---JSON-safe typed value. Numeric vectors use X/Y/Z/W in their normal Unity
---component order.
---@class PrefabPatchValue
---@field kind PrefabPatchValueKind The active payload representation.
---@field boolean? boolean The Boolean payload.
---@field integer? integer The integer payload.
---@field float? number The floating-point payload.
---@field string? string The string or raw JSON payload.
---@field serializedType? string The assembly-qualified type for managed-reference payloads.
---@field x? number The first vector, quaternion, or color component.
---@field y? number The second vector, quaternion, or color component.
---@field z? number The third vector, quaternion, or color component.
---@field w? number The fourth vector, quaternion, or color component.

---One serialized field/property value captured from an arbitrary component.
---Visual, C#, and Lua authoring all emit this property-stream representation.
---@class PrefabPatchSerializedValue
---@field propertyPath string The Unity SerializedProperty path.
---@field value PrefabPatchValue The value written at the property path.

---One Unity object reference removed from a serialized component payload and
---restored after every patch-owned object and component has been created.
---@class PrefabPatchSerializedReference
---@field propertyPath string The Unity SerializedProperty path.
---@field reference PrefabPatchReference The reference restored after object creation.

---Serialized payload used for added patch-owned objects and AddComponent
---operations.
---@class PrefabPatchComponentFragment
---@field componentId string The stable patch-local component ID.
---@field componentType string The assembly-qualified concrete component type.
---@field values? PrefabPatchSerializedValue[] The serialized non-reference values.
---@field references? PrefabPatchSerializedReference[] The deferred Unity object references.

---An inline, patch-owned hierarchy fragment. Every object has an explicit ID,
---allowing later required patches to target it without hierarchy-name lookup.
---@class PrefabPatchObjectFragment
---@field objectId string The stable patch-local GameObject ID.
---@field name? string The created GameObject name.
---@field transformType? string The assembly-qualified Transform or RectTransform type.
---@field active? boolean Whether the object is active after composition.
---@field layer? integer The Unity layer assigned to the object.
---@field tag? string The Unity tag assigned to the object.
---@field isStatic? boolean Whether the object uses Unity's static flag.
---@field localPosition? PrefabPatchValue The local-position payload.
---@field localRotation? PrefabPatchValue The local-rotation payload.
---@field localScale? PrefabPatchValue The local-scale payload.
---@field anchorMin? PrefabPatchValue The RectTransform anchor-min payload.
---@field anchorMax? PrefabPatchValue The RectTransform anchor-max payload.
---@field anchoredPosition? PrefabPatchValue The RectTransform anchored-position payload.
---@field sizeDelta? PrefabPatchValue The RectTransform size-delta payload.
---@field pivot? PrefabPatchValue The RectTransform pivot payload.
---@field components? PrefabPatchComponentFragment[] Components created on this object.
---@field children? PrefabPatchObjectFragment[] Child objects created beneath this object.

---Lua frontend for the public declarative prefab-patch schema. Lua tables are
---converted directly into the same model used by C# and visual authoring.
---@class PrefabPatchLuaBuilder
local PrefabPatchLuaBuilder = {}

---Places the patch in the Early pass.
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Early() end

---Places the patch in the Late pass.
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Late() end

---Places the patch in the First bucket of its pass.
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:First() end

---Places the patch in the Last bucket of its pass.
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Last() end

---Requires the supplied mod IDs to be active.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Needs(...) end

---Disables the patch when any supplied mod ID is active.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Conflicts(...) end

---Requires the supplied patch IDs to be enabled.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:NeedsPatch(...) end

---Disables the patch when any supplied patch ID is enabled.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:ConflictsPatch(...) end

---Orders this patch before the supplied patch IDs in the same bucket.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:BeforePatch(...) end

---Orders this patch after the supplied patch IDs in the same bucket.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:AfterPatch(...) end

---Orders this patch before patches owned by the supplied mods.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Before(...) end

---Orders this patch after patches owned by the supplied mods.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:After(...) end

---Adds configuration values that participate in cache invalidation.
---@param ... string
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Configuration(...) end

---Writes a serialized value on the target object or component.
---@param operationId string
---@param target PrefabPatchTarget
---@param propertyPath string
---@param value boolean | number | string | PrefabPatchValue
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Set(operationId, target, propertyPath, value) end

---@param operationId string
---@param hierarchyPath string
---@param componentType string
---@param propertyPath string
---@param value boolean | number | string | PrefabPatchValue
---@param componentOrdinal? integer
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:SetComponent(operationId, hierarchyPath, componentType, propertyPath, value, componentOrdinal) end

---Targets a stock GameObject by hierarchy path.
---@param hierarchyPath string
---@return PrefabPatchStockTarget
function PrefabPatchLuaBuilder:GameObject(hierarchyPath) end

---Targets a stock component by assembly-qualified type and ordinal.
---@param hierarchyPath string
---@param componentType string
---@param componentOrdinal? integer
---@return PrefabPatchStockTarget
function PrefabPatchLuaBuilder:Component(hierarchyPath, componentType, componentOrdinal) end

---Writes an Addressable or target-local Unity object reference.
---@param operationId string
---@param target PrefabPatchTarget
---@param propertyPath string
---@param reference PrefabPatchReference
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Reference(operationId, target, propertyPath, reference) end

---Changes a target GameObject's active state.
---@param operationId string
---@param target PrefabPatchTarget
---@param active boolean
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Active(operationId, target, active) end

---Suppresses a target GameObject in the effective prefab.
---@param operationId string
---@param target PrefabPatchTarget
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:Suppress(operationId, target) end

---Adds an inline patch-owned hierarchy beneath a target parent.
---@param operationId string
---@param parent PrefabPatchTarget?
---@param fragment PrefabPatchObjectFragment
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:AddObject(operationId, parent, fragment) end

---Adds a serialized component fragment to a target GameObject.
---@param operationId string
---@param target PrefabPatchTarget
---@param component PrefabPatchComponentFragment
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:AddComponent(operationId, target, component) end

---Removes the targeted component from the effective prefab.
---@param operationId string
---@param target PrefabPatchTarget
---@return PrefabPatchLuaBuilder
function PrefabPatchLuaBuilder:RemoveComponent(operationId, target) end

---Normalizes relationships, capabilities, ownership, and the manifest hash.
---@return JsonUserData
function PrefabPatchLuaBuilder:Build() end

---Builds and registers the manifest for the current play session.
function PrefabPatchLuaBuilder:Register() end

---Begins a declarative prefab patch for one stock Addressables key.
---@param name string
---@param target string
---@return PrefabPatchLuaBuilder
function PatchManagerCore:Prefab(name, target) end
