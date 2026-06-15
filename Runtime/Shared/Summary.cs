using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;

namespace PatchManager.Shared;

/// <summary>
/// Per-run summary of patch loading and application across the Early, Default, and Late passes.
/// </summary>
public class Summary
{
    /// <summary>
    /// Mod IDs the patching system has registered.
    /// </summary>
    public HashSet<string> RecognizedModIds = new();

    /// <summary>
    /// Lua files that failed to load.
    /// </summary>
    public List<(string filename, string reason)> ErroredFiles = new();

    /// <summary>
    /// Result of a single application of a patch to an asset.
    /// </summary>
    public enum ApplicationState
    {
        /// <summary>The patch was applied.</summary>
        Applied,

        /// <summary>The patch removed the target asset from the result.</summary>
        Removed,

        /// <summary>The patch was skipped.</summary>
        Skipped,

        /// <summary>The patch errored while running.</summary>
        Errored,
    }

    /// <summary>
    /// One record of an apply, skip, error, or remove for a single patch against a single asset within
    /// a specific pass.
    /// </summary>
    public class SummaryEntry
    {
        /// <summary>The name of the patch the entry corresponds to.</summary>
        public string Name;

        /// <summary>The pass the patch ran in.</summary>
        public PatchDefinition.PatchPass Pass;

        /// <summary>The application state of the patch.</summary>
        public ApplicationState State;

        /// <summary>Reason the patch was skipped, or the error context if it errored.</summary>
        [CanBeNull] public string Context;
    }

    /// <summary>
    /// Address and per-pass patch entries recorded for a single asset.
    /// </summary>
    public class AssetSummary
    {
        /// <summary>
        /// The asset's name as targeted via <c>:Named</c>.
        /// </summary>
        public string AssetName;

        /// <summary>
        /// The asset's addressables address.
        /// </summary>
        public string Address;

        /// <summary>
        /// Patch entries recorded for this asset, keyed by the pass they ran in.
        /// </summary>
        public Dictionary<PatchDefinition.PatchPass, List<SummaryEntry>> EntriesByPass = new();

        /// <summary>
        /// True if any pass recorded at least one entry for this asset, false otherwise.
        /// </summary>
        public bool HasEntries => EntriesByPass.Values.Any(l => l.Count > 0);

        /// <summary>
        /// Total number of entries across every pass.
        /// </summary>
        public int EntryCount => EntriesByPass.Values.Sum(l => l.Count);
    }

    /// <summary>
    /// Patches that were removed during the setup phase before any pass ran.
    /// </summary>
    public List<(string patchName, string status, string context)> RemovedPatches = new();

    /// <summary>
    /// Per-label asset summaries, in the order labels were first touched.
    /// </summary>
    public List<(string labelName, List<AssetSummary> assets)> Summaries = new();

    [CanBeNull] private List<AssetSummary> _currentLabel;
    [CanBeNull] private AssetSummary _currentAsset;
    private PatchDefinition.PatchPass _currentPass = PatchDefinition.PatchPass.Default;

    /// <summary>
    /// Marks a patch as removed during setup (failed mod or patch constraint, or caught in a cycle).
    /// </summary>
    /// <param name="name">The patch name.</param>
    /// <param name="status">Short status reason (<c>MISSING</c>, <c>CONFLICT</c>, <c>CYCLE</c>).</param>
    /// <param name="context">Optional detail describing the status.</param>
    public void Remove(string name, string status, [CanBeNull] string context = null)
    {
        RemovedPatches.Add((name, status, context));
    }

    /// <summary>
    /// Sets the pass that subsequent <see cref="Apply" />, <see cref="Skip" />,
    /// <see cref="Error(string, string)" />, and <see cref="RemovedAsset" /> calls are stamped with.
    /// </summary>
    /// <param name="pass">The pass currently being run.</param>
    public void BeginPass(PatchDefinition.PatchPass pass)
    {
        _currentPass = pass;
    }

    /// <summary>
    /// Begins or resumes the given label.
    /// </summary>
    /// <remarks>
    /// Reuses the existing asset list when the label has already been started so multi-pass entries
    /// accumulate under one heading.
    /// </remarks>
    /// <param name="labelName">The label name.</param>
    public void BeginLabel(string labelName)
    {
        for (var i = 0; i < Summaries.Count; i++)
        {
            if (Summaries[i].labelName == labelName)
            {
                _currentLabel = Summaries[i].assets;
                return;
            }
        }

        var result = new List<AssetSummary>();
        Summaries.Add((labelName, result));
        _currentLabel = result;
    }

    /// <summary>
    /// Begins or resumes the given asset under the current label. Reuses the existing
    /// <see cref="AssetSummary" /> when the asset has been started before so multi-pass entries
    /// accumulate under one asset heading.
    /// </summary>
    /// <param name="assetName">The asset's name.</param>
    /// <param name="address">The addressables address.</param>
    public void BeginAsset(string assetName, string address)
    {
        foreach (var asset in _currentLabel!)
        {
            if (asset.AssetName == assetName)
            {
                _currentAsset = asset;
                return;
            }
        }

        var fresh = new AssetSummary { AssetName = assetName, Address = address };
        _currentLabel.Add(fresh);
        _currentAsset = fresh;
    }

