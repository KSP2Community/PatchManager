using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// A callback a mod registers to run at the start or end of each feature set's patch run.
    /// </summary>
    /// <remarks>
    /// Mod bodies run once, so state a script keeps between patches carries over from one set's run to the next.
    /// A begin callback is where a mod resets that state. Like a patch, it needs its mod's default feature.
    /// </remarks>
    [MoonSharpUserData]
    public class RunCallback
    {
        /// <summary>
        /// The ID of the mod that registered the callback.
        /// </summary>
        [MoonSharpHidden] public string ModId;

        /// <summary>
        /// The callback, handed the set being built.
        /// </summary>
        [MoonSharpHidden] public Action<RunFeatures> Callback;

        /// <summary>
        /// The qualified features that must be on for the callback to fire, beyond its mod's default feature.
        /// </summary>
        [MoonSharpHidden] public HashSet<string> NeedsFeatures = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The qualified features that must be off for the callback to fire.
        /// </summary>
        [MoonSharpHidden] public HashSet<string> ConflictsFeatures = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Fires the callback only when these features are on.
        /// </summary>
        /// <param name="ids">The feature IDs, namespaced to the mod when they do not already carry a namespace.</param>
        /// <returns>The callback for chaining.</returns>
        public RunCallback Feature(params string[] ids) => this;

        /// <summary>
        /// Fires the callback only when these features are off.
        /// </summary>
        /// <param name="ids">The feature IDs, namespaced to the mod when they do not already carry a namespace.</param>
        /// <returns>The callback for chaining.</returns>
        public RunCallback NoFeature(params string[] ids) => this;
    }

    /// <summary>
    /// The read-only feature set a patch run is building, as run callbacks see it.
    /// </summary>
    [MoonSharpUserData]
    public class RunFeatures
    {
        private readonly ISet<string> _enabled;

        /// <summary>
        /// Wraps a set of enabled feature IDs.
        /// </summary>
        /// <param name="enabled">The enabled qualified feature IDs.</param>
        public RunFeatures(ISet<string> enabled)
        {
            _enabled = enabled;
        }

        /// <summary>
        /// Gets the enabled feature IDs.
        /// </summary>
        public IEnumerable<string> Ids => _enabled;

        /// <summary>
        /// Checks whether a feature is on in this run.
        /// </summary>
        /// <param name="id">The qualified feature ID.</param>
        /// <returns>True if the feature is on, false otherwise.</returns>
        public bool Has(string id) => _enabled.Contains(id);
    }
}
