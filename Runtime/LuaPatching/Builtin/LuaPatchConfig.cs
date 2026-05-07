using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using PatchManager.Core.Cache;
using ReduxLib.Configuration;
using UnityEngine;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// Redux configs exposed to patch manager. Bindings declared through this object are recorded in
/// <see cref="ConfigReplay" /> so changes to their values invalidate the patch cache.
/// </summary>
[MoonSharpUserData]
public class LuaPatchConfig
{
    /// <summary>
    /// The current config file.
    /// </summary>
    [MoonSharpHidden] public IConfigFile ConfigFile;

    /// <summary>
    /// The mod ID the script that owns this <see cref="LuaPatchConfig" /> was loaded under. Used as the
    /// inventory slice key so <see cref="ConfigReplay" /> can replay the bindings on hot-cache launches.
    /// </summary>
    [MoonSharpHidden] public string ModId;

    /// <summary>
    /// For standalone single-file patches (no SpaceWarp descriptor), the absolute path to the .lua file.
    /// Null for descriptor-mod patches. Persisted in the replay slice so subsequent launches can verify
    /// the file still exists before re-binding its settings.
    /// </summary>
    [MoonSharpHidden] public string StandaloneLuaPath;

    /// <summary>
    /// Binds a boolean config for use in the patching engine. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value, <c>false</c> if not passed.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public bool Bool(string section, string name, bool defaultValue = false, string description = "")
    {
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description);
        var ranWith = entry.Value;
        AddToReplay("bool", section, name, description, defaultValue, ranWith);
        return (bool)ranWith;
    }

    /// <summary>
    /// Binds a floating-point config for use in the patching engine. Any changes to the value will
    /// trigger an invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value, <c>0</c> if not passed.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public double Float(string section, string name, double defaultValue = 0, string description = "")
    {
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description);
        var ranWith = entry.Value;
        AddToReplay("float", section, name, description, defaultValue, ranWith);
        return Convert.ToDouble(ranWith);
    }

    /// <summary>
    /// Binds a floating-point config constrained to a range. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="min">The minimum acceptable value, inclusive.</param>
    /// <param name="max">The maximum acceptable value, inclusive.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public double Float(string section, string name, double defaultValue, double min, double max, string description = "")
    {
        var constraint = new RangeConstraint<double>(min, max);
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description, constraint);
        var ranWith = entry.Value;
        AddToReplay("float", section, name, description, defaultValue, ranWith, constraint);
        return Convert.ToDouble(ranWith);
    }

    /// <summary>
    /// Binds an integer config for use in the patching engine. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value, <c>0</c> if not passed.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public int Integer(string section, string name, int defaultValue = 0, string description = "")
    {
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description);
        var ranWith = entry.Value;
        AddToReplay("integer", section, name, description, defaultValue, ranWith);
        return Convert.ToInt32(ranWith);
    }

    /// <summary>
    /// Binds an integer config constrained to a range. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="min">The minimum acceptable value, inclusive.</param>
    /// <param name="max">The maximum acceptable value, inclusive.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public int Integer(string section, string name, int defaultValue, int min, int max, string description = "")
    {
        var constraint = new RangeConstraint<int>(min, max);
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description, constraint);
        var ranWith = entry.Value;
        AddToReplay("integer", section, name, description, defaultValue, ranWith, constraint);
        return Convert.ToInt32(ranWith);
    }

    /// <summary>
    /// Binds a string config for use in the patching engine. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value, empty string if not passed.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public string String(string section, string name, string defaultValue = "", string description = "")
    {
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description);
        var ranWith = entry.Value;
        AddToReplay("string", section, name, description, defaultValue, ranWith);
        return (string)ranWith;
    }

    /// <summary>
    /// Binds a string config constrained to a list of acceptable values. Any changes to the value will
    /// trigger an invalidation of the patch cache on the next launch.
    /// </summary>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="acceptableValues">The set of values the config will accept.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value.</returns>
    public string String(string section, string name, string defaultValue, string[] acceptableValues, string description = "")
    {
        var constraint = new ListConstraint<string>(acceptableValues);
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultValue, description, constraint);
        var ranWith = entry.Value;
        AddToReplay("string", section, name, description, defaultValue, ranWith, constraint);
        return (string)ranWith;
    }

    /// <summary>
    /// Binds a color config for use in the patching engine. Any changes to the value will trigger an
    /// invalidation of the patch cache on the next launch. Colors are passed and returned as Lua tables;
    /// either keyed (<c>{r=, g=, b=, a=}</c>) or indexed (<c>{1, 0.5, 0, 1}</c>) input is accepted, and
    /// output is keyed.
    /// </summary>
    /// <param name="ctx">MoonSharp execution context, supplied automatically by the runtime.</param>
    /// <param name="section">The section of the config file this will be bound in.</param>
    /// <param name="name">The name of the value.</param>
    /// <param name="defaultValue">The default value as a Lua table, transparent black if not passed.</param>
    /// <param name="description">The description of the config value.</param>
    /// <returns>The current config value as a Lua table with <c>r</c>, <c>g</c>, <c>b</c>, <c>a</c> fields.</returns>
    public Table Color(ScriptExecutionContext ctx, string section, string name, Table defaultValue = null, string description = "")
    {
        var defaultColor = TableToColor(defaultValue);
        var sect = ConfigFile.GetOrCreateSection(section, null);
        var entry = sect.Bind(name, defaultColor, description);
        var ranWith = (Color)entry.Value;
        AddToReplay("color", section, name, description, defaultColor, ranWith);
        return ColorToTable(ctx.GetScript(), ranWith);
    }

    private static Color TableToColor(Table t)
    {
        if (t == null) return default;
        return new Color(
            ReadChannel(t, 1, "r", "red", 0f),
            ReadChannel(t, 2, "g", "green", 0f),
            ReadChannel(t, 3, "b", "blue", 0f),
            ReadChannel(t, 4, "a", "alpha", 1f)
        );
    }

    private static float ReadChannel(Table t, int index, string shortKey, string longKey, float fallback)
    {
        var v = t.Get(shortKey);
        if (v.Type == DataType.Number) return (float)v.Number;
        v = t.Get(longKey);
        if (v.Type == DataType.Number) return (float)v.Number;
        v = t.Get(index);
        if (v.Type == DataType.Number) return (float)v.Number;
        return fallback;
    }

    private static Table ColorToTable(Script script, Color c)
    {
        var t = new Table(script);
        t["r"] = (double)c.r;
        t["g"] = (double)c.g;
        t["b"] = (double)c.b;
        t["a"] = (double)c.a;
        return t;
    }


    private void AddToReplay(string type, string section, string name, string description, object def, object ran, [CanBeNull] IValueConstraint constraint = null)
    {
        ConfigReplay.RecordEntry(ModId, StandaloneLuaPath, type, section, name, description, def, ran, constraint);
    }
}
