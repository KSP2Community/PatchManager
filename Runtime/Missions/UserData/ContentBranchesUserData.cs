using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

/// <summary>
/// Indexed-list wrapper for a mission's <c>ContentBranches</c> array, keyed by each branch's <c>ID</c>.
/// </summary>
[MoonSharpUserData]
public class ContentBranchesUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the content-branches array.
    /// </summary>
    /// <param name="token">The <c>ContentBranches</c> JSON array.</param>
    public ContentBranchesUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return RequireString(source["ID"], "ContentBranches[].ID");
    }
}
