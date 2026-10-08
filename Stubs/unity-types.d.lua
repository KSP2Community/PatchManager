---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Code/Root/Vector3d.cs

---@class _Color : _JsonUserDataBase
---@field r number
---@field g number
---@field b number
---@field a number

---@alias Color _Color | { r: number, g: number, b: number, a: number }

---@class _Curve : _JsonUserDataBase
---@field keys JsonList<Keyframe>
---@field length integer
---@field preWrapMode WrapMode
---@field postWrapMode WrapMode

---@alias Curve _Curve | { keys: JsonList<Keyframe>, length: integer, preWrapMode: WrapMode, postWrapMode: WrapMode }

---@class _Keyframe : _JsonUserDataBase
---@field time number
---@field value number
---@field inTangent number
---@field outTangent number
---@field inWeight number
---@field outWeight number
---@field weightedMode WeightedMode
---@field tangentMode integer

---@alias Keyframe _Keyframe | { time: number, value: number, inTangent: number, outTangent: number, inWeight: number, outWeight: number, weightedMode: WeightedMode, tangentMode: integer }

---@class _Quaternion : _JsonUserDataBase
---@field x number
---@field y number
---@field z number
---@field w number

---@alias Quaternion _Quaternion | { x: number, y: number, z: number, w: number }

---@class _Vector2 : _JsonUserDataBase
---@field x number
---@field y number

---@alias Vector2 _Vector2 | { x: number, y: number }

---@class _Vector3 : _JsonUserDataBase
---@field x number
---@field y number
---@field z number

---@alias Vector3 _Vector3 | { x: number, y: number, z: number }

---Represents a 3D vector with double-precision floating-point components.
---@class _Vector3d : _JsonUserDataBase
---@field x number The x component of the vector.
---@field y number The y component of the vector.
---@field z number The z component of the vector.

---@alias Vector3d _Vector3d | { x: number, y: number, z: number }

---@class _Vector4 : _JsonUserDataBase
---@field x number
---@field y number
---@field z number
---@field w number

---@alias Vector4 _Vector4 | { x: number, y: number, z: number, w: number }
