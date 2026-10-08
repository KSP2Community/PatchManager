---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/Missions/UserData/MissionUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/UserData/StageUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/UserData/StagesUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/UserData/ContentBranchesUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/UserData/MissionRewardUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/Converters/MissionConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/MissionsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Missions/MissionsTypes.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionData.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionStage.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionReward.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionRewardDefinition.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionProgressScope.cs
-- Source: Assets/Code/KSP/game/Missions/MissionBranch.cs
-- Source: Assets/Code/KSP/game/Missions/MissionContentBranch.cs
-- Source: Assets/Code/KSP/game/Missions/Condition.cs
-- Source: Assets/Code/KSP/game/Missions/ConditionSet.cs
-- Source: Assets/Code/KSP/game/Missions/PropertyCondition.cs
-- Source: Assets/Code/KSP/game/Missions/EventCondition.cs
-- Source: Assets/Code/KSP/game/Missions/ScriptCondition.cs
-- Source: Assets/Code/KSP/game/Missions/ConditionTypes.cs
-- Source: Assets/Code/Root/IMissionAction.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionOwner.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionType.cs
-- Source: Assets/Code/KSP/game/Missions/State/MissionState.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/UIDisplayType.cs
-- Source: Assets/Code/KSP/game/Missions/Definitions/MissionRewardType.cs
-- Source: Assets/Code/KSP/game/Missions/LogicalOperator.cs
-- Source: Assets/Code/KSP/game/Missions/PropertyOperator.cs
-- Source: Assets/Code/KSP/IO/IOProvider.cs

---Mission definition wrapper exposing the `missionStages` array as a typed StagesUserData and
---the optional `ContentBranches` array as a typed ContentBranchesUserData.
---@class MissionUserData : _MissionData, ExtensibleJsonUserData
---@field missionStages StagesUserData The ordered list of stages that make up this mission.
---@field ContentBranches ContentBranchesUserData? The list of optional content branches available within this mission.

---Mission stage wrapper exposing the optional `MissionReward` object as a typed MissionRewardUserData.
---@class StageUserData : _MissionStage, ExtensibleJsonUserData
---@field MissionReward MissionRewardUserData? The reward definition applied to the player upon completion of this stage.

---Indexed-list wrapper for a mission's `missionStages` array, keyed by each stage's `name`, wrapping
---each entry in a typed StageUserData.
---@class StagesUserData : IndexedListUserData<StageUserData>

---@class ContentBranchUserData : _MissionContentBranch, JsonUserData

---Indexed-list wrapper for a mission's `ContentBranches` array, keyed by each branch's `ID`.
---@class ContentBranchesUserData : IndexedListUserData<ContentBranchUserData>

---@class MissionRewardDefinitionUserData : _MissionRewardDefinition, JsonUserData

---Indexed-list wrapper for a stage's `MissionRewardDefinitions` array, keyed by each reward's `MissionRewardType`.
---@class MissionRewardUserData : IndexedListUserData<MissionRewardDefinitionUserData>

---@class ConditionSetUserData : _ConditionSet, JsonUserData

---Lua submodule exposed as `PM.Missions`, providing patches and creation helpers for missions, stages,
---conditions, and actions.
---@class MissionsLuaModule
local MissionsLuaModule = {}

---Registers a mission patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<MissionUserData, any> patch The registered patch.
function MissionsLuaModule:Patch(name) end

---Returns the assembly-qualified type name of the property watcher registered under name.
---@param name string The watcher's short name as registered in `MissionsTypes.PropertyWatchers`.
---@return string typeName The assembly-qualified type name of the watcher.
function MissionsLuaModule:GetPropertyWatcher(name) end

---Returns the assembly-qualified type name of the message registered under name.
---@param name string The message's short name as registered in `MissionsTypes.Messages`.
---@return string typeName The assembly-qualified type name of the message.
function MissionsLuaModule:GetMessage(name) end

