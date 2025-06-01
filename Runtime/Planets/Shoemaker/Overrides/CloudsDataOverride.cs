// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using JetBrains.Annotations;
using KSP.VolumeCloud;
using UnityEngine;

namespace Shoemaker.Overrides
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
    
        public void ApplyTo(VolumeCloudConfiguration.CloudsData obj)
        {
            isEnable.Apply(value => obj.isEnable = value ?? false);
            castShadow.Apply(value => obj.castShadow = value ?? false);
            bakeCloudMipmap.Apply(value => obj.bakeCloudMipmap = value ?? 0);
            currentBakedCloudMipMap.Apply(value => obj.currentBakedCloudMipmap = value ?? 0);
            cloudsType.Apply(value => obj.cloudsType = value ?? VolumeCloudConfiguration.CloudsLayerType.Cumulus);
            cloudHeightRange.Apply(value => obj.cloudHeightRange  = value ?? Vector2.zero);
            bakedCloudHeight.Apply(value => obj.bakedCloudHeight = value ?? 0);
            cloudsLayerRotate.Apply(value => obj.cloudsLayerRotate  = value ?? Vector3.zero);
            enableWind.Apply(value => obj.enableWind = value ?? false);
            windDirection.Apply(value => obj.windDirection = value ?? Vector2.zero);
            movementSpeed.Apply(value => obj.movementSpeed  = value ?? 0f);
            evolveSpeed.Apply(value => obj.evolveSpeed  = value ?? 0f);
            topOffset.Apply(value => obj.topOffset = value ?? 0f);
            isFold.Apply(value => obj.isFold = value ?? false);
        }
    }
}

