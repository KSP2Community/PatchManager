using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes;
using SassyPatchGrammar;
using System.Reflection;
using JetBrains.Annotations;
using PatchManager.Generic.SassyPatching.Rulesets;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Utility;
using PatchManager.Shared;
using UniLinq;
using Unity.VisualScripting;
using UnityEngine;

namespace PatchManager.SassyPatching.Execution
{
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


        /// <summary>
        /// This contains all the constant text based libraries that have been registered
        /// </summary>
        public static readonly Dictionary<string, string> AllRawLibraries = new();

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
        /// This logs errors in this universe
        /// </summary>
        public readonly Action<string> ErrorLogger;


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
        public Universe(Action<string> errorLogger, Action<string> messageLogger, List<string> allMods)
        {
            ErrorLogger = errorLogger;
            MessageLogger = messageLogger;
            LoadedLabels = new List<string>(_preloadedLabels);
            AllMods = allMods;
            MessageLogger("Setup universe!");
            SetupBasePriorities(allMods);
            LoadAllRawPatches();
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

        internal class ParserListener : IAntlrErrorListener<IToken>
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

        internal class LexerListener : IAntlrErrorListener<int>
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

        private void LoadAllRawPatches()
        {
            var tokenTransformer = new Transformer(msg => throw new LoadException(msg));
            foreach (var (id, raw) in AllRawLibraries)
            {
                try
                {
                    MessageLogger.Invoke($"Loading library {id}");
                    var charStream = CharStreams.fromString(raw);
                    var lexerErrorGenerator = new LexerListener(id, ErrorLogger);
                    var lexer = new sassy_lexer(charStream);
                    lexer.AddErrorListener(lexerErrorGenerator);
                    if (lexerErrorGenerator.Errored)
                        throw new LoadException("lexer errors detected");
                    var tokenStream = new CommonTokenStream(lexer);
                    var parser = new sassy_parser(tokenStream);
                    var parserErrorGenerator = new ParserListener(id, ErrorLogger);
                    parser.AddErrorListener(parserErrorGenerator);
                    if (parserErrorGenerator.Errored)
                        throw new LoadException("parser errors detected");
                    var patchContext = parser.patch();
                    tokenTransformer.Errored = false;
                    var patch = tokenTransformer.Visit(patchContext) as SassyPatch;
                    var lib = new SassyPatchLibrary(patch);
                    AllLibraries[id] = lib;
                }
                catch (Exception e)
                {
                    ErrorLogger($"Could not load library: {id} due to: {e.Message}");
                }
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
                LoadSingleLibrary(modId, library.Name, CharStreams.fromPath(library.FullName), tokenTransformer);
            }

            foreach (var patch in directory.EnumerateFiles("*.patch", SearchOption.AllDirectories))
            {
                LoadSinglePatch(modId, patch.Name, CharStreams.fromPath(patch.FullName), tokenTransformer);
            }
        }

        /// <summary>
        /// Loads a single patch
        /// </summary>
        /// <param name="patch">The file info of the patch file</param>
        /// <param name="cwd">The working directory to generate the patch mod id against</param>
        public void LoadSinglePatchFile(FileInfo patch, DirectoryInfo cwd)
        {
            var tokenTransformer = new Transformer(msg => throw new LoadException(msg));
            var name = Path.GetFileNameWithoutExtension(patch.FullName);
            var id = patch.Directory!.FullName.MakeRelativePathTo(cwd.FullName).Replace("\\", "-");
            if (name.StartsWith("_"))
            {
                LoadSingleLibrary(id, name, CharStreams.fromPath(patch.FullName), tokenTransformer);
            }
            else
            {
                LoadSinglePatch(id, name, CharStreams.fromPath(patch.FullName), tokenTransformer);
            }
        }

        /// <summary>
        /// Loads a single patch
        /// </summary>
        /// <param name="asset">The text asset to load</param>
        /// <param name="modId">Mod ID</param>
        public void LoadPatchAsset(TextAsset asset, string modId)
        {
            var tokenTransformer = new Transformer(msg => throw new LoadException(msg));
            if (asset.name.StartsWith("_"))
            {
                LoadSingleLibrary(modId, asset.name, CharStreams.fromString(asset.text), tokenTransformer);
            }
            else
            {
                LoadSinglePatch(modId, asset.name, CharStreams.fromString(asset.text), tokenTransformer);
            }
        }

        private void LoadSinglePatch(string modId, string name, ICharStream charStream, Transformer tokenTransformer)
        {
            if (name.StartsWith("_"))
            {
                return;
            }

            try
            {
                MessageLogger.Invoke($"Loading patch {modId}:{name}");
                var lexer = new sassy_lexer(charStream);
                var lexerErrorGenerator = new LexerListener($"{modId}:{name}", ErrorLogger);
                lexer.AddErrorListener(lexerErrorGenerator);
                if (lexerErrorGenerator.Errored)
                {
                    throw new LoadException("lexer errors detected");
                }

                var tokenStream = new CommonTokenStream(lexer);
                var parser = new sassy_parser(tokenStream);
                var parserErrorGenerator = new ParserListener($"{modId}:{name}", ErrorLogger);
                parser.AddErrorListener(parserErrorGenerator);
                var patchContext = parser.patch();
                if (parserErrorGenerator.Errored)
                {
                    throw new LoadException("parser errors detected");
                }

                tokenTransformer.Errored = false;
                // var gEnv = new GlobalEnvironment(this, modId);
                // var env = new Environment(gEnv);
                var ctx = tokenTransformer.Visit(patchContext) as SassyPatch;
                _toRegister.Add((modId, ctx));
                // lib = new SassyPatchLibrary(patch);
            }
            catch (Exception e)
            {
                ErrorLogger($"Could not run patch: {modId}:{name} due to: {e}");
            }
        }

        private void LoadSingleLibrary(string modId, string name, ICharStream charStream, Transformer tokenTransformer)
        {
            string libName = modId + ":" + name.Replace(".patch", "").TrimFirst();
            try
            {
                MessageLogger.Invoke($"Loading library {libName}");
                var lexerErrorGenerator = new LexerListener(libName, ErrorLogger);
                var lexer = new sassy_lexer(charStream);
                lexer.AddErrorListener(lexerErrorGenerator);
                if (lexerErrorGenerator.Errored)
                {
                    throw new LoadException("lexer errors detected");
                }

                var tokenStream = new CommonTokenStream(lexer);
                var parser = new sassy_parser(tokenStream);
                var parserErrorGenerator = new ParserListener(libName, ErrorLogger);
                parser.AddErrorListener(parserErrorGenerator);
                if (parserErrorGenerator.Errored)
                {
                    throw new LoadException("parser errors detected");
                }

                var patchContext = parser.patch();
                tokenTransformer.Errored = false;
                var patch = tokenTransformer.Visit(patchContext) as SassyPatch;
                var lib = new SassyPatchLibrary(patch);
                AllLibraries[libName] = lib;
            }
            catch (Exception e)
            {
                ErrorLogger($"Could not load library: {libName} due to: {e.Message}");
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
                var interpLabel = label.Interpolate(environment);
                if (!Configs.TryGetValue(interpLabel, out var labelDict))
                    labelDict = Configs[interpLabel] = new Dictionary<string, DataValue>();
                var subEnv = new Environment(environment.GlobalEnvironment,environment);
                if (name != null)
                {
                    var interpName = name.Interpolate(environment);
                    if (labelDict.TryGetValue(interpName, out var toAddValue))
                    {
                        subEnv["value"] = toAddValue;
                    }
                    else
                    {
                        subEnv["value"] = labelDict[interpName] = new DataValue(DataValue.DataType.None);
                    }

                    var result = updateExpression.Compute(subEnv);
                    if (result.IsDeletion)
                    {
                        labelDict.Remove(name);
                    }
                    else
                    {
                        labelDict[interpName] = result;
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

        public static void RegisterRawLibrary(string modId, string name, string raw)
        {
            AllRawLibraries.Add($"{modId}:{name}", raw);
        }
        
        
        #region Patch running

        
        public int TotalPatchCount;
        public List<SassyTextPatcher> GenericPatches = new();
        public Dictionary<string, List<SassyTextPatcher>> LabelPatches = new();
        public Dictionary<string, List<SassyTextPatcher>> NamePatches = new();
        public Dictionary<string, Dictionary<string, List<SassyTextPatcher>>> LabelNamePatches = new();
        
        
        private void RegisterPatcher(SassyTextPatcher patcher)
        {
            TotalPatchCount += 1;
            if (patcher.RuleSet is JsonRuleset && patcher.AssetType != null)
            {
                if (LabelPatches.TryGetValue(patcher.AssetType, out var patchers))
                {
                    AddSorted(patchers, patcher);
                }
                else
                {
                    LabelPatches[patcher.AssetType] = new List<SassyTextPatcher> {patcher};
                }
            }
            else if (patcher.RuleSet.Labels == null && patcher.AssetName == null)
            {
                AddSorted(GenericPatches, patcher);
            } else if (patcher.RuleSet.Labels != null && patcher.AssetName == null)
            {
                foreach (var label in patcher.RuleSet.Labels)
                {
                    if (LabelPatches.TryGetValue(label, out var patchers))
                    {
                        AddSorted(patchers, patcher);
                    }
                    else
                    {
                        LabelPatches[label] = new List<SassyTextPatcher> { patcher };
                    }
                }
            } else if (patcher.RuleSet.Labels == null && patcher.AssetName != null)
            {
                if (NamePatches.TryGetValue(patcher.AssetName, out var patchers))
                {
                    AddSorted(patchers, patcher);
                }
                else
                {
                    NamePatches[patcher.AssetName] = new List<SassyTextPatcher> { patcher };
                }
            } else if (patcher.RuleSet.Labels != null && patcher.AssetName != null)
            {
                foreach (var label in patcher.RuleSet.Labels)
                {
                    if (LabelNamePatches.TryGetValue(label, out var namePatchers))
                    {
                        if (namePatchers.TryGetValue(patcher.AssetName, out var patchers))
                        {
                            AddSorted(patchers, patcher);
                        }
                        else
                        {
                            namePatchers[patcher.AssetName] = new List<SassyTextPatcher> { patcher };
                        }
                    }
                    else
                    {
                        LabelNamePatches[label] = new Dictionary<string, List<SassyTextPatcher>>
                        {
                            [patcher.AssetName] = new() { patcher }
                        };
                    }
                }
            }
        }


        private static void AddSorted(List<SassyTextPatcher> patchers, SassyTextPatcher patcher)
        {
            var index = patchers.FindIndex(x => x.Priority > patcher.Priority);
            if (index == -1)
            {
                patchers.Add(patcher);
            }
            else
            {
                patchers.Insert(index,patcher);
            }
        }
        
        public string RunAllPatchesFor(string label, string name, string data, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            var enumerator = new PatchEnumerator(GenericPatches, LabelPatches.GetValueOrDefault(label), NamePatches.GetValueOrDefault(name), LabelNamePatches.GetValueOrDefault(label)?.GetValueOrDefault(name));
            ISelectable previous = null;
            while (enumerator.Next is { } next)
            {
                try
                {

                    if (previous == null)
                    {
                        if (next.TryPatchBegin(label, name, data, out previous)) patchCount++;
                    }
                    else
                    {
                        if (next.TryPatch(label, name, ref previous, out var stop)) patchCount++;
                        if (stop)
                        {
                            return string.Empty;
                        }
                    }
                }
                catch (Exception e)
                {
                    errorCount += 1;
                    ErrorLogger($"Patching {label}:{name} failed due to {e.Message}");
                }
            }

            return previous?.Serialize() ?? data;
        }

        public string RunAllPatchesFor(string label, string name, ISelectable selectable, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            var enumerator = new PatchEnumerator(GenericPatches, LabelPatches.GetValueOrDefault(label), NamePatches.GetValueOrDefault(name), LabelNamePatches.GetValueOrDefault(label)?.GetValueOrDefault(name));
            while (enumerator.Next is { } next)
            {
                try
                {
                    if (next.TryPatch(label, name, ref selectable, out var stop)) patchCount++;
                    if (stop)
                    {
                        return string.Empty;
                    }
                }
                catch (Exception e)
                {
                    errorCount += 1;
                    ErrorLogger($"Patching {label}:{name} failed due to {e.Message}");
                }
            }
            return selectable.Serialize();
        }

        private class PatchEnumerator
        {
            private List<SassyTextPatcher> _generic;
            [CanBeNull] private List<SassyTextPatcher> _label;
            [CanBeNull] private List<SassyTextPatcher> _name;
            [CanBeNull] private List<SassyTextPatcher> _labelName;
            private int _genericIndex = 0;
            private int _labelIndex = 0;
            private int _nameIndex = 0;
            private int _labelNameIndex = 0;
            public PatchEnumerator(List<SassyTextPatcher> generic, List<SassyTextPatcher> label,
                List<SassyTextPatcher> name, List<SassyTextPatcher> labelName)
            {
                _generic = generic;
                _label = label;
                _name = name;
                _labelName = labelName;
            }

            [CanBeNull]
            public SassyTextPatcher Next
            {
                get
                {
                    SassyTextPatcher generic = null;
                    SassyTextPatcher label = null;
                    SassyTextPatcher name = null;
                    SassyTextPatcher labelName = null;
                    var minPriority = ulong.MaxValue;

                    if (_genericIndex < _generic.Count)
                    {
                        generic = _generic[_genericIndex];
                        minPriority = Math.Min(minPriority, generic.Priority);
                    }

                    if (_label != null && _labelIndex < _label.Count)
                    {
                        label = _label[_labelIndex];
                        minPriority = Math.Min(minPriority, label.Priority);
                    }

                    if (_name != null && _nameIndex < _name.Count)
                    {
                        name = _name[_nameIndex];
                        minPriority = Math.Min(minPriority, name.Priority);
                    }

                    if (_labelName != null && _labelNameIndex < _labelName.Count)
                    {
                        labelName = _labelName[_labelNameIndex];
                        minPriority = Math.Min(minPriority, labelName.Priority);
                    }

                    if (generic != null && minPriority == generic.Priority)
                    {
                        _genericIndex += 1;
                        return generic;
                    }
                    
                    if (label != null && minPriority == label.Priority)
                    {
                        _labelIndex += 1;
                        return label;
                    }

                    if (name != null && minPriority == name.Priority)
                    {
                        _nameIndex += 1;
                        return name;
                    }

                    if (labelName != null && minPriority == labelName.Priority)
                    {
                        _labelNameIndex += 1;
                        return labelName;
                    }

                    return null;
                }
            }
        }
        
        #endregion

        public List<SassyGenerator> Generators = new();

        public void RegisterGenerator(SassyGenerator generator)
        {
            Generators.Add(generator);
        }
    }
}