using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

/// <summary>
/// Indexed-list wrapper for a volume cloud's <c>cumulusList</c>, keyed by each layer's <c>layerName</c>.
/// </summary>
[MoonSharpUserData]
public class CloudUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the cumulus list array.
    /// </summary>
    /// <param name="token">The <c>cumulusList</c> JSON array.</param>
    public CloudUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return RequireString(source["layerName"], "cumulusList[].layerName");
    }
}