---Creates a new mission stage with the given name and runs callback against it for
---further configuration.
---@param name string The stage name.
---@param callback fun(stage: StageUserData) Callback that receives the new stage for further configuration.
---@return StageUserData stage The created stage.
function MissionsLuaModule:CreateStage(name, callback) end

---Returns a condition set that requires every supplied condition to be true.
---@param ... Condition The conditions to combine.
---@return ConditionSetUserData conditionSet A condition-set wrapper representing the conjunction.
function MissionsLuaModule:And(...) end

---Returns a condition set that requires at least one supplied condition to be true.
---@param ... Condition The conditions to combine.
---@return ConditionSetUserData conditionSet A condition-set wrapper representing the disjunction.
function MissionsLuaModule:Or(...) end

---Returns a condition set that inverts the supplied condition.
---@param condition Condition The condition to negate.
---@return ConditionSetUserData conditionSet A condition-set wrapper representing the negation.
function MissionsLuaModule:Not(condition) end

---Creates a new mission action of the given short type name and runs callback against
---the underlying JSON for further configuration.
---@param type string The action's short name as registered in `MissionsTypes.Actions`.
---@param callback fun(action: JsonUserData) Callback that receives the action's JSON for further configuration.
---@return JsonUserData action A wrapper around the configured action JSON.
---@error Thrown when type resolves to a class without a parameterless constructor.
function MissionsLuaModule:Action(type, callback) end

---Represents a mission definition, including its identity, type, state, stages, and branching structure.
---@class _MissionData : _JsonUserDataBase
---@field ID string The unique identifier for this mission.
---@field MissionGroup string The group identifier this mission belongs to.
---@field name string The display name of the mission.
---@field description string The description text of the mission.
---@field GameModeFeatureId string The game mode feature identifier that gates availability of this mission.
---@field Layer string The mission layer this mission belongs to, which a campaign pack selects missions by.
---@field type MissionType The classification type of this mission.
---@field Owner MissionOwner The owner category assigned to this mission.
---@field state MissionState The current lifecycle state of this mission.
---@field missionScript string The asset key or path of the script associated with this mission.
---@field missionStages JsonList<MissionStage> The ordered list of stages that make up this mission.
---@field currentStageIndex integer The zero-based index into missionStages identifying the currently active stage.
---@field Hidden boolean A value indicating whether this mission is hidden from the player-facing mission list.
---@field MissionGranterKey string The localization key used to identify the entity or system that grants this mission.
---@field TriumphLoopVideoKey string The asset key for the looping triumph video played on mission completion.
---@field VisibleRewards boolean A value indicating whether the rewards for this mission are displayed to the player before completion.
---@field RequiresNewVessel boolean A value indicating whether only craft built after this mission was picked up may work on it.
---@field pendingCompletionTest boolean A value indicating whether a stage completion evaluation is pending for the current frame.
---@field maxStageID integer The highest stage ID that has been issued so far, used as the basis for generating the next unique stage ID.
---@field uiDisplayType UIDisplayType The UI display mode used when presenting this mission in the interface.
---@field ExceptionBranches JsonList<MissionBranch> The list of exception branches that can redirect mission flow when their conditions are met.
---@field PreRequisiteBranches JsonList<MissionBranch> The list of pre-requisite branches evaluated during the first stage to determine alternate entry points.
---@field ContentBranches JsonList<MissionContentBranch> The list of optional content branches available within this mission.
---@field MissionSaveAssetKey string The asset key for the save file loaded when starting a tutorial mission that requires a pre-built save state.
---@field MissionVariables JsonTable<string> Mission-scoped string values captured while the mission is running.

