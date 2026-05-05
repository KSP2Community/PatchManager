---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/PlanetsLuaModule.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Converters/CelestialBodyConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Converters/GalaxyConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Converters/VolumeCloudConverter.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/UserData/CelestialBodyUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/UserData/GalaxyUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/UserData/VolumeCloudUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/UserData/CloudUserData.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Overrides/AtmosphereOverride.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Overrides/CloudsDataOverride.cs
-- Source: ksp2redux/Assets/Modules/PatchManager/Runtime/Planets/Overrides/VolumeCloudConfigurationOverride.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/CelestialBodyDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/CelestialBodyProperties.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/Definitions/CelestialBodyRingData.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/SerializedCelestialBody.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/SerializedGalaxyDefinition.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/SerializedOrbitProperties.cs
-- Source: ksp2redux/Assets/Code/KSP/Sim/SerializedOribiterDefinition.cs


-- =====================================================================
-- Submodule
-- =====================================================================

---Lua submodule exposed as `PM.Planets`, providing patches for celestial bodies, the default galaxy,
---and atmosphere / cloud overrides.
---@class PlanetsLuaModule
local PlanetsLuaModule = {}

---Registers a celestial-body patch with the given namespaced patch name.
---@param name string The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<CelestialBodyUserData, any> patch The registered patch.
function PlanetsLuaModule:Patch(name) end

---Registers a patch against the default galaxy definition.
---@param name string The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<GalaxyUserData, JsonUserData> patch The registered patch.
function PlanetsLuaModule:PatchDefaultGalaxy(name) end

---Registers a patch against the `atmosphere_overrides` label with the given namespaced patch name.
---@param name string The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<AtmosphereOverrideUserData, any> patch The registered patch.
function PlanetsLuaModule:PatchAtmosphereOverride(name) end

---Creates a new atmosphere override for the given body and runs callback against it
---for further configuration.
---@param name string The body name the override applies to.
---@param callback fun(override: AtmosphereOverrideUserData) Callback that receives the new override for further configuration.
function PlanetsLuaModule:CreateAtmosphereOverride(name, callback) end

---Registers a patch against the `volume_cloud_overrides` label with the given namespaced patch name.
---@param name string The patch's local name; namespaced with the host mod's ID.
---@return LuaPatch<VolumeCloudUserData, any> patch The registered patch.
function PlanetsLuaModule:PatchCloudOverride(name) end

---Creates a new volume-cloud override for the given body and runs callback against it
---for further configuration.
---@param name string The body name the override applies to.
---@param callback fun(override: VolumeCloudUserData) Callback that receives the new override for further configuration.
function PlanetsLuaModule:CreateCloudOverride(name, callback) end


-- =====================================================================
-- UserData wrappers
-- =====================================================================

---JSON UserData wrapping a celestial body's `data` subtree while preserving the full envelope for round-tripping.
---The wrapped Token is the inner `data` object (shape `CelestialBodyProperties`); the outer envelope is held
---internally for serialization round-trip.
---@class CelestialBodyUserData : _CelestialBodyProperties, JsonUserData

---Synthetic wrapper around a `SerializedCelestialBody` JSON entry within a galaxy definition.
---@class SerializedCelestialBodyUserData : _SerializedCelestialBody, JsonUserData

---@class _GalaxyUserDataBodyIndexer
---@field [string] JsonUserData

---Galaxy definition wrapper that exposes each celestial body in the galaxy's `CelestialBodies` array as a
---virtual property keyed by GUID.
---@class GalaxyUserData : _SerializedGalaxyDefinition, _GalaxyUserDataBodyIndexer, ExtensibleJsonUserData
local GalaxyUserData = {}

---Adds a new celestial body with the given GUID to the galaxy and runs callback against
---it for further configuration.
---@param planetName string The new body's GUID.
---@param callback fun(body: SerializedCelestialBodyUserData) Callback that receives the new body for further configuration.
function GalaxyUserData:Add(planetName, callback) end

