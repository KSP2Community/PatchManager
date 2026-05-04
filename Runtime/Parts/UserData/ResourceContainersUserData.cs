using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Parts.UserData;

[MoonSharpUserData]
public class ResourceContainersUserData : IndexedListUserData
{
    public ResourceContainersUserData(JArray token) : base(token)
    {
    }
    
    public override string Name(JToken source)
    {
        return source["name"].Value<string>();
    }
    
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

    public bool Has(string type) => HasKey(type);
}