---@alias MissionData _MissionData | { ID: string, MissionGroup: string, name: string, description: string, GameModeFeatureId: string, Layer: string, type: MissionType, Owner: MissionOwner, state: MissionState, missionScript: string, missionStages: JsonList<MissionStage>, currentStageIndex: integer, Hidden: boolean, MissionGranterKey: string, TriumphLoopVideoKey: string, VisibleRewards: boolean, RequiresNewVessel: boolean, pendingCompletionTest: boolean, maxStageID: integer, uiDisplayType: UIDisplayType, ExceptionBranches: JsonList<MissionBranch>, PreRequisiteBranches: JsonList<MissionBranch>, ContentBranches: JsonList<MissionContentBranch>, MissionSaveAssetKey: string, MissionVariables: JsonTable<string> }

---Represents a single stage within a mission definition, including its actions, branches, conditions, and reward configuration.
---@class _MissionStage : _JsonUserDataBase
---@field StageID integer The numeric identifier that uniquely identifies this stage within its parent mission.
---@field name string The display name of this stage.
---@field description string The human-readable description of this stage.
---@field Objective string The objective text shown to the player for this stage.
---@field DisplayObjective boolean Whether the objective text for this stage is visible to the player.
---@field RevealObjectiveOnActivate boolean Whether the objective text is hidden until this stage becomes active.
---@field MissionRewardType MissionRewardType The category of reward granted when this stage completes.
---@field RewardAmount string The serialized string representation of the reward amount for this stage.
---@field MissionReward MissionReward The reward definition applied to the player upon completion of this stage.
---@field IgnoreExceptionBranches boolean Whether exception branches are skipped during branch evaluation for this stage.
---@field ProgressScope MissionProgressScope Whether this stage is progressed by the campaign or by an individual vessel.
---@field actions JsonList<MissionAction> The ordered list of actions executed when this stage is activated.
---@field branches JsonList<MissionBranch> The list of conditional branches that can redirect mission flow to another stage.
---@field parentMissionID string The identifier of the mission that owns this stage.
---@field scriptableCondition Condition The serialized JSON representation of the condition tree, used for persistence and deserialization.
---@field completed boolean Whether this stage has been successfully completed.
---@field active boolean Whether this stage is currently active and being evaluated.

---@alias MissionStage _MissionStage | { StageID: integer, name: string, description: string, Objective: string, DisplayObjective: boolean, RevealObjectiveOnActivate: boolean, MissionRewardType: MissionRewardType, RewardAmount: string, MissionReward: MissionReward, IgnoreExceptionBranches: boolean, ProgressScope: MissionProgressScope, actions: JsonList<MissionAction>, branches: JsonList<MissionBranch>, parentMissionID: string, scriptableCondition: Condition, completed: boolean, active: boolean }

---Represents a conditional branch in a mission that evaluates a Condition and transitions to a target stage when the condition is met.
---@class _MissionBranch : _JsonUserDataBase
---@field condition Condition The condition that must be met for this branch to transition to the target stage.
---@field TargetStage integer The stage ID this branch transitions to when its condition is satisfied.
---@field ExceptionBranch boolean Indicates whether this branch represents an exception path in the mission flow.
---@field IsPreRequisiteBranch boolean Gets a value indicating whether this branch acts as a pre-requisite gate before the owning stage proceeds.
---@field IsExceptionBranch boolean Gets a value indicating whether this branch is currently marked as an exception branch.

---@alias MissionBranch _MissionBranch | { condition: Condition, TargetStage: integer, ExceptionBranch: boolean, IsPreRequisiteBranch: boolean, IsExceptionBranch: boolean }

---Represents a named branch of mission actions that can be activated or deactivated on demand by branch ID.
---@class _MissionContentBranch : _JsonUserDataBase
---@field ID string The identifier used by the runtime to look up this branch. Brief, Debrief, and OnSubmit have auto-trigger hooks. Other identifiers must be invoked manually.
---@field actions JsonList<MissionAction> The actions executed when this branch is activated, deactivated, or reset.

---@alias MissionContentBranch _MissionContentBranch | { ID: string, actions: JsonList<MissionAction> }