---Volume-cloud configuration wrapper that exposes the `cumulusList` array as a typed
---CloudUserData rather than a raw JsonUserData.
---@class VolumeCloudUserData : _VolumeCloudConfigurationOverride, ExtensibleJsonUserData
---@field cumulusList CloudUserData The cumulus layer list, exposed as a name-indexed CloudUserData.

---Synthetic per-element wrapper for entries of a `CloudUserData`.
---@class CloudLayerUserData : _CloudsDataOverride, JsonUserData

---Indexed-list wrapper for a volume cloud's `cumulusList`, keyed by each layer's `layerName`.
---@class CloudUserData : IndexedListUserData<CloudLayerUserData>

---Synthetic wrapper around an `AtmosphereOverride` JSON entry, used by `PM.Planets:PatchAtmosphereOverride`
---and `PM.Planets:CreateAtmosphereOverride`.
---@class AtmosphereOverrideUserData : _AtmosphereOverride, JsonUserData


-- =====================================================================
-- Override JSON schemas (atmosphere_overrides / volume_cloud_overrides)
-- =====================================================================

---Override applied to a planet's AtmosphereModel. Every field except `PlanetName` is optional;
---only fields with a value are written through to the model on apply.
---This is the on-disk JSON shape stored under the `atmosphere_overrides` addressables label
---(file naming `atmosphere_override_<PlanetName>`).
---@class _AtmosphereOverride : _JsonUserDataBase
---@field PlanetName string
---@field IsGasGiant? boolean
---@field Exposure? Vector2
---@field SunAngleRadius? number
---@field SunZenithAngle? number
---@field SolarIrradiance? Vector3
---@field SunDirectionExposureModifier? number
---@field TransmittanceTint? number
---@field NoonColorStrength? number
---@field SunsetColorStrength? number
---@field ColorTransitionScale? number
---@field BottomRadius? number
---@field AtmosphereHeight? number
---@field GroundAlbedo? Color
---@field RayleighScattering? Vector3
---@field RayleighScatteringScale? number
---@field RayleighExponentialDistribution? number
---@field MieScattering? Vector3
---@field MieScatteringScale? number
---@field MieAnisotropy? number
---@field MieExponentialDistribution? number
---@field AbsorptionScale? number
---@field Absorption? Vector3
---@field AbsorptionMaxDensity? number
---@field AbsorptionHeightMinMax? Vector2
---@field TransmittanceTexture? string Addressables key of the replacement transmittance Texture2D.
---@field IrradianceTexture? string Addressables key of the replacement irradiance Texture2D.
---@field ScatteringTexture? string Addressables key of the replacement scattering Texture3D.

---@alias AtmosphereOverride _AtmosphereOverride | { PlanetName: string, IsGasGiant?: boolean, Exposure?: Vector2, SunAngleRadius?: number, SunZenithAngle?: number, SolarIrradiance?: Vector3, SunDirectionExposureModifier?: number, TransmittanceTint?: number, NoonColorStrength?: number, SunsetColorStrength?: number, ColorTransitionScale?: number, BottomRadius?: number, AtmosphereHeight?: number, GroundAlbedo?: Color, RayleighScattering?: Vector3, RayleighScatteringScale?: number, RayleighExponentialDistribution?: number, MieScattering?: Vector3, MieScatteringScale?: number, MieAnisotropy?: number, MieExponentialDistribution?: number, AbsorptionScale?: number, Absorption?: Vector3, AbsorptionMaxDensity?: number, AbsorptionHeightMinMax?: Vector2, TransmittanceTexture?: string, IrradianceTexture?: string, ScatteringTexture?: string }

