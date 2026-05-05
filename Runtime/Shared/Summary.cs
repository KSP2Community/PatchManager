using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Unity.VisualScripting;
using VehiclePhysics;

namespace PatchManager.Shared;

/// <summary>
/// This is a class that holds the summary state for patches
/// </summary>
public class Summary
{
    /// <summary>
    /// A list of recognized mod names in the patching system
    /// </summary>
    public HashSet<string> RecognizedModIds = new();
    
    /// <summary>
    /// The files that failed even loading
    /// </summary>
    public List<(string filename, string reason)> ErroredFiles = new();

    /// <summary>
    /// What was the state of the application
    /// </summary>
    public enum ApplicationState
    {
        /// <summary>
        /// This patch was applied
        /// </summary>
        Applied,

        /// <summary>
        /// This patch removed the given asset from the result
        /// </summary>
        Removed,

        /// <summary>
        /// This patch skipped running
        /// </summary>
        Skipped,

        /// <summary>
        /// This patch had an error while running
        /// </summary>
        Errored,
    }

    /// <summary>
    /// This is an entry into the patch summary
    /// </summary>
    public class SummaryEntry
    {
        /// <summary>
        /// The name of the patch that is this summary entry
        /// </summary>
        public string Name;

        /// <summary>
        /// What was the result of this patch
        /// </summary>
        public ApplicationState State;

        /// <summary>
        /// Why was the patch skipped, or why did it error
        /// </summary>
        [CanBeNull] public string Context;
    }

    /// <summary>
    /// This is all the patches that were removed during the setup phase
    /// </summary>
    public List<(string patchName, string status, string context)> RemovedPatches = new();


    /// <summary>
    /// All of the current summaries
    /// </summary>
    public List<(string labelName, List<(string assetName, string address, List<SummaryEntry> entries)>)> Summaries =
        new();

    [CanBeNull] private List<(string assetName, string address, List<SummaryEntry> entries)> _currentLabel;
    [CanBeNull] private List<SummaryEntry> _currentEntries;

    /// <summary>
    /// Mark a patch as removed
    /// </summary>
    /// <param name="name">The patch name</param>
    /// <param name="status">The short status reason (e.g. <c>MISSING</c>, <c>CONFLICT</c>, <c>CYCLE</c>).</param>
    /// <param name="context">Optional detail describing the status (e.g. the missing mod or conflicting patch).</param>
    public void Remove(string name, string status, [CanBeNull] string context = null)
    {
        RemovedPatches.Add((name, status, context));
    }

    /// <summary>
    /// Note that we are beginning the given label in the summary
    /// </summary>
    /// <param name="labelName">The label name</param>
    public void BeginLabel(string labelName)
    {
        var result = new List<(string assetName, string address, List<SummaryEntry> entries)>();
        Summaries.Add((labelName, result));
        _currentLabel = result;
    }

    /// <summary>
    /// Note that we are starting to patch the given asset in the summary
    /// </summary>
    /// <param name="assetName">The name of the asset as targeted via :Named</param>
    /// <param name="address">The address of the label</param>
    public void BeginAsset(string assetName, string address)
    {
        var result = new List<SummaryEntry>();
        _currentLabel!.Add((assetName, address, result));
        _currentEntries = result;
    }

    /// <summary>
    /// Number of brand-new assets created by patches in this run.
    /// </summary>
    public int NewAssetCount;

    /// <summary>
    /// Like <see cref="BeginAsset" />, but flags this asset as a brand-new one created during patching so it counts in <see cref="NewAssetCount" />.
    /// </summary>
    /// <param name="assetName">The new asset's name.</param>
    /// <param name="address">The asset's addressables address.</param>
    public void BeginNewAsset(string assetName, string address)
    {
        NewAssetCount++;
        BeginAsset(assetName, address);
    }

    /// <summary>
    /// Mark a patch as applied
    /// </summary>
    /// <param name="patchName">The patch to mark as applied</param>
    public void Apply(string patchName)
    {
        _currentEntries!.Add(new SummaryEntry
        {
            Name = patchName,
            State = ApplicationState.Applied,
        });
    }

    /// <summary>
    /// Mark a patch as having removed an asset
    /// </summary>
    /// <param name="patchName">The patch that removed the asset</param>
    public void RemovedAsset(string patchName)
    {
        _currentEntries!.Add(new SummaryEntry
        {
            Name = patchName,
            State = ApplicationState.Removed,
        });
    }