---Represents the reward associated with a mission, containing a collection of MissionRewardDefinition entries.
---@class _MissionReward : _JsonUserDataBase
---@field MissionRewardDefinitions JsonList<MissionRewardDefinition> The reward definitions that make up this mission reward.

---@alias MissionReward _MissionReward | { MissionRewardDefinitions: JsonList<MissionRewardDefinition> }

---Represents a single reward associated with a mission.
---@class _MissionRewardDefinition : _JsonUserDataBase
---@field MissionRewardType MissionRewardType The type of reward granted by this mission definition.
---@field RewardAmount number The numerical amount of the reward. Interpretation depends on the reward type.
---@field RewardKey string An optional addressable or identifier key for the reward, such as an unlocked content key.

---@alias MissionRewardDefinition _MissionRewardDefinition | { MissionRewardType: MissionRewardType, RewardAmount: number, RewardKey: string }

---A base class for mission conditions, providing virtual evaluation and lifecycle methods used by concrete condition implementations.
---@class _ConditionBase : _JsonUserDataBase

---A composite Condition that evaluates a collection of child conditions combined by a LogicalOperator.
---@class _ConditionSet : _ConditionBase
---@field Children JsonList<Condition> The child Condition instances that make up this condition set.
---@field ConditionMode LogicalOperator The LogicalOperator used to combine the results of the child conditions.
---@field ConditionType "ConditionSet" Gets the string identifier for this condition type.

---@alias ConditionSet _ConditionSet | { Children: JsonList<Condition>, ConditionMode: LogicalOperator, ConditionType: "ConditionSet" }

---Condition that tests a PropertyWatcher value against a threshold using a PropertyOperator comparison.
---@class _PropertyCondition : _ConditionBase
---@field RequireCurrentValue boolean Gets or sets a value indicating whether the condition must hold true at the current evaluation time rather than once it has been met.
---@field valueMet boolean Tracks whether the condition value has been met at least once.
---@field PropertyTypeAQN string The assembly-qualified name of the PropertyWatcher type to instantiate for this condition.
---@field TestWatchedValue number The numeric threshold against which the watched double property value is compared.
---@field TestWatchedstring string The string threshold against which the watched string property value is compared.
---@field TestWatchedStringVariable string Optional mission variable whose string value replaces TestWatchedstring as the comparison threshold.
---@field TestWatchedInt integer The integer threshold against which the watched integer or enum property value is compared.
---@field TestWatchedBool boolean The boolean threshold against which the watched boolean property value is compared.
---@field propOperator PropertyOperator The PropertyOperator used to compare the watched property value against the test threshold.
---@field isInput boolean Gets or sets a value indicating whether the property value is read from a mission input parameter rather than directly from the watcher.
---@field Inputstring string The name of the mission input parameter to use when isInput is true.
---@field CaptureValueAsVariable string Mission variable that receives a string value when this condition matches.
---@field CapturePropertyTypeAQN string Optional string-valued watcher whose output is captured. When empty, the condition's main watcher is used.
---@field CaptureIsInput boolean Whether the capture watcher uses CaptureInputstring as its input parameter.
---@field CaptureInputstring string Optional input passed to the capture watcher.
---@field ConditionType "PropertyCondition" Gets the condition type identifier string for this condition.

---@alias PropertyCondition _PropertyCondition | { RequireCurrentValue: boolean, valueMet: boolean, PropertyTypeAQN: string, TestWatchedValue: number, TestWatchedstring: string, TestWatchedStringVariable: string, TestWatchedInt: integer, TestWatchedBool: boolean, propOperator: PropertyOperator, isInput: boolean, Inputstring: string, CaptureValueAsVariable: string, CapturePropertyTypeAQN: string, CaptureIsInput: boolean, CaptureInputstring: string, ConditionType: "PropertyCondition" }

