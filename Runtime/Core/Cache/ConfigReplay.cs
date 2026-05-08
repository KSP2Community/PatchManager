using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PatchManager.Core.Cache.Json;
using PatchManager.Shared;
using ReduxLib.Configuration;
using UnityEngine;

namespace PatchManager.Core.Cache;

/// <summary>
/// Tracks <c>Config:</c> bindings declared by Lua patches so they can be replayed on launches where the
/// patch cache is hot and the scripts therefore do not re-run. Replay re-creates the bindings against the
/// appropriate <see cref="IConfigFile" /> so the settings UI is populated, and compares the current value
/// to the value the patch ran with to flag stale caches that need invalidating.
/// </summary>
public static class ConfigReplay
{
    private static readonly Dictionary<string, IConfigFile> _configMap = new();
    private static readonly Dictionary<string, StandaloneConfigInfo> _standaloneInfo = new();

    /// <summary>
    /// Section/name pairs whose current value differs from the value the patch was last run with.
    /// Populated by <see cref="ReplayAll" />. Each entry is in the form <c>{modId}:{section}/{name}</c>.
    /// </summary>
    public static List<string> ChangedKeys { get; } = new();

    /// <summary>
    /// True if at least one tracked config value has changed since the last patch run, false otherwise.
    /// Callers treat this as a cue to invalidate the patch cache before applying patches.
    /// </summary>
    public static bool HasStaleConfigs => ChangedKeys.Count > 0;

    /// <summary>
    /// Config files for standalone single-file patches whose .lua file still exists on disk. SpaceWarp does
    /// not manage these (no descriptor), so the host is responsible for wiring them into the settings
    /// menu. Populated as standalone patches are resolved in <see cref="GetConfigFileForMod" /> or replayed
    /// in <see cref="ReplayAll" />.
    /// </summary>
    public static IReadOnlyCollection<StandaloneConfigInfo> StandaloneConfigs => _standaloneInfo.Values;

    /// <summary>
    /// Resolves the config file a patch's bindings should live in. Descriptor mods reuse the SpaceWarp
    /// descriptor's <c>ConfigFile</c> so PatchManager bindings land in the same file as the rest of the
    /// mod's config; standalone single-file patches get a sidecar file alongside the .lua.
    /// </summary>
    /// <param name="modId">The mod ID. For standalone patches this is the patch filename stem.</param>
    /// <param name="standaloneLuaPath">For standalone single-file patches, the absolute path to the .lua file. Null for descriptor mods.</param>
    /// <returns>The config file for the given patch.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="standaloneLuaPath" /> is null and no SpaceWarp descriptor is registered for <paramref name="modId" />.</exception>
    public static IConfigFile GetConfigFileForMod(string modId, [CanBeNull] string standaloneLuaPath = null)
    {
        var key = standaloneLuaPath ?? modId;
        if (_configMap.TryGetValue(key, out var existing))
        {
            return existing;
        }

        if (standaloneLuaPath == null)
        {
            if (SpaceWarp2.API.Mods.PluginList.TryGetDescriptor(modId) is { } descriptor)
            {
                return _configMap[key] = descriptor.ConfigFile;
            }

            throw new InvalidOperationException(
                $"No SpaceWarp descriptor for mod '{modId}' and no standalone path supplied; cannot resolve config file."
            );
        }

        var dir = Path.GetDirectoryName(standaloneLuaPath) ?? ".";
        var stem = Path.GetFileNameWithoutExtension(standaloneLuaPath);
        var sidecarPath = Path.Combine(dir, stem + "-config.json");
        var file = new JsonConfigFile(sidecarPath);
        _configMap[key] = file;
        _standaloneInfo[standaloneLuaPath] = new StandaloneConfigInfo(modId, standaloneLuaPath, file);
        return file;
    }

