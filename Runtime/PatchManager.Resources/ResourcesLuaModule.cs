using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Resources.UserData;

namespace PatchManager.Resources;

[PatchManagerModule("Resources")]
[MoonSharpUserData]
public class ResourcesLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public ResourcesLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public LuaPatch PatchAll(Script script, Func<JsonUserData, string> callback)
    {
        return _core.PatchAll(script, "Resource", "resources", callback.ToPatchMethod());
    }

    public LuaPatch Patch(Script script, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "Resource", "resources", name, callback.ToPatchMethod());
    }

    public void NewRecipe(string name, Action<RecipeUserData> callback)
    {
        var value =
            $"{{\n    \"version\": 0.1,\n    \"useExternal\": false,\n    \"isRecipe\": true,\n    \"recipeData\": {{\n        \"name\": \"{name}\",\n        \"displayNameKey\": \"Resource/DisplayName/Unknown\",\n        \"abbreviationKey\": \"Resource/Abbreviation/UK\",\n        \"resourceIconAssetAddress\": \"\",\n        \"vfxFuelType\": \"NoFuel\",\n        \"ingredients\": []\n    }}    \n}}";
        var typed = new RecipeUserData(JObject.Parse(value));
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Resource", "resources", name, ud);
    }

    public void NewResource(string name, Action<ResourceUserData> callback)
    {
        var value =
            $"{{\n    \"version\": 0.1,\n    \"useExternal\": false,\n    \"data\": {{\n        \"name\": \"{name}\",\n        \"displayNameKey\": \"Resource/DisplayName/Unknown\",\n        \"abbreviationKey\": \"Resource/Abbreviation/UK\",\n        \"isTweakable\": true,\n        \"isVisible\": true,\n        \"massPerUnit\": 0,\n        \"volumePerUnit\": 0,\n        \"specificHeatCapacityPerUnit\": 0,\n        \"flowMode\": 0,\n        \"transferMode\": 0,\n        \"costPerUnit\": 0,\n\t\"NonStageable\": false,\n        \"resourceIconAssetAddress\": \"\",\n        \"vfxFuelType\": \"NoFuel\"     \n    }}    \n}}";
        var typed = new ResourceUserData(JObject.Parse(value));
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Resource", "resources", name, ud);
    }

    private static int _nextId;

    public void RegisterUnits(DynValue value)
    {
        _core.New("JSON", "resource_units", Guid.NewGuid().ToString(), value);
    }
}
