using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.Shared.Interfaces;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// The state that all executing patches share
/// </summary>
public class Universe
{
    /// <summary>
    /// This contains all rule sets that have been found in all assemblies
    /// </summary>
    public static readonly Dictionary<string, IPatcherRuleSet> RuleSets;

    /// <summary>
    /// This contains all the managed libraries that have been found in all assemblies
    /// </summary>
    public static readonly Dictionary<string, PatchLibrary> AllManagedLibraries;

    static Universe()
    {
        RuleSets = new();
        AllManagedLibraries = new();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Only use public rule sets
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (typeof(IPatcherRuleSet).IsAssignableFrom(type) && type.GetCustomAttributes(typeof(PatcherRulesetAttribute), false).FirstOrDefault() is PatcherRulesetAttribute rsAttribute)
                {
                    RuleSets[rsAttribute.RulesetName] = (IPatcherRuleSet)Activator.CreateInstance(type);
                }

                if (type.GetCustomAttributes(typeof(SassyLibraryAttribute), false).FirstOrDefault() is SassyLibraryAttribute sassyLibraryAttribute)
                {
                    var name = sassyLibraryAttribute.Mod + ":" + sassyLibraryAttribute.Library;
                    AllManagedLibraries[name] = new ManagedPatchLibrary(type);
                }
            }
        }
    }
    
    /// <summary>
    /// All stages defined by every mod
    /// </summary>
    public Dictionary<string, ulong> AllStages = new();

    // Populated from Space Warps mod list come 1.3.0
    public List<string> AllMods = new();

    /// <summary>
    /// This is an action that is taken
    /// </summary>
    public readonly Action<ITextPatcher> RegisterPatcher;

    /// <summary>
    /// Create a new universal state
    /// </summary>
    /// <param name="registerPatcher">This action receives patchers and registers them for later execution</param>
    public Universe(Action<ITextPatcher> registerPatcher)
    {
        RegisterPatcher = registerPatcher;
    }

    public Dictionary<string,PatchLibrary> AllLibraries = new(AllManagedLibraries);
}