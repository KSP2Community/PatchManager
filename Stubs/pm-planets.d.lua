---@meta
-- AUTO-GENERATED from analysis of codebase and assets - do not edit by hand.
-- Source: Assets/Modules/PatchManager/Runtime/Planets/PlanetsLuaModule.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Converters/CelestialBodyConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Converters/GalaxyConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Converters/VolumeCloudConverter.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/UserData/CelestialBodyUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/UserData/GalaxyUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/UserData/VolumeCloudUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/UserData/CloudUserData.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Overrides/AtmosphereOverride.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Overrides/VolumeCloudConfigurationOverride.cs
-- Source: Assets/Modules/PatchManager/Runtime/Planets/Overrides/CloudsDataOverride.cs
-- Source: Assets/Code/KSP/game/Load/LoadCelestialBodyDataFilesFlowAction.cs
-- Source: Assets/Code/KSP/Sim/Definitions/CelestialBodyCore.cs
-- Source: Assets/Code/KSP/Sim/Definitions/CelestialBodyData.cs
-- Source: Assets/Code/KSP/Sim/Definitions/CelestialBodyRingData.cs
-- Source: Assets/Code/KSP/Sim/Definitions/SerializedPredefinedSimObject.cs
-- Source: Assets/Code/KSP/Sim/Definitions/SerializedPredefinedColonyObject.cs
-- Source: Assets/Code/KSP/Sim/SerializedGalaxyDefinition.cs
-- Source: Assets/Code/KSP/Sim/SerializedHomeWorld.cs
-- Source: Assets/Code/KSP/Sim/SerializedSpaceCenter.cs
-- Source: Assets/Code/KSP/Sim/SerializedCelestialBody.cs
-- Source: Assets/Code/KSP/Sim/SerializedOrbitProperties.cs
-- Source: Assets/Code/KSP/Sim/SerializedOribiterDefinition.cs

---Lua submodule exposed as `PM.Planets`, providing patches for celestial bodies, the default galaxy,
---and atmosphere / cloud overrides.
---@class PlanetsLuaModule
local PlanetsLuaModule = {}

---Registers a celestial-body patch with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<CelestialBodyUserData, any> patch The registered patch.
function PlanetsLuaModule:Patch(name) end

---Registers a patch against the default galaxy definition.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<GalaxyUserData, JsonUserData> patch The registered patch.
function PlanetsLuaModule:PatchDefaultGalaxy(name) end

---Registers a patch against the galaxy definition with the given key.
---@param galaxyDefinitionKey string The key of the galaxy definition to patch.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<GalaxyUserData, JsonUserData> patch The registered patch.
function PlanetsLuaModule:PatchGalaxy(galaxyDefinitionKey, name) end

---Creates a new galaxy definition with no bodies under the given key and runs callback
---against it for further configuration.
---@param galaxyDefinitionKey string The key that saves and campaign packs load the galaxy definition by.
---@param callback fun(galaxy: GalaxyUserData) Callback that receives the new galaxy definition for further configuration.
function PlanetsLuaModule:CreateGalaxy(galaxyDefinitionKey, callback) end

---Registers a patch against the `atmosphere_overrides` label with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<AtmosphereOverrideUserData, any> patch The registered patch.
function PlanetsLuaModule:PatchAtmosphereOverride(name) end

---Creates a new atmosphere override for the given body and runs callback against it
---for further configuration.
---@param name string The body name the override applies to.
---@param callback fun(override: AtmosphereOverrideUserData) Callback that receives the new override for further configuration.
function PlanetsLuaModule:CreateAtmosphereOverride(name, callback) end

---Registers a patch against the `volume_cloud_overrides` label with the given namespaced patch name.
---@param name string The patch's local name, namespaced with the host mod's ID.
---@return PatchDefinition<VolumeCloudUserData, any> patch The registered patch.
function PlanetsLuaModule:PatchCloudOverride(name) end

---Creates a new volume-cloud override for the given body and runs callback against it
---for further configuration.
---@param name string The body name the override applies to.
---@param callback fun(override: VolumeCloudUserData) Callback that receives the new override for further configuration.
function PlanetsLuaModule:CreateCloudOverride(name, callback) end

---JSON UserData wrapping a celestial body's `data` subtree while preserving the full envelope for round-tripping.
---@class CelestialBodyUserData : _CelestialBodyData, JsonUserData

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
---@field cumulusList CloudUserData

