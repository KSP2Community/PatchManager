---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/ModConfig.cs
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/ConfigBuilder.cs
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/ConfigHandle.cs
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/ConfigView.cs
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/ConfigTypes.cs
-- Source: ksp2redux/Packages/SpaceWarp/Runtime/Core/API/Config/LuaColor.cs

---The Lua `Config` global - a mod's own config namespace. Declare entries through Define,
---fetch existing ones through Get, and reach another mod's config through Mod.
---@class ModConfig

---Begins defining a config entry in the given section and name. Chain the builder methods and finish with
---`:Bind()` to get a live handle.
---@param section string The config section.
---@param name string The entry name within the section.
---@return ConfigBuilder builder A fluent builder for the entry.
function ModConfig:Define(section, name) end

---Returns a live handle over an existing entry in this mod's own config, or `nil` if it does not
---exist. Use Define to create entries.
---@param section string The config section.
---@param name string The entry name within the section.
---@return ConfigHandle? handle A handle over the entry, or nil.
function ModConfig:Get(section, name) end

---Returns a read/write view over another mod's config, reached by its mod id.
---@param modId string The other mod's id.
---@return ConfigView view A view over that mod's config.
---@error If no mod with that id has a config file.
function ModConfig:Mod(modId) end

---Fluent builder for a config entry. Accumulates the entry's shape and binds it at Bind,
---returning a live ConfigHandle.
---@class ConfigBuilder

---Sets the entry's config type (built-in such as `Double`, or a custom type).
---@param type any
---@return ConfigBuilder
function ConfigBuilder:Type(type) end

---Sets the default value, in the type's rich domain. Applied only on the first launch.
---@param value any
---@return ConfigBuilder
function ConfigBuilder:Default(value) end

---Constrains an ordered value to an inclusive range. `steps` sets the slider granularity
---and `format` the slider's number format, which defaults to a type-appropriate format
---when omitted.
---@param min number
---@param max number
---@param steps? integer
---@param format? string
---@return ConfigBuilder
function ConfigBuilder:Range(min, max, steps, format) end

---Constrains the value to an enumerated set, given as a Lua array of rich values.
---@param list any
---@return ConfigBuilder
function ConfigBuilder:Values(list) end

---Sets the entry's description text.
---@param text string
---@return ConfigBuilder
function ConfigBuilder:Desc(text) end

---Sets the localization key for the entry's display name in the settings menu.
---@param key string
---@return ConfigBuilder
function ConfigBuilder:NameLoc(key) end

---Sets the localization key for the entry's description in the settings menu.
---@param key string
---@return ConfigBuilder
function ConfigBuilder:DescLoc(key) end

---Attaches a metadata tag to the entry. Multiple allowed.
---@param name string
---@return ConfigBuilder
function ConfigBuilder:Tag(name) end

---Binds the entry and returns a live handle over it.
---@return ConfigHandle
function ConfigBuilder:Bind() end

---A live view over a bound config entry. Reads and writes go through the entry's config type, so scripts see
---rich values while storage holds the C# value the settings menu renders.
---@class ConfigHandle
---@field value any The current value in the config type's rich Lua domain. Reading deserializes the stored value. Writing serializes, stores (which saves the file), and fires change callbacks.

---Registers a callback fired whenever the value changes, receiving the old and new values in the rich
---Lua domain.
---@param fn fun(old: any, new: any) The Lua callback, called as `fn(old, new)`.
function ConfigHandle:OnChange(fn) end

---Determines whether the entry carries the given metadata tag.
---@param tag string The tag to check for.
---@return boolean True if the entry has the tag, false otherwise.
function ConfigHandle:HasTag(tag) end

---A read/write view over a config file - a mod's own, or another mod's via `Config:Mod(id)`. Reaches
---existing entries with Get; it cannot define new ones (defining is an own-config operation).
---@class ConfigView

---Returns a live handle over an existing entry, or `nil` if the section or entry does not exist (the
---owning mod may not have bound it yet).
---@param section string The config section.
---@param name string The entry name within the section.
---@return ConfigHandle? handle A handle over the entry, or nil.
function ConfigView:Get(section, name) end

---A string config value, stored as a string.
---The global name is `String`.
---@class StringType

---An integer config value, stored as an int.
---The global name is `Integer`.
---@class IntegerType

---A floating-point config value, stored as a double.
---The global name is `Double`.
---@class DoubleType

---A boolean config value, stored as a bool.
---The global name is `Boolean`.
---@class BooleanType

---A color config value: a LuaColor on the script side, a `UnityEngine.Color` in storage
---so the settings menu renders it as a color picker. Accepts a LuaColor or a table (keyed
---`{r=, g=, b=, a=}` or indexed `{1, 0.5, 0, 1}`) when serializing.
---The global name is `Color`.
---@class ColorType

---The Lua-side representation of a color config value.
---@class LuaColor
---@field r number The red channel.
---@field g number The green channel.
---@field b number The blue channel.
---@field a number The alpha channel.
---@field [integer] number 1-based channel access matching Lua convention: [1] is r, [2] is g, [3] is b, [4] is a.
