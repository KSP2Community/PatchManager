using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Discord.Sdk;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Runtime.LuaPatching.Utility;
using ReduxLib.Logging;
using UnityEngine;
using UnityEngine.Audio;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// Lua Based Execution
    /// </summary>
    public class Universe
    {
        /// <summary>
        /// Static universe instance
        /// </summary>
        public readonly Action<string> ErrorLogger;
        public readonly Action<string> MessageLogger;
        public readonly HashSet<string> AllMods;
        public Universe(Action<string> errorLogger, Action<string> messageLogger, List<string> allMods)
        {
            ErrorLogger = errorLogger;
            MessageLogger = messageLogger;
            AllMods = allMods.ToHashSet();
            var pmc = new PatchManagerCore(this);
            PatchManagerLibraryInstance = UserData.Create(pmc);
            foreach (var (k, v) in SubmoduleTypes)
            {
                Submodules[k] = UserData.Create(Activator.CreateInstance(v, pmc, this));
            }
            SetupBasePriorities(allMods);
        }

        /// <summary>
        /// The current instance of the patch manager library as a DynValue
        /// </summary>
        public DynValue PatchManagerLibraryInstance;
        public Dictionary<string, DynValue> Submodules = new();
        public static Dictionary<string, Type> SubmoduleTypes = new();
        public static Dictionary<string, IConverter> Converters = new();
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
                AllStages[mod] = stage;
                MessageLogger($"Adding stage: {mod}");
                var post = new Stage();
                post.RunsAfter.Add(mod);
                lastPost = $"{mod}:post";
                AllStages[lastPost] = post;
                MessageLogger($"Adding stage: {lastPost}");
                LastImplicitWithinMod[mod] = mod;
            }
            LastImplicitGlobal = lastPost;
            MessageLogger($"Last implicit global: {lastPost}");
        }
        static Universe()
        {
            MoonSharpExceptionWrapPatch.Install();

            UserData.RegistrationPolicy = new FallbackRegistrationPolicy();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                UserData.RegisterAssembly(assembly, false);
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attributes = type.GetCustomAttributes(true);
                    if (attributes.OfType<MoonSharpUserDataAttribute>().Any())
                    {
                        DelegateRegistry.RegisterDelegatesFromMethodsOf(type);
                    }

                    if (attributes.OfType<PatchManagerModuleAttribute>().FirstOrDefault() is { } pmma)
                    {
                        if (!attributes.OfType<MoonSharpUserDataAttribute>().Any())
                        {
                            Debug.LogWarning($"Universe Preinitialization, found Patch Manager module {pmma.SubmoduleName} without MoonSharpUserData Attribute, skipping!");
                            continue;
                        }

                        SubmoduleTypes[pmma.SubmoduleName] = type;
                    }

                    if (attributes.OfType<ConverterAttribute>().FirstOrDefault() is { } conv)
                    {
                        if (!typeof(IConverter).IsAssignableFrom(type))
                        {
                            Debug.LogWarning($"Universe Preinitialization, found Patch Manager converter {conv.Name} that does not implement IConverter, skipping");
                            continue;
                        }
                        Converters[conv.Name] = (IConverter)Activator.CreateInstance(type);
                    }
                }
            }
        }

        private static PatchManagerScriptLoader _managerScriptLoader = new();
        
        #region Patch Loading

        public int LibraryCount = 0;

        public void LoadSinglePatchFile(FileInfo file, DirectoryInfo directoryInfo)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = Path.GetFileNameWithoutExtension(file.Name),
                    ["Location"] = directoryInfo.FullName,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            patchScript.Globals.RegisterModuleType<JsonModule>();
            try
            {
                patchScript.DoString(File.ReadAllText(file.FullName));
            }
            catch (InterpreterException e)
            {
                ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                ErrorLogger($"{file.FullName} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }
        }
        
        public void LoadPatchesInDirectory(DirectoryInfo directory, string modId)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = modId,
                    ["Location"] = directory.FullName,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            patchScript.Globals.RegisterModuleType<JsonModule>();

            foreach (var file in directory.EnumerateFiles("*.lua", SearchOption.AllDirectories)
                         .Where(f => !f.Name.StartsWith("_")))
            {
                try
                {
                    patchScript.DoString(File.ReadAllText(file.FullName));
                }
                catch (InterpreterException e)
                {
                    ErrorLogger($"{file.FullName} failed to load: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    ErrorLogger($"{file.FullName} failed to load: {e.Message}");
                    ErrorLogger(e.ToString());
                }
            }

            LibraryCount += directory.EnumerateFiles("_*.lua", SearchOption.AllDirectories).Count();
        }

        public void LoadPatchAsset(TextAsset textAsset, string modId)
        {
            var patchScript = new Script(CoreModules.Preset_SoftSandbox)
            {
                Options =
                {
                    ScriptLoader = _managerScriptLoader
                },
                Globals =
                {
                    ["ModId"] = modId,
                    ["PM"] = PatchManagerLibraryInstance
                }
            };
            patchScript.Globals.RegisterModuleType<JsonModule>();
            try
            {
                patchScript.DoString(textAsset.text);
            }
            catch (InterpreterException e)
            {
                ErrorLogger($"{textAsset.name} failed to load: {e.DecoratedMessage}");
                ErrorLogger(e.ToString());
            }
            catch (Exception e)
            {
                ErrorLogger($"{textAsset.name} failed to load: {e.Message}");
                ErrorLogger(e.ToString());
            }
        }
        #endregion

        #region Patch/Stage Registering

        public Dictionary<string, Stage> AllStages = new();
        
        public List<LuaAsset> AllNewAssets = new();
        public Dictionary<string,List<LuaPatch>> AllPatches = new();

        public void AddPatch(LuaPatch patch)
        {
            if (AllPatches.TryGetValue(patch.Label, out var l))
            {
                l.Add(patch);
            }
            else
            {
                AllPatches[patch.Label] = new List<LuaPatch> { patch };
            }
            PatchedLabels.Add(patch.Label);
        }

        public void AddAsset(LuaAsset asset)
        {
            AllNewAssets.Add(asset);
            PatchedLabels.Add(asset.Label);
        }

        public void AddStage(string name, Stage stage)
        {
            AllStages.Add(name, stage);
        }

        #endregion

        #region Stage Sorting

        public Dictionary<string, ulong> SortedStages = new();
        private void SortStages()
        {
            MessageLogger($"Sorting {AllStages.Count} stages");
            List<string> sortedStages = new();
            var hs = AllStages.Keys.ToHashSet();
            foreach (var (k,v) in AllStages)
            {
                v.UpdateRequirements(hs);
            }
            Dictionary<string, Stage> toSort = new(AllStages);
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
                SortedStages[stage] = n++;
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

        #endregion
        
        #region Patch Running

        public int TotalPatchCount;

        public HashSet<string> PatchedLabels = new();

        public void SetupPatchesForRun()
        {
            PatchedLabels = AllPatches.Keys.ToHashSet();
            foreach (var (k, v) in AllPatches)
            {
                v.Sort((x, y) =>
                {
                    var xPrio = SortedStages.GetValueOrDefault(x.Stage, ulong.MaxValue);
                    var yPrio = SortedStages.GetValueOrDefault(y.Stage, ulong.MaxValue);
                    return xPrio.CompareTo(yPrio);
                });
            }
        }

        public JToken RunAllPatchesFor(string label, string name, JToken data, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            IConverter? previousConverter = null;
            DynValue? previousInstance = null;
            bool anyApplied = false;
            foreach (var patch in GetAllSortedPatchesFor(label, name))
            {
                try
                {
                    if (previousInstance == null)
                    {
                        previousConverter = patch.ConverterInstance;
                        previousInstance = patch.ApplyFirst(data);
                    }
                    else if (ReferenceEquals(previousConverter, patch.ConverterInstance))
                    {
                        previousInstance = patch.ApplyInChain(previousInstance);
                    }
                    else
                    {
                        var stringValue = previousConverter.ToJson(previousInstance);
                        previousConverter = patch.ConverterInstance;
                        previousInstance = patch.ApplyFirst(stringValue);
                    }

                    anyApplied = true;
                    patchCount++;
                }
                catch (InterpreterException e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {label}:{name} failed due to: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {label}:{name} failed due to {e.Message}");
                    ErrorLogger(e.ToString());
                }
            }
            return anyApplied ? previousConverter!.ToJson(previousInstance) : data;
        }

        public JToken RunAllPatchesFor(LuaAsset asset, out int patchCount, out int errorCount)
        {
            patchCount = 0;
            errorCount = 0;
            foreach (var patch in GetAllSortedPatchesFor(asset.Label, asset.Name))
            {
                try
                {
                    if (ReferenceEquals(asset.ConverterInstance, patch.ConverterInstance))
                    {
                        asset.CurrentValue = patch.ApplyInChain(asset.CurrentValue);
                    }
                    else
                    {
                        var stringValue = asset.ConverterInstance.ToJson(asset.CurrentValue);
                        asset.ConverterInstance = patch.ConverterInstance;
                        asset.CurrentValue = patch.ApplyFirst(stringValue);
                    }

                    patchCount++;
                }
                catch (InterpreterException e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {asset.Label}:{asset.Name} failed due to: {e.DecoratedMessage}");
                    ErrorLogger(e.ToString());
                }
                catch (Exception e)
                {
                    errorCount++;
                    ErrorLogger($"Patching {asset.Label}:{asset.Name} failed due to {e.Message}");
                    ErrorLogger(e.ToString());
                }
            }

            return asset.ConverterInstance.ToJson(asset.CurrentValue);
        }


        public IEnumerable<LuaPatch> GetAllSortedPatchesFor(string label, string name) =>
            AllPatches.TryGetValue(label, out var patches)
                ? patches.Where(patcher => string.IsNullOrEmpty(patcher.Name) || MatchesPattern(name, patcher.Name))
                : Enumerable.Empty<LuaPatch>();

        #endregion
        
        #region utilities
        public static bool MatchesPattern(string name, string pattern) =>
            Regex.IsMatch(name, $"^{pattern.Replace("*", ".*").Replace("?", ".?")}$");
        #endregion
    }
}