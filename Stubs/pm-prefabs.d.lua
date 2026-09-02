---@meta
-- Patch Manager declarative prefab-patch frontend.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PatchManagerCore.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Builtin/PrefabPatchLuaBuilder.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PrefabPatching/PrefabPatchBuilder.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/PrefabPatching/PrefabPatchModel.cs

---Describes how Patch Manager locates a GameObject or Component in the stock
---prefab. Key-first Lua authoring normally uses `hierarchyPath`; manifests
---compiled from visual prefab variants may use `siblingIndices` instead.
---@class PrefabPatchRuntimeLocator
---@field siblingIndices integer[]? Zero-based sibling indices from the prefab root to the target.
---@field hierarchyPath string? Slash-separated hierarchy path; the root name is optional.
---@field targetKind '"GameObject"'|'"Component"' Whether the locator resolves a GameObject or one of its Components.
---@field componentType string? Full CLR component type name when `targetKind` is `"Component"`.
---@field componentOrdinal integer? Zero-based ordinal when a GameObject has multiple Components of the same type.
---@field displayPath string? Human-readable path used in diagnostics.

---Targets an object or Component inherited from the stock prefab.
---@class PrefabPatchStockTarget
---@field kind '"Stock"'
---@field objectType string Full CLR type name expected at the resolved location.
---@field runtimeLocator PrefabPatchRuntimeLocator Runtime traversal information for the stock prefab.

---Targets a GameObject introduced by this patch or by another named patch.
---Use an unqualified `ownerPatchId` for a patch in the current mod; Patch
---Manager adds the current mod ID when the manifest is registered.
---@class PrefabPatchOwnedTarget
---@field kind '"PatchOwned"'
---@field ownerPatchId string Owning patch ID, either a local patch name or a fully qualified `modId:patchName`.
---@field objectId string Stable object ID declared by the owning patch's `PrefabPatchObjectFragment`.
---@field objectType string? Expected CLR object type; normally `UnityEngine.GameObject`.
---@field runtimeLocator PrefabPatchRuntimeLocator? Optional diagnostic locator; patch-owned identity is based on IDs.

---Targets a Component introduced by this patch or by another named patch.
---Use an unqualified `ownerPatchId` for a patch in the current mod.
---@class PrefabPatchOwnedComponentTarget
---@field kind '"PatchComponent"'
---@field ownerPatchId string Owning patch ID, either a local patch name or a fully qualified `modId:patchName`.
---@field componentId string Stable component ID declared by the owning patch's `PrefabPatchComponentFragment`.
---@field objectType string? Expected full CLR component type name.

---A stock target, a patch-owned GameObject, or a patch-owned Component.
---@alias PrefabPatchTarget
---| PrefabPatchStockTarget
---| PrefabPatchOwnedTarget
---| PrefabPatchOwnedComponentTarget

---References an asset loaded through Addressables.
---@class PrefabPatchAddressableReference
---@field kind '"Addressable"'
---@field address string Addressables key of the referenced Unity object.
---@field expectedType string? Full CLR type name used to validate the loaded object.

---References another object represented by a prefab-patch target.
---@class PrefabPatchTargetReference
---@field kind '"Target"'
---@field target PrefabPatchTarget Target whose resolved Unity object should be assigned.
---@field expectedType string? Full CLR type name used to validate the resolved object.

---A Unity object reference assigned by a prefab-patch operation.
---@alias PrefabPatchReference
---| PrefabPatchAddressableReference
---| PrefabPatchTargetReference

---The serialized value kinds accepted by the prefab-patch composer.
---@alias PrefabPatchValueKind
---| '"Boolean"'
---| '"Integer"'
---| '"Float"'
---| '"String"'
---| '"Vector2"'
---| '"Vector3"'
---| '"Vector4"'
---| '"Quaternion"'
---| '"Color"'
---| '"ArraySize"'
---| '"ManagedReference"'
---| '"Json"'

---A JSON-safe typed value for a Unity serialized property. Only the payload
---fields required by `kind` need to be supplied. Plain Lua booleans, numbers,
---and strings can be passed to `Set`; use this class for vectors, colors,
---integers, array sizes, managed references, or explicit JSON payloads.
---@class PrefabPatchValue
---@field kind PrefabPatchValueKind Selects which payload fields Patch Manager reads.
---@field boolean boolean? Payload for `"Boolean"`.
---@field integer integer? Payload for `"Integer"` and `"ArraySize"`.
---@field float number? Payload for `"Float"`.
---@field string string? Payload for `"String"` or serialized `"Json"`.
---@field serializedType string? Assembly-qualified concrete type for `"ManagedReference"`.
---@field x number? X or red component for vector, quaternion, and color values.
---@field y number? Y or green component for vector, quaternion, and color values.
---@field z number? Z or blue component for Vector3, Vector4, Quaternion, and Color values.
---@field w number? W or alpha component for Vector4, Quaternion, and Color values.

