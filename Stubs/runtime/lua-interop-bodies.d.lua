---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceContainerStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceContainerDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceConsumerDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceGeneratorDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceCostConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ResourceOwnerDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ContainedResourceDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/PartResourceDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CrewStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CrewMemberStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CrewMemberDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CelestialBodyDefinitionLua.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CelestialBodyDefinition_ScienceParamsLua.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CelestialBodyPropertiesConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/CelestialBodyPropertiesIncrementalConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ColonyDefinitionConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/ColonyStateConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/SettingsPropertiesConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/KerbalVarietyAttributeRuleConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/KerbalVarietyCategoryParsingRuleConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/KerbalVarietyRangeRuleConverter.cs
-- Source: ksp2redux/Assets/Code/KSP/ScriptInterop/impl/moonsharp/FloatCurveConverter.cs

--#region Resources

---A by-value resource amount: a resource name with its current and maximum stored units.
---@class ResourceState
-- alt: KSP.Sim.State.ResourceState
---@field name string
---@field amount integer
---@field maxAmount integer

---A by-value snapshot of a resource container's contents.
---@class ResourceContainerState
-- alt: KSP.Sim.State.ResourceContainerState
---@field resources table<string, ResourceState>

---A resource container component definition.
-- alt: ResourceContainerDefinition, KSP.Sim.Definitions.ResourceContainerDefinition
---@class ResourceContainer

---A resource consumer component definition.
-- alt: ResourceConsumerDefinition, KSP.Sim.Definitions.ResourceConsumerDefinition
---@class ResourceConsumer

---A resource generator component definition.
-- alt: ResourceGeneratorDefinition, KSP.Sim.Definitions.ResourceGeneratorDefinition
---@class ResourceGenerator

---A resource owner component definition.
-- alt: ResourceOwnerDefinition, KSP.Sim.Definitions.ResourceOwnerDefinition
---@class ResourceOwner

---A part resource cost entry (resource name plus the unit count it costs).
---@class PartResourceCost
-- alt: KSP.Sim.Definitions.PartResourceCostDefinition
---@field name string
---@field resourceUnits number

---A single resource entry contained within a part (name plus initial and capacity units).
---@class ContainedResourceDefinitionInterop
-- alt: KSP.Sim.Definitions.ContainedResourceDefinition
---@field name string
---@field initialUnits integer
---@field capacityUnits integer

---A resource definition - density, cost, flow behaviour, and display info for a resource type.
---@class PartResource
-- alt: PartResourceDefinition, KSP.Sim.PartResourceDefinition
---@field name string
---@field displayName? string
---@field abbreviation? string
---@field density number
---@field volume number
---@field unitCost number
---@field hsp number Specific heat capacity.
---@field isTweakable boolean
---@field isVisible boolean
---@field flowMode ResourceFlowModeInterop
---@field transfer ResourceTransferModeInterop
---@field type ResourceType
---@field r number Color red channel (the definition table also carries the color shape).
---@field g number Color green channel.
---@field b number Color blue channel.
---@field a number Color alpha channel.

--#endregion

--#region Crew

---@alias CrewState
---| "Available" # values come from the KSP.Sim.CrewState enum, marshalled as its name
-- alt: KSP.Sim.CrewState

---A by-value snapshot of one crew member's seating state.
---@class CrewMemberState
-- alt: KSP.Sim.State.CrewMemberState
---@field inPartGuid string
---@field seatIndex integer
---@field state CrewState

---A crew member definition.
-- alt: CrewMemberDefinition, KSP.Sim.Definitions.CrewMemberDefinition
---@class Crew

--#endregion

--#region Celestial

---A celestial body definition - a body key paired with its properties.
---@class CelestialBody
-- alt: CelestialBodyDefinition, KSP.Sim.Definitions.CelestialBodyDefinition
---@field key string
---@field properties CelestialBodyPropertiesInterop