---@class CloudLayerUserData : _CloudsDataOverride, JsonUserData

---Indexed-list wrapper for a volume cloud's `cumulusList`, keyed by each layer's `layerName`.
---@class CloudUserData : IndexedListUserData<CloudLayerUserData>

---@class AtmosphereOverrideUserData : _AtmosphereOverride, JsonUserData

---@class _AtmosphereOverride : _JsonUserDataBase
---@field PlanetName string
---@field Layer? string
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
---@field TransmittanceTexture? string
---@field IrradianceTexture? string
---@field ScatteringTexture? string

---@alias AtmosphereOverride _AtmosphereOverride | { PlanetName: string, Layer?: string, IsGasGiant?: boolean, Exposure?: Vector2, SunAngleRadius?: number, SunZenithAngle?: number, SolarIrradiance?: Vector3, SunDirectionExposureModifier?: number, TransmittanceTint?: number, NoonColorStrength?: number, SunsetColorStrength?: number, ColorTransitionScale?: number, BottomRadius?: number, AtmosphereHeight?: number, GroundAlbedo?: Color, RayleighScattering?: Vector3, RayleighScatteringScale?: number, RayleighExponentialDistribution?: number, MieScattering?: Vector3, MieScatteringScale?: number, MieAnisotropy?: number, MieExponentialDistribution?: number, AbsorptionScale?: number, Absorption?: Vector3, AbsorptionMaxDensity?: number, AbsorptionHeightMinMax?: Vector2, TransmittanceTexture?: string, IrradianceTexture?: string, ScatteringTexture?: string }

---@class _VolumeCloudConfigurationOverride : _JsonUserDataBase
---@field bodyName string
---@field Layer? string
---@field exclusiveLayer? boolean
---@field CloudsRotateAll? Vector3
---@field planetRadius? number
---@field enableColorMap? boolean
---@field enableVerticalColor? boolean
---@field colorMapIntensity? number
---@field verticalColorIntensity? number
---@field overallSize? number
---@field vortexCloudHeightRange? Vector2
---@field cumulusList JsonList<CloudsDataOverride>
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

---@alias VolumeCloudConfigurationOverride _VolumeCloudConfigurationOverride | { bodyName: string, Layer?: string, exclusiveLayer?: boolean, CloudsRotateAll?: Vector3, planetRadius?: number, enableColorMap?: boolean, enableVerticalColor?: boolean, colorMapIntensity?: number, verticalColorIntensity?: number, overallSize?: number, vortexCloudHeightRange?: Vector2, cumulusList: JsonList<CloudsDataOverride>, cloudCoverageModifier?: number, detailVariationRange?: number, enableShadows?: boolean, enableLayerShadows?: boolean, volumetricShadowDensity?: number, volumetricShadowLodBias?: number, volumetricShadowDistance?: number, shadowOpacity?: number, shadowMapStrength?: number, layerShadowDensity?: number, ambientColor?: Color, EnableCloudGI?: boolean, cloudGIIntensity?: number, cloudGITint?: number, lightPenetrateDistance?: number, multiScatteringScattering?: number, extinctionByLightPosition?: number, opticsDistanceScale?: number, silverSpreadG?: number, bloomStrengthG?: number, silverSpreadUnderCloudG?: number, bloomStrengthUnderCloudG?: number, silverSpread?: number, bloomStrength?: number, silverSpreadUnderCloud?: number, bloomStrengthUnderCloud?: number, ambientScale?: number, scatteringScale?: number, cloudsDensityScale?: number, enableGodray?: boolean, godrayIntensity?: number, godrayVisibleDistance?: number, godrayStepSize?: number, sampleLightStepSize?: number, sampleLightStepCount?: number, cloudDensityRangeEmitGodray?: Vector2, godrayAttenuation?: number, godrayFadeHeight?: number, godrayBlurSize?: number, IsBlurGodray?: boolean, antiBandingAmplify?: number, useScaleCloudsOnly?: boolean, raymarchStepSize?: number, increaseRaymarchStepByDistance?: boolean, distanceRatio?: number, maxRaymarchStepSize?: number, cullingEdgeClouds?: boolean, cullingStrength?: number, autoMipmap?: boolean, scaleCloudMaskNormalTileRate?: number, cascadedResolutionRange?: number, mipmapScale?: number, enableFadeout?: boolean, startFadeoutHeight?: number, endFadeoutHeight?: number }

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

