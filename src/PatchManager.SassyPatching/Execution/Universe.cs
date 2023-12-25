using Antlr4.Runtime;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes;
using PatchManager.Shared.Interfaces;
using SassyPatchGrammar;
using System.Reflection;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;

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

    private static List<string> _preloadedLabels;

    static Universe()
    {
        RuleSets = new();
        AllManagedLibraries = new();
        _preloadedLabels = new();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Only use public rule sets
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (typeof(IPatcherRuleSet).IsAssignableFrom(type))
                {
                    var rsAttribute = type.GetCustomAttribute<PatcherRulesetAttribute>();
                    if (rsAttribute != null)
                    {
                        RuleSets[rsAttribute.RulesetName] = (IPatcherRuleSet)Activator.CreateInstance(type);
                        _preloadedLabels.AddRange(rsAttribute.PreloadLabels);
                    }
                }

                var sassyLibraryAttribute = type.GetCustomAttribute<SassyLibraryAttribute>();
                if (sassyLibraryAttribute != null)
                {
                    var name = sassyLibraryAttribute.Mod + ":" + sassyLibraryAttribute.Library;
                    AllManagedLibraries[name] = new ManagedPatchLibrary(type);
                    Console.WriteLine($"Registered a managed library, {name}");
                }
            }
        }
    }


    public Dictionary<string, Dictionary<string, DataValue>> Configs = new();
#nullable enable
    public List<(long priority, string label, string? name, Expression updateExpression, Environment snapshot)> ConfigUpdates = new();
    public void AddConfigUpdater(long priority, string label, string? name, Expression updateExpression, Environment snapshot)
    {
        if (ConfigUpdates.Count == 0)
        {
            ConfigUpdates.Add((priority, label, name, updateExpression,snapshot));
            return;
        }

        for (var i = 0; i < ConfigUpdates.Count; i++)
        {
            if (ConfigUpdates[i].priority < priority) continue;

            ConfigUpdates.Insert(i,(priority, label, name, updateExpression,snapshot));
            return;
        }
        ConfigUpdates.Add((priority, label, name, updateExpression,snapshot));
    }
