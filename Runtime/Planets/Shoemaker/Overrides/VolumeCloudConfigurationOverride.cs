using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.VolumeCloud;
using UnityEngine;
using static Shoemaker.Utility.Extensions;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Shoemaker.Overrides
{ 
    [PublicAPI]
    public class VolumeCloudConfigurationOverride : IOverride<VolumeCloudConfiguration>
    {
        public string bodyName;
        public bool? exclusiveLayer;
        public Vector3? CloudsRotateAll;
        public float? planetRadius;
        public bool? enableColorMap;
        public bool? enableVerticalColor;
        public float? colorMapIntensity;
        public float? verticalColorIntensity;
        // [CanBeNull] public Gradient ColorVertical;
        public float? overallSize;
        public Vector2? vortexCloudHeightRange;
        // todo: lenticular list
        public List<CloudsDataOverride> cumulusList = new List<CloudsDataOverride>();
        // todo: cumulus list
        // todo: cumulus index
        public float? cloudCoverageModifier;
        public float? detailVariationRange;
        public bool? enableShadows;
        public bool? enableLayerShadows;
        public float? volumetricShadowDensity;
        public float? volumetricShadowLodBias;
        public float? volumetricShadowDistance;
        public float? shadowOpacity;
        public float? shadowMapStrength;
        public float? layerShadowDensity;
        public Color? ambientColor;
        public bool? EnableCloudGI;
        public float? cloudGIIntensity;
        public float? cloudGITint;
        public float? lightPenetrateDistance;
        public float? multiScatteringScattering;
        public float? extinctionByLightPosition;
        public float? opticsDistanceScale;
        public float? silverSpreadG;
        public float? bloomStrengthG;
        public float? silverSpreadUnderCloudG;
        public float? bloomStrengthUnderCloudG;
        public float? silverSpread;
        public float? bloomStrength;
        public float? silverSpreadUnderCloud;
        public float? bloomStrengthUnderCloud;
        public float? ambientScale;
        public float? scatteringScale;
        public float? cloudsDensityScale;
        public bool? enableGodray;
        public float? godrayIntensity;
        public float? godrayVisibleDistance;
        public float? godrayStepSize;
        public float? sampleLightStepSize;
        public float? sampleLightStepCount;
        public Vector2? cloudDensityRangeEmitGodray;
        public float? godrayAttenuation;
        public float? godrayFadeHeight;
        public float? godrayBlurSize;
        public bool? IsBlurGodray;
        public float? antiBandingAmplify;
        public bool? useScaleCloudsOnly;
        public float? raymarchStepSize;
        public bool? increaseRaymarchStepByDistance;
        public float? distanceRatio;
        public float? maxRaymarchStepSize;
        public bool? cullingEdgeClouds;
        public float? cullingStrength;
        public bool? autoMipmap;
        public float? scaleCloudMaskNormalTileRate;
        public float? cascadedResolutionRange;
        public float? mipmapScale;
        public bool? enableFadeout;
        public float? startFadeoutHeight;
        public float? endFadeoutHeight;
        
        public void ApplyTo(VolumeCloudConfiguration obj)
        {
            exclusiveLayer.Apply(ref obj.exclusiveLayer);
            CloudsRotateAll.Apply(ref obj.CloudsRotateAll);
            planetRadius.Apply(ref obj.planetRadius);
            enableColorMap.Apply(ref obj.enableColorMap);
            enableVerticalColor.Apply(ref obj.enableVerticalColor);
            colorMapIntensity.Apply(ref obj.colorMapIntensity);
            verticalColorIntensity.Apply(ref obj.verticalColorIntensity);
            overallSize.Apply(ref obj.overallSize);
            vortexCloudHeightRange.Apply(ref obj.vortexCloudHeightRange);
            foreach (var item in cumulusList)
                if (obj.cumulusList.FirstOrDefault(x => x.layerName == item.layerName) is {} first) item.ApplyTo(first);
            cloudCoverageModifier.Apply(ref obj.cloudCoverageModifier);
            detailVariationRange.Apply(ref obj.detailVariationRange);
            enableShadows.Apply(ref obj.enableShadows);
            enableLayerShadows.Apply(ref obj.enableLayerShadows);
            volumetricShadowDensity.Apply(ref obj.volumetricShadowDensity);
            volumetricShadowLodBias.Apply(ref obj.volumetricShadowLodBias);
            volumetricShadowDistance.Apply(ref obj.volumetricShadowDistance);
            shadowOpacity.Apply(ref obj.shadowOpacity);
            shadowMapStrength.Apply(ref obj.shadowMapStrength);
            layerShadowDensity.Apply(ref obj.layerShadowDensity);
            ambientColor.Apply(ref obj.ambientColor);
            EnableCloudGI.Apply(ref obj.EnableCloudGI);
            cloudGIIntensity.Apply(ref obj.cloudGIIntensity);
            cloudGITint.Apply(ref obj.cloudGITint);
            lightPenetrateDistance.Apply(ref obj.lightPenetrateDistance);
            multiScatteringScattering.Apply(ref obj.multiScatteringScattering);
            extinctionByLightPosition.Apply(ref obj.extinctionByLightPosition);
            opticsDistanceScale.Apply(ref obj.opticsDistanceScale);
            silverSpreadG.Apply(ref obj.silverSpreadG);
            bloomStrengthG.Apply(ref obj.bloomStrengthG);
            silverSpreadUnderCloudG.Apply(ref obj.silverSpreadUnderCloudG);
            bloomStrengthUnderCloudG.Apply(ref obj.bloomStrengthUnderCloudG);
            silverSpread.Apply(ref obj.silverSpread);
            bloomStrength.Apply(ref obj.bloomStrength);
            silverSpreadUnderCloud.Apply(ref obj.silverSpreadUnderCloud);
            bloomStrengthUnderCloud.Apply(ref obj.bloomStrengthUnderCloud);
            ambientScale.Apply(ref obj.ambinetScale);
            scatteringScale.Apply(ref obj.scatteringScale);
            cloudsDensityScale.Apply(ref obj.cloudsDensityScale);
            enableGodray.Apply(ref obj.enableGodray);
            godrayIntensity.Apply(ref obj.godrayIntensity);
            godrayVisibleDistance.Apply(ref obj.godrayVisableDistance);
            godrayStepSize.Apply(ref obj.godrayStepSize);
            sampleLightStepSize.Apply(ref obj.sampleLightStepSize);
            sampleLightStepCount.Apply(ref obj.sampleLightStepCount);
            cloudDensityRangeEmitGodray.Apply(ref obj.cloudDensityRangeEmitGodray);
            godrayAttenuation.Apply(ref obj.godrayAttenuation);
            godrayFadeHeight.Apply(ref obj.godrayFadeHeight);
            godrayBlurSize.Apply(ref obj.godrayBlurSize);
            IsBlurGodray.Apply(ref obj.IsBlurGodray);
            antiBandingAmplify.Apply(ref obj.antiBandingAmplify);
            useScaleCloudsOnly.Apply(ref obj.useScaleCloudsOnly);
            raymarchStepSize.Apply(ref obj.raymarchStepSize);
            increaseRaymarchStepByDistance.Apply(ref obj.increaceRaymarchStepByDistance);
            distanceRatio.Apply(ref obj.distanceRatio);
            maxRaymarchStepSize.Apply(ref obj.maxRaymarchStepSize);
            cullingEdgeClouds.Apply(ref obj.cullingEdgeClouds);
            cullingStrength.Apply(ref obj.cullingStrength);
            autoMipmap.Apply(ref obj.autoMipmap);
            scaleCloudMaskNormalTileRate.Apply(ref obj.scaleCloudMaskNormalTileRate);
            cascadedResolutionRange.Apply(ref obj.cascadedResolutionRange);
            mipmapScale.Apply(ref obj.mipmapScale);
            enableFadeout.Apply(ref obj.enableFadeout);
            startFadeoutHeight.Apply(ref obj.startFadeoutHeight);
            endFadeoutHeight.Apply(ref obj.endFadeoutHeight);
        }
    }
}