---Override applied to a `VolumeCloudConfiguration`. Every field except `bodyName` is optional;
---only fields with a value are written through on apply.
---This is the on-disk JSON shape stored under the `volume_cloud_overrides` addressables label
---(file naming `volume_cloud_override_<bodyName>`).
---@class _VolumeCloudConfigurationOverride : _JsonUserDataBase
---@field bodyName string
---@field exclusiveLayer? boolean
---@field CloudsRotateAll? Vector3
---@field planetRadius? number
---@field enableColorMap? boolean
---@field enableVerticalColor? boolean
---@field colorMapIntensity? number
---@field verticalColorIntensity? number
---@field overallSize? number
---@field vortexCloudHeightRange? Vector2
---@field cumulusList JsonList<CloudsDataOverride> Cumulus layer overrides; each entry is matched to the live layer with the same `layerName`.
---@field cloudCoverageModifier? number
---@field detailVariationRange? number
---@field enableShadows? boolean
---@field enableLayerShadows? boolean
---@field volumetricShadowDensity? number
---@field volumetricShadowLodBias? number
---@field volumetricShadowDistance? number
---@field shadowOpacity? number
---@field shadowMapStrength? number
---@field layerShadowDensity? number
---@field ambientColor? Color
---@field EnableCloudGI? boolean
---@field cloudGIIntensity? number
---@field cloudGITint? number
---@field lightPenetrateDistance? number
---@field multiScatteringScattering? number
---@field extinctionByLightPosition? number
---@field opticsDistanceScale? number
---@field silverSpreadG? number
---@field bloomStrengthG? number
---@field silverSpreadUnderCloudG? number
---@field bloomStrengthUnderCloudG? number
---@field silverSpread? number
---@field bloomStrength? number
---@field silverSpreadUnderCloud? number
---@field bloomStrengthUnderCloud? number
---@field ambientScale? number
---@field scatteringScale? number
---@field cloudsDensityScale? number
---@field enableGodray? boolean
---@field godrayIntensity? number
---@field godrayVisibleDistance? number
---@field godrayStepSize? number
---@field sampleLightStepSize? number
---@field sampleLightStepCount? number
---@field cloudDensityRangeEmitGodray? Vector2
---@field godrayAttenuation? number
---@field godrayFadeHeight? number
---@field godrayBlurSize? number
---@field IsBlurGodray? boolean
---@field antiBandingAmplify? number
---@field useScaleCloudsOnly? boolean
---@field raymarchStepSize? number
---@field increaseRaymarchStepByDistance? boolean
---@field distanceRatio? number
---@field maxRaymarchStepSize? number
---@field cullingEdgeClouds? boolean
---@field cullingStrength? number
---@field autoMipmap? boolean
---@field scaleCloudMaskNormalTileRate? number
---@field cascadedResolutionRange? number
---@field mipmapScale? number
---@field enableFadeout? boolean
---@field startFadeoutHeight? number
---@field endFadeoutHeight? number

