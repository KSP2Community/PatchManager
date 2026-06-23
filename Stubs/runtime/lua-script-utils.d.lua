---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/Redux/Lua/InputLibrary.cs
-- Source: ksp2redux/Assets/Code/Redux/Lua/FilesLibrary.cs
-- Source: ksp2redux/Assets/Code/Redux/Lua/WaitLibrary.cs
-- Source: ksp2redux/Assets/Code/Redux/Lua/Scripting/ScriptContextLibrary.cs

---@class Input
Input = {}

---@param key string
---@return boolean
function Input.GetKey(key) end

---@param key string
---@return boolean
function Input.GetKeyDown(key) end

---@param key string
---@return boolean
function Input.GetKeyUp(key) end

-- The Wait surface. Each call has two forms:
--   blocking - omit the callback and the call yields the running coroutine until satisfied (only valid
--     inside a coroutine context: a console body or a Game.StartCoroutine body).
--   callback - pass a callback and the wait runs on a coroutine, invoking the callback when satisfied. This
--     form works from a synchronous body too and returns the coroutine handle. Callbacks are deprecated in
--     favour of the blocking form.
---@class Wait
Wait = {}

---@param seconds number
---@param callback? fun()
---@return LuaCoroutineHandle? handle The coroutine handle when a callback is given.
function Wait.Seconds(seconds, callback) end

---@param predicate fun(): boolean
---@param timeoutSeconds number
---@param callback? fun(satisfied: boolean)
---@return boolean|LuaCoroutineHandle result True/false in the blocking form, the coroutine handle with a callback.
function Wait.Until(predicate, timeoutSeconds, callback) end

---@param eventName string
---@param timeoutSeconds number
---@param callback? fun()
---@return boolean|LuaCoroutineHandle result Not implemented - warns and falls back to a timed wait.
function Wait.Event(eventName, timeoutSeconds, callback) end

---@param key string
---@param callback? fun()
---@return LuaCoroutineHandle? handle
function Wait.Input(key, callback) end

---@param key string
---@param callback? fun()
---@return LuaCoroutineHandle? handle
function Wait.InputDown(key, callback) end

---@param key string
---@param callback? fun()
---@return LuaCoroutineHandle? handle
function Wait.InputUp(key, callback) end

---@param key string
---@param holdSeconds number
---@param callback? fun()
---@return LuaCoroutineHandle? handle
function Wait.InputHeld(key, holdSeconds, callback) end

-- Files are sandboxed to the calling environment's own folder (a mod's folder, or the Lua folder for the
-- console). Paths are relative to that folder and may not escape it.
---@class Files
Files = {}

---@param relativePath string
---@return string[]
function Files.List(relativePath) end

---@param relativePath string
---@return string
function Files.Read(relativePath) end

---@param relativePath string
---@param text string
---@return boolean
function Files.Write(relativePath, text) end

-- The running console/CLI script's own identity and stop state. Present only in the console/script
-- environment, not in mod environments (a mod identifies itself through ModId/Location).
---@class Script
Script = {}

---@return string
function Script.Id() end

---@return string
function Script.Name() end

---@return string
function Script.Kind() end

---@return boolean
function Script.Stop() end
