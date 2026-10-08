---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/JsonUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Utility/ExtensibleJsonUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/LuaPatching/Utility/IndexedListUserData.cs

---@class _JsonUserDataBase
---@field Count integer Gets the number of elements in the wrapped array, or 0 when the token is not an array (also logs a debug message).
local _JsonUserDataBase = {}

---Removes the array element at the given position (0-based from C#, 1-based from Lua via the LuaIndex converter).
---@param index integer The array position to remove.
---@error Thrown when the token is not `JTokenType.Array` or when the index is out of range.
function _JsonUserDataBase:RemoveAt(index) end

---Removes the object property with the given key.
---@param key string The property name to remove.
---@error Thrown when the token is not `JTokenType.Object`.
function _JsonUserDataBase:Remove(key) end

---Returns an iterator suitable for Lua's `__pairs` / `__ipairs` metamethods.
---@return fun(): any iterator The iterator callback.
function _JsonUserDataBase:Pairs() end

---Removes every element from the wrapped array.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Clear() end

---Inserts an element at the given position (0-based from C#, 1-based from Lua), shifting later elements right.
---@param index integer The position to insert at.
---@param value any The element to insert.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Insert(index, value) end

---Appends an element to the end of the wrapped array.
---@param value any The element to append.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Append(value) end

---Enumerates the property names of the wrapped object.
---@return fun(): string keys The property names of the wrapped object.
function _JsonUserDataBase:Keys() end

---Returns whether the wrapped object contains a property with the given key.
---@param key string The property name to test.
---@return boolean present True if the wrapped object contains `key`, false otherwise.
function _JsonUserDataBase:HasKey(key) end

---Invokes `callback` with the value at `key` when the key is present.
---Does nothing otherwise.
---@param key string The property name to patch.
---@param callback fun(value: any) The callback to invoke with the existing value.
function _JsonUserDataBase:Patch(key, callback) end

---Removes every array element that matches the given predicate.
---@param callback fun(value: any): boolean The predicate to test each element against.
function _JsonUserDataBase:RemoveWhere(callback) end

---Lua-facing wrapper exposing a Newtonsoft `JToken` as a table-like UserData.
---@class JsonUserData : _JsonUserDataBase
---@field [integer] any
---@field [string] any

---@class _JsonList<T> : JsonUserData
---@field [integer] T

---@alias JsonList<T> _JsonList<T> | T[]

---@class _JsonTable<T> : JsonUserData
---@field [string] T

---@alias JsonTable<T> _JsonTable<T> | { [string]: T }

---JSON-object UserData base class that lets subclasses override or add string-keyed properties.
---@class ExtensibleJsonUserData : JsonUserData

---JSON-array UserData base class that exposes the array as a name-indexed lookup table.
---@class IndexedListUserData<T> : JsonUserData
---@field [string] T
---@field [integer] JsonUserData
local IndexedListUserData = {}

---Rebuilds the name-index map without rebuilding the cached Lua conversions.
function IndexedListUserData:SoftRefresh() end

---Returns the lookup name for the given item.
---@param source T The item to extract the name from.
---@return string name The lookup name for the item.
function IndexedListUserData:Name(source) end

---Invokes `callback` with the value at `key` when the key is present.
---Does nothing otherwise.
---@param key string The property name to patch.
---@param callback fun(value: T) The callback to invoke with the existing value.
function IndexedListUserData:Patch(key, callback) end
