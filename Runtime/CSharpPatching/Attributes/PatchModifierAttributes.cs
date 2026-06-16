using System;
using System.Reflection;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;
using PatchManager.Shared;

namespace PatchManager.CSharpPatching.Attributes
{
    /// <summary>
    /// An attribute that contributes ordering or targeting to the patch it decorates, mirroring one of the
    /// PatchDefinition fluent methods. The discovery applies every modifier on a patch method to its PatchDefinition.
    /// </summary>
    public interface IPatchModifier
    {
        /// <summary>
        /// Applies this modifier to <paramref name="patch" />. <paramref name="owner" /> is the [PMPatch] class
        /// instance, used to resolve method-name references.
        /// </summary>
        void Apply(PatchDefinition patch, object owner);
    }

    /// <summary>Runs the patch in the Early pass.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EarlyAttribute : Attribute, IPatchModifier
    {
        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Early();
    }

    /// <summary>Runs the patch in the Late pass.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LateAttribute : Attribute, IPatchModifier
    {
        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Late();
    }

    /// <summary>Runs the patch before Default and Last patches in its pass.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FirstAttribute : Attribute, IPatchModifier
    {
        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.First();
    }

    /// <summary>Runs the patch after First and Default patches in its pass.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LastAttribute : Attribute, IPatchModifier
    {
        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Last();
    }

    /// <summary>Targets only the assets with these names. Supports * and ? wildcards.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NamedAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _names;

        /// <param name="names">The asset names to target.</param>
        public NamedAttribute(params string[] names) => _names = names;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Named(_names);
    }

    /// <summary>Rejects the assets with these names. Supports * and ? wildcards.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NotNamedAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _names;

        /// <param name="names">The asset names to reject.</param>
        public NotNamedAttribute(params string[] names) => _names = names;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.NotNamed(_names);
    }

    /// <summary>Requires these mod IDs to be loaded.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NeedsAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The required mod IDs.</param>
        public NeedsAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Needs(_ids);
    }

    /// <summary>Refuses to run alongside these mod IDs.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ConflictsAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The conflicting mod IDs.</param>
        public ConflictsAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Conflicts(_ids);
    }

    /// <summary>Requires these other patches to run.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NeedsPatchAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The required patch IDs.</param>
        public NeedsPatchAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.NeedsPatch(_ids);
    }

    /// <summary>Refuses to run alongside these patches.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ConflictsPatchAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The conflicting patch IDs.</param>
        public ConflictsPatchAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.ConflictsPatch(_ids);
    }

    /// <summary>Runs after every patch from these mods.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AfterAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The mod IDs whose patches this runs after.</param>
        public AfterAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.After(_ids);
    }

    /// <summary>Runs after these patches when present.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AfterPatchAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The patch IDs to run after.</param>
        public AfterPatchAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.AfterPatch(_ids);
    }

    /// <summary>Runs before every patch from these mods.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class BeforeAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The mod IDs whose patches this runs before.</param>
        public BeforeAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.Before(_ids);
    }

    /// <summary>Runs before these patches when present.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class BeforePatchAttribute : Attribute, IPatchModifier
    {
        private readonly string[] _ids;

        /// <param name="ids">The patch IDs to run before.</param>
        public BeforePatchAttribute(params string[] ids) => _ids = ids;

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.BeforePatch(_ids);
    }

    /// <summary>
    /// Gates the patch on a predicate method declared on the same class. The method takes the asset wrapper and
    /// returns a bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequiresAttribute : Attribute, IPatchModifier
    {
        private readonly string _predicateMethod;
        private readonly string _message;

        /// <param name="predicateMethod">The name of a sibling method used as the predicate, via nameof.</param>
        /// <param name="message">Optional message logged when the predicate rejects an asset.</param>
        public RequiresAttribute(string predicateMethod, string message = null)
        {
            _predicateMethod = predicateMethod;
            _message = message;
        }

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner)
        {
            var mi = owner.GetType().GetMethod(_predicateMethod,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (mi == null)
            {
                Logging.LogError($"[Requires] could not find predicate method '{_predicateMethod}' on {owner.GetType().Name}");
                return;
            }

            var target = mi.IsStatic ? null : owner;
            patch.Requires(dv => (bool)mi.Invoke(target, new[] { dv.UserData?.Object }), _message);
        }
    }

    /// <summary>
    /// Requires the asset to expose a key, optionally with a sibling predicate method run against the value at that key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HasAttribute : Attribute, IPatchModifier
    {
        private readonly string _key;
        private readonly string _predicateMethod;

        /// <summary>Optional message logged when the requirement fails.</summary>
        public string Message { get; set; }

        /// <param name="key">The key the asset must expose.</param>
        public HasAttribute(string key) => _key = key;

        /// <param name="key">The key the asset must expose.</param>
        /// <param name="predicateMethod">A sibling method run against the value at the key, by nameof.</param>
        public HasAttribute(string key, string predicateMethod)
        {
            _key = key;
            _predicateMethod = predicateMethod;
        }

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner)
        {
            if (_predicateMethod == null)
            {
                patch.Has(_key, null, Message);
                return;
            }

            var mi = owner.GetType().GetMethod(_predicateMethod,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (mi == null)
            {
                Logging.LogError($"[Has] could not find predicate method '{_predicateMethod}' on {owner.GetType().Name}");
                return;
            }

            var target = mi.IsStatic ? null : owner;
            patch.Has(_key, dv => (bool)mi.Invoke(target, new[] { AsJson(dv) }), Message);
        }

        private static JsonUserData AsJson(DynValue dv) =>
            dv.UserData?.Object as JsonUserData ?? new JsonUserData(JsonUserData.GetJTokenForDynValue(dv));
    }

    /// <summary>Requires the asset to not expose a key.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HasNoAttribute : Attribute, IPatchModifier
    {
        private readonly string _key;
        private readonly string _message;

        /// <param name="key">The key the asset must not expose.</param>
        /// <param name="message">Optional message logged when the key is present.</param>
        public HasNoAttribute(string key, string message = null)
        {
            _key = key;
            _message = message;
        }

        /// <inheritdoc />
        public void Apply(PatchDefinition patch, object owner) => patch.HasNo(_key, _message);
    }
}