    /// <summary>
    /// Records (or overwrites) a binding into the inventory's replay slice for the given mod. Called from
    /// <see cref="LuaPatching.Builtin.LuaPatchConfig" /> as each <c>Config:</c> method runs.
    /// </summary>
    /// <param name="modId">The mod ID the calling patch was loaded under.</param>
    /// <param name="standaloneLuaPath">For standalone single-file patches, the absolute path to the .lua file. Null for descriptor mods.</param>
    /// <param name="type">Type tag matching the <c>Config:</c> method that produced the entry.</param>
    /// <param name="section">The config-file section the binding lives in.</param>
    /// <param name="name">The binding's key within its section.</param>
    /// <param name="description">Description string the patch declared.</param>
    /// <param name="defaultValue">Default value the patch declared, serialized as JSON.</param>
    /// <param name="ranWith">The current value at the time the patch ran, persisted so subsequent launches can detect changes.</param>
    /// <param name="constraint">Optional constraint declared by the patch.</param>
    public static void RecordEntry(
        string modId,
        [CanBeNull] string standaloneLuaPath,
        string type,
        string section,
        string name,
        string description,
        object defaultValue,
        object ranWith,
        [CanBeNull] IValueConstraint constraint = null
    )
    {
        if (!CacheManager.Inventory.SerializedConfigs.TryGetValue(modId, out var slice))
        {
            CacheManager.Inventory.SerializedConfigs[modId] = slice = new ConfigReplaySlice
            {
                IsStandalone = standaloneLuaPath != null,
                LuaPath = standaloneLuaPath
            };
        }
        else
        {
            slice.IsStandalone = standaloneLuaPath != null;
            slice.LuaPath = standaloneLuaPath;
        }

        slice.Entries[CompositeKey(section, name)] = new ConfigReplayEntry
        {
            Type = type,
            Section = section,
            Name = name,
            Description = description,
            Default = defaultValue == null ? JValue.CreateNull() : JToken.FromObject(defaultValue),
            RanWith = ranWith == null ? JValue.CreateNull() : JToken.FromObject(ranWith),
            Constraint = SerializeConstraint(constraint)
        };
    }

    /// <summary>
    /// Replays every recorded binding against its mod's config file. Re-binds the entry so it shows up in
    /// the settings UI even on hot-cache launches, and flags <see cref="ChangedKeys" /> for any entry whose
    /// current value differs from the recorded ran-with.
    /// </summary>
    /// <remarks>
    /// Should run early at startup, after SpaceWarp has populated <c>PluginList</c> but before deciding
    /// whether to use the patch cache. Slices for standalone patches whose .lua file no longer exists, or
    /// descriptor mods that are not loaded this run, are pruned from the inventory: the recorded ran-with
    /// no longer reflects reality. User-set values stay in the underlying .json config files and are
    /// re-bound automatically when the mod returns and its patches run again.
    /// </remarks>
    public static void ReplayAll()
    {
        ChangedKeys.Clear();
        var configs = CacheManager.Inventory.SerializedConfigs;
        var toPrune = new List<string>();

        foreach (var (modId, slice) in configs)
        {
            IConfigFile file;
            if (slice.IsStandalone)
            {
                if (string.IsNullOrEmpty(slice.LuaPath) || !File.Exists(slice.LuaPath))
                {
                    Logging.LogDebug($"ConfigReplay: standalone '{modId}' missing on disk, pruning replay slice");
                    toPrune.Add(modId);
                    continue;
                }

                file = GetConfigFileForMod(modId, slice.LuaPath);
            }
            else
            {
                if (SpaceWarp2.API.Mods.PluginList.TryGetDescriptor(modId) is not { } descriptor)
                {
                    Logging.LogDebug($"ConfigReplay: descriptor '{modId}' not loaded, pruning replay slice");
                    toPrune.Add(modId);
                    continue;
                }

                file = descriptor.ConfigFile;
                _configMap[modId] = file;
            }

            foreach (var (compositeKey, record) in slice.Entries)
            {
                try
                {
                    RebindAndCheck(file, modId, record);
                }
                catch (Exception e)
                {
                    Logging.LogWarning($"ConfigReplay: failed to replay '{modId}:{compositeKey}' -- {e.Message}");
                }
            }
        }

        foreach (var modId in toPrune)
        {
            configs.Remove(modId);
        }
    }

