using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Resources.UserData;

[MoonSharpUserData]
public class RecipeUserData : ExtensibleJsonUserData
{
    public JToken FullToken;
    public RecipeUserData(JToken token) : base(token["recipeData"])
    {
        FullToken = token;
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "ingredients";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "ingredients")
        {
            return MoonSharp.Interpreter.UserData.Create(new IngredientsUserData((JArray)Token["ingredients"]));
        }
        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "ingredients")
            throw new Exception("You must instead use the methods on the Ingredients array.");
        return false;
    }

    public override bool TryToRemove(string property)
    {
        throw new Exception("You cannot remove this property.");
    }
}