---Represents the serialized data definition for a celestial body, including its physical, atmospheric, rotational, and decorative properties.
---@class _CelestialBodyData : _JsonUserDataBase
---@field bodyName string The internal identifier name of the celestial body.
---@field Layer? string The layer this copy of the body's data belongs to, or nil for the default copy.
---@field assetKeyScaled string The asset key used to load the scaled-space representation of the body.
---@field assetKeySimulation string The asset key used to load the simulation-space representation of the body.
---@field bodyDisplayName string The localized display name shown to players for the celestial body.
---@field bodyDescription string The localized descriptive text shown to players for the celestial body.
---@field isStar boolean A value indicating whether this body is classified as a star.
---@field isHomeWorld boolean A value indicating whether this body is the home world of the player's space program.
---@field navballSwitchAltitudeHigh number The altitude above sea level, in metres, at which the navball automatically switches to orbit mode when ascending past this threshold.
---@field navballSwitchAltitudeLow number The altitude above sea level, in metres, at which the navball automatically switches to surface mode when descending past this threshold.
---@field hasSolidSurface boolean A value indicating whether this body has a solid surface that vessels can land on.
---@field hasOcean boolean A value indicating whether this body has an ocean layer.
---@field HasLocalSpace boolean A value indicating whether this body has a local physics space that activates near its surface.
---@field radius number The mean radius of the body, in metres.
---@field gravityASL number The gravitational acceleration at sea level on this body, in m/s^2.
---@field oceanAltitude number The altitude of the ocean surface relative to the reference radius, in metres.
---@field oceanDensity number The density of the body's ocean fluid, in kg/m^3.
---@field MinTerrainHeight number The minimum terrain height offset below the reference radius, in metres.
---@field MaxTerrainHeight number The maximum terrain height offset above the reference radius, in metres.
---@field TerrainHeightScale number The scale factor applied to raw terrain height values when constructing the surface mesh.
---@field TerrainHeightMultiplier? number The factor applied to every terrain height in the body's PQS data when its local space loads. Defaults to 1.
---@field TimeWarpAltitudeOffset number The additional altitude offset above the atmosphere or terrain height at which time warp rates above 4x become permitted.
---@field SphereOfInfluenceCalculationType integer The integer code identifying which sphere-of-influence calculation method applies to this body.
---@field ForcedSphereOfInfluence number The manually specified sphere-of-influence radius, in meters, that overrides the dynamically calculated value.
---@field hasSolarRotationPeriod boolean A value indicating whether this body's rotation period is derived from its solar orbit rather than a fixed sidereal period.
---@field hasInverseRotationThresholdClamp boolean A value indicating whether the inverse-rotation threshold altitude is clamped to a safe range.
---@field hasInverseRotation boolean A value indicating whether vessels above a threshold altitude experience an inverted surface-rotation reference frame.
---@field isRotating boolean A value indicating whether this body rotates on its axis over time.
---@field isTidallyLocked boolean A value indicating whether this body's rotation period is locked to its orbital period around its parent.
---@field inverseRotThresholdAltitude number The altitude above sea level, in metres, below which the inverse-rotation reference frame is not applied.
---@field initialRotation number The initial rotation angle of the body at epoch time zero, in degrees.
---@field rotationPeriod number The sidereal rotation period of the body, in seconds.
---@field axialTilt Vector3d The axial tilt of the body expressed as Euler angles, in degrees.
---@field hasAtmosphere boolean A value indicating whether this body has an atmosphere.
---@field atmosphereContainsOxygen boolean A value indicating whether the atmosphere contains free oxygen, enabling air-breathing engines.
---@field atmosphereDepth number The altitude above sea level, in metres, at which the atmosphere ends.
---@field atmosphereTemperatureSeaLevel number The atmospheric temperature at sea level, in Kelvin.
---@field atmospherePressureSeaLevel number The atmospheric pressure at sea level, in kilopascals.
---@field atmosphereMolarMass number The mean molar mass of the atmospheric gas mixture, in kg/mol.
---@field atmosphereAdiabaticIndex number The adiabatic index (ratio of specific heats) of the atmospheric gas mixture.
---@field reentryHarshness number The multiplier on reentry heating in this body's atmosphere, 1 for Kerbin.
---@field atmosphericReentryVFXGradient string The asset key of the color gradient used for the atmospheric re-entry visual effect.
---@field useAtmospherePressureCurve boolean A value indicating whether atmospheric pressure is determined by atmospherePressureCurve rather than the analytic formula.
---@field useAtmosphereTemperatureCurve boolean A value indicating whether atmospheric temperature is determined by BodyAltitudeTemperatureCurve rather than the analytic formula.
---@field useAtmosphereHumidityCurve boolean A value indicating whether relative humidity is determined by BodyAltitudeRelativeHumidityCurve rather than a constant.
---@field atmospherePressureCurve FloatCurve The curve mapping altitude to atmospheric pressure, used when useAtmospherePressureCurve is true.
---@field BodyAltitudeTemperatureCurve FloatCurve The curve mapping altitude to atmospheric temperature, used when useAtmosphereTemperatureCurve is true.
---@field BodyAltitudeSurfaceFluxCurve FloatCurve The curve mapping altitude to the solar flux received at the body's surface, used for thermal calculations.
---@field BodyAltitudeFluxCurve FloatCurve The curve mapping altitude to the ambient thermal flux from the body, used for thermal calculations.
---@field BodyAltitudeRelativeHumidityCurve FloatCurve The curve mapping altitude to relative humidity, used when useAtmosphereHumidityCurve is true.
---@field BodySurfaceFluxMapPath string The asset path of the texture map encoding surface thermal flux values per geographic location.
---@field BodySurfaceFluxScale number The scalar multiplier applied to values sampled from the surface flux map.
---@field StarLuminosity number The total luminosity of the star, in watts, used to compute solar flux at orbiting bodies.
---@field ringGroupData JsonList<CelestialBodyRingData> The list of ring group definitions that describe the body's planetary ring system.
---@field MineDustColor Vector4 The RGBA color of the dust particle effect spawned when a vessel mines the body's surface.
---@field LocalSimObjectsData JsonList<SerializedPredefinedSimObject> The serialized representations of predefined simulation objects located in the body's local space, used for JSON persistence.
---@field LocalColonyObjectsData JsonList<SerializedPredefinedColonyObject> The serialized representations of predefined colony objects located in the body's local space, used for JSON persistence.

