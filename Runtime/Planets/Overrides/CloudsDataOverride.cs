// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using JetBrains.Annotations;
using KSP.VolumeCloud;
using UnityEngine;
using static PatchManager.Planets.Utility.Extensions;

namespace PatchManager.Planets.Overrides
{
    [PublicAPI]
    public class CloudsDataOverride : IOverride<VolumeCloudConfiguration.CloudsData>
    {
        public string layerName;
        public bool? isEnable;
        public bool? castShadow;
        public float? bakeCloudMipmap;
        public float? currentBakedCloudMipMap;
        public VolumeCloudConfiguration.CloudsLayerType? cloudsType;
        public Vector2? cloudHeightRange;
        public float? bakedCloudHeight;
        public Vector3? cloudsLayerRotate;
        public bool? enableWind;
        public Vector2? windDirection;
        public float? movementSpeed;
        public float? evolveSpeed;
        public float? topOffset;
        public bool? isFold;
        public float? baseTexureTile;
        public float? coverageScale;
        public float? evanish;
        public float? detailAmount;
        public float? cloudsMaskBias;
        public float? upperFalloff;
        public float? lowerFalloff;
        public float? detailAltitudeShift;
        public bool? enableDetailTexture;
        public float? detailTextureTile;
        public float? detailStrength;
        public float? cloudsDensity;
        public float? normalScale;
        public Color? scaleCloudColor = Color.white;
        
        public void ApplyTo(VolumeCloudConfiguration.CloudsData obj)
        {
            isEnable.Apply(ref obj.isEnable);
            castShadow.Apply(ref obj.castShadow);
            bakeCloudMipmap.Apply(ref obj.bakeCloudMipmap);
            currentBakedCloudMipMap.Apply(ref obj.currentBakedCloudMipmap);
            cloudsType.Apply(ref obj.cloudsType);
            cloudHeightRange.Apply(ref obj.cloudHeightRange);
            bakedCloudHeight.Apply(ref obj.bakedCloudHeight);
            cloudsLayerRotate.Apply(ref obj.cloudsLayerRotate);
            enableWind.Apply(ref obj.enableWind);
            windDirection.Apply(ref obj.windDirection);
            movementSpeed.Apply(ref obj.movementSpeed);
            evolveSpeed.Apply(ref obj.evolveSpeed);
            topOffset.Apply(ref obj.topOffset);
            isFold.Apply(ref obj.isFold);
            if (obj is VolumeCloudConfiguration.CumulusData cumulus)
            {
                baseTexureTile.Apply(ref cumulus.baseTexureTile);
                coverageScale.Apply(ref cumulus.coverageScale);
                evanish.Apply(ref cumulus.evanish);
                detailAmount.Apply(ref cumulus.detailAmount);
                cloudsMaskBias.Apply(ref cumulus.cloudsMaskBias);
                upperFalloff.Apply(ref cumulus.upperFalloff);
                lowerFalloff.Apply(ref cumulus.lowerFalloff);
                detailAltitudeShift.Apply(ref cumulus.detailAltitudeShift);
                enableDetailTexture.Apply(ref cumulus.enableDetailTexture);
                detailTextureTile.Apply(ref cumulus.detailTextureTile);
                detailStrength.Apply(ref cumulus.detailStrength);
                cloudsDensity.Apply(ref cumulus.cloudsDensity);
                normalScale.Apply(ref cumulus.normalScale);
                scaleCloudColor.Apply(ref cumulus.scaleCloudColor);
            }
        }
    }
}

