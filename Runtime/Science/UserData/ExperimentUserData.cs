using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Science.UserData;

[MoonSharpUserData]
public class ExperimentUserData : JsonUserData
{
    public JToken FullToken;

    public ExperimentUserData(JToken token) : base(token["Data"])
    {
        FullToken = token;
    }
}