    /// <summary>
    /// Mark a patch as having been skipped
    /// </summary>
    /// <param name="patchName">The patch that was skipped</param>
    /// <param name="reason">The reason that the patch was skipped</param>
    public void Skip(string patchName, string reason)
    {
        _currentEntries!.Add(new SummaryEntry
        {
            Name = patchName,
            State = ApplicationState.Skipped,
            Context = reason,
        });
    }

    /// <summary>
    /// Mark a patch as having errored out
    /// </summary>
    /// <param name="patchName">The patch that errored out</param>
    /// <param name="reason">The error message</param>
    public void Error(string patchName, string reason)
    {
        _currentEntries!.Add(new SummaryEntry
        {
            Name = patchName,
            State = ApplicationState.Errored,
            Context = reason,
        });
    }
    
    /// <summary>
    /// Mark a patch as having errored out
    /// </summary>
    /// <param name="patchName">The patch that errored out</param>
    /// <param name="reason">The actual exception</param>
    public void Error(string patchName, Exception reason)
    {
        _currentEntries!.Add(new SummaryEntry
        {
            Name = patchName,
            State = ApplicationState.Errored,
            Context = ContextFromException(reason),
        });
    }

    /// <summary>
    /// Mark a file as having failed to load
    /// </summary>
    /// <param name="filename">The file that failed to load</param>
    /// <param name="reason">The actual exception</param>
    public void ErrorFile(string filename, Exception reason)
    {
        ErroredFiles.Add((filename, ContextFromException(reason)));
    }

    private static string ContextFromException(Exception reason)
    {
        if (reason is InterpreterException e)
        {
            return e.DecoratedMessage ?? e.Message ?? "<no message>";
        }
        return string.IsNullOrEmpty(reason.StackTrace) ? reason.Message.Trim() : reason.Message.Trim() + "\n" + reason.StackTrace;
    }

    /// <summary>
    /// Dumps the entire summary to a string
    /// </summary>
    /// <returns>a string form of the summary</returns>
    public string Dump()
    {
        var sb = new StringBuilder();

        var allEntries = Summaries.SelectMany(s => s.Item2).SelectMany(a => a.entries).ToList();
        var totalPatches = allEntries.Count(p => p.State == ApplicationState.Applied || p.State == ApplicationState.Removed);
        var patchedAssets = Summaries
            .SelectMany(s => s.Item2)
            .Count(a => a.entries.Any(p => p.State == ApplicationState.Applied || p.State == ApplicationState.Removed));
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

        sb.AppendLine("Recognized Mod IDs:");
        foreach (var id in RecognizedModIds)
        {
            sb.AppendLine($"    {id}");
        }
        sb.AppendLine("");
        if (ErroredFiles.Count > 0)
        {
            sb.AppendLine("Errored Lua Files:");
            var nameWidth = ErroredFiles.Max(x => x.filename.Length);
            foreach (var (name, reason) in ErroredFiles)
            {
                sb.Append("    ");
                sb.Append(name.PadRight(nameWidth));
                sb.Append("    ");
                sb.AppendLine("ERRORED");
                if (!string.IsNullOrEmpty(reason))
                {
                    AppendContext(sb, reason, "        ");
                }
            }
            sb.AppendLine("");
        }

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

        foreach (var (label, entries) in Summaries
                     .Where(x => x.Item2
                         .Any(y => y.entries.Count > 0)))
        {
            sb.AppendLine($"Label - {label}:");
            var labelNameWidth = entries
                .Where(e => e.entries.Count > 0)
                .SelectMany(e => e.entries)
                .Max(p => p.Name.Length);
            foreach (var (name, address, patches) in entries.Where(e => e.entries.Count > 0))
            {
                sb.AppendLine($"    Asset - {name}:");
                var nameWidth = labelNameWidth;
                foreach (var patch in patches)
                {
                    sb.Append("        ");
                    sb.Append(patch.Name.PadRight(nameWidth));
                    sb.Append("    ");
                    switch (patch.State)
                    {
                        case ApplicationState.Applied:
                            sb.AppendLine("APPLIED");
                            break;
                        case ApplicationState.Removed:
                            sb.AppendLine("REMOVED TARGET");
                            break;
                        case ApplicationState.Skipped:
                            sb.AppendLine("SKIPPED");
                            if (!string.IsNullOrEmpty(patch.Context))
                            {
                                AppendContext(sb, patch.Context, "            ");
                            }
                            break;
                        case ApplicationState.Errored:
                            sb.AppendLine("ERRORED");
                            if (!string.IsNullOrEmpty(patch.Context))
                            {
                                AppendContext(sb, patch.Context, "            ");
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
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