---@alias VolumeCloudConfigurationOverride _VolumeCloudConfigurationOverride | { bodyName: string, exclusiveLayer?: boolean, CloudsRotateAll?: Vector3, planetRadius?: number, enableColorMap?: boolean, enableVerticalColor?: boolean, colorMapIntensity?: number, verticalColorIntensity?: number, overallSize?: number, vortexCloudHeightRange?: Vector2, cumulusList: JsonList<CloudsDataOverride>, cloudCoverageModifier?: number, detailVariationRange?: number, enableShadows?: boolean, enableLayerShadows?: boolean, volumetricShadowDensity?: number, volumetricShadowLodBias?: number, volumetricShadowDistance?: number, shadowOpacity?: number, shadowMapStrength?: number, layerShadowDensity?: number, ambientColor?: Color, EnableCloudGI?: boolean, cloudGIIntensity?: number, cloudGITint?: number, lightPenetrateDistance?: number, multiScatteringScattering?: number, extinctionByLightPosition?: number, opticsDistanceScale?: number, silverSpreadG?: number, bloomStrengthG?: number, silverSpreadUnderCloudG?: number, bloomStrengthUnderCloudG?: number, silverSpread?: number, bloomStrength?: number, silverSpreadUnderCloud?: number, bloomStrengthUnderCloud?: number, ambientScale?: number, scatteringScale?: number, cloudsDensityScale?: number, enableGodray?: boolean, godrayIntensity?: number, godrayVisibleDistance?: number, godrayStepSize?: number, sampleLightStepSize?: number, sampleLightStepCount?: number, cloudDensityRangeEmitGodray?: Vector2, godrayAttenuation?: number, godrayFadeHeight?: number, godrayBlurSize?: number, IsBlurGodray?: boolean, antiBandingAmplify?: number, useScaleCloudsOnly?: boolean, raymarchStepSize?: number, increaseRaymarchStepByDistance?: boolean, distanceRatio?: number, maxRaymarchStepSize?: number, cullingEdgeClouds?: boolean, cullingStrength?: number, autoMipmap?: boolean, scaleCloudMaskNormalTileRate?: number, cascadedResolutionRange?: number, mipmapScale?: number, enableFadeout?: boolean, startFadeoutHeight?: number, endFadeoutHeight?: number }

---Override applied to a `VolumeCloudConfiguration.CloudsData` layer. Layers are matched by `layerName`;
---only fields with a value are written through on apply. The `baseTexureTile`-onwards fields apply only
---when the matched live layer is a `CumulusData`.
---@class _CloudsDataOverride : _JsonUserDataBase
---@field layerName string
---@field isEnable? boolean
---@field castShadow? boolean
---@field bakeCloudMipmap? number
---@field currentBakedCloudMipMap? number
---@field cloudsType? CloudsLayerType
---@field cloudHeightRange? Vector2
---@field bakedCloudHeight? number
---@field cloudsLayerRotate? Vector3
---@field enableWind? boolean
---@field windDirection? Vector2
---@field movementSpeed? number
---@field evolveSpeed? number
---@field topOffset? number
---@field isFold? boolean
---@field baseTexureTile? number
---@field coverageScale? number
---@field evanish? number
---@field detailAmount? number
---@field cloudsMaskBias? number
---@field upperFalloff? number
---@field lowerFalloff? number
---@field detailAltitudeShift? number
---@field enableDetailTexture? boolean
---@field detailTextureTile? number
---@field detailStrength? number
---@field cloudsDensity? number
---@field normalScale? number
---@field scaleCloudColor? Color

---@alias CloudsDataOverride _CloudsDataOverride | { layerName: string, isEnable?: boolean, castShadow?: boolean, bakeCloudMipmap?: number, currentBakedCloudMipMap?: number, cloudsType?: CloudsLayerType, cloudHeightRange?: Vector2, bakedCloudHeight?: number, cloudsLayerRotate?: Vector3, enableWind?: boolean, windDirection?: Vector2, movementSpeed?: number, evolveSpeed?: number, topOffset?: number, isFold?: boolean, baseTexureTile?: number, coverageScale?: number, evanish?: number, detailAmount?: number, cloudsMaskBias?: number, upperFalloff?: number, lowerFalloff?: number, detailAltitudeShift?: number, enableDetailTexture?: boolean, detailTextureTile?: number, detailStrength?: number, cloudsDensity?: number, normalScale?: number, scaleCloudColor?: Color }


-- =====================================================================
-- Celestial body schema (the inner `data` subtree wrapped by CelestialBodyUserData)
-- =====================================================================

---Represents a celestial body definition, pairing a string key with optional CelestialBodyProperties.
---@class _CelestialBodyDefinition : _JsonUserDataBase
---@field key string
---@field properties? CelestialBodyProperties

---@alias CelestialBodyDefinition _CelestialBodyDefinition | { key: string, properties?: CelestialBodyProperties }