---One non-object serialized property captured for a patch-owned Component.
---@class PrefabPatchSerializedValue
---@field propertyPath string Unity `SerializedProperty` path relative to the Component.
---@field value PrefabPatchValue Typed value written to the property.

---One Unity object reference captured separately from a Component's scalar
---serialized values so it can be restored after all patch-owned objects exist.
---@class PrefabPatchSerializedReference
---@field propertyPath string Unity `SerializedProperty` path relative to the Component.
---@field reference PrefabPatchReference Object reference written to the property.

---Serialized definition of a Component introduced by a prefab patch.
---@class PrefabPatchComponentFragment
---@field componentId string Stable ID unique within the owning patch and usable by later patches.
---@field componentType string Assembly-qualified CLR component type to add.
---@field values PrefabPatchSerializedValue[]? Non-object serialized properties to initialize.
---@field references PrefabPatchSerializedReference[]? Unity object references restored after object creation.

---Inline hierarchy definition for a GameObject introduced by a prefab patch.
---Every object has a stable `objectId`, allowing dependent patches to target it
---without relying on its display name or hierarchy position.
---@class PrefabPatchObjectFragment
---@field objectId string Stable ID unique within the owning patch.
---@field name string? GameObject name; defaults to the object ID when omitted.
---@field transformType string? Assembly-qualified Transform type; use RectTransform for UI objects.
---@field active boolean? Initial active state; defaults to true.
---@field layer integer? Initial Unity layer.
---@field tag string? Initial Unity tag; defaults to `Untagged`.
---@field isStatic boolean? Initial `GameObject.isStatic` value.
---@field localPosition PrefabPatchValue? Local Transform position.
---@field localRotation PrefabPatchValue? Local Transform rotation.
---@field localScale PrefabPatchValue? Local Transform scale.
---@field anchorMin PrefabPatchValue? RectTransform minimum anchor.
---@field anchorMax PrefabPatchValue? RectTransform maximum anchor.
---@field anchoredPosition PrefabPatchValue? RectTransform anchored position.
---@field sizeDelta PrefabPatchValue? RectTransform size delta.
---@field pivot PrefabPatchValue? RectTransform pivot.
---@field components PrefabPatchComponentFragment[]? Components introduced on this GameObject.
---@field children PrefabPatchObjectFragment[]? Nested patch-owned child GameObjects.

---Fluent Lua frontend for one declarative prefab patch. Builder methods mutate
---the pending manifest and return the same builder unless documented otherwise.
---@class PrefabPatchLuaBuilder
PrefabPatchLuaBuilder = {}

---Places the patch in the Early pass.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Early() end

---Places the patch in the Late pass.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Late() end

---Places the patch in the First ordering bucket within its pass.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:First() end

---Places the patch in the Last ordering bucket within its pass.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Last() end

---Requires all listed mods. Patch Manager skips this patch when a required mod
---is unavailable.
---@param ... string The required mod IDs.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Needs(...) end

---Declares this patch incompatible with the listed mods.
---@param ... string The conflicting mod IDs.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Conflicts(...) end

---Requires all listed prefab patches. Local names are qualified with the
---current mod ID; use `modId:patchName` to reference another mod's patch.
---@param ... string Required local names or fully qualified patch IDs.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:NeedsPatch(...) end

---Declares this patch incompatible with the listed prefab patches. Local names
---are qualified with the current mod ID.
---@param ... string Conflicting local names or fully qualified patch IDs.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:ConflictsPatch(...) end

---Orders this patch before the listed patches when they are present.
---@param ... string Local names or fully qualified patch IDs to run before.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:BeforePatch(...) end

---Orders this patch after the listed patches when they are present.
---@param ... string Local names or fully qualified patch IDs to run after.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:AfterPatch(...) end

---Orders this patch before every present prefab patch owned by the listed mods.
---@param ... string Mod IDs whose patches should run later.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Before(...) end

---Orders this patch after every present prefab patch owned by the listed mods.
---@param ... string Mod IDs whose patches should run earlier.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:After(...) end

