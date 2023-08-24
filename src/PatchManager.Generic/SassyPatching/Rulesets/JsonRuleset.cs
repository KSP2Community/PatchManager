using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Generic.SassyPatching.Rulesets;

/// <summary>
/// This is a generic json patching ruleset
/// </summary>
[PatcherRuleset("json")]
public class JsonRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label)
    {
        return true;
    }

    /// <inheritdoc />
    public ISelectable ConvertToSelectable(string type, string name, string jsonData)
    {
        Console.WriteLine($"Converting to JTokenSelectable {type}:{name}");
        return new JTokenSelectable(() => { }, JToken.Parse(jsonData), name, type);
    }
}