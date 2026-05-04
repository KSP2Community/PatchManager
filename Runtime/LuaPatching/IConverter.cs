using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// Bidirectional converter between a JSON token and the Lua-facing <see cref="DynValue" /> exposed to patch scripts.
    /// </summary>
    /// <remarks>
    /// Each addressables label that PatchManager handles is associated with a converter. The converter produces the
    /// value patch scripts receive, and serializes the edited value back to JSON so the patched asset can be written
    /// to the cache. The produced value can be any UserData object the converter chooses; wrapping the source
    /// <see cref="JToken" /> directly (typically via a <see cref="JsonUserData" /> subclass) is the recommended
    /// pattern but not required. Implementations are registered with the patch universe at module init time.
    /// </remarks>
    public interface IConverter
    {
        /// <summary>
        /// Produces the Lua-facing value patch functions receive for a single asset.
        /// </summary>
        /// <remarks>
        /// Implementations must return <see cref="DynValue.Nil" /> when <paramref name="json" /> is <c>null</c>,
        /// to round-trip cleanly with <see cref="ToJson" />.
        /// </remarks>
        /// <param name="json">The asset's parsed JSON.</param>
        /// <returns>The value to hand to patch scripts.</returns>
        public DynValue FromJson(JToken json);

        /// <summary>
        /// Serializes a patched Lua value back to its JSON representation.
        /// </summary>
        /// <remarks>
        /// Implementations must return <c>null</c> when <paramref name="value" /> is <see cref="DynValue.Nil" />,
        /// to round-trip cleanly with <see cref="FromJson" />. The patching pipeline interprets a <c>null</c> result
        /// as a request to delete the asset.
        /// </remarks>
        /// <param name="value">The Lua value produced or mutated by a patch function.</param>
        /// <returns>The JSON token to write to the patched asset, or <c>null</c> to delete the asset.</returns>
        [CanBeNull]
        public JToken ToJson(DynValue value);
    }
}
