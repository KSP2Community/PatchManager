---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/LuaScriptUtilityMgr.cs

---@class Input
local Input = {}

---@param key string
---@return boolean
function Input.GetKey(key) end

---@param key string
---@return boolean
function Input.GetKeyDown(key) end

---@param key string
---@return boolean
function Input.GetKeyUp(key) end

---@class Script
---@field Log ScriptLog
---@field Wait ScriptWait
---@field Files ScriptFiles
local Script = {}

---@return string
function Script.Id() end

---@return string
function Script.Name() end

---@return string
function Script.Kind() end

---@return boolean
function Script.Stop() end

---@class ScriptLog
local ScriptLog = {}

---@param text string
function ScriptLog.Info(text) end

---@param text string
function ScriptLog.Warn(text) end

---@param text string
function ScriptLog.Error(text) end

---@param text string
function ScriptLog.Debug(text) end

---@class ScriptWait
local ScriptWait = {}

---@param seconds number
---@param callback fun()
---@return boolean
function ScriptWait.Seconds(seconds, callback) end

---@param predicate any
---@param timeoutSeconds number
---@param callback fun()
---@return boolean
function ScriptWait.Until(predicate, timeoutSeconds, callback) end

---@param eventName string
---@param timeoutSeconds number
---@param callback fun()
---@return boolean
function ScriptWait.Event(eventName, timeoutSeconds, callback) end

---@param key string
---@param callback fun()
---@return boolean
function ScriptWait.Input(key, callback) end

---@param key string
---@param callback fun()
---@return boolean
function ScriptWait.InputDown(key, callback) end

---@param key string
---@param callback fun()
---@return boolean
function ScriptWait.InputUp(key, callback) end

---@param key string
---@param holdSeconds number
---@param callback fun()
---@return boolean
function ScriptWait.InputHeld(key, holdSeconds, callback) end

---@class ScriptFiles
local ScriptFiles = {}

---@param relativePath string
---@return string[]
function ScriptFiles.List(relativePath) end

---@param relativePath string
---@return string
function ScriptFiles.Read(relativePath) end

---@param relativePath string
---@param text string
---@return boolean
function ScriptFiles.Write(relativePath, text) end
