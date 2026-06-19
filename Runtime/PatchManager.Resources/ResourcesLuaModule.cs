using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Resources.UserData;

namespace PatchManager.Resources;

/// <summary>
/// Lua submodule exposed as <c>PM.Resources</c>, providing patches and creation helpers for resource and recipe definitions.
/// </summary>
[PatchManagerModule("Resources")]
[MoonSharpUserData]
public class ResourcesLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public ResourcesLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }
    /// <summary>
    /// Registers a resource patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every resource by default; restrict it via <see cref="LuaPatch.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="script">The host Lua script; its <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
    /// <param name="name">The patch's local name; namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch Patch(Script script, string name)
    {
        return _core.Patch(script, "Resource", "resources", name);
    }

    /// <summary>
    /// Creates a new recipe-style resource definition with the given name and runs <paramref name="callback" />
    /// against it for further configuration.
    /// </summary>
    /// <param name="name">The recipe's resource name.</param>
    /// <param name="callback">Callback that receives the new recipe for further configuration.</param>
    public void NewRecipe(string name, Action<RecipeUserData> callback)
    {
        var value =
            $"{{\n    \"version\": 0.1,\n    \"useExternal\": false,\n    \"isRecipe\": true,\n    \"recipeData\": {{\n        \"name\": \"{name}\",\n        \"displayNameKey\": \"Resource/DisplayName/Unknown\",\n        \"abbreviationKey\": \"Resource/Abbreviation/UK\",\n        \"resourceIconAssetAddress\": \"\",\n        \"vfxFuelType\": \"NoFuel\",\n        \"ingredients\": []\n    }}    \n}}";
        JObject parsed;
        try
        {
            parsed = JObject.Parse(value);
        }
        catch (JsonReaderException e)
        {
            throw new ScriptRuntimeException($"PM.Resources:NewRecipe: '{name}' produced invalid JSON ({e.Message}); avoid quote and backslash characters in the name.");
        }
        var typed = new RecipeUserData(parsed);
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Resource", "resources", name, ud);
    }

    /// <summary>
    /// Creates a new plain resource definition with the given name and runs <paramref name="callback" /> against
    /// it for further configuration.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <param name="callback">Callback that receives the new resource for further configuration.</param>
    public void NewResource(string name, Action<ResourceUserData> callback)
    {
        var value =
            $"{{\n    \"version\": 0.1,\n    \"useExternal\": false,\n    \"data\": {{\n        \"name\": \"{name}\",\n        \"displayNameKey\": \"Resource/DisplayName/Unknown\",\n        \"abbreviationKey\": \"Resource/Abbreviation/UK\",\n        \"isTweakable\": true,\n        \"isVisible\": true,\n        \"massPerUnit\": 0,\n        \"volumePerUnit\": 0,\n        \"specificHeatCapacityPerUnit\": 0,\n        \"flowMode\": 0,\n        \"transferMode\": 0,\n        \"costPerUnit\": 0,\n\t\"NonStageable\": false,\n        \"resourceIconAssetAddress\": \"\",\n        \"vfxFuelType\": \"NoFuel\"     \n    }}    \n}}";
        JObject parsed;
        try
        {
            parsed = JObject.Parse(value);
        }
        catch (JsonReaderException e)
        {
            throw new ScriptRuntimeException($"PM.Resources:NewResource: '{name}' produced invalid JSON ({e.Message}); avoid quote and backslash characters in the name.");
        }
        var typed = new ResourceUserData(parsed);
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Resource", "resources", name, ud);
    }

    private static int _nextId;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _nextId = default;
    }

    /// <summary>
    /// Registers a JSON resource-units definition, queueing it under the <c>resource_units</c> addressables label
    /// with a sequential synthetic name.
    /// </summary>
    /// <param name="value">The JSON value describing the resource units, typically a <see cref="JsonUserData" /> wrapping a <c>JObject</c>.</param>
    public void RegisterUnits(DynValue value)
    {
        _core.New("JSON", "resource_units", $"resource_units_{_nextId++}", value);
    }
}
