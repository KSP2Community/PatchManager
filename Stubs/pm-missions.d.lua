---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/MissionsLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/MissionsTypes.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/Converters/MissionConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/UserData/MissionUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/UserData/StageUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/UserData/StagesUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/UserData/ContentBranchesUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Missions/UserData/MissionRewardUserData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionData.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionStage.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionReward.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionRewardDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionRewardType.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionType.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/MissionOwner.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Definitions/UIDisplayType.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/State/MissionState.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/MissionBranch.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/MissionContentBranch.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/Condition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/ConditionSet.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/ConditionTypes.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/PropertyCondition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/EventCondition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/ScriptCondition.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/LogicalOperator.cs
-- Source: ksp2redux/Assets/Code/KSP/game/Missions/PropertyOperator.cs
-- Source: ksp2redux/Assets/Code/Root/IMissionAction.cs

--#region UserData wrappers

---Mission definition wrapper exposing the `missionStages` array as a typed StagesUserData and
---the optional `ContentBranches` array as a typed ContentBranchesUserData.
---@class MissionUserData : _MissionData, ExtensibleJsonUserData
---@field missionStages StagesUserData
---@field ContentBranches ContentBranchesUserData?

---Mission stage wrapper exposing the optional `MissionReward` object as a typed MissionRewardUserData.
---@class StageUserData : _MissionStage, ExtensibleJsonUserData
---@field MissionReward MissionRewardUserData?

---Indexed-list wrapper for a mission's `missionStages` array, keyed by each stage's `name`, wrapping
---each entry in a typed StageUserData.
---Iterable with `pairs()`.
---@class StagesUserData : IndexedListUserData<StageUserData>

---Synthetic per-element wrapper for entries of a `ContentBranchesUserData`.
---@class ContentBranchUserData : _MissionContentBranch, JsonUserData

---Indexed-list wrapper for a mission's `ContentBranches` array, keyed by each branch's `ID`.
---Iterable with `pairs()`.
---@class ContentBranchesUserData : IndexedListUserData<ContentBranchUserData>

---Synthetic per-element wrapper for entries of a `MissionRewardUserData`.
---@class MissionRewardDefinitionUserData : _MissionRewardDefinition, JsonUserData

---Indexed-list wrapper for a stage's `MissionRewardDefinitions` array, keyed by each reward's `MissionRewardType`.
---Iterable with `pairs()`.
---@class MissionRewardUserData : IndexedListUserData<MissionRewardDefinitionUserData>

--#endregion

--#region MissionsLuaModule

---Lua submodule exposed as `PM.Missions`, providing patches and creation helpers for missions, stages,
---conditions, and actions.
---@class MissionsLuaModule
local MissionsLuaModule = {}

---Registers a mission patch with the given namespaced patch name.
---@param name string  The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<MissionUserData, any> patch  The registered patch.
function MissionsLuaModule:Patch(name) end

---Returns the assembly-qualified type name of the property watcher registered under name.
---@param name string  The watcher's short name as registered in `MissionsTypes.PropertyWatchers`.
---@return string  The assembly-qualified type name of the watcher.
function MissionsLuaModule:GetPropertyWatcher(name) end

---Returns the assembly-qualified type name of the message registered under name.
---@param name string  The message's short name as registered in `MissionsTypes.Messages`.
---@return string  The assembly-qualified type name of the message.
function MissionsLuaModule:GetMessage(name) end

---Creates a new mission stage with the given name and runs callback against it for
---further configuration.
---@param name string  The stage name.
---@param callback fun(stage: StageUserData)  Callback that receives the new stage for further configuration.
---@return StageUserData stage  The created stage.
function MissionsLuaModule:CreateStage(name, callback) end

---Synthetic wrapper around a `ConditionSet` JSON returned by `:And`, `:Or`, and `:Not`.
---@class ConditionSetUserData : _ConditionSet, JsonUserData

---Returns a condition set that requires every supplied condition to be true.
---@param ... Condition  The conditions to combine.
---@return ConditionSetUserData  A condition-set wrapper representing the conjunction.
function MissionsLuaModule:And(...) end

---Returns a condition set that requires at least one supplied condition to be true.
---@param ... Condition  The conditions to combine.
---@return ConditionSetUserData  A condition-set wrapper representing the disjunction.
function MissionsLuaModule:Or(...) end

---Returns a condition set that inverts the supplied condition.
---@param condition Condition  The condition to negate.
---@return ConditionSetUserData  A condition-set wrapper representing the negation.
function MissionsLuaModule:Not(condition) end