---Represents the definition data for a celestial body, including physical, atmospheric, ocean, rotation,
---and science parameters.
---This is the JSON shape that lives inside the celestial body envelope's `data` subtree.
---@class _CelestialBodyProperties : _JsonUserDataBase
---@field assetKeySimulation string
---@field assetKeyScaled string
---@field bodyName string
---@field bodyDisplayName string
---@field bodyDescription string
---@field gravityASL number
---@field radius number
---@field isHomeWorld boolean
---@field oceanAltitude number
---@field oceanDensity number
---@field MinTerrainHeight number
---@field MaxTerrainHeight number
---@field TerrainHeightScale number
---@field TimeWarpAltitudeOffset number
---@field SphereOfInfluenceCalculationType integer
---@field navballSwitchAltitudeHigh number
---@field navballSwitchAltitudeLow number
---@field hasOcean boolean
---@field HasLocalSpace boolean
---@field oceanUseFog boolean
---@field oceanFogPQSDepth number
---@field oceanFogPQSDepthRecip number
---@field oceanFogDensityStart number
---@field oceanFogDensityEnd number
---@field oceanFogDensityPQSMult number
---@field oceanFogDensityAltScalar number
---@field oceanFogDensityExponent number
---@field oceanFogColorStart Color
---@field oceanFogColorEnd Color
---@field oceanFogDawnFactor number
---@field oceanSkyColorMult number
---@field oceanSkyColorOpacityBase number
---@field oceanSkyColorOpacityAltMult number
---@field oceanAFGBase number
---@field oceanAFGAltMult number
---@field oceanAFGMin number
---@field oceanSunBase number
---@field oceanSunAltMult number
---@field oceanSunMin number
---@field oceanAFGLerp boolean
---@field oceanMinAlphaFogDistance number
---@field oceanMaxAlbedoFog number
---@field oceanMaxAlphaFog number
---@field oceanAlbedoDistanceScalar number
---@field oceanAlphaDistanceScalar number
---@field minOrbitalDistance number
---@field hasAtmosphere boolean
---@field atmosphereContainsOxygen boolean
---@field atmosphereDepth number
---@field atmosphereTemperatureSeaLevel number
---@field atmospherePressureSeaLevel number
---@field atmosphereMolarMass number
---@field atmosphereAdiabaticIndex number
---@field atmosphericReentryVFXGradient string
---@field atmosphereTemperatureLapseRate number
---@field atmosphereGasMassLapseRate number
---@field useAtmosphereTemperatureCurve boolean
---@field isAtmosphereTemperatureCurveNormalized boolean
---@field useAtmosphereHumidityCurve boolean
---@field BodyAltitudeTemperatureCurve FloatCurve
---@field BodyAltitudeSurfaceFluxCurve FloatCurve
---@field BodyAltitudeFluxCurve FloatCurve
---@field BodyAltitudeRelativeHumidityCurve FloatCurve
---@field BodySurfaceFluxScale number
---@field BodySurfaceFluxMapPath string
---@field StarLuminosity number
---@field albedo number
---@field emissivity number
---@field coreTemperatureOffset number
---@field convectionMultiplier number
---@field shockTemperatureMultiplier number
---@field useAtmospherePressureCurve boolean
---@field isAtmospherePressureCurveNormalized boolean
---@field atmospherePressureCurve FloatCurve
---@field hasSolidSurface boolean
---@field ringGroupData JsonList<CelestialBodyRingData>
---@field scaledElipRadMult Vector3d
---@field scaledRadiusHorizMultiplier number
---@field rotates boolean
---@field isRotating? boolean Legacy JSON field name for `rotates` (present in stock `Kerbin.bytes`); current converters read `rotates`.
---@field rotationPeriod number
---@field hasSolarRotationPeriod boolean
---@field initialRotation number
---@field axialTilt Quaternion
---@field isTidallyLocked boolean
---@field clampInverseRotThreshold boolean
---@field hasInverseRotationThresholdClamp? boolean Legacy JSON field name for `clampInverseRotThreshold` (present in stock `Kerbin.bytes`); current converters read `clampInverseRotThreshold`.
---@field hasInverseRotation boolean
---@field inverseRotThresholdAltitude number
---@field scaledShaderFadeFar number
---@field scaledShaderFadeNear number
---@field MineDustColor Vector4
---@field IsStar boolean

