using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Builtins;

[SassyLibrary("builtin","meta")]
[PublicAPI]
public class MetaBuiltins
{
    [SassyMethod("exists-mod")]
    public static bool ExistsMod(GlobalEnvironment globalEnvironment, [SassyName("mod-name")] string modName) => globalEnvironment.Universe.AllMods.Contains(modName);

    [SassyMethod("exists-function")]
    public static bool ExistsFunction(GlobalEnvironment environment, [SassyName("function-name")] string functionName) =>
        environment.AllFunctions.Keys.Contains(functionName);
    
}