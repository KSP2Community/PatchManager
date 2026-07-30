---@meta
-- Patch Manager declarative prefab-patch frontend.

---@alias PrefabPatchTarget table
---| { kind: '"Stock"', objectType: string, runtimeLocator: table }
---| { kind: '"PatchOwned"', ownerPatchId: string, objectId: string, objectType: string?, runtimeLocator: table? }
---| { kind: '"PatchComponent"', ownerPatchId: string, componentId: string, objectType: string? }

---@alias PrefabPatchReference table
---| { kind: '"Addressable"', address: string, expectedType: string? }
---| { kind: '"Target"', target: PrefabPatchTarget, expectedType: string? }

---@class PrefabPatchValue
---@field kind '"Boolean"'|'"Integer"'|'"Float"'|'"String"'|'"Vector2"'|'"Vector3"'|'"Vector4"'|'"Quaternion"'|'"Color"'|'"ArraySize"'|'"ManagedReference"'|'"Json"'
---@field boolean boolean?
---@field integer integer?
---@field float number?
---@field string string?
---@field serializedType string?
---@field x number?
---@field y number?
---@field z number?
---@field w number?

---@class PrefabPatchSerializedValue
---@field propertyPath string
---@field value PrefabPatchValue

---@class PrefabPatchSerializedReference
---@field propertyPath string
---@field reference PrefabPatchReference

---@class PrefabPatchComponentFragment
---@field componentId string Stable ID unique within the patch.
---@field componentType string Assembly-qualified CLR component type.
---@field values PrefabPatchSerializedValue[]?
---@field references PrefabPatchSerializedReference[]?

---@class PrefabPatchObjectFragment
---@field objectId string Stable ID unique within the patch.
---@field name string?
---@field transformType string? Assembly-qualified Transform or RectTransform type.
---@field active boolean?
---@field layer integer?
---@field tag string?
---@field isStatic boolean?
---@field localPosition PrefabPatchValue?
---@field localRotation PrefabPatchValue?
---@field localScale PrefabPatchValue?
---@field anchorMin PrefabPatchValue?
---@field anchorMax PrefabPatchValue?
---@field anchoredPosition PrefabPatchValue?
---@field sizeDelta PrefabPatchValue?
---@field pivot PrefabPatchValue?
---@field components PrefabPatchComponentFragment[]?
---@field children PrefabPatchObjectFragment[]?

---@class PrefabPatchLuaBuilder
PrefabPatchLuaBuilder = {}

---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Early() end
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Late() end
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:First() end
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Last() end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Needs(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Conflicts(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:NeedsPatch(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:ConflictsPatch(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:BeforePatch(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:AfterPatch(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Before(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:After(...) end
---@param ... string
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Configuration(...) end
---@param operationId string
---@param target PrefabPatchTarget
---@param propertyPath string
---@param value boolean|number|string|PrefabPatchValue
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Set(operationId, target, propertyPath, value) end
---@param operationId string
---@param hierarchyPath string Slash-separated path; the root name is optional.
---@param componentType string Full CLR type name, for example UnityEngine.UI.Image.
---@param propertyPath string
---@param value boolean|number|string|PrefabPatchValue
---@param componentOrdinal? integer Zero-based ordinal when the GameObject has multiple components of this type.
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:SetComponent(operationId, hierarchyPath, componentType, propertyPath, value, componentOrdinal) end
---@param hierarchyPath string Slash-separated path; the root name is optional.
---@return PrefabPatchTarget target
function PrefabPatchLuaBuilder:GameObject(hierarchyPath) end
---@param hierarchyPath string Slash-separated path; the root name is optional.
---@param componentType string Full CLR type name.
---@param componentOrdinal? integer Zero-based ordinal when the GameObject has multiple components of this type.
---@return PrefabPatchTarget target
function PrefabPatchLuaBuilder:Component(hierarchyPath, componentType, componentOrdinal) end
---@param operationId string
---@param target PrefabPatchTarget
---@param propertyPath string
---@param reference PrefabPatchReference
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Reference(operationId, target, propertyPath, reference) end
---@param operationId string
---@param target PrefabPatchTarget
---@param active boolean
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Active(operationId, target, active) end
---@param operationId string
---@param target PrefabPatchTarget
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:Suppress(operationId, target) end
---@param operationId string
---@param parent PrefabPatchTarget?
---@param fragment PrefabPatchObjectFragment
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:AddObject(operationId, parent, fragment) end
---@param operationId string
---@param target PrefabPatchTarget
---@param component PrefabPatchComponentFragment
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:AddComponent(operationId, target, component) end
---@param operationId string
---@param target PrefabPatchTarget
---@return PrefabPatchLuaBuilder self
function PrefabPatchLuaBuilder:RemoveComponent(operationId, target) end
---@return table manifest
function PrefabPatchLuaBuilder:Build() end
---@return table manifest
function PrefabPatchLuaBuilder:Register() end

---@param name string Patch-local name; ModId is prepended automatically.
---@param target string Stock prefab Addressables key.
---@return PrefabPatchLuaBuilder builder
function PatchManagerCore:Prefab(name, target) end
