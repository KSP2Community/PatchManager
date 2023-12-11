using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Statements;
using PatchManager.Shared.Interfaces;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// This is the class that all sassy patches get converted to
/// </summary>
public class SassyTextPatcher : ITextPatcher
{
    
    // This is a snapshot of the environment before the patch was registered, note it will still reference the same global environment as its file, as that is only mutated by function declarations
    // Same w/ universe environment, as that should only contain stage definitions and such
    private Environment _environmentSnapshot;
    private SelectionBlock _rootSelectionBlock;

    internal SassyTextPatcher(Environment environmentSnapshot, SelectionBlock rootSelectionBlock)
    {
        _environmentSnapshot = environmentSnapshot;
        _rootSelectionBlock = rootSelectionBlock;
        // var stage = rootSelectionBlock.Attributes.OfType<RunAtStageAttribute>().FirstOrDefault();
        // if (stage == null)
        // {
        //     Priority = ulong.MaxValue;
        // }
        // else
        // {
        //     var global = environmentSnapshot.GlobalEnvironment;
        //     string stageName;
        //     if (stage.Stage.Contains(':'))
        //     {
        //         stageName = stage.Stage;
        //     }
        //     else
        //     {
        //         stageName = global.ModGuid + ":" + stage.Stage;
        //     }
        //
        //     // If this errors then we don't register the patch, but we should give a more friendly thing to this at some point
        //     Priority = global.Universe.AllStages[stageName];
        // }
        var global = environmentSnapshot.GlobalEnvironment;
        var universe = global.Universe;
        ulong minPriority = 0;
        ulong maxPriority = ulong.MaxValue;
        ulong chosenPriority = universe.AllStages[global.ModGuid];
        bool doAverage = false;
        foreach (var attribute in rootSelectionBlock.Attributes)
        {
            switch (attribute)
            {
                case RunBeforeStageAttribute runBeforeStageAttribute:
                {
                    if (universe.AllStages.TryGetValue(runBeforeStageAttribute.Stage, out var minStageValue))
                    {
                        doAverage = true;
                        minPriority = Math.Max(minPriority, minStageValue);
                    } else if (universe.AllStages.TryGetValue($"{global.ModGuid}:{runBeforeStageAttribute.Stage}",
                                   out minStageValue))
                    {
                        doAverage = true;
                        minPriority = Math.Max(minPriority, minStageValue);
                    }
                    break;
                }
                case RunAfterStageAttribute runAfterStageAttribute:
                {
                    if (universe.AllStages.TryGetValue(runAfterStageAttribute.Stage, out var maxStageValue))
                    {
                        doAverage = true;
                        maxPriority = Math.Max(maxPriority, maxStageValue);
                    } else if (universe.AllStages.TryGetValue($"{global.ModGuid}:{runAfterStageAttribute.Stage}",
                                   out maxStageValue))
                    {
                        doAverage = true;
                        maxPriority = Math.Max(maxPriority, maxStageValue);
                    }
                    break;
                }
            }
        }

        if (doAverage)
        {
            chosenPriority = minPriority + ((maxPriority - minPriority) / 2);
        }

        Priority = chosenPriority;
    }

    /// <inheritdoc />
    public ulong Priority { get; }

    /// <inheritdoc />
    public bool TryPatch(string patchType, string name, ref string patchData)
    {
        return _rootSelectionBlock.ExecuteFresh(_environmentSnapshot,patchType, name, ref patchData);
    }
}