---Creates a new mission action of the given short type name and runs callback against
---the underlying JSON for further configuration.
---@param type string  The action's short name as registered in `MissionsTypes.Actions`.
---@param callback fun(action: JsonUserData)  Callback that receives the action's JSON for further configuration.
---@return JsonUserData  A wrapper around the configured action JSON.
---@error Thrown when type resolves to a class without a parameterless constructor.
function MissionsLuaModule:Action(type, callback) end

--#endregion

--#region Mission JSON schemas (root MissionData and nested types)

---Represents a mission definition, including its identity, type, state, stages, and branching structure.
---@class _MissionData : _JsonUserDataBase
---@field ID string
---@field MissionGroup string
---@field name string
---@field description string
---@field GameModeFeatureId string
---@field type MissionType
---@field Owner MissionOwner
---@field state MissionState
---@field missionScript string
---@field missionStages JsonList<MissionStage>
---@field currentStageIndex integer
---@field Hidden boolean
---@field MissionGranterKey string
---@field TriumphLoopVideoKey string
---@field VisibleRewards boolean
---@field pendingCompletionTest boolean
---@field maxStageID integer
---@field uiDisplayType UIDisplayType
---@field ExceptionBranches JsonList<MissionBranch>
---@field PreRequisiteBranches JsonList<MissionBranch>
---@field ContentBranches JsonList<MissionContentBranch>
---@field MissionSaveAssetKey string

---@alias MissionData _MissionData | { ID: string, MissionGroup: string, name: string, description: string, GameModeFeatureId: string, type: MissionType, Owner: MissionOwner, state: MissionState, missionScript: string, missionStages: JsonList<MissionStage>, currentStageIndex: integer, Hidden: boolean, MissionGranterKey: string, TriumphLoopVideoKey: string, VisibleRewards: boolean, pendingCompletionTest: boolean, maxStageID: integer, uiDisplayType: UIDisplayType, ExceptionBranches: JsonList<MissionBranch>, PreRequisiteBranches: JsonList<MissionBranch>, ContentBranches: JsonList<MissionContentBranch>, MissionSaveAssetKey: string }

---Represents a single stage within a mission definition, including its actions, branches, conditions, and reward configuration.
---@class _MissionStage : _JsonUserDataBase
---@field StageID integer
---@field name string
---@field description string
---@field Objective string
---@field DisplayObjective boolean
---@field RevealObjectiveOnActivate boolean
---@field MissionRewardType MissionRewardType
---@field RewardAmount string
---@field MissionReward MissionReward
---@field IgnoreExceptionBranches boolean
---@field actions JsonList<MissionAction>
---@field branches JsonList<MissionBranch>
---@field parentMissionID string
---@field scriptableCondition Condition
---@field completed boolean
---@field active boolean

---@alias MissionStage _MissionStage | { StageID: integer, name: string, description: string, Objective: string, DisplayObjective: boolean, RevealObjectiveOnActivate: boolean, MissionRewardType: MissionRewardType, RewardAmount: string, MissionReward: MissionReward, IgnoreExceptionBranches: boolean, actions: JsonList<MissionAction>, branches: JsonList<MissionBranch>, parentMissionID: string, scriptableCondition: Condition, completed: boolean, active: boolean }

---Represents a conditional branch in a mission that evaluates a Condition and transitions to a target stage when the condition is met.
---@class _MissionBranch : _JsonUserDataBase
---@field condition Condition
---@field TargetStage integer
---@field ExceptionBranch boolean
---@field IsPreRequisiteBranch boolean
---@field IsExceptionBranch boolean

---@alias MissionBranch _MissionBranch | { condition: Condition, TargetStage: integer, ExceptionBranch: boolean, IsPreRequisiteBranch: boolean, IsExceptionBranch: boolean }

---Represents a named branch of mission actions that can be activated or deactivated on demand by branch ID.
---@class _MissionContentBranch : _JsonUserDataBase
---@field ID string
---@field actions JsonList<MissionAction>

---@alias MissionContentBranch _MissionContentBranch | { ID: string, actions: JsonList<MissionAction> }

---Represents the reward associated with a mission, containing a collection of MissionRewardDefinition entries.
---@class _MissionReward : _JsonUserDataBase
---@field MissionRewardDefinitions JsonList<MissionRewardDefinition>

---@alias MissionReward _MissionReward | { MissionRewardDefinitions: JsonList<MissionRewardDefinition> }

---Represents a single reward associated with a mission.
---@class _MissionRewardDefinition : _JsonUserDataBase
---@field MissionRewardType MissionRewardType
---@field RewardAmount number
---@field RewardKey string

