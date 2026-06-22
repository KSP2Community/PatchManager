---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/Redux/Lua/GameLifecycleLibrary.cs
-- Source: ksp2redux/Assets/Code/Redux/Lua/LuaCoroutineHandle.cs
-- Source: ksp2redux/Assets/Code/KSP/game/GameState.cs

---The Lua `Game` global through which mod scripts register live-game lifecycle closures.
---@class GameLifecycleLibrary
---@field State string The name of the current game state (for example "FlightView").
---@field Time number The current universe time in seconds.
---@field IsPaused boolean Whether the game is paused.
local GameLifecycleLibrary = {}

---Registers a closure to fire every frame for the calling mod.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:Update(fn) end

---Registers a closure to fire every physics step for the calling mod.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:FixedUpdate(fn) end

---Registers a closure to fire whenever any game state is entered, receiving the state as an argument.
---@param fn fun(state: integer) The closure to register.
function GameLifecycleLibrary:SceneEnter(fn) end

---Registers a closure to fire whenever any game state is left, receiving the state as an argument.
---@param fn fun(state: integer) The closure to register.
function GameLifecycleLibrary:SceneExit(fn) end

---Registers a closure to fire when the active vessel changes, receiving the new vessel as an argument.
---@param fn fun(vessel: any) The closure to register.
function GameLifecycleLibrary:OnActiveVesselChanged(fn) end

---Registers a closure to fire only when the flight view is entered.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:OnFlight(fn) end

---Registers a closure to fire only when the Kerbal Space Center is entered.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:OnKSC(fn) end

---Registers a closure to fire only when the vehicle assembly editor is entered.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:OnEditor(fn) end

---Registers a closure to fire only when the main menu is entered.
---@param fn fun() The closure to register.
function GameLifecycleLibrary:OnMainMenu(fn) end

---Starts a coroutine over the given function, resumed every frame until it finishes or is stopped.
---@param fn fun() The function to run as a coroutine.
---@return LuaCoroutineHandle handle A handle for stopping or polling the coroutine.
---@error Game.StartCoroutine expects a function.
function GameLifecycleLibrary:StartCoroutine(fn) end

---Sets the paused state of the game.
---@param paused boolean True to pause, false to resume.
---@return boolean ok True if the game's paused state matches the requested value afterwards.
function GameLifecycleLibrary:SetPaused(paused) end

---Warps to the given time-warp rate, snapping a rate factor to the nearest available rate.
---@param rateOrIndex number A warp-rate index, or a rate factor matched to the nearest available rate.
---@return boolean ok True if the warp rate was set, false if time warp is unavailable.
function GameLifecycleLibrary:WarpTo(rateOrIndex) end

---Warps forward to the given universal time.
---@param universalTime number The universe time to warp to.
---@return boolean ok True if the warp was started, false if time warp is unavailable.
function GameLifecycleLibrary:WarpToTime(universalTime) end

---A running Lua coroutine, returned to the script that started it.
---@class LuaCoroutineHandle
---@field IsDone boolean Whether the coroutine has finished or been stopped.
---@field IsRunning boolean Whether the coroutine is still running.
local LuaCoroutineHandle = {}

---Stops the coroutine so the runner drops it on the next frame.
function LuaCoroutineHandle:Stop() end

---Represents the active high-level game state, identifying which scene or view is currently running.
---The `GameState` global is a table mapping each member name to its numeric value, for comparison
---against a scene callback's argument (for example `GameState.FlightView`).
---@class GameState
---@field Invalid integer 0
---@field WarmUpLoading integer 1
---@field MainMenu integer 2
---@field KerbalSpaceCenter integer 3
---@field VehicleAssemblyBuilder integer 10
---@field BaseAssemblyEditor integer 11
---@field FlightView integer 20
---@field ColonyView integer 21
---@field Map3DView integer 22
---@field PhotoMode integer 30
---@field MetricsMode integer 31
---@field PlanetViewer integer 32
---@field Loading integer 33
---@field TrainingCenter integer 40
---@field MissionControl integer 41
---@field TrackingStation integer 42
---@field ResearchAndDevelopment integer 43
---@field Launchpad integer 44
---@field Runway integer 45
---@field Flag integer 46
