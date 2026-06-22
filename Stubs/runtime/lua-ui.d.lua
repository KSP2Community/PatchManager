---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/ScriptUI/LuaUIMgr.cs

---The UI global. A dot-style module table for building lightweight in-game
---windows, controls, and dialogs from a script. Window and control handles
---are integer ids returned by the create calls.
---@class LuaUIMgr
local LuaUIMgr = {}

---Creates a window and returns its id. The options table accepts the keys
---`title` (string), `x`/`y`/`width`/`height` (number), and `visible` (boolean).
---@param options any
---@return integer
function LuaUIMgr.Window(options) end

---Closes and destroys a window by id. Returns true if the window existed.
---@param windowId integer
---@return boolean
function LuaUIMgr.Close(windowId) end

---Shows a previously hidden window by id. Returns true if the window existed.
---@param windowId integer
---@return boolean
function LuaUIMgr.Show(windowId) end

---Hides a window by id. Returns true if the window existed.
---@param windowId integer
---@return boolean
function LuaUIMgr.Hide(windowId) end

---Sets a window's title bar text. Returns true if the window existed.
---@param windowId integer
---@param title string
---@return boolean
function LuaUIMgr.SetTitle(windowId, title) end

---Adds a text label to a window. Returns the new control id.
---@param windowId integer
---@param text string
---@return integer
function LuaUIMgr.Label(windowId, text) end

---Adds a button to a window. `onClick` is invoked with no arguments when
---clicked. Returns the new control id.
---@param windowId integer
---@param text string
---@param onClick fun()
---@return integer
function LuaUIMgr.Button(windowId, text, onClick) end

---Adds a checkbox to a window. `onChanged` is invoked with the new boolean
---value. Returns the new control id.
---@param windowId integer
---@param text string
---@param value boolean
---@param onChanged fun(value: boolean)
---@return integer
function LuaUIMgr.Checkbox(windowId, text, value, onChanged) end

---Adds a text field to a window. `onChanged` is invoked with the new string
---value. Returns the new control id.
---@param windowId integer
---@param label string
---@param value string
---@param onChanged fun(value: string)
---@return integer
function LuaUIMgr.TextField(windowId, label, value, onChanged) end

---Adds a numeric field to a window. `onChanged` is invoked with the new number
---value. Returns the new control id.
---@param windowId integer
---@param label string
---@param value number
---@param onChanged fun(value: number)
---@return integer
function LuaUIMgr.NumberField(windowId, label, value, onChanged) end

---Adds a dropdown to a window. `items` is a table of string choices and
---`selectedIndex` is the initial zero-based selection. `onChanged` is invoked
---with the selected string value and its index. Returns the new control id.
---@param windowId integer
---@param label string
---@param items any
---@param selectedIndex integer
---@param onChanged fun(value: string, index: integer)
---@return integer
function LuaUIMgr.Dropdown(windowId, label, items, selectedIndex, onChanged) end

---Adds a slider to a window bounded by `min` and `max`. `onChanged` is invoked
---with the new number value. Returns the new control id.
---@param windowId integer
---@param label string
---@param min number
---@param max number
---@param value number
---@param onChanged fun(value: number)
---@return integer
function LuaUIMgr.Slider(windowId, label, min, max, value, onChanged) end

---Adds a horizontal separator to a window. Returns the new control id.
---@param windowId integer
---@return integer
function LuaUIMgr.Separator(windowId) end

---Sets the text or label of a control. Returns true if the control existed and
---supported a text change.
---@param windowId integer
---@param controlId integer
---@param text string
---@return boolean
function LuaUIMgr.SetText(windowId, controlId, text) end

---Sets the value of a control. The accepted value type depends on the control
---kind (boolean, string, number, or dropdown selection). Returns true on success.
---@param windowId integer
---@param controlId integer
---@param value any
---@return boolean
function LuaUIMgr.SetValue(windowId, controlId, value) end

---Enables or disables a control. Returns true if the control existed.
---@param windowId integer
---@param controlId integer
---@param enabled boolean
---@return boolean
function LuaUIMgr.SetEnabled(windowId, controlId, enabled) end

---Removes a control from a window. Returns true if the control existed.
---@param windowId integer
---@param controlId integer
---@return boolean
function LuaUIMgr.Remove(windowId, controlId) end

---Shows an alert dialog with a single dismiss action. `onClose` is invoked when
---the dialog closes. Returns the new dialog id. Alias of Alert.
---@param title string
---@param message string
---@param onClose fun()
---@return integer
function LuaUIMgr.Dialog(title, message, onClose) end

---Shows an alert dialog with a single dismiss action. `onClose` is invoked when
---the dialog closes. Returns the new dialog id.
---@param title string
---@param message string
---@param onClose fun()
---@return integer
function LuaUIMgr.Alert(title, message, onClose) end

---Shows a confirm dialog. `onConfirm` runs when confirmed and `onCancel` runs
---when cancelled. Returns the new dialog id.
---@param title string
---@param message string
---@param onConfirm fun()
---@param onCancel fun()
---@return integer
function LuaUIMgr.Confirm(title, message, onConfirm, onCancel) end

---Closes an open dialog by id. Returns true if the dialog was open and got closed.
---@param dialogId integer
---@return boolean
function LuaUIMgr.CloseDialog(dialogId) end
