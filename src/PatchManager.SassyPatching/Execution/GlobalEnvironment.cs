using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// Describes a global (per patch file) environment used by the sassy patching engine
/// </summary>
public class GlobalEnvironment
{
    /// <summary>
    /// This references the global state that all patches being run share
    /// </summary>
    public Universe Universe;

    /// <summary>
    /// The Guid of the mod this patch file is from
    /// </summary>
    public string ModGuid;

    public List<string> ImportedLibraries = new();

    public Dictionary<string, PatchFunction> AllFunctions = new();

    public Dictionary<string, PatchMixin> AllMixins = new();


    internal GlobalEnvironment(Universe universe, string modGuid)
    {
        Universe = universe;
        ModGuid = modGuid;
    }

    internal void Import(Environment rootEnvironment, string libraryName)
    {
        var fullyQualifiedLibraryName = libraryName.Contains(":") ? libraryName : $"{ModGuid}:{libraryName}";
        if (fullyQualifiedLibraryName.EndsWith(":*"))
        {
            var baseName = fullyQualifiedLibraryName.Replace(":*", ":");
            var anyFound = false;
            foreach (var library in Universe.AllLibraries.Where(library => library.Key.StartsWith(baseName)))
            {
                anyFound = true;
                if (ImportedLibraries.Contains(library.Key))
                {
                    continue;
                }

                library.Value.RegisterInto(rootEnvironment);
                ImportedLibraries.Add(library.Key);
            }

            if (!anyFound)
            {
                throw new ImportException();
            }
        } else 
        {
            if (ImportedLibraries.Contains(fullyQualifiedLibraryName))
                return;
            if (Universe.AllLibraries.TryGetValue(fullyQualifiedLibraryName, out var library))
            {
                library.RegisterInto(rootEnvironment);
                ImportedLibraries.Add(fullyQualifiedLibraryName);
            }
            else
            {
                throw new ImportException();
            }
        }
    }
}