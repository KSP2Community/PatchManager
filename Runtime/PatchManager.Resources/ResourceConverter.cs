using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Resources.UserData;

namespace PatchManager.Resources;

[Converter("Resource")]
public class ResourceConverter : IConverter
{
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        var obj = (JObject)json;
        if (obj.ContainsKey("isRecipe") && obj["isRecipe"]!.Value<bool>())
        {
            return MoonSharp.Interpreter.UserData.Create(new RecipeUserData(obj));
        }
        return  MoonSharp.Interpreter.UserData.Create(new ResourceUserData(obj));
    }

    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        if (value.UserData.Object is RecipeUserData recipeUserData)
        {
            return recipeUserData.FullToken.ToString(Formatting.Indented);
        }
        else if (value.UserData.Object is ResourceUserData resourceUserData)
        {
            return resourceUserData.FullToken.ToString(Formatting.Indented);
        }
        else
        {
            return null;
        }
    }
}