---@alias MissionRewardDefinition _MissionRewardDefinition | { MissionRewardType: MissionRewardType, RewardAmount: number, RewardKey: string }

--#endregion

--#region Mission condition polymorphism

---A base class for mission conditions, providing virtual evaluation and lifecycle methods used by concrete condition implementations.
---@class _ConditionBase : _JsonUserDataBase
---@field ConditionType ConditionTypeName

---@alias ConditionBase _ConditionBase | { ConditionType: ConditionTypeName }

---A composite Condition that evaluates a collection of child conditions combined by a LogicalOperator.
---@class _ConditionSet : _ConditionBase
---@field ConditionType "ConditionSet"
---@field Children JsonList<Condition>
---@field ConditionMode LogicalOperator

---@alias ConditionSet _ConditionSet | { ConditionType: "ConditionSet", Children: JsonList<Condition>, ConditionMode: LogicalOperator }

---Condition that tests a PropertyWatcher value against a threshold using a PropertyOperator comparison.
---@class _PropertyCondition : _ConditionBase
---@field ConditionType "PropertyCondition"
---@field RequireCurrentValue boolean
---@field PropertyTypeAQN string
---@field TestWatchedValue number
---@field TestWatchedstring string
---@field TestWatchedInt integer
---@field TestWatchedBool boolean
---@field propOperator PropertyOperator
---@field isInput boolean
---@field Inputstring string

---@alias PropertyCondition _PropertyCondition | { ConditionType: "PropertyCondition", RequireCurrentValue: boolean, PropertyTypeAQN: string, TestWatchedValue: number, TestWatchedstring: string, TestWatchedInt: integer, TestWatchedBool: boolean, propOperator: PropertyOperator, isInput: boolean, Inputstring: string }

---Condition that evaluates to true when a specified game message event type has been observed.
---@class _EventCondition : _ConditionBase
---@field ConditionType "EventCondition"
---@field EventTypeAQN string

---@alias EventCondition _EventCondition | { ConditionType: "EventCondition", EventTypeAQN: string }

---Condition that evaluates success by subscribing to a configurable message event type and delegating evaluation to a ScriptMethodReference script.
---@class _ScriptCondition : _ConditionBase
---@field ConditionType "ScriptCondition"
---@field triggerEventTypeAQN string
---@field EvaluationScript ScriptMethodReference

---@alias ScriptCondition _ScriptCondition | { ConditionType: "ScriptCondition", triggerEventTypeAQN: string, EvaluationScript: ScriptMethodReference }

---Union of every concrete mission condition shape; falls back to `table` for runtime-discovered or unregistered subtypes.
---@alias Condition table | ConditionSet | PropertyCondition | EventCondition | ScriptCondition

--#endregion

--#region Mission action polymorphism

---Mission action JSON. Concrete subclasses are discovered at runtime from `MissionsTypes.Actions`; the JSON
---shape varies per action type and carries a Newtonsoft `$type` discriminator.
---@alias MissionAction table

--#endregion

--#region Enums (string-serialized in mission JSON)

---The owner category of a mission.
---@alias MissionOwner
---| "None"
---| "Global"
---| "Agency"
---| "Player"

---Classification of a mission as primary, secondary, tutorial, or first-time user experience.
---@alias MissionType
---| "Primary"
---| "Secondary"
---| "Tutorial"
---| "FTUE"

---The execution state of a mission.
---@alias MissionState
---| "Inactive"
---| "Active"
---| "Complete"
---| "Failed"
---| "Invalid"

---Represents the UI display context in which a mission is presented.
---@alias UIDisplayType
---| "Default"
---| "Video"
---| "Flight"
---| "VAB"
---| "VAB_Flight"

---The type of reward granted upon completing a mission.
---@alias MissionRewardType
---| "None"
---| "SciencePoints"

---Logical operator used to combine mission conditions.
---@alias LogicalOperator
---| "Invalid"
---| "OR"
---| "AND"
---| "XOR"
---| "NOT"
---| "Count"

---Comparison operator for evaluating a mission property value against a target condition.
---@alias PropertyOperator
---| "Invalid"
---| "LESSER"
---| "EQUAL"
---| "GREATER"
---| "Count"

---The kind of condition used in a mission, serialized as the `ConditionType` discriminator.
---@alias ConditionTypeName
---| "Invalid"
---| "EventCondition"
---| "ConditionSet"
---| "PropertyCondition"
---| "ScriptCondition"
---| "Count"

--#endregion
