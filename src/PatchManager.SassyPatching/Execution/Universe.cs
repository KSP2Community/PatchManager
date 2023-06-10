using Antlr4.Runtime;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes;
using PatchManager.Shared.Interfaces;
using SassyPatchGrammar;

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
    /// This logs errors in this universe
    /// </summary>
    public readonly Action<string> ErrorLogger;

    /// <summary>
    /// Create a new universal state
    /// </summary>
    /// <param name="registerPatcher">This action receives patchers and registers them for later execution</param>
    public Universe(Action<ITextPatcher> registerPatcher, Action<string> errorLogger)
    {
        RegisterPatcher = registerPatcher;
        ErrorLogger = errorLogger;
    }

    // TODO: Fix this so that other mods stages get their guids working
    public Dictionary<string,PatchLibrary> AllLibraries = new(AllManagedLibraries);

    private class LoadException : Exception
    {
        public LoadException(string message) : base(message)
        {
        }
    }

    private class LoadListener : IAntlrErrorListener<IToken>
    {
        internal static readonly LoadListener Instance = new();

        private LoadListener()
        {
            
        }
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            throw new LoadException($"{line}:{charPositionInLine}: msg");
        }
    }

    /// <summary>
    /// Loads all patches from a directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="modId">The ID of the mod to load the guid as</param>
    public void LoadPatchesInDirectory(DirectoryInfo directory, string modId)
    {
        
        var tokenTransformer = new Transformer(msg => throw new LoadException(msg));
        foreach (var library in directory.EnumerateFiles("_*.patch", SearchOption.AllDirectories))
        {
            string name = modId + ":" + library.Name.Replace(".patch", "").TrimFirst();
            try
            {
                var charStream = CharStreams.fromPath(library.FullName);
                var lexer = new sassy_lexer(charStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new sassy_parser(tokenStream);
                parser.AddErrorListener(LoadListener.Instance);
                var patchContext = parser.patch();
                tokenTransformer.Errored = false;
                var patch = tokenTransformer.Visit(patchContext) as SassyPatch;
                var lib = new SassyPatchLibrary(patch);
                AllLibraries[name] = lib;
            }
            catch (Exception e)
            {
                ErrorLogger($"Could not load library: {name} due to: {e.Message}");
            }
        }

        foreach (var patch in directory.EnumerateFiles("*.patch", SearchOption.AllDirectories))
        {
            if (patch.Name.StartsWith("_")) continue;
            try
            {
                var charStream = CharStreams.fromPath(patch.FullName);
                var lexer = new sassy_lexer(charStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new sassy_parser(tokenStream);
                parser.AddErrorListener(LoadListener.Instance);
                var patchContext = parser.patch();
                tokenTransformer.Errored = false;
                var gEnv = new GlobalEnvironment(this,modId);
                var env = new Environment(gEnv);
                var ctx = tokenTransformer.Visit(patchContext) as SassyPatch;
                ctx?.ExecuteIn(env);
                // lib = new SassyPatchLibrary(patch);
            }
            catch (Exception e)
            {
                ErrorLogger($"Could not run patch: {modId}:{patch.Name} due to: {e.Message}");
            }
        }
    }
}