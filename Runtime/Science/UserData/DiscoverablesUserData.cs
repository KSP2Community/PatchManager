using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Science.UserData;

/// <summary>
/// Indexed-list wrapper for a science region's discoverables, keyed by each entry's <c>ScienceRegionId</c>, while
/// preserving the full envelope for round-tripping.
/// </summary>
[MoonSharpUserData]
public class DiscoverablesUserData : IndexedListUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>Discoverables</c> array.
    /// </summary>
    public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the discoverables envelope, exposing its <c>Discoverables</c> array.
    /// </summary>
    /// <param name="token">The discoverables JSON envelope (containing a <c>Discoverables</c> array).</param>
    public DiscoverablesUserData(JToken token) : base((JArray)token["Discoverables"])
    {
        FullToken = token;
    }


    /// <summary>
    /// Gets the celestial body's name from the envelope's <c>BodyName</c> field.
    /// </summary>
    public string BodyName { get => FullToken["BodyName"].Value<string>(); }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["ScienceRegionId"].Value<string>();
    }
}
