---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Newtonsoft.Json default serialization of UnityEngine math types
-- Source: ksp2redux/Assets/Code/Root/Vector3d.cs

---JSON shape of `UnityEngine.Bounds`.
---@class Bounds : JsonUserData
---@field m_Center Vector3
---@field m_Extent Vector3

---JSON shape of `UnityEngine.Color`.
---@class Color : JsonUserData
---@field r number
---@field g number
---@field b number
---@field a number

---JSON shape of `UnityEngine.Color32`. Components are 0-255 integers.
---@class Color32 : JsonUserData
---@field r integer
---@field g integer
---@field b integer
---@field a integer

---JSON shape of `UnityEngine.AnimationCurve` (the Newtonsoft serialization). Named `Curve` for brevity.
---@class Curve : JsonUserData
---@field m_Curve JsonList<Keyframe>
---@field m_PostInfinity integer
---@field m_PreInfinity integer
---@field m_RotationOrder integer
---@field serializedVersion string

---JSON shape of `UnityEngine.Gradient`. Newtonsoft uses Unity's serialization layout with up to 8 key slots.
---@class Gradient : JsonUserData
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

---JSON shape of `UnityEngine.GradientAlphaKey`.
---@class GradientAlphaKey : JsonUserData
---@field alpha number
---@field time number

---JSON shape of `UnityEngine.GradientColorKey`.
---@class GradientColorKey : JsonUserData
---@field color Color
---@field time number

---JSON shape of `UnityEngine.Keyframe` - a single point on an `AnimationCurve`.
---@class Keyframe : JsonUserData
---@field inTangent number
---@field inWeight number
---@field outTangent number
---@field outWeight number
---@field tangentMode integer
---@field time number
---@field value number
---@field weightedMode integer

---JSON shape of `UnityEngine.LayerMask`. Serialized as a single integer mask.
---@alias LayerMask integer

---JSON shape of `UnityEngine.Quaternion`.
---@class Quaternion : JsonUserData
---@field x number
---@field y number
---@field z number
---@field w number

---JSON shape of `UnityEngine.Rect`.
---@class Rect : JsonUserData
---@field height number
---@field width number
---@field x number
---@field y number

---JSON shape of `UnityEngine.Vector2`.
---@class Vector2 : JsonUserData
---@field x number
---@field y number

---JSON shape of `UnityEngine.Vector3`.
---@class Vector3 : JsonUserData
---@field x number
---@field y number
---@field z number

---JSON shape of KSP's double-precision `Vector3d`.
---@class Vector3d : JsonUserData
---@field x number
---@field y number
---@field z number

---JSON shape of `UnityEngine.Vector4`.
---@class Vector4 : JsonUserData
---@field x number
---@field y number
---@field z number
---@field w number