---@alias CelestialBodyData _CelestialBodyData | { bodyName: string, Layer?: string, assetKeyScaled: string, assetKeySimulation: string, bodyDisplayName: string, bodyDescription: string, isStar: boolean, isHomeWorld: boolean, navballSwitchAltitudeHigh: number, navballSwitchAltitudeLow: number, hasSolidSurface: boolean, hasOcean: boolean, HasLocalSpace: boolean, radius: number, gravityASL: number, oceanAltitude: number, oceanDensity: number, MinTerrainHeight: number, MaxTerrainHeight: number, TerrainHeightScale: number, TerrainHeightMultiplier?: number, TimeWarpAltitudeOffset: number, SphereOfInfluenceCalculationType: integer, ForcedSphereOfInfluence: number, hasSolarRotationPeriod: boolean, hasInverseRotationThresholdClamp: boolean, hasInverseRotation: boolean, isRotating: boolean, isTidallyLocked: boolean, inverseRotThresholdAltitude: number, initialRotation: number, rotationPeriod: number, axialTilt: Vector3d, hasAtmosphere: boolean, atmosphereContainsOxygen: boolean, atmosphereDepth: number, atmosphereTemperatureSeaLevel: number, atmospherePressureSeaLevel: number, atmosphereMolarMass: number, atmosphereAdiabaticIndex: number, reentryHarshness: number, atmosphericReentryVFXGradient: string, useAtmospherePressureCurve: boolean, useAtmosphereTemperatureCurve: boolean, useAtmosphereHumidityCurve: boolean, atmospherePressureCurve: FloatCurve, BodyAltitudeTemperatureCurve: FloatCurve, BodyAltitudeSurfaceFluxCurve: FloatCurve, BodyAltitudeFluxCurve: FloatCurve, BodyAltitudeRelativeHumidityCurve: FloatCurve, BodySurfaceFluxMapPath: string, BodySurfaceFluxScale: number, StarLuminosity: number, ringGroupData: JsonList<CelestialBodyRingData>, MineDustColor: Vector4, LocalSimObjectsData: JsonList<SerializedPredefinedSimObject>, LocalColonyObjectsData: JsonList<SerializedPredefinedColonyObject> }

---Represents the ring data for a celestial body, defining inner and outer radii and a density curve.
---@class _CelestialBodyRingData : _JsonUserDataBase
---@field innerRadius number The inner radius of the ring, measured from the center of the celestial body.
---@field outerRadius number The outer radius of the ring, measured from the center of the celestial body.
---@field density FloatCurve A curve defining the density distribution of the ring across its radial extent.

