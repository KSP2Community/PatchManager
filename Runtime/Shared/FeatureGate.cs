using System;
using System.Collections.Generic;
using SpaceWarp2.API.Features;

namespace PatchManager.Shared
{
    /// <summary>
    /// Decides whether a patch, created asset or run callback takes part in the feature set being built.
    /// </summary>
    /// <remarks>
    /// Everything a mod registers needs that mod's default feature. Redux is the exception: its patches carry no
    /// implicit feature and each states the feature it belongs to.
    /// </remarks>
    public static class FeatureGate
    {
        /// <summary>
        /// The mod whose registrations carry no implicit default feature.
        /// </summary>
        public const string EXPLICIT_MOD_ID = "Ksp2Redux";

        /// <summary>
        /// The umbrella feature the main menu set starts from.
        /// </summary>
        public const string MAIN_MENU_FEATURE = "Ksp2Redux:All";

        // Packs and game modes are chosen before a set exists, so every set sees all of them
        private static readonly HashSet<string> GlobalLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            "campaign_pack",
            "game_mode"
        };

        /// <summary>
        /// Checks whether a label's content is the same in every feature set, so nothing in it is gated.
        /// </summary>
        /// <param name="label">The addressables label.</param>
        /// <returns>True if the label is global, false otherwise.</returns>
        public static bool IsGlobalLabel(string label) => label != null && GlobalLabels.Contains(label);

        /// <summary>
        /// Checks a registration against the enabled features.
        /// </summary>
        /// <param name="modId">The registering mod.</param>
        /// <param name="needs">The qualified features that must be on.</param>
        /// <param name="conflicts">The qualified features that must be off.</param>
        /// <param name="enabled">The enabled features, or <c>null</c> to let everything through.</param>
        /// <param name="reason">The first unmet requirement, when the registration is dropped.</param>
        /// <returns>True if the registration takes part, false otherwise.</returns>
        public static bool Allows(
            string modId,
            IEnumerable<string> needs,
            IEnumerable<string> conflicts,
            ISet<string> enabled,
            out string reason
        )
        {
            reason = null;
            if (enabled == null)
                return true;

            if (
                !string.IsNullOrEmpty(modId)
                && !string.Equals(modId, EXPLICIT_MOD_ID, StringComparison.OrdinalIgnoreCase)
                && !enabled.Contains(FeatureIds.DefaultFor(modId))
            )
            {
                reason = FeatureIds.DefaultFor(modId);
                return false;
            }

            foreach (string needed in needs)
            {
                if (enabled.Contains(needed))
                    continue;

                reason = needed;
                return false;
            }

            foreach (string conflicting in conflicts)
            {
                if (!enabled.Contains(conflicting))
                    continue;

                reason = $"not {conflicting}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds the set the main menu and a fresh boot run with: the umbrella main menu feature plus every
        /// installed mod's default feature, closed over their requirements.
        /// </summary>
        /// <param name="registry">The feature registry.</param>
        /// <param name="installedModIds">The ids of the installed, enabled mods.</param>
        /// <returns>The enabled feature ids.</returns>
        public static IReadOnlyCollection<string> BootSet(
            FeatureRegistry registry,
            IEnumerable<string> installedModIds
        )
        {
            var requested = new List<string> { MAIN_MENU_FEATURE };
            foreach (string modId in installedModIds)
            {
                requested.Add(FeatureIds.DefaultFor(modId));
            }

            return registry.Resolve(requested).Enabled;
        }
    }
}