    private static void RebindAndCheck(IConfigFile file, string modId, ConfigReplayEntry record)
    {
        var section = file.GetOrCreateSection(record.Section, null);
        var constraint = ReconstituteConstraint(record.Constraint, record.Type);

        IConfigEntry entry = record.Type switch
        {
            "bool" => section.Bind(record.Name, record.Default.ToObject<bool>(), record.Description, constraint),
            "float" => section.Bind(record.Name, record.Default.ToObject<double>(), record.Description, constraint),
            "integer" => section.Bind(record.Name, record.Default.ToObject<int>(), record.Description, constraint),
            "string" => section.Bind(record.Name, record.Default.ToObject<string>() ?? "", record.Description, constraint),
            "color" => section.Bind(record.Name, record.Default.ToObject<Color>(), record.Description, constraint),
            _ => null
        };

        if (entry == null) return;

        var currentJson = entry.Value == null ? JValue.CreateNull() : JToken.FromObject(entry.Value);
        if (!JToken.DeepEquals(currentJson, record.RanWith))
        {
            ChangedKeys.Add($"{modId}:{record.Section}/{record.Name}");
        }
    }

    private static string CompositeKey(string section, string name) => $"{section}/{name}";

    [CanBeNull]
    private static ConfigReplayConstraint SerializeConstraint([CanBeNull] IValueConstraint constraint)
    {
        return constraint switch
        {
            RangeConstraint<double> rd => new ConfigReplayConstraint
            {
                Kind = "range",
                Min = JToken.FromObject(rd.Minimum),
                Max = JToken.FromObject(rd.Maximum)
            },
            RangeConstraint<int> ri => new ConfigReplayConstraint
            {
                Kind = "range",
                Min = JToken.FromObject(ri.Minimum),
                Max = JToken.FromObject(ri.Maximum)
            },
            ListConstraint<string> ls => new ConfigReplayConstraint
            {
                Kind = "list",
                Values = JArray.FromObject(ls.AcceptableValues)
            },
            _ => null
        };
    }

    [CanBeNull]
    private static IValueConstraint ReconstituteConstraint([CanBeNull] ConfigReplayConstraint c, string type)
    {
        if (c == null) return null;
        return (c.Kind, type) switch
        {
            ("range", "float") => new RangeConstraint<double>(c.Min.ToObject<double>(), c.Max.ToObject<double>()),
            ("range", "integer") => new RangeConstraint<int>(c.Min.ToObject<int>(), c.Max.ToObject<int>()),
            ("list", "string") => new ListConstraint<string>(c.Values.ToObject<string[]>()),
            _ => null
        };
    }

    /// <summary>
    /// Identity and config-file pair for an standalone single-file patch. Exposed via
    /// <see cref="StandaloneConfigs" /> so the host can wire these files into the settings menu, since
    /// SpaceWarp does not manage them (they have no descriptor).
    /// </summary>
    public sealed class StandaloneConfigInfo
    {
        /// <summary>
        /// The mod ID, equal to the .lua filename stem.
        /// </summary>
        public string ModId { get; }

        /// <summary>
        /// Absolute path to the .lua file.
        /// </summary>
        public string LuaPath { get; }

        /// <summary>
        /// The sidecar config file at <c>{lua-dir}/{stem}-config.json</c>.
        /// </summary>
        public IConfigFile ConfigFile { get; }

        internal StandaloneConfigInfo(string modId, string luaPath, IConfigFile configFile)
        {
            ModId = modId;
            LuaPath = luaPath;
            ConfigFile = configFile;
        }
    }
}