    /// <summary>
    /// Number of brand-new assets created by patches in this run.
    /// </summary>
    public int NewAssetCount;

    /// <summary>
    /// Like <see cref="BeginAsset" /> but counts the asset as a brand-new creation.
    /// </summary>
    /// <param name="assetName">The new asset's name.</param>
    /// <param name="address">The asset's addressables address.</param>
    public void BeginNewAsset(string assetName, string address)
    {
        NewAssetCount++;
        BeginAsset(assetName, address);
    }

    /// <summary>
    /// Marks a patch as applied in the current pass.
    /// </summary>
    /// <param name="patchName">The patch to mark.</param>
    public void Apply(string patchName)
    {
        AddEntry(new SummaryEntry
        {
            Name = patchName,
            Pass = _currentPass,
            State = ApplicationState.Applied,
        });
    }

    /// <summary>
    /// Marks a patch as having removed an asset in the current pass.
    /// </summary>
    /// <param name="patchName">The patch that removed the asset.</param>
    public void RemovedAsset(string patchName)
    {
        AddEntry(new SummaryEntry
        {
            Name = patchName,
            Pass = _currentPass,
            State = ApplicationState.Removed,
        });
    }

    /// <summary>
    /// Marks a patch as skipped in the current pass.
    /// </summary>
    /// <param name="patchName">The patch.</param>
    /// <param name="reason">Skip reason.</param>
    public void Skip(string patchName, string reason)
    {
        AddEntry(new SummaryEntry
        {
            Name = patchName,
            Pass = _currentPass,
            State = ApplicationState.Skipped,
            Context = reason,
        });
    }

    /// <summary>
    /// Marks a patch as errored in the current pass.
    /// </summary>
    /// <param name="patchName">The patch.</param>
    /// <param name="reason">Error message.</param>
    public void Error(string patchName, string reason)
    {
        AddEntry(new SummaryEntry
        {
            Name = patchName,
            Pass = _currentPass,
            State = ApplicationState.Errored,
            Context = reason,
        });
    }

    /// <summary>
    /// Marks a patch as errored in the current pass.
    /// </summary>
    /// <param name="patchName">The patch.</param>
    /// <param name="reason">The actual exception.</param>
    public void Error(string patchName, Exception reason)
    {
        AddEntry(new SummaryEntry
        {
            Name = patchName,
            Pass = _currentPass,
            State = ApplicationState.Errored,
            Context = ContextFromException(reason),
        });
    }

    /// <summary>
    /// Marks a Lua file as having failed to load.
    /// </summary>
    /// <param name="filename">The file.</param>
    /// <param name="reason">The actual exception.</param>
    public void ErrorFile(string filename, Exception reason)
    {
        ErroredFiles.Add((filename, ContextFromException(reason)));
    }

    private void AddEntry(SummaryEntry entry)
    {
        if (_currentAsset == null) return;
        if (!_currentAsset.EntriesByPass.TryGetValue(_currentPass, out var list))
        {
            list = new List<SummaryEntry>();
            _currentAsset.EntriesByPass[_currentPass] = list;
        }
        list.Add(entry);
    }

    private static string ContextFromException(Exception reason)
    {
        if (reason is InterpreterException e)
        {
            return e.DecoratedMessage ?? e.Message ?? "<no message>";
        }
        return string.IsNullOrEmpty(reason.StackTrace)
            ? reason.Message.Trim()
            : reason.Message.Trim() + "\n" + reason.StackTrace;
    }

    private IEnumerable<SummaryEntry> AllEntries =>
        Summaries.SelectMany(s => s.assets).SelectMany(a => a.EntriesByPass.Values).SelectMany(l => l);

