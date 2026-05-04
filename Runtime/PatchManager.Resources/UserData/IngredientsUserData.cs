using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Resources.UserData;

[MoonSharpUserData]
public class IngredientsUserData : IndexedListUserData
{
    public IngredientsUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["name"].Value<string>();
    }

    public void Add(Script script, string name, string unitsPerRecipeUnit)
    {
        Append(DynValue.NewTable(new Table(script)
        {
            ["name"] = DynValue.NewString(name),
            ["unitsPerRecipeUnit"] = DynValue.NewString(unitsPerRecipeUnit)
        }));
    }
}