using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Statements;
using PatchManager.Shared.Interfaces;

namespace PatchManager.SassyPatching.Execution;

public class SassyGenerator : ITextAssetGenerator
{// This is a snapshot of the environment before the patch was registered, note it will still reference the same global environment as its file, as that is only mutated by function declarations
    // Same w/ universe environment, as that should only contain stage definitions and such
    private Environment _environmentSnapshot;
    private SelectionBlock _rootSelectionBlock;
    private List<DataValue> _arguments;

    internal SassyGenerator(Environment environmentSnapshot, SelectionBlock rootSelectionBlock, List<DataValue> arguments)
    {
        _environmentSnapshot = environmentSnapshot;
        _rootSelectionBlock = rootSelectionBlock;
        _arguments = arguments;
        var stage = rootSelectionBlock.Attributes.OfType<RunAtStageAttribute>().FirstOrDefault();
        if (stage == null)
        {
            Priority = ulong.MaxValue;
        }
        else
        {
            var global = environmentSnapshot.GlobalEnvironment;
            string stageName;
            if (stage.Stage.Contains(':'))
            {
                stageName = stage.Stage;
            }
            else
            {
                stageName = global.ModGuid + ":" + stage.Stage;
            }

            // If this errors then we don't register the patch, but we should give a more friendly thing to this at some point
            Priority = global.Universe.AllStages[stageName];
        }
    }

    /// <inheritdoc />
    public ulong Priority { get; }
    public string Create(out string label, out string name)
    {
        var val = _rootSelectionBlock.ExecuteCreation(_environmentSnapshot, _arguments);
        label = val.Label;
        name = val.Name;
        return val.Text;
    }
}