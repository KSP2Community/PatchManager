using System;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// A type-safe fluent wrapper over a queued <see cref="PatchDefinition" />.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T" /> is the asset wrapper the patch's converter produces, for example PartUserData, so
    /// <see cref="Do(Action{T})" /> and <see cref="Requires(Func{T,bool},string)" /> take that type with no cast and
    /// no chance of a wrong-type lambda.
    /// </remarks>
    /// <typeparam name="T">The asset wrapper type the patch's converter produces.</typeparam>
    public readonly struct PatchBuilder<T>
    {
        private readonly PatchDefinition _patch;

        internal PatchBuilder(PatchDefinition patch) => _patch = patch;

        /// <summary>Runs the patch in the Early pass.</summary>
        public PatchBuilder<T> Early() { _patch.Early(); return this; }

        /// <summary>Runs the patch in the Late pass.</summary>
        public PatchBuilder<T> Late() { _patch.Late(); return this; }

        /// <summary>Runs before Default and Last patches in the same pass.</summary>
        public PatchBuilder<T> First() { _patch.First(); return this; }

        /// <summary>Runs after First and Default patches in the same pass.</summary>
        public PatchBuilder<T> Last() { _patch.Last(); return this; }

        /// <summary>Targets only the assets with these names. Supports * and ? wildcards.</summary>
        public PatchBuilder<T> Named(params string[] names) { _patch.Named(names); return this; }

        /// <summary>Rejects the assets with these names. Supports * and ? wildcards.</summary>
        public PatchBuilder<T> NotNamed(params string[] names) { _patch.NotNamed(names); return this; }

        /// <summary>Requires these mod IDs to be loaded.</summary>
        public PatchBuilder<T> Needs(params string[] ids) { _patch.Needs(ids); return this; }

        /// <summary>Refuses to run alongside these mod IDs.</summary>
        public PatchBuilder<T> Conflicts(params string[] ids) { _patch.Conflicts(ids); return this; }

        /// <summary>Requires these other patches to run.</summary>
        public PatchBuilder<T> NeedsPatch(params string[] ids) { _patch.NeedsPatch(ids); return this; }

        /// <summary>Refuses to run alongside these patches.</summary>
        public PatchBuilder<T> ConflictsPatch(params string[] ids) { _patch.ConflictsPatch(ids); return this; }

        /// <summary>Runs after every patch from these mods.</summary>
        public PatchBuilder<T> After(params string[] ids) { _patch.After(ids); return this; }

        /// <summary>Runs after these patches when present.</summary>
        public PatchBuilder<T> AfterPatch(params string[] ids) { _patch.AfterPatch(ids); return this; }

        /// <summary>Runs before every patch from these mods.</summary>
        public PatchBuilder<T> Before(params string[] ids) { _patch.Before(ids); return this; }

        /// <summary>Runs before these patches when present.</summary>
        public PatchBuilder<T> BeforePatch(params string[] ids) { _patch.BeforePatch(ids); return this; }

        /// <summary>Gates the patch on a typed predicate evaluated against each candidate asset.</summary>
        public PatchBuilder<T> Requires(Func<T, bool> predicate, string message = null)
        {
            _patch.Requires(dv => predicate((T)dv.UserData?.Object), message);
            return this;
        }

        /// <summary>Gates the patch on a constant value.</summary>
        public PatchBuilder<T> Requires(bool gate, string message = null) { _patch.Requires(gate, message); return this; }

        /// <summary>Requires the asset to expose a key.</summary>
        public PatchBuilder<T> Has(string key, string message = null) { _patch.Has(key, null, message); return this; }

        /// <summary>Requires the asset to expose a key whose value satisfies the predicate.</summary>
        public PatchBuilder<T> Has(string key, Func<JsonUserData, bool> predicate, string message = null)
        {
            _patch.Has(key, dv => predicate(JsonUserData.Wrap(dv)), message);
            return this;
        }

        /// <summary>Requires the asset to not expose a key.</summary>
        public PatchBuilder<T> HasNo(string key, string message = null) { _patch.HasNo(key, message); return this; }

        /// <summary>Sets the typed apply callback that keeps the asset.</summary>
        public void Do(Action<T> body) => _patch.Do(dv =>
        {
            body((T)dv.UserData?.Object);
            return null;
        });

        /// <summary>Sets the typed apply callback returning whether to keep or remove the asset.</summary>
        public void Do(Func<T, PatchResult> body) =>
            _patch.Do(dv => body((T)dv.UserData?.Object) == PatchResult.Remove ? "remove" : null);
    }
}
