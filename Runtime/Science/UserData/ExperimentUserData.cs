using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Science.UserData;

/// <summary>
/// Science experiment wrapper exposing the inner <c>data</c> subtree while preserving the full envelope for round-tripping.
/// </summary>
[MoonSharpUserData]
public class ExperimentUserData : JsonUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>data</c> subtree.
    /// </summary>
    [MoonSharpHidden] public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the experiment envelope, exposing the inner <c>data</c> subtree to patch scripts.
    /// </summary>
    /// <param name="token">The experiment JSON envelope (containing a <c>data</c> object).</param>
    public ExperimentUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
    }
}
