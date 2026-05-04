using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;

namespace PatchManager.Resources.UserData;

public class ResourceUserData : JsonUserData
{
    public JToken FullToken;
    public ResourceUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
    }
}