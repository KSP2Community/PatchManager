using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Resources.UserData;

/// <summary>
/// Plain resource definition wrapper exposing the inner <c>data</c> subtree while preserving the full envelope for round-tripping.
/// </summary>
[MoonSharpUserData]
public class ResourceUserData : JsonUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>data</c> subtree.
    /// </summary>
    [MoonSharpHidden] public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the resource envelope, exposing the inner <c>data</c> subtree to patch scripts.
    /// </summary>
    /// <param name="token">The resource JSON envelope (containing a <c>data</c> object).</param>
    public ResourceUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
    }
}
