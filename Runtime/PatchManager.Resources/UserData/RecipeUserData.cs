using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Resources.UserData;

/// <summary>
/// Recipe-style resource definition wrapper exposing the inner <c>recipeData</c> subtree while preserving the full
/// envelope, and exposing the <c>ingredients</c> array as a typed <see cref="IngredientsUserData" />.
/// </summary>
[MoonSharpUserData]
public class RecipeUserData : ExtensibleJsonUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>recipeData</c> subtree.
    /// </summary>
    [MoonSharpHidden] public JToken FullToken;

    /// <summary>
    /// Creates a wrapper for the recipe envelope, exposing the inner <c>recipeData</c> subtree to patch scripts.
    /// </summary>
    /// <param name="token">The recipe JSON envelope (containing a <c>recipeData</c> object).</param>
    public RecipeUserData(JToken token) : base(token["recipeData"])
    {
        FullToken = token;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "ingredients";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "ingredients")
        {
            return MoonSharp.Interpreter.UserData.Create(new IngredientsUserData((JArray)Token["ingredients"]));
        }
        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "ingredients")
            throw new Exception("You must instead use the methods on the Ingredients array.");
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        throw new Exception("You cannot remove this property.");
    }
}
