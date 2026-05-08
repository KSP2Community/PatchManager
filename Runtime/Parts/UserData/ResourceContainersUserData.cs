using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Parts.UserData;

/// <summary>
/// Indexed-list wrapper for a part's <c>resourceContainers</c> array, keyed by each container's <c>name</c>.
/// </summary>
[MoonSharpUserData]
public class ResourceContainersUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the resource-containers array.
    /// </summary>
    /// <param name="token">The <c>resourceContainers</c> JSON array.</param>
    public ResourceContainersUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return RequireString(source["name"], "resourceContainers[].name");
    }

    /// <summary>
    /// Adds a new resource container with the given resource type, capacity, and optional initial fill.
    /// </summary>
    /// <param name="script">The active script (used to construct the underlying Lua table).</param>
    /// <param name="type">The resource name to store.</param>
    /// <param name="capacity">The container's capacity in units.</param>
    /// <param name="initial">The initial fill in units; defaults to 0.</param>
    /// <param name="nonStageable">Whether the container is exempt from staging; defaults to false.</param>
    public void Add(Script script, string type, double capacity, double initial = 0, bool nonStageable = false)
    {
        Append(DynValue.NewTable(new Table(script)
        {
            ["name"] = DynValue.NewString(type),
            ["capacityUnits"] = DynValue.NewNumber(capacity),
            ["initialUnits"] = DynValue.NewNumber(initial),
            ["NonStageable"] = nonStageable
        }));
    }

    /// <summary>
    /// Returns whether the part contains a resource container of the given type.
    /// </summary>
    /// <param name="type">The resource name to look up.</param>
    /// <returns>True if such a container exists, false otherwise.</returns>
    public bool Has(string type) => HasKey(type);
}