---@alias CelestialBodyProperties _CelestialBodyProperties | { assetKeySimulation: string, assetKeyScaled: string, bodyName: string, bodyDisplayName: string, bodyDescription: string, gravityASL: number, radius: number, isHomeWorld: boolean, oceanAltitude: number, oceanDensity: number, MinTerrainHeight: number, MaxTerrainHeight: number, TerrainHeightScale: number, TimeWarpAltitudeOffset: number, SphereOfInfluenceCalculationType: integer, navballSwitchAltitudeHigh: number, navballSwitchAltitudeLow: number, hasOcean: boolean, HasLocalSpace: boolean, oceanUseFog: boolean, oceanFogPQSDepth: number, oceanFogPQSDepthRecip: number, oceanFogDensityStart: number, oceanFogDensityEnd: number, oceanFogDensityPQSMult: number, oceanFogDensityAltScalar: number, oceanFogDensityExponent: number, oceanFogColorStart: Color, oceanFogColorEnd: Color, oceanFogDawnFactor: number, oceanSkyColorMult: number, oceanSkyColorOpacityBase: number, oceanSkyColorOpacityAltMult: number, oceanAFGBase: number, oceanAFGAltMult: number, oceanAFGMin: number, oceanSunBase: number, oceanSunAltMult: number, oceanSunMin: number, oceanAFGLerp: boolean, oceanMinAlphaFogDistance: number, oceanMaxAlbedoFog: number, oceanMaxAlphaFog: number, oceanAlbedoDistanceScalar: number, oceanAlphaDistanceScalar: number, minOrbitalDistance: number, hasAtmosphere: boolean, atmosphereContainsOxygen: boolean, atmosphereDepth: number, atmosphereTemperatureSeaLevel: number, atmospherePressureSeaLevel: number, atmosphereMolarMass: number, atmosphereAdiabaticIndex: number, atmosphericReentryVFXGradient: string, atmosphereTemperatureLapseRate: number, atmosphereGasMassLapseRate: number, useAtmosphereTemperatureCurve: boolean, isAtmosphereTemperatureCurveNormalized: boolean, useAtmosphereHumidityCurve: boolean, BodyAltitudeTemperatureCurve: FloatCurve, BodyAltitudeSurfaceFluxCurve: FloatCurve, BodyAltitudeFluxCurve: FloatCurve, BodyAltitudeRelativeHumidityCurve: FloatCurve, BodySurfaceFluxScale: number, BodySurfaceFluxMapPath: string, StarLuminosity: number, albedo: number, emissivity: number, coreTemperatureOffset: number, convectionMultiplier: number, shockTemperatureMultiplier: number, useAtmospherePressureCurve: boolean, isAtmospherePressureCurveNormalized: boolean, atmospherePressureCurve: FloatCurve, hasSolidSurface: boolean, ringGroupData: JsonList<CelestialBodyRingData>, scaledElipRadMult: Vector3d, scaledRadiusHorizMultiplier: number, rotates: boolean, isRotating?: boolean, rotationPeriod: number, hasSolarRotationPeriod: boolean, initialRotation: number, axialTilt: Quaternion, isTidallyLocked: boolean, clampInverseRotThreshold: boolean, hasInverseRotationThresholdClamp?: boolean, hasInverseRotation: boolean, inverseRotThresholdAltitude: number, scaledShaderFadeFar: number, scaledShaderFadeNear: number, MineDustColor: Vector4, IsStar: boolean }