---Condition that evaluates to true when a specified game message event type has been observed.
---@class _EventCondition : _ConditionBase
---@field eventObserved boolean
---@field inputString string
---@field EventTypeAQN string The assembly-qualified name of the event type used to resolve eventType at initialization.
---@field ConditionType "EventCondition" Gets the string identifier for this condition type.

---@alias EventCondition _EventCondition | { eventObserved: boolean, inputString: string, EventTypeAQN: string, ConditionType: "EventCondition" }

---Condition that evaluates success by subscribing to a configurable message event type and delegating evaluation to a ScriptMethodReference script.
---@class _ScriptCondition : _ConditionBase
---@field scriptSucceeded boolean
---@field triggerEventTypeAQN string The assembly-qualified name of the message event type used to trigger evaluation.
---@field EvaluationScript ScriptMethodReference The ScriptMethodReference that is invoked to evaluate this condition.
---@field ConditionType "ScriptCondition" Gets the string identifier for this condition type.

---@alias ScriptCondition _ScriptCondition | { scriptSucceeded: boolean, triggerEventTypeAQN: string, EvaluationScript: ScriptMethodReference, ConditionType: "ScriptCondition" }

---@alias Condition ConditionSet | PropertyCondition | EventCondition | ScriptCondition

---Interface for a mission action, defining activation, deactivation, reset, parent-data binding, editor metadata, and deep-copy behavior.
---@alias MissionAction table

---The owner category of a mission.
---@alias MissionOwner
---| "None" # No owner assigned to the mission.
---| "Global" # Indicates the mission is owned globally, not tied to a specific agency or player.
---| "Agency" # Indicates the mission is owned by an agency.
---| "Player" # Indicates the mission is owned by the player.

---Classification of a mission as primary, secondary, tutorial, or first-time user experience.
---@alias MissionType
---| "Primary" # The main, required mission in a mission set.
---| "Secondary" # An optional side mission that supplements the primary mission.
---| "Tutorial" # A guided tutorial mission that teaches gameplay mechanics.
---| "FTUE" # A first-time user experience mission presented to new players on their initial session.

---The execution state of a mission.
---@alias MissionState
---| "Inactive" # The mission has not been started or activated.
---| "Active" # The mission is currently in progress.
---| "Complete" # The mission has been successfully completed.
---| "Failed" # The mission ended without meeting its objectives.
---| "Invalid" # The mission state is unrecognized or has not been set.

---Represents the UI display context in which a mission is presented.
---@alias UIDisplayType
---| "Default" # The default display context, with no specific scene restriction.
---| "Video" # A video or cinematic display context.
---| "Flight" # The flight scene display context.
---| "VAB" # The Vehicle Assembly Building scene display context.
---| "VAB_Flight" # Both the Vehicle Assembly Building and flight scene display contexts.

---The type of reward granted upon completing a mission.
---@alias MissionRewardType
---| "None" # Indicates that no reward is granted.
---| "SciencePoints" # Indicates that a science points reward is granted.

---Logical operator used to combine mission conditions.
---@alias LogicalOperator
---| "Invalid" # Represents an uninitialized or unrecognized logical operator.
---| "OR" # Represents the logical OR operator, which evaluates to true when at least one operand is true.
---| "AND" # Represents the logical AND operator, which evaluates to true when all operands are true.
---| "XOR" # Represents the logical XOR operator, which evaluates to true when an odd number of operands are true.
---| "NOT" # Represents the logical NOT operator, which inverts the truth value of a single operand.
---| "Count" # The total number of defined logical operator values.

---Comparison operator for evaluating a mission property value against a target condition.
---@alias PropertyOperator
---| "Invalid" # A sentinel value indicating an unrecognized or unset comparison operator.
---| "LESSER" # Evaluates as true when the property value is less than the target value.
---| "EQUAL" # Evaluates as true when the property value is equal to the target value.
---| "GREATER" # Evaluates as true when the property value is greater than the target value.
---| "Count" # The total number of named comparison operators, used for bounds checking and iteration.
