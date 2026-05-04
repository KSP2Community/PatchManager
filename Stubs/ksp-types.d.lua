---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/Root/FloatCurve.cs
-- Source: ksp2redux/Assets/Code/KSP/Utilities/Scripting/ScriptMethodReference.cs
-- Source: ksp2redux/Assets/Code/KSP/Utilities/Scripting/ScriptExecutionContext.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/impl/IGGuid.cs
-- Source: ksp2redux/Library/PackageCache/com.unity.addressables@8460f1c9c927/Runtime/AssetReference.cs

---Serializable wrapper around a Unity AnimationCurve for evaluating float values over a time range.
---@class FloatCurve : JsonUserData
---@field fCurve Curve
---@field _minTime number
---@field _maxTime number

---Represents a serializable reference to a named method within a script asset, including the target execution context and method identifier.
---@class ScriptMethodReference : JsonUserData
---@field TargetContext ScriptExecutionContext
---@field ScriptFileAsset string
---@field ScriptMethod string

---Execution context category for a script.
---@alias ScriptExecutionContext
---| 0  # Invalid
---| 1  # Main
---| 2  # Simulation
---| 3  # Mission
---| 4  # Mod
---| 5  # Count

---Represents an in-game globally-unique identifier wrapping a Guid, with support for configurable generation modes including networked-synced multiplayer generation.
---@class IGGuid : JsonUserData
---@field Guid string
---@field DebugName string?

---Reference to an addressable asset.  This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime.
---To determine if the reference is set, use RuntimeKeyIsValid().
---@class AssetReference : JsonUserData
---@field m_AssetGUID string The GUID of an asset.
---@field m_SubObjectName string?
---@field m_SubObjectType string?
