using System.Collections.Generic;
using JetBrains.Annotations;
using KSP.VolumeCloud;
using Shoemaker.Overrides;

namespace Shoemaker
{
    [PublicAPI]
    public static class OverrideManager
    {
        public static Dictionary<string, AtmosphereOverride> AtmosphereOverrides = new();
        public static Dictionary<string, double> Scales = new();
        public static Dictionary<string, VolumeCloudConfigurationOverride> VolumeCloudOverrides = new();
    
    }
}

