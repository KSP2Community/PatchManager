using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Science.UserData;

/// <summary>
/// Indexed-list wrapper for a body's science regions, keyed by each region's <c>id</c>, while preserving the full
/// envelope for round-tripping and exposing the body name and situation data as typed properties.
/// </summary>
[MoonSharpUserData]
public class ScienceRegionsUserData : IndexedListUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>Regions</c> array.
    /// </summary>
    [MoonSharpHidden] public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the science regions envelope, exposing its <c>Regions</c> array.
    /// </summary>
    /// <param name="token">The science regions JSON envelope (containing a <c>Regions</c> array).</param>
    public ScienceRegionsUserData(JToken token) : base(RequireArray(token["Regions"], "Regions"))
    {
        FullToken = token;
    }


    /// <summary>
    /// Gets the celestial body's name from the envelope's <c>BodyName</c> field.
    /// </summary>
    public string BodyName { get => RequireString(FullToken["BodyName"], "BodyName"); }

    /// <summary>
    /// Gets or sets the situation data wrapping the envelope's <c>SituationData</c> field.
    /// </summary>
    public DynValue SituationData {
        get => GetFromJToken(FullToken["SituationData"]);
        set => FullToken["SituationData"] = GetJTokenForDynValue(value);
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return RequireString(source["Id"], "Regions[].Id");
    }
}
