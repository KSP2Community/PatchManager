using JetBrains.Annotations;
using KSP.Game;
using KSP.Rendering;
using UnityEngine;

namespace Shoemaker.Overrides
{
    [PublicAPI]
    [UsedImplicitly]
    public struct AtmosphereOverride : IOverride<AtmosphereModel>
    {
        public string PlanetName;
        public bool? IsGasGiant;
        public Vector2? Exposure;
        public float? SunAngleRadius;
        public float? SunZenithAngle;
        public Vector3? SolarIrradiance;
        public float? SunDirectionExposureModifier;
        public float? TransmittanceTint;
        public float? NoonColorStrength;
        public float? SunsetColorStrength;
        public float? ColorTransitionScale;
        public float? BottomRadius;
        public float? AtmosphereHeight;
        public Color? GroundAlbedo;
        public Vector3? RayleighScattering;
        public float? RayleighScatteringScale;
        public float? RayleighExponentialDistribution;
        public Vector3? MieScattering;
        public float? MieScatteringScale;
        public float? MieAnistropy;
        public float? MieExponentialDistribution;
        public float? AbsorptionScale;
        public Vector3? Absorption;
        public float? AbsorptionMaxDensity;
        public Vector2? AbsorptionHeightMinMax;
        [CanBeNull] public string TransmittanceTexture;
        [CanBeNull] public string IrradianceTexture;
        [CanBeNull] public string ScatteringTexture;

        public void ApplyTo(AtmosphereModel atmosphereModel)
        {
            if (IsGasGiant.HasValue) atmosphereModel.IsGasGiant = IsGasGiant.Value;
            if (Exposure.HasValue) atmosphereModel.Exposure = Exposure.Value;
            if (SunAngleRadius.HasValue) atmosphereModel.SunAngleRadius = SunAngleRadius.Value;
            if (SunZenithAngle.HasValue) atmosphereModel.SunZenithAngle = SunZenithAngle.Value;
            if (SolarIrradiance.HasValue) atmosphereModel.SolarIrradiance = SolarIrradiance.Value;
            if (SunDirectionExposureModifier.HasValue)
                atmosphereModel.SunDirectionExposureModifier = SunDirectionExposureModifier.Value;
            if (TransmittanceTint.HasValue) atmosphereModel.TransmittanceTint = TransmittanceTint.Value;
            if (NoonColorStrength.HasValue) atmosphereModel.NoonColorStrengh = NoonColorStrength.Value;
            if (SunsetColorStrength.HasValue) atmosphereModel.SunsetColorStrengh = SunsetColorStrength.Value;
            if (ColorTransitionScale.HasValue) atmosphereModel.ColorTransitionScale = ColorTransitionScale.Value;
            if (BottomRadius.HasValue) atmosphereModel.BottomRadius = BottomRadius.Value;
            if (AtmosphereHeight.HasValue) atmosphereModel.AtmosphereHeight = AtmosphereHeight.Value;
            if (GroundAlbedo.HasValue) atmosphereModel.GroundAlbedo = GroundAlbedo.Value;
            if (RayleighScattering.HasValue) atmosphereModel.RayleighScattering = RayleighScattering.Value;
            if (RayleighScatteringScale.HasValue) atmosphereModel.RayleighScatteringScale = RayleighScatteringScale.Value;
            if (RayleighExponentialDistribution.HasValue)
                atmosphereModel.RayleighExponentialDistribution = RayleighExponentialDistribution.Value;
            if (MieScattering.HasValue) atmosphereModel.MieScattering = MieScattering.Value;
            if (MieScatteringScale.HasValue) atmosphereModel.MieScatteringScale = MieScatteringScale.Value;
            if (MieAnistropy.HasValue) atmosphereModel.MieAnisotropy = MieAnistropy.Value;
            if (MieExponentialDistribution.HasValue)
                atmosphereModel.MieExponentialDistribution = MieExponentialDistribution.Value;
            if (AbsorptionScale.HasValue) atmosphereModel.AbsorptionScale = AbsorptionScale.Value;
            if (Absorption.HasValue) atmosphereModel.Absorption = Absorption.Value;
            if (AbsorptionHeightMinMax.HasValue) atmosphereModel.AbsorptionHeightMinMax = AbsorptionHeightMinMax.Value;
            if (!string.IsNullOrEmpty(TransmittanceTexture))
            {
                GameManager.Instance.Assets.TryLoad<Texture2D>(TransmittanceTexture,
                    asset => atmosphereModel.TransmittanceTexture = asset);
            }
            if (!string.IsNullOrEmpty(IrradianceTexture))
            {
                GameManager.Instance.Assets.TryLoad<Texture2D>(IrradianceTexture,
                    asset => atmosphereModel.IrradianceTexture = asset);
            }
            if (!string.IsNullOrEmpty(ScatteringTexture))
            {
                GameManager.Instance.Assets.TryLoad<Texture3D>(ScatteringTexture,
                    asset => atmosphereModel.ScatteringTexture = asset);
            }
        }
    }
}