    /// <summary>
    /// Renders the summary as a string for the patch summary log.
    /// </summary>
    /// <returns>The rendered summary.</returns>
    public string Dump()
    {
        var sb = new StringBuilder();

        var allEntries = AllEntries.ToList();
        var totalPatches = allEntries.Count(p => p.State == ApplicationState.Applied || p.State == ApplicationState.Removed);
        var patchedAssets = Summaries
            .SelectMany(s => s.assets)
            .Count(a => a.EntriesByPass.Values.Any(l =>
                l.Any(p => p.State == ApplicationState.Applied || p.State == ApplicationState.Removed)));
        var errors = allEntries.Count(p => p.State == ApplicationState.Errored) + ErroredFiles.Count;

        sb.AppendLine("Statistics:");
        var statLabels = new[] { "Total Patches", "Patched Assets", "New Assets", "Errors" };
        var statValues = new[] { totalPatches, patchedAssets, NewAssetCount, errors };
        var statLabelWidth = statLabels.Max(s => s.Length);
        for (var i = 0; i < statLabels.Length; i++)
        {
            sb.Append("    ");
            sb.Append(statLabels[i].PadRight(statLabelWidth));
            sb.Append("    ");
            sb.AppendLine(statValues[i].ToString());
        }
        sb.AppendLine("");

        sb.AppendLine("Per-Pass Statistics:");
        foreach (PatchDefinition.PatchPass pass in Enum.GetValues(typeof(PatchDefinition.PatchPass)))
        {
            var passEntries = allEntries.Where(p => p.Pass == pass).ToList();
            if (passEntries.Count == 0) continue;
            var applied = passEntries.Count(p => p.State == ApplicationState.Applied || p.State == ApplicationState.Removed);
            var skipped = passEntries.Count(p => p.State == ApplicationState.Skipped);
            var errored = passEntries.Count(p => p.State == ApplicationState.Errored);
            sb.AppendLine($"    {pass,-7}    Applied: {applied}    Skipped: {skipped}    Errored: {errored}");
        }
        sb.AppendLine("");

        var erroredWithContext = Summaries
            .SelectMany(s => s.assets.SelectMany(a =>
                a.EntriesByPass.SelectMany(kv =>
                    kv.Value
                        .Where(e => e.State == ApplicationState.Errored)
                        .Select(e => (label: s.labelName, asset: a.AssetName, pass: kv.Key, entry: e)))))
            .ToList();

        if (erroredWithContext.Count > 0 || ErroredFiles.Count > 0)
        {
            sb.AppendLine("All Errors:");
            foreach (var (filename, reason) in ErroredFiles)
            {
                sb.AppendLine($"    [Lua File] {filename}");
                if (!string.IsNullOrEmpty(reason))
                {
                    AppendContext(sb, reason, "        ");
                }
            }
            foreach (var (label, asset, pass, entry) in erroredWithContext)
            {
                sb.AppendLine($"    [{pass,-7}] {label} / {asset} / {entry.Name}");
                if (!string.IsNullOrEmpty(entry.Context))
                {
                    AppendContext(sb, entry.Context, "        ");
                }
            }
            sb.AppendLine("");
        }

        sb.AppendLine("Recognized Mod IDs:");
        foreach (var id in RecognizedModIds)
        {
            sb.AppendLine($"    {id}");
        }
        sb.AppendLine("");

        if (RemovedPatches.Count > 0)
        {
            sb.AppendLine("Removed Patches:");
            var nameWidth = RemovedPatches.Max(x => x.patchName.Length);
            foreach (var (name, status, context) in RemovedPatches)
            {
                sb.Append("    ");
                sb.Append(name.PadRight(nameWidth));
                sb.Append("    ");
                sb.AppendLine(status);
                if (!string.IsNullOrEmpty(context))
                {
                    AppendContext(sb, context, "        ");
                }
            }
            sb.AppendLine("");
        }

        foreach (var (label, assets) in Summaries.Where(x => x.assets.Any(a => a.HasEntries)))
        {
            sb.AppendLine($"Label - {label}:");
            var nameWidth = assets
                .Where(a => a.HasEntries)
                .SelectMany(a => a.EntriesByPass.Values).SelectMany(l => l)
                .Max(p => p.Name.Length);
            foreach (var asset in assets.Where(a => a.HasEntries))
            {
                sb.AppendLine($"    Asset - {asset.AssetName}:");
                foreach (var pass in asset.EntriesByPass.Keys.OrderBy(p => (int)p))
                {
                    var entries = asset.EntriesByPass[pass];
                    if (entries.Count == 0) continue;
                    sb.AppendLine($"        Pass - {pass}:");
                    foreach (var entry in entries)
                    {
                        sb.Append("            ");
                        sb.Append(entry.Name.PadRight(nameWidth));
                        sb.Append("    ");
                        switch (entry.State)
                        {
                            case ApplicationState.Applied:
                                sb.AppendLine("APPLIED");
                                break;
                            case ApplicationState.Removed:
                                sb.AppendLine("REMOVED TARGET");
                                break;
                            case ApplicationState.Skipped:
                                sb.AppendLine("SKIPPED");
                                if (!string.IsNullOrEmpty(entry.Context))
                                {
                                    AppendContext(sb, entry.Context, "                ");
                                }
                                break;
                            case ApplicationState.Errored:
                                sb.AppendLine("ERRORED");
                                if (!string.IsNullOrEmpty(entry.Context))
                                {
                                    AppendContext(sb, entry.Context, "                ");
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            sb.AppendLine("");
        }

        return sb.ToString();
    }

    private static void AppendContext(StringBuilder sb, string context, string indent)
    {
        var lines = context.Replace("\r", "").Split('\n');
        foreach (var line in lines)
        {
            sb.Append(indent);
            sb.AppendLine(line);
        }
    }
}