---@alias CelestialBodyRingData _CelestialBodyRingData | { innerRadius: number, outerRadius: number, density: FloatCurve }

---Represents the serialized form of a PredefinedSimObject.
---@class _SerializedPredefinedSimObject : _JsonUserDataBase
---@field Name string The name used to identify the predefined simulation object.
---@field RelativeTo string The name of the simulation object that this object's transform is relative to.
---@field ReferenceFrame TransformFrameType The transform frame type used to interpret the local position and rotation.
---@field LocalPosition Vector3d The local position of the predefined simulation object, relative to its reference frame.
---@field LocalRotation Quaternion The local rotation of the predefined simulation object, relative to its reference frame.
---@field FixedGuid boolean Flag indicating whether the simulation object uses a fixed GUID rather than a generated one.

---@alias SerializedPredefinedSimObject _SerializedPredefinedSimObject | { Name: string, RelativeTo: string, ReferenceFrame: TransformFrameType, LocalPosition: Vector3d, LocalRotation: Quaternion, FixedGuid: boolean }

---Represents the serialized state of a PredefinedColonyObject, capturing population fill, telemetry, and CommNet source configuration.
---@class _SerializedPredefinedColonyObject : _JsonUserDataBase
---@field SimObjectName string The name of the simulation object associated with this predefined colony entry.
---@field HasPopulationComponent boolean Indicates whether the colony object includes a population component.
---@field PopulationFillLimit integer The maximum number of population entries that the colony object may be filled with.
---@field PopulationFillVacancyDelay integer The delay, in seconds, before a vacated population slot becomes eligible to be refilled.
---@field PopulationFillLimitEmptySubIdOnly boolean Indicates whether the population fill limit applies only to entries with an empty sub-identifier.
---@field PopulationReuseNonVeterans boolean Indicates whether existing non-veteran kerbals may be reused when filling this colony object.
---@field PopulationReuseVeterans boolean Indicates whether existing veteran kerbals may be reused when filling this colony object.
---@field PopulationCreateNewNonVeterans boolean Indicates whether new non-veteran kerbals may be generated to fill this colony object.
---@field PopulationCreateNewVeterans boolean Indicates whether new veteran kerbals may be generated to fill this colony object.
---@field HasTelemetryComponent boolean Indicates whether the colony object includes a telemetry component.
---@field IsCommNetSource boolean Indicates whether the colony object acts as a communication network signal source.

---@alias SerializedPredefinedColonyObject _SerializedPredefinedColonyObject | { SimObjectName: string, HasPopulationComponent: boolean, PopulationFillLimit: integer, PopulationFillVacancyDelay: integer, PopulationFillLimitEmptySubIdOnly: boolean, PopulationReuseNonVeterans: boolean, PopulationReuseVeterans: boolean, PopulationCreateNewNonVeterans: boolean, PopulationCreateNewVeterans: boolean, HasTelemetryComponent: boolean, IsCommNetSource: boolean }

---Represents a serialized galaxy definition, including its name, version, and celestial bodies.
---@class _SerializedGalaxyDefinition : _JsonUserDataBase
---@field Name string Name of the galaxy definition.
---@field Version string Version string of the galaxy definition.
---@field LocalizationKey string Localization key for the display name of the galaxy definition.
---@field DescriptionLocalizationKey string Localization key for the description shown on the galaxy definition's card, or nil for none.
---@field ImageKey string Addressable key of the sprite shown on the galaxy definition's card, or nil to show the title and description only.
---@field HomeWorld SerializedHomeWorld The home world of the galaxy, and the space center placed on it.
---@field CelestialBodies JsonList<SerializedCelestialBody> List of serialized celestial bodies that make up the galaxy.

---@alias SerializedGalaxyDefinition _SerializedGalaxyDefinition | { Name: string, Version: string, LocalizationKey: string, DescriptionLocalizationKey: string, ImageKey: string, HomeWorld: SerializedHomeWorld, CelestialBodies: JsonList<SerializedCelestialBody> }

---Represents the serialized home world of a galaxy definition, and the space center placed on it.
---@class _SerializedHomeWorld : _JsonUserDataBase
---@field Body string GUID of the celestial body that is the home world.
---@field RemoveOtherSpaceCenters boolean Whether a space center on any other body is removed when that body's local space loads.
---@field SpaceCenter? SerializedSpaceCenter The space center to place on the home world, or nil to keep the one its world prefab already has.