---Declares configuration identifiers that affect the patch, making them part
---of the resolved-plan cache input.
---@param ... string Stable configuration identifiers used by this patch.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Configuration(...) end

---Adds a typed serialized-property assignment to any prefab-patch target.
---Plain Lua numbers are emitted as `"Float"` values; use `PrefabPatchValue`
---when an integer or another explicit value kind is required.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget Object or Component whose property should change.
---@param propertyPath string Unity `SerializedProperty` path to write.
---@param value boolean|number|string|PrefabPatchValue Value to serialize.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Set(operationId, target, propertyPath, value) end

---Convenience form of `Set` for a Component inherited from the stock prefab.
---@param operationId string Stable operation ID unique within this patch.
---@param hierarchyPath string Slash-separated GameObject path; the root name is optional.
---@param componentType string Full CLR component type name, for example `UnityEngine.UI.Image`.
---@param propertyPath string Unity `SerializedProperty` path to write.
---@param value boolean|number|string|PrefabPatchValue Value to serialize.
---@param componentOrdinal? integer Zero-based ordinal when the GameObject has multiple Components of this type.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:SetComponent(
    operationId,
    hierarchyPath,
    componentType,
    propertyPath,
    value,
    componentOrdinal
)
end

---Creates a target for a GameObject inherited from the stock prefab.
---@param hierarchyPath string Slash-separated path; the root name is optional.
---@return PrefabPatchStockTarget target A reusable stock GameObject target.
function PrefabPatchLuaBuilder:GameObject(hierarchyPath) end

---Creates a target for a Component inherited from the stock prefab.
---@param hierarchyPath string Slash-separated GameObject path; the root name is optional.
---@param componentType string Full CLR component type name.
---@param componentOrdinal? integer Zero-based ordinal when the GameObject has multiple Components of this type.
---@return PrefabPatchStockTarget target A reusable stock Component target.
function PrefabPatchLuaBuilder:Component(hierarchyPath, componentType, componentOrdinal) end

---Adds a Unity object-reference assignment to a serialized property.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget Object or Component whose property should change.
---@param propertyPath string Unity `SerializedProperty` path to write.
---@param reference PrefabPatchReference Addressable or prefab-target reference to assign.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Reference(operationId, target, propertyPath, reference) end

---Sets a target GameObject active or inactive.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget GameObject target whose active state should change.
---@param active boolean Desired active state.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Active(operationId, target, active) end

---Deactivates a stock or patch-owned GameObject instead of destroying it, so
---later patches can still resolve the same target.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget GameObject target to suppress.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:Suppress(operationId, target) end

---Adds a patch-owned GameObject hierarchy. Passing `nil` as `parent` attaches
---the fragment directly beneath the effective prefab root.
---@param operationId string Stable operation ID unique within this patch.
---@param parent PrefabPatchTarget? Parent GameObject target, or nil for the prefab root.
---@param fragment PrefabPatchObjectFragment Hierarchy fragment to create.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:AddObject(operationId, parent, fragment) end

---Adds a patch-owned Component to an existing GameObject.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget GameObject target that receives the Component.
---@param component PrefabPatchComponentFragment Component definition to create.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:AddComponent(operationId, target, component) end

---Removes a Component resolved by the supplied target.
---@param operationId string Stable operation ID unique within this patch.
---@param target PrefabPatchTarget Component target to remove.
---@return PrefabPatchLuaBuilder self The same builder for chaining.
function PrefabPatchLuaBuilder:RemoveComponent(operationId, target) end

---Finalizes the pending manifest and returns its JSON-compatible public form
---without registering it with the prefab-patch runtime.
---@return JsonUserData manifest Table-like serialized prefab-patch manifest.
function PrefabPatchLuaBuilder:Build() end

---Finalizes and registers the prefab patch with Patch Manager. This method is
---terminal and intentionally returns no CLR manifest to Lua.
function PrefabPatchLuaBuilder:Register() end

---Begins a declarative prefab patch for one stock Addressables prefab. This can
---only be called while Patch Manager's registration phase is open.
---@param name string Patch-local name; the current mod ID is prepended automatically.
---@param target string Stock prefab Addressables key.
---@return PrefabPatchLuaBuilder builder Builder used to declare and register the patch.
---@error Thrown outside patch registration, when `name` is invalid, or when `target` is not a string key.
function PatchManagerCore:Prefab(name, target) end
