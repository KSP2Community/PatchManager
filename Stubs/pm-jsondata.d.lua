---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/JsonUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Utility/ExtensibleJsonUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/LuaPatching/Utility/IndexedListUserData.cs

---@class _JsonUserDataBase
---@field Count integer Gets the number of elements in the wrapped array, or 0 when the token is not an array (also logs a debug message).
local _JsonUserDataBase = {}

---Removes the array element at the given 1-indexed position.
---@param index integer The 1-indexed array position to remove.
---@error Thrown when the token is not `JTokenType.Array` or when the index is out of range.
function _JsonUserDataBase:RemoveAt(index) end

---Remove all items that match a passed predicate.
---@param callback fun(value: any): boolean The predicate to check against.
function _JsonUserDataBase:RemoveWhere(callback) end

---Removes the object property with the given key.
---@param key string The property name to remove.
---@error Thrown when the token is not `JTokenType.Object`.
function _JsonUserDataBase:Remove(key) end

---Returns an iterator suitable for Lua's `__pairs` / `__ipairs` metamethods.
---Bound to both `__pairs` and `__ipairs`, so `pairs` and `ipairs` in Lua produce the same iterator.
---Arrays yield 1-indexed (index, value) tuples; objects yield (key, value) tuples; any other token type
---yields nothing and logs a debug message.
---@return any iterator The iterator callback.
function _JsonUserDataBase:Pairs() end

---Removes every element from the wrapped array.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Clear() end

---Inserts an element at the given 1-indexed position, shifting later elements right.
---@param index integer The 1-indexed position to insert at.
---@param value any     The element to insert.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Insert(index, value) end

---Appends an element to the end of the wrapped array.
---@param value any The element to append.
---@error Thrown when the token is not `JTokenType.Array`.
function _JsonUserDataBase:Append(value) end

---Enumerates the property names of the wrapped object.
---@return string[] keys The property names of the wrapped object.
function _JsonUserDataBase:Keys() end

---Returns whether the wrapped object contains a property with the given key.
---@param key string The property name to test.
---@return boolean present True if the wrapped object contains `key`, false otherwise.
function _JsonUserDataBase:HasKey(key) end

---Invokes `callback` with the value at `key` when the key is present;
---does nothing otherwise.
---@param key string             The property name to patch.
---@param callback fun(value: any) The callback to invoke with the existing value.
function _JsonUserDataBase:Patch(key, callback) end

---Lua-facing wrapper exposing a Newtonsoft JToken as a table-like UserData.
---@class JsonUserData : _JsonUserDataBase
---@field [integer] any
---@field [string] any

---A `JsonUserData` known to wrap a JSON array of `T`. Has all `JsonUserData` methods, but the array-shaped ones (`[integer]`, `Insert`, `Append`, `Clear`, `Count`, `RemoveAt`) are the appropriate surface. Element accesses through `[integer]`, `Insert`, and `Append` are typed as `T`.
---@class _JsonList<T> : JsonUserData
---@field [integer] T

---@alias JsonList<T> _JsonList<T> | T[]

---A `JsonUserData` known to wrap a JSON object of `T`-valued string keys. Has all `JsonUserData` methods, but the object-shaped ones (`[string]`, `Remove`, `Keys`, `HasKey`, `Patch`, `Pairs`) are the appropriate surface. Value accesses through `[string]` are typed as `T`.
---@class _JsonTable<T> : JsonUserData
---@field [string] T

---@alias JsonTable<T> _JsonTable<T> | { [string]: T }

---@class ExtensibleJsonUserData : JsonUserData

---@class IndexedListUserData<T> : JsonUserData
---@field [string] T
---@field [integer] T
local IndexedListUserData = {}

---Rebuilds the name-index map without rebuilding the cached Lua conversions.
function IndexedListUserData:SoftRefresh() end

---Returns the lookup name for the given item.
---Implementations typically read a known property off `source` (for example `"name"`
---or `"engineID"`). The returned string becomes the key Lua scripts use to address the item.
---@param source T The item to extract the name from.
---@return string name The lookup name for the item.
function IndexedListUserData:Name(source) end

---Invokes `callback` with the typed item at `key` when the key is present; refreshes the
---name-index map afterwards so a callback that mutates the item's name re-keys the list.
---@param key string The lookup name of the item to patch.
---@param callback fun(item: T) The callback to invoke with the existing item.
function IndexedListUserData:Patch(key, callback) end