---@alias SerializedHomeWorld _SerializedHomeWorld | { Body: string, RemoveOtherSpaceCenters: boolean, SpaceCenter?: SerializedSpaceCenter }

---Represents the serialized placement of a space center prefab on a galaxy's home world.
---@class _SerializedSpaceCenter : _JsonUserDataBase
---@field Prefab string Addressable key of the space center prefab.
---@field Latitude number Latitude of the space center's origin, in degrees.
---@field Longitude number Longitude of the space center's origin, in degrees.
---@field Altitude number Height of the space center's origin above the drawn terrain directly below it, in meters.
---@field Heading number Rotation of the space center clockwise from north, in degrees. At zero the prefab's -Z axis points north.

---@alias SerializedSpaceCenter _SerializedSpaceCenter | { Prefab: string, Latitude: number, Longitude: number, Altitude: number, Heading: number }

---Represents the serialized form of a celestial body, including its identity and orbital configuration.
---@class _SerializedCelestialBody : _JsonUserDataBase
---@field GUID string Unique identifier for this celestial body.
---@field referenceBodyGuid string Unique identifier of the reference body that this celestial body orbits.
---@field OrbitProperties SerializedOrbitProperties Serialized orbital properties describing the orbit of this celestial body.
---@field OrbiterProperties SerializedOribiterDefinition Serialized orbiter definition describing the physical and orbital characteristics of this celestial body as an orbiter.
---@field Layer? string Layer whose copy of this body's data the galaxy uses, or nil for the default copy.

---@alias SerializedCelestialBody _SerializedCelestialBody | { GUID: string, referenceBodyGuid: string, OrbitProperties: SerializedOrbitProperties, OrbiterProperties: SerializedOribiterDefinition, Layer?: string }

---Represents a serializable set of Keplerian orbital elements and the reference body for an orbit.
---@class _SerializedOrbitProperties : _JsonUserDataBase
---@field referenceBodyGuid string GUID identifying the celestial body used as the gravitational reference for this orbit.
---@field inclination number Orbital inclination in degrees, measured relative to the reference plane.
---@field eccentricity number Orbital eccentricity describing the shape of the orbit (0 is circular, less than 1 is elliptical).
---@field semiMajorAxis number Semi-major axis of the orbit in meters, equal to half the longest diameter of the orbital ellipse.
---@field longitudeOfAscendingNode number Longitude of the ascending node in degrees, defining the orbit's orientation relative to the vernal direction.
---@field argumentOfPeriapsis number Argument of periapsis in degrees, measuring the angle from the ascending node to the periapsis direction in the orbital plane.
---@field meanAnomalyAtEpoch number Mean anomaly at the reference epoch in radians, defining the body's position along the orbit at the epoch time.
---@field epoch number Reference time in universal seconds at which meanAnomalyAtEpoch is defined.

---@alias SerializedOrbitProperties _SerializedOrbitProperties | { referenceBodyGuid: string, inclination: number, eccentricity: number, semiMajorAxis: number, longitudeOfAscendingNode: number, argumentOfPeriapsis: number, meanAnomalyAtEpoch: number, epoch: number }

---Represents the serialized display configuration for an orbiter, including orbit and node colors, camera-to-semi-major-axis visibility ratios, and texture offset settings.
---@class _SerializedOribiterDefinition : _JsonUserDataBase
---@field orbitColor Color The color used to render the orbit line for this orbiter.
---@field nodeColor Color The color used to render maneuver and orbit nodes for this orbiter.
---@field lowerCamVsSmaRatio number The lower threshold of the camera-distance-to-semi-major-axis ratio below which the orbit renderer becomes visible.
---@field upperCamVsSmaRatio number The upper threshold of the camera-distance-to-semi-major-axis ratio above which the orbit renderer is hidden.
---@field autoTextureOffset boolean A value indicating whether the orbit texture offset is computed automatically based on orbital parameters.
---@field textureOffset number The explicit texture offset applied along the orbit line when autoTextureOffset is false.

---@alias SerializedOribiterDefinition _SerializedOribiterDefinition | { orbitColor: Color, nodeColor: Color, lowerCamVsSmaRatio: number, upperCamVsSmaRatio: number, autoTextureOffset: boolean, textureOffset: number }