---Represents the science data multipliers and altitude thresholds for a celestial body.
---@class _CelestialBodyProperties_ScienceParams : _JsonUserDataBase
---@field landedDataValue number
---@field splashedDataValue number
---@field flyingLowDataValue number
---@field flyingHighDataValue number
---@field inSpaceLowDataValue number
---@field inSpaceHighDataValue number
---@field recoveryValue number
---@field flyingAltitudeThreshold number
---@field spaceAltitudeThreshold number

---@alias CelestialBodyProperties_ScienceParams _CelestialBodyProperties_ScienceParams | { landedDataValue: number, splashedDataValue: number, flyingLowDataValue: number, flyingHighDataValue: number, inSpaceLowDataValue: number, inSpaceHighDataValue: number, recoveryValue: number, flyingAltitudeThreshold: number, spaceAltitudeThreshold: number }

---Represents the ring data for a celestial body, defining inner and outer radii and a density curve.
---@class _CelestialBodyRingData : _JsonUserDataBase
---@field innerRadius number
---@field outerRadius number
---@field density FloatCurve

---@alias CelestialBodyRingData _CelestialBodyRingData | { innerRadius: number, outerRadius: number, density: FloatCurve }


-- =====================================================================
-- Galaxy schema (wrapped by GalaxyUserData)
-- =====================================================================

---Represents a serialized galaxy definition, including its name, version, and celestial bodies.
---This is the on-disk JSON shape stored under the `Galaxy` converter (file naming `GalaxyDefinition_*`).
---@class _SerializedGalaxyDefinition : _JsonUserDataBase
---@field Name string
---@field Version string
---@field CelestialBodies JsonList<SerializedCelestialBody>

---@alias SerializedGalaxyDefinition _SerializedGalaxyDefinition | { Name: string, Version: string, CelestialBodies: JsonList<SerializedCelestialBody> }

---Represents the serialized form of a celestial body, including its identity and orbital configuration.
---@class _SerializedCelestialBody : _JsonUserDataBase
---@field GUID string
---@field referenceBodyGuid string
---@field OrbitProperties SerializedOrbitProperties
---@field OrbiterProperties SerializedOribiterDefinition
---@field PrefabKey? string Present in stock `GalaxyDefinition_Default` JSON but not declared on the C# class. Reachable when reading existing data; not produced by `PM` or by the C# serializer.

---@alias SerializedCelestialBody _SerializedCelestialBody | { GUID: string, referenceBodyGuid: string, OrbitProperties: SerializedOrbitProperties, OrbiterProperties: SerializedOribiterDefinition, PrefabKey?: string }

---Represents a serializable set of Keplerian orbital elements and the reference body for an orbit.
---@class _SerializedOrbitProperties : _JsonUserDataBase
---@field referenceBodyGuid string
---@field inclination number
---@field eccentricity number
---@field semiMajorAxis number
---@field longitudeOfAscendingNode number
---@field argumentOfPeriapsis number
---@field meanAnomalyAtEpoch number
---@field epoch number

---@alias SerializedOrbitProperties _SerializedOrbitProperties | { referenceBodyGuid: string, inclination: number, eccentricity: number, semiMajorAxis: number, longitudeOfAscendingNode: number, argumentOfPeriapsis: number, meanAnomalyAtEpoch: number, epoch: number }

---Represents the serialized display configuration for an orbiter, including orbit and node colors,
---camera-to-semi-major-axis visibility ratios, and texture offset settings.
---@class _SerializedOribiterDefinition : _JsonUserDataBase
---@field orbitColor Color
---@field nodeColor Color
---@field lowerCamVsSmaRatio number
---@field upperCamVsSmaRatio number
---@field autoTextureOffset boolean
---@field textureOffset number

---@alias SerializedOribiterDefinition _SerializedOribiterDefinition | { orbitColor: Color, nodeColor: Color, lowerCamVsSmaRatio: number, upperCamVsSmaRatio: number, autoTextureOffset: boolean, textureOffset: number }
