using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching;

/// <summary>
/// A named patch pass and the passes it is ordered against.
/// </summary>
/// <remarks>
/// Passes are full sweeps over every patched label, run in the order their constraints sort into. A declared
/// pass starts out after <see cref="EARLY" /> and before <see cref="LATE" />. Order that no constraint states is
/// left to the sort and may differ between runs.
/// </remarks>
[MoonSharpUserData]
public class PassDefinition
{
    /// <summary>
    /// The built-in pass that runs before every other pass unless a constraint says otherwise.
    /// </summary>
    public const string EARLY = "Early";

    /// <summary>
    /// The built-in pass that patches run in when they name no pass.
    /// </summary>
    public const string DEFAULT = "Default";

    /// <summary>
    /// The built-in pass that runs after every other pass unless a constraint says otherwise.
    /// </summary>
    public const string LATE = "Late";

    /// <summary>
    /// The pass's namespaced name, <c>modId:&lt;supplied-name&gt;</c> for every pass but the built-in ones.
    /// </summary>
    public string Name;

    /// <summary>
    /// The ID of the mod that declared the pass, used to namespace the passes it refers to.
    /// </summary>
    public string PassModId;

    /// <summary>
    /// The passes this pass runs after when they exist.
    /// </summary>
    [MoonSharpHidden] public HashSet<string> AfterPasses = new() { EARLY };

    /// <summary>
    /// The passes this pass runs before when they exist.
    /// </summary>
    [MoonSharpHidden] public HashSet<string> BeforePasses = new() { LATE };

    /// <summary>
    /// Makes this pass run after the given passes when they exist.
    /// </summary>
    /// <param name="passes">The passes to run after, namespaced to the host mod when they are not built-in and do not already carry a namespace.</param>
    /// <returns>The pass instance for chaining.</returns>
    public PassDefinition After(params string[] passes)
    {
        foreach (var pass in passes)
        {
            AfterPasses.Add(NormalizeName(PassModId, pass));
        }

        return this;
    }

    /// <summary>
    /// Makes this pass run before the given passes when they exist.
    /// </summary>
    /// <param name="passes">The passes to run before, namespaced to the host mod when they are not built-in and do not already carry a namespace.</param>
    /// <returns>The pass instance for chaining.</returns>
    public PassDefinition Before(params string[] passes)
    {
        foreach (var pass in passes)
        {
            BeforePasses.Add(NormalizeName(PassModId, pass));
        }

        return this;
    }

    /// <summary>
    /// Removes every ordering constraint from this pass, the default ones after Early and before Late included.
    /// </summary>
    /// <returns>The pass instance for chaining.</returns>
    public PassDefinition ClearOrdering()
    {
        AfterPasses.Clear();
        BeforePasses.Clear();
        return this;
    }

    /// <summary>
    /// Namespaces a pass name to a mod unless it is built-in or already carries a namespace.
    /// </summary>
    /// <param name="modId">The mod ID to namespace the name under.</param>
    /// <param name="name">The pass name.</param>
    /// <returns>The namespaced pass name.</returns>
    public static string NormalizeName(string modId, string name) =>
        name is EARLY or DEFAULT or LATE || name.Contains(':') ? name : $"{modId}:{name}";

    /// <summary>
    /// Creates the three built-in passes, ordered Early, then Default, then Late.
    /// </summary>
    /// <returns>The built-in passes.</returns>
    public static IEnumerable<PassDefinition> CreateBuiltInPasses()
    {
        yield return new PassDefinition
        {
            Name = EARLY,
            AfterPasses = new HashSet<string>(),
            BeforePasses = new HashSet<string> { DEFAULT }
        };
        yield return new PassDefinition
        {
            Name = DEFAULT,
            AfterPasses = new HashSet<string> { EARLY },
            BeforePasses = new HashSet<string> { LATE }
        };
        yield return new PassDefinition
        {
            Name = LATE,
            AfterPasses = new HashSet<string> { DEFAULT },
            BeforePasses = new HashSet<string>()
        };
    }
}
