using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Planets.UserData;

/// <summary>
/// JSON UserData wrapping a celestial body's <c>data</c> subtree while preserving the full envelope for round-tripping.
/// </summary>
/// <remarks>
/// Patches operate on the inner <c>data</c> object as the wrapped <see cref="JsonUserData.Token" />, but the
/// converter serializes <see cref="FullToken" /> back so the wrapping JSON envelope is preserved.
/// </remarks>
[MoonSharpUserData]
public class CelestialBodyUserData : JsonUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>data</c> subtree.
    /// </summary>
    [MoonSharpHidden] public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the celestial body envelope, exposing the inner <c>data</c> subtree to patch scripts.
    /// </summary>
    /// <param name="token">The celestial body JSON envelope (containing a <c>data</c> object).</param>
    public CelestialBodyUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
    }
}
