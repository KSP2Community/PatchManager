---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Code/Root/FloatCurve.cs
-- Source: Assets/Code/KSP/Utilities/Scripting/ScriptMethodReference.cs
-- Source: Assets/Code/KSP/Utilities/Scripting/ScriptExecutionContext.cs
-- Source: Assets/Code/KSP/Sim/impl/IGGuid.cs
-- Source: Assets/Code/KSP/Sim/Definitions/ModuleProperty.cs
-- Source: Assets/Code/KSP/Sim/Definitions/IModuleProperty.cs
-- Source: Assets/Code/KSP/Api/CoreTypes/PropertyReadonly.cs
-- Source: Library/PackageCache/com.unity.addressables@42676f2a154e/Runtime/AssetReference.cs

---Serializable wrapper around a Unity AnimationCurve for evaluating float values over a time range.
---@class _FloatCurve : _JsonUserDataBase
---@field fCurve Curve
---@field _minTime number
---@field _maxTime number

---@alias FloatCurve _FloatCurve | { fCurve: Curve, _minTime: number, _maxTime: number }

---Represents a serializable reference to a named method within a script asset, including the target execution context and method identifier.
---@class _ScriptMethodReference : _JsonUserDataBase
---@field TargetContext ScriptExecutionContext The execution context category that determines where the referenced script method runs.
---@field ScriptFileAsset string The asset path or identifier of the script file that contains the referenced method.
---@field ScriptMethod string The name of the method within ScriptFileAsset to invoke.

---@alias ScriptMethodReference _ScriptMethodReference | { TargetContext: ScriptExecutionContext, ScriptFileAsset: string, ScriptMethod: string }

---Execution context category for a script.
---@alias ScriptExecutionContext
---| 0 # Invalid - An uninitialized or unrecognized execution context.
---| 1 # Main - The main game execution context.
---| 2 # Simulation - The physics simulation execution context.
---| 3 # Mission - The mission scripting execution context.
---| 4 # Mod - The mod scripting execution context.
---| 5 # Count - The total number of defined execution context values, used for array sizing and iteration bounds.

---Represents an in-game globally-unique identifier wrapping a Guid, with support for configurable generation modes including networked-synced multiplayer generation.
---@class _IGGuid : _JsonUserDataBase
---@field Guid string Gets the underlying Guid value.
---@field DebugName string? Gets the optional human-readable debug label attached to this identifier, or nil in non-debug builds.

---@alias IGGuid _IGGuid | { Guid: string, DebugName: string? }

---Represents a module data property that extends Property<T> with IModuleProperty support, including optional read-only access and a custom string representation delegate.
---@class _ModuleProperty<T> : _JsonUserDataBase
---@field ContextKey string Gets or sets the key that identifies the module data context for this property.
---@field storedValue T The backing field that stores the current value of this property.

---@alias ModuleProperty<T> _ModuleProperty<T> | { ContextKey: string, storedValue: T }

---Reference to an addressable asset.  This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime.
---To determine if the reference is set, use RuntimeKeyIsValid().
---@class _AssetReference : _JsonUserDataBase
---@field m_AssetGUID string The GUID of an asset
---@field m_SubObjectName string?
---@field m_SubObjectType string?

---@alias AssetReference _AssetReference | { m_AssetGUID: string, m_SubObjectName: string?, m_SubObjectType: string? }
