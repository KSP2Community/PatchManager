using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Resources.UserData;

namespace PatchManager.Resources;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Resource"</c>; routes JSON to a typed <see cref="RecipeUserData" />
/// or <see cref="ResourceUserData" /> based on whether the asset's <c>isRecipe</c> flag is set.
/// </summary>
[Converter("Resource")]
public class ResourceConverter : IConverter
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        if (value.UserData.Object is RecipeUserData recipeUserData)
        {
            return recipeUserData.FullToken;
        }
        if (value.UserData.Object is ResourceUserData resourceUserData)
        {
            return resourceUserData.FullToken;
        }
        throw new System.Exception($"ResourceConverter.ToJson: unsupported value type {value.UserData.Object?.GetType()}");
    }
}
