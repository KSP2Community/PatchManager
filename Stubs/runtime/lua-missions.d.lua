---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/KSP2MissionLuaInterface.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/State/MissionState.cs

---Represents the Lua scripting interface for KSP2 missions, exposing mission
---activation and state-management callbacks to the game script environment.
---A dot-style module table.
---@class KSP2MissionLuaInterface
local KSP2MissionLuaInterface = {}

---Activates a mission by id, adding it to the set of active missions.
---@param missionID string
function KSP2MissionLuaInterface.ActivateMission(missionID) end

---Sets the execution state of a mission by id. `state` is the integer index of
---the MissionState enum (0 Inactive, 1 Active, 2 Complete, 3 Failed, 4 Invalid).
---@param missionID string
---@param state integer
function KSP2MissionLuaInterface.SetMissionState(missionID, state) end
