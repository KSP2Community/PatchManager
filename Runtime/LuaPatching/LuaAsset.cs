using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// A new asset queued for creation by a Lua patch script.
    /// </summary>
    /// <remarks>
    /// Creation helpers return it so a script can gate the asset with <see cref="Feature" /> and
    /// <see cref="NoFeature" />. Like a patch, it also needs its mod's default feature.
    /// </remarks>
    [MoonSharpUserData]
    public class LuaAsset
    {
        /// <summary>
        /// The converter currently producing JSON for <see cref="CurrentValue" />. Replaced when a chained patch
        /// switches to a different converter.
        /// </summary>
        [MoonSharpHidden] public IConverter ConverterInstance;

        /// <summary>
        /// The asset's current Lua-facing value. Initialized at creation time and replaced by each patch that
        /// runs against this asset.
        /// </summary>
        [MoonSharpHidden] public DynValue CurrentValue;

        /// <summary>
        /// The addressables label the asset is tagged with for group-based loading.
        /// </summary>
        [MoonSharpHidden] public string Label;

        /// <summary>
        /// The addressables address of the asset (globally unique).
        /// </summary>
        [MoonSharpHidden] public string Name;

        /// <summary>
        /// The ID of the mod that created the asset.
        /// </summary>
        [MoonSharpHidden] public string ModId;

        /// <summary>
        /// The qualified features that must be on for the asset to be created, beyond its mod's default feature.
        /// </summary>
        [MoonSharpHidden] public HashSet<string> NeedsFeatures = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The qualified features that must be off for the asset to be created.
        /// </summary>
        [MoonSharpHidden] public HashSet<string> ConflictsFeatures = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates the asset only when these features are on.
        /// </summary>
        /// <param name="ids">The feature IDs, namespaced to the creating mod when they do not already carry a namespace.</param>
        /// <returns>The asset for chaining.</returns>
        public LuaAsset Feature(params string[] ids)
        {
            foreach (var id in ids)
            {
                NeedsFeatures.Add(FeatureIds.Qualify(ModId, id));
            }

            return this;
        }

        /// <summary>
        /// Creates the asset only when these features are off.
        /// </summary>
        /// <param name="ids">The feature IDs, namespaced to the creating mod when they do not already carry a namespace.</param>
        /// <returns>The asset for chaining.</returns>
        public LuaAsset NoFeature(params string[] ids)
        {
            foreach (var id in ids)
            {
                ConflictsFeatures.Add(FeatureIds.Qualify(ModId, id));
            }

            return this;
        }
    }
}
