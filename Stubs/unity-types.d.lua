---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Newtonsoft.Json default serialization of UnityEngine math types
-- Source: ksp2redux/Assets/Code/Root/Vector3d.cs

---JSON shape of `UnityEngine.Bounds`.
---@class _Bounds : _JsonUserDataBase
---@field m_Center Vector3
---@field m_Extent Vector3

---@alias Bounds _Bounds | { m_Center: Vector3, m_Extent: Vector3 }

---JSON shape of `UnityEngine.Color`.
---@class _Color : _JsonUserDataBase
---@field r number
---@field g number
---@field b number
---@field a number

---@alias Color _Color | { r: number, g: number, b: number, a: number }

---JSON shape of `UnityEngine.Color32`. Components are 0-255 integers.
---@class _Color32 : _JsonUserDataBase
---@field r integer
---@field g integer
---@field b integer
---@field a integer

---@alias Color32 _Color32 | { r: integer, g: integer, b: integer, a: integer }

---JSON shape of `UnityEngine.AnimationCurve` (the Newtonsoft serialization). Named `Curve` for brevity.
---@class _Curve : _JsonUserDataBase
---@field m_Curve JsonList<Keyframe>
---@field m_PostInfinity integer
---@field m_PreInfinity integer
---@field m_RotationOrder integer
---@field serializedVersion string

---@alias Curve _Curve | { m_Curve: JsonList<Keyframe>, m_PostInfinity: integer, m_PreInfinity: integer, m_RotationOrder: integer, serializedVersion: string }

---JSON shape of `UnityEngine.Gradient`. Newtonsoft uses Unity's serialization layout with up to 8 key slots.
---@class _Gradient : _JsonUserDataBase
---@field atime0 integer
---@field atime1 integer
---@field atime2 integer
---@field atime3 integer
---@field atime4 integer
---@field atime5 integer
---@field atime6 integer
---@field atime7 integer
---@field ctime0 integer
---@field ctime1 integer
---@field ctime2 integer
---@field ctime3 integer
---@field ctime4 integer
---@field ctime5 integer
---@field ctime6 integer
---@field ctime7 integer
---@field key0 Color
---@field key1 Color
---@field key2 Color
---@field key3 Color
---@field key4 Color
---@field key5 Color
---@field key6 Color
---@field key7 Color
---@field m_Mode integer
---@field m_NumAlphaKeys integer
---@field m_NumColorKeys integer
---@field serializedVersion string

---@alias Gradient _Gradient | { atime0: integer, atime1: integer, atime2: integer, atime3: integer, atime4: integer, atime5: integer, atime6: integer, atime7: integer, ctime0: integer, ctime1: integer, ctime2: integer, ctime3: integer, ctime4: integer, ctime5: integer, ctime6: integer, ctime7: integer, key0: Color, key1: Color, key2: Color, key3: Color, key4: Color, key5: Color, key6: Color, key7: Color, m_Mode: integer, m_NumAlphaKeys: integer, m_NumColorKeys: integer, serializedVersion: string }

---JSON shape of `UnityEngine.GradientAlphaKey`.
---@class _GradientAlphaKey : _JsonUserDataBase
---@field alpha number
---@field time number

---@alias GradientAlphaKey _GradientAlphaKey | { alpha: number, time: number }

---JSON shape of `UnityEngine.GradientColorKey`.
---@class _GradientColorKey : _JsonUserDataBase
---@field color Color
---@field time number

---@alias GradientColorKey _GradientColorKey | { color: Color, time: number }

---JSON shape of `UnityEngine.Keyframe` - a single point on an `AnimationCurve`.
---@class _Keyframe : _JsonUserDataBase
---@field inTangent number
---@field inWeight number
---@field outTangent number
---@field outWeight number
---@field tangentMode integer
---@field time number
---@field value number
---@field weightedMode integer

---@alias Keyframe _Keyframe | { inTangent: number, inWeight: number, outTangent: number, outWeight: number, tangentMode: integer, time: number, value: number, weightedMode: integer }

---JSON shape of `UnityEngine.LayerMask`. Serialized as a single integer mask.
---@alias LayerMask integer

---JSON shape of `UnityEngine.Quaternion`.
---@class _Quaternion : _JsonUserDataBase
---@field x number
---@field y number
---@field z number
---@field w number

---@alias Quaternion _Quaternion | { x: number, y: number, z: number, w: number }

---JSON shape of `UnityEngine.Rect`.
---@class _Rect : _JsonUserDataBase
---@field height number
---@field width number
---@field x number
---@field y number

---@alias Rect _Rect | { height: number, width: number, x: number, y: number }

---JSON shape of `UnityEngine.Vector2`.
---@class _Vector2 : _JsonUserDataBase
---@field x number
---@field y number

---@alias Vector2 _Vector2 | { x: number, y: number }

---JSON shape of `UnityEngine.Vector3`.
---@class _Vector3 : _JsonUserDataBase
---@field x number
---@field y number
---@field z number

---@alias Vector3 _Vector3 | { x: number, y: number, z: number }

---JSON shape of KSP's double-precision `Vector3d`.
---@class _Vector3d : _JsonUserDataBase
---@field x number
---@field y number
---@field z number

---@alias Vector3d _Vector3d | { x: number, y: number, z: number }

---JSON shape of `UnityEngine.Vector4`.
---@class _Vector4 : _JsonUserDataBase
---@field x number
---@field y number
---@field z number
---@field w number

---@alias Vector4 _Vector4 | { x: number, y: number, z: number, w: number }