---The full physical, atmospheric, ocean, and rotation property set of a celestial body.
---@class CelestialBodyPropertiesInterop
-- alt: KSP.Sim.Definitions.CelestialBodyProperties
---@field bodyName string
---@field bodyDisplayName string
---@field bodyDescription string
---@field gravityASL number
---@field radius number
---@field assetKeySimulation? string
---@field assetKeyScaled? string
---@field isHomeWorld? boolean
---@field hasOcean? boolean
---@field oceanUseFog? boolean
---@field oceanFogPQSDepth? number
---@field oceanFogPQSDepthRecip? number
---@field oceanFogDensityStart? number
---@field oceanFogDensityEnd? number
---@field oceanFogDensityPQSMult? number
---@field oceanFogDensityAltScalar? number
---@field oceanFogDensityExponent? number
---@field oceanFogColorStart? Color
---@field oceanFogColorEnd? Color
---@field oceanFogDawnFactor? number
---@field oceanSkyColorMult? number
---@field oceanSkyColorOpacityBase? number
---@field oceanSkyColorOpacityAltMult? number
---@field oceanDensity? number
---@field oceanAFGBase? number
---@field oceanAFGAltMult? number
---@field oceanAFGMin? number
---@field oceanSunBase? number
---@field oceanSunAltMult? number
---@field oceanAFGLerp? boolean
---@field oceanMinAlphaFogDistance? number
---@field oceanMaxAlbedoFog? number
---@field oceanMaxAlphaFog? number
---@field oceanAlbedoDistanceScalar? number
---@field oceanAlphaDistanceScalar? number
---@field minOrbitalDistance? number
---@field hasAtmosphere? boolean
---@field atmosphereContainsOxygen? boolean
---@field atmosphereDepth? number
---@field atmosphereTemperatureSeaLevel? number
---@field atmospherePressureSeaLevel? number
---@field atmosphereMolarMass? number
---@field atmosphereAdiabaticIndex? number
---@field atmosphericReentryVFXGradient? string
---@field useAtmosphereTemperatureCurve? boolean
---@field isAtmosphereTemperatureCurveNormalized? boolean
---@field atmosphereTemperatureCurve? FloatCurveInterop
---@field latitudeTemperatureBiasCurve? FloatCurveInterop
---@field latitudeTemperatureSunMultCurve? FloatCurveInterop
---@field StarLuminosity? number
---@field albedo? number
---@field emissivity? number
---@field coreTemperatureOffset? number
---@field convectionMultiplier? number
---@field shockTemperatureMultiplier? number
---@field useAtmospherePressureCurve? boolean
---@field isAtmospherePressureCurveNormalized? boolean
---@field atmospherePressureCurve? FloatCurveInterop
---@field hasSolidSurface? boolean
---@field scaledElipRadMult? Vec3d
---@field scaledRadiusHorizMultiplier? number
---@field rotates? boolean
---@field rotationPeriod? number
---@field hasSolarRotationPeriod? boolean
---@field initialRotation? number
---@field isTidallyLocked? boolean
---@field clampInverseRotThreshold? boolean
---@field hasInverseRotation? boolean
---@field inverseRotThresholdAltitude? number
---@field isStar? boolean
---@field sunlightPrefab? string
---@field axialTilt? Vec3d Euler angles, in degrees, of the body's axial tilt.
---@field scaledShaderFadeFar? number
---@field scaledShaderFadeNear? number

---An incremental (sparse) overlay of celestial body rotation properties - any field may be absent.
---@class CelestialBodyPropertiesIncremental
-- alt: KSP.Sim.Definitions.CelestialBodyPropertiesIncremental
---@field rotationPeriod? number
---@field initialRotation? number
---@field isTidallyLocked? boolean

---Per-situation science data values for a celestial body.
---@class ScienceParams
-- alt: KSP.Sim.Definitions.CelestialBodyProperty.ScienceParams
---@field landedDataValue number
---@field splashedDataValue number
---@field flyingLowDataValue number
---@field flyingHighDataValue number
---@field inSpaceLowDataValue number
---@field inSpaceHighDataValue number
---@field recoveryValue number
---@field flyingAltitudeThreshold number
---@field spaceAltitudeThreshold number

--#endregion

--#region Colony

---A colony definition.
---@class Colony
-- alt: ColonyDefinition, KSP.Sim.Definitions.ColonyDefinition
---@field colonyName string

---A by-value snapshot of a colony's state.
---@class ColonyState
-- alt: KSP.Sim.State.ColonyState
---@field radius number

--#endregion

--#region Settings

---A settings configuration: a named group of setting definitions.
---@class SettingsProperties
-- alt: KSP.Sim.Definitions.SettingsProperties
---@field name string
---@field settings SettingsProperty[]

---A single setting definition within a SettingsProperties group.
---@class SettingsProperty
---@field settingName string
---@field descText? string
---@field settingType "CHECKBOX" | "DROPDOWN" | "SLIDER" | "RADIO"
---@field menuLocation string Parsed from a SettingsMenuLocation enum name.
---@field defaultValue string

--#endregion

--#region Kerbal variety

---A rule mapping a kerbal attribute to a named range rule, with optional dependencies.
---@class KerbalVarietyAttributeRule
-- alt: KSP.Contexts.Kerbal.KerbalVarietyAttributeRule
---@field attributeName string
---@field dependsOn string[]
---@field attributeRangeRuleKey string
---@field applyFunction? string

---A rule that classifies kerbal names into a category by key matching.
---@class KerbalVarietyCategoryParsingRule
-- alt: KSP.Contexts.Kerbal.KerbalVarietyCategoryParsingRule
---@field CategoryName string
---@field StartsWithKeys string[]
---@field ContainsKeys string[]
---@field EndsWithKeys string[]

---A rule defining how a variety value is sampled (min/max, a set, a manifest, or a static value).
---@class VarietyRangeRule
-- alt: KSP.Contexts.Kerbal.VarietyRangeRule
---@field name string
---@field varietyRangeType any KSP.Modding.Variety.VarietyRangeType - "UNDEFINED" | "MINMAX" | "SET" | "MANIFEST" | "STATIC" | "WEIGHTEDMANIFEST". No registered TypeInterop converter.
---@field valueType Type The CLR Type the range produces.
---@field probabilityOfEmpty number
---@field min any Typed as valueType at runtime.
---@field max any Typed as valueType at runtime.
---@field specificOptions any[] Present when varietyRangeType is "SET".
---@field subcategoryKey? string Present for "MANIFEST" / "WEIGHTEDMANIFEST".
---@field attachToName? string
---@field overrideValue any Present when varietyRangeType is "STATIC".

--#endregion

--#region Curves

---A FloatCurve marshalled by value as an array of keyframe tables.
---@class FloatCurveInterop
-- alt: FloatCurve
---@field [integer] FloatCurveKeyframe

---A single FloatCurve keyframe. inTangent / outTangent are omitted when zero.
---@class FloatCurveKeyframe
---@field time number
---@field value number
---@field inTangent? number
---@field outTangent? number

--#endregion
