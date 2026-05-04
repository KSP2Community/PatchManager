using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Planets.UserData;

[MoonSharpUserData]
public class CelestialBodyUserData : JsonUserData
{
    public JToken FullToken;
    public CelestialBodyUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
    }
}