#nullable disable
    /// <summary>
    /// All stages defined by every mod
    /// </summary>
    public Dictionary<string, ulong> AllStages = new();

    // Populated from Space Warps mod list come 1.3.0
    public List<string> AllMods;

    /// <summary>
    /// This is an action that is taken
    /// </summary>
    public readonly Action<ITextPatcher> RegisterPatcher;


    /// <summary>
    /// Register a generator patch
    /// </summary>
    public readonly Action<ITextAssetGenerator> RegisterGenerator;

    /// <summary>
    /// This logs errors in this universe
    /// </summary>
    private readonly Action<string> _errorLogger;


    /// <summary>
    /// The list of labels that this universe needs loaded to respond to
    /// </summary>
    public List<string> LoadedLabels;

    /// <summary>
    /// This logs any message that is not an error in the universe
    /// </summary>
    public readonly Action<string> MessageLogger;

    private readonly List<(string id, SassyPatch patch)> _toRegister = new();

    /// <summary>
    /// Create a new universal state
    /// </summary>
    /// <param name="registerPatcher">This action receives patchers and registers them for later execution</param>
    /// <param name="errorLogger">The action to be taken to log an error</param>
    /// <param name="messageLogger">The action to be taken to log a message</param>
    public Universe(Action<ITextPatcher> registerPatcher, Action<string> errorLogger, Action<string> messageLogger, Action<ITextAssetGenerator> registerGenerator, List<string> allMods)
    {
        RegisterPatcher = registerPatcher;
        _errorLogger = errorLogger;
        MessageLogger = messageLogger;
        RegisterGenerator = registerGenerator;
        LoadedLabels = new List<string>(_preloadedLabels);
        AllMods = allMods;
        MessageLogger("Setup universe!");
        SetupBasePriorities(allMods);
    }

    // TODO: Fix this so that other mods stages get their guids working
    /// <summary>
    /// All the libraries in this "universe"
    /// </summary>
    public readonly Dictionary<string, PatchLibrary> AllLibraries = new(AllManagedLibraries);

    private class LoadException : Exception
    {
        public LoadException(string message) : base(message)
        {
        }
    }

    private class ParserListener : IAntlrErrorListener<IToken>
    {
        internal bool Errored;
        internal Action<string> ErrorLogger;
        internal string File;
        internal ParserListener(string file, Action<string> errorLogger)
        {
            File = file;
            ErrorLogger = errorLogger;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine,
            string msg, RecognitionException e)
        {
            Errored = true;
            ErrorLogger.Invoke($"error parsing {File} - {line}:{charPositionInLine}: {msg}");
        }
    }

    private class LexerListener : IAntlrErrorListener<int>
    {
        internal bool Errored;
        internal Action<string> ErrorLogger;
        internal string File;
        internal LexerListener(string file, Action<string> errorLogger)
        {
            File = file;
            ErrorLogger = errorLogger;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
            int charPositionInLine,
            string msg, RecognitionException e)
        {
            Errored = true;
            ErrorLogger.Invoke($"error lexing {File} - {line}:{charPositionInLine}: {msg}");
        }
    }
    /// <summary>
    /// Loads all patches from a directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="modId">The ID of the mod to load the guid as</param>
    public void LoadPatchesInDirectory(DirectoryInfo directory, string modId)
    {
        // MessageLogger.Invoke($"Loading patches from {directory} (modId: {modId})");
        var tokenTransformer = new Transformer(msg => throw new LoadException(msg));
        foreach (var library in directory.EnumerateFiles("_*.patch", SearchOption.AllDirectories))
        {
            LoadSingleLibrary(modId, library, tokenTransformer);
        }

        foreach (var patch in directory.EnumerateFiles("*.patch", SearchOption.AllDirectories))
        {
            LoadSinglePatch(modId, patch, tokenTransformer);
        }
    }

    private void LoadSinglePatch(string modId, FileInfo patch, Transformer tokenTransformer)
    {
        if (patch.Name.StartsWith("_"))
            return;
        try
        {
            MessageLogger.Invoke($"Loading patch {modId}:{patch.Name}");
            var charStream = CharStreams.fromPath(patch.FullName);
            var lexer = new sassy_lexer(charStream);
            var lexerErrorGenerator = new LexerListener($"{modId}:{patch.Name}", _errorLogger);
            lexer.AddErrorListener(lexerErrorGenerator);
            if (lexerErrorGenerator.Errored)
                throw new LoadException("lexer errors detected");
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new sassy_parser(tokenStream);
            var parserErrorGenerator = new ParserListener($"{modId}:{patch.Name}", _errorLogger);
            parser.AddErrorListener(parserErrorGenerator);
            var patchContext = parser.patch();
            if (parserErrorGenerator.Errored)
                throw new LoadException("parser errors detected");
            tokenTransformer.Errored = false;
            // var gEnv = new GlobalEnvironment(this, modId);
            // var env = new Environment(gEnv);
            var ctx = tokenTransformer.Visit(patchContext) as SassyPatch;
            _toRegister.Add((modId, ctx));
            // lib = new SassyPatchLibrary(patch);
        }
        catch (Exception e)
        {
            _errorLogger($"Could not run patch: {modId}:{patch.Name} due to: {e.Message}");
        }
    }

    private void LoadSingleLibrary(string modId, FileInfo library, Transformer tokenTransformer)
    {
        string name = modId + ":" + library.Name.Replace(".patch", "").TrimFirst();
        try
        {
            MessageLogger.Invoke($"Loading library {name}");
            var charStream = CharStreams.fromPath(library.FullName);
            var lexerErrorGenerator = new LexerListener(name, _errorLogger);
            var lexer = new sassy_lexer(charStream);
            lexer.AddErrorListener(lexerErrorGenerator);
            if (lexerErrorGenerator.Errored)
                throw new LoadException("lexer errors detected");
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new sassy_parser(tokenStream);
            var parserErrorGenerator = new ParserListener(name, _errorLogger);
            parser.AddErrorListener(parserErrorGenerator);
            if (parserErrorGenerator.Errored)
                throw new LoadException("parser errors detected");
            var patchContext = parser.patch();
            tokenTransformer.Errored = false;
            var patch = tokenTransformer.Visit(patchContext) as SassyPatch;
            var lib = new SassyPatchLibrary(patch);
            AllLibraries[name] = lib;
        }
        catch (Exception e)
        {
            _errorLogger($"Could not load library: {name} due to: {e.Message}");
        }
    }

    public List<SassyTextPatcher> SassyTextPatchers = new();
    public void RegisterPatcherToUniverse(SassyTextPatcher sassyTextPatcher)
    {
        SassyTextPatchers.Add(sassyTextPatcher);
    }

    /// <summary>
    /// This registers every patch in the files in the to register list
    /// </summary>
    public void RegisterAllPatches()
    {
        foreach (var (modId, patch) in _toRegister)
        {
            var gEnv = new GlobalEnvironment(this, modId);
            var env = new Environment(gEnv);
            patch.ExecuteIn(env);
        }

        // Now we get to do the fun stuff with stages
        foreach (var stage in UnsortedStages.Values)
            stage.UpdateRequirements(UnsortedStages.Keys.ToList());
        SortStages();
        // Now lets update configs
        foreach (var (_,label,name,updateExpression,environment) in ConfigUpdates)
        {
            if (!Configs.TryGetValue(label, out var labelDict))
                labelDict = Configs[label] = new Dictionary<string, DataValue>();
            var subEnv = new Environment(environment.GlobalEnvironment,environment);
            if (name != null)
            {
                if (labelDict.TryGetValue(name, out var toAddValue))
                {
                    subEnv["value"] = toAddValue;
                }
                else
                {
                    subEnv["value"] = labelDict[name] = new DataValue(DataValue.DataType.None);
                }

                var result = updateExpression.Compute(subEnv);
                if (result.IsDeletion)
                {
                    labelDict.Remove(name);
                }
                else
                {
                    labelDict[name] = result;
                }
            }
            else
            {
                subEnv["value"] = DataValue.From(labelDict);
                var result = updateExpression.Compute(subEnv);
                if (result.IsDeletion)
                {
                    labelDict.Clear();
                }
                else if (!result.IsDictionary)
                {
                    throw new InterpreterException(updateExpression.Coordinate,
                        "Updating a config label must result in a dictionary or deletion value.");
                }
                else
                {
                    Configs[label] = result.Dictionary;
                }
            }
        }

        foreach (var patcher in SassyTextPatchers)
        {
            var stage = patcher.PriorityString;
            var modId = patcher.OriginalGuid;
            if (AllStages.TryGetValue(stage, out var priority))
            {
                patcher.Priority = priority;
            } else if (AllStages.TryGetValue($"{modId}:{stage}", out priority))
            {
                patcher.Priority = priority;
            } else if (AllStages.TryGetValue(modId, out priority))
            {
                patcher.Priority = priority;
            }

            RegisterPatcher(patcher);
        }
    }

    private void SortStages()
    {
        MessageLogger($"Sorting {UnsortedStages.Count} stages");
        List<string> sortedStages = new();
        Dictionary<string, Stage> toSort = new(UnsortedStages);
        while (toSort.Count > 0)
        {
            if (!SingleSortStep(toSort, sortedStages))
            {
                throw new Exception(
                    $"Unable to sort stages to define patch order, the following stages cause a circular dependency: {string.Join(", ", toSort.Keys)}");
            }
        }

        // For debug purposes
        MessageLogger("Sorted stages!");
        ulong n = 0;
        foreach (var stage in sortedStages)
        {
            MessageLogger($"{stage}: {n}");
            AllStages[stage] = n++;
        }
    }


    private static bool SingleSortStep(
        Dictionary<string, Stage> toBeSorted,
        List<string> sortedStages
    )
    {
        var remove = "";
        var found = false;
        foreach (var (name, stage) in toBeSorted)
        {
            if (!stage.RunsAfter.All(sortedStages.Contains) || toBeSorted.Values.Any(x => x.RunsBefore.Contains(name)))
            {
                continue;
            }

            remove = name;
            found = true;
            sortedStages.Add(name);
            break;
        }

        if (found)
        {
            toBeSorted.Remove(remove);
        }
        return found;
    }

    public void PatchLabels(params string[] labels)
    {
        LoadedLabels.AddRange(labels);
    }

    public readonly Dictionary<string, Stage> UnsortedStages = new();
    public readonly Dictionary<string, string> LastImplicitWithinMod = new();
    public string LastImplicitGlobal = "";

    private void SetupBasePriorities(List<string> modLoadOrder)
    {
        MessageLogger($"Setting up base priorities with mod load order: {string.Join(", ", modLoadOrder)}");
        var lastPost = "";
        foreach (var mod in modLoadOrder)
        {
            var stage = new Stage();
            if (lastPost.Length > 0)
                stage.RunsAfter.Add(lastPost);
            UnsortedStages[mod] = stage;
            MessageLogger($"Adding stage: {mod}");
            var post = new Stage();
            post.RunsAfter.Add(mod);
            lastPost = $"{mod}:post";
            UnsortedStages[lastPost] = post;
            MessageLogger($"Adding stage: {lastPost}");
            LastImplicitWithinMod[mod] = mod;
        }
        LastImplicitGlobal = lastPost;
        MessageLogger($"Last implicit global: {lastPost}");
    }
}