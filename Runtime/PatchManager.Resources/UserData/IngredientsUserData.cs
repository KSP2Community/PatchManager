using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Resources.UserData;

/// <summary>
/// Indexed-list wrapper for a recipe's <c>ingredients</c> array, keyed by each ingredient's <c>name</c>.
/// </summary>
[MoonSharpUserData]
public class IngredientsUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the ingredients array.
    /// </summary>
    /// <param name="token">The <c>ingredients</c> JSON array.</param>
    public IngredientsUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["name"].Value<string>();
    }

    /// <summary>
    /// Adds a new ingredient with the given name and units-per-recipe-unit ratio.
    /// </summary>
    /// <param name="script">The active script (used to construct the underlying Lua table).</param>
    /// <param name="name">The ingredient's resource name.</param>
    /// <param name="unitsPerRecipeUnit">How many units of the ingredient are consumed per unit of recipe output.</param>
    public void Add(Script script, string name, double unitsPerRecipeUnit)
    {
        Append(DynValue.NewTable(new Table(script)
        {
            ["name"] = DynValue.NewString(name),
            ["unitsPerRecipeUnit"] = DynValue.NewNumber(unitsPerRecipeUnit)
        }));
    }
}
