using System;
using System.Collections.Generic;
using PatchManager.LuaPatching;
using PatchManager.Shared;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Collects fluent <c>PM.&lt;Domain&gt;.Patch(...)</c> registrations until PatchManager flushes them into the
    /// universe, just before patches are set up for the run.
    /// </summary>
    /// <remarks>
    /// Registering after the flush throws - it is too late to take part in the patch flow.
    /// </remarks>
    internal static class FluentPatchRegistry
    {
        private static readonly List<PatchDefinition> Pending = new();
        private static bool _closed;

        /// <summary>
        /// Builds a patch for the given converter and label, queues it for registration, and returns it for chaining.
        /// </summary>
        /// <param name="modId">The mod ID the patch is namespaced under.</param>
        /// <param name="converter">The name of the converter that produces the asset wrapper.</param>
        /// <param name="label">The label selecting which assets the patch targets.</param>
        /// <param name="name">The patch name, unique within the mod ID.</param>
        /// <returns>The queued patch, for fluent chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when registration has already closed, or when the converter name is unknown.
        /// </exception>
        public static PatchDefinition Build(string modId, string converter, string label, string name)
        {
            if (_closed)
            {
                throw new InvalidOperationException(
                    $"Fluent C# patch '{modId}:{name}' was registered too late. C# fluent patches must be " +
                    "registered during pre-initialization (for example a mod's OnPreInitialized), before " +
                    "PatchManager runs its patch flow.");
            }

            if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
            {
                throw new InvalidOperationException($"Unknown converter '{converter}' for fluent patch '{name}'.");
            }

            var patch = new PatchDefinition
            {
                ConverterInstance = converterInstance,
                Label = label,
                Name = modId + ':' + name,
                PatchModId = modId
            };
            Pending.Add(patch);
            return patch;
        }

        /// <summary>
        /// Flushes every queued fluent patch into <paramref name="universe" /> and closes registration.
        /// </summary>
        /// <remarks>
        /// Called once by PatchManager, just before SetupPatchesForRun.
        /// </remarks>
        /// <param name="universe">The universe to add the queued patches to.</param>
        public static void Flush(Universe universe)
        {
            _closed = true;
            foreach (var patch in Pending)
            {
                universe.AddPatch(patch);
            }

            if (Pending.Count > 0)
            {
                Logging.LogInfo($"Flushed {Pending.Count} fluent C# patch(es)");
            }

            Pending.Clear();
        }
    }
}
