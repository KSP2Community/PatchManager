using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// Describes a local environment/scope (per function/patch) used by the patching engine
/// </summary>
public class Environment
{
    /// <summary>
    /// The global environment that this environment is a part of
    /// </summary>
    public GlobalEnvironment GlobalEnvironment;
    /// <summary>
    /// The parent of this scope
    /// </summary>
    [CanBeNull] public readonly Environment Parent;
    /// <summary>
    /// The list of variables in this scope
    /// </summary>
    public Dictionary<string, DataValue> ScopedValues;


    [CanBeNull] private List<Node> _slotActions;

    [CanBeNull]
    public List<Node> SlotActions
    {
        get => _slotActions ?? Parent?.SlotActions;
        set => _slotActions = value;
    }
    
    /// <summary>
    /// Creates a new environment
    /// </summary>
    /// <param name="globalEnvironment">The global environment that this environment will be a part of</param>
    /// <param name="parent">The parent of the environment</param>
    public Environment(GlobalEnvironment globalEnvironment, [CanBeNull] Environment parent = null)
    {
        GlobalEnvironment = globalEnvironment;
        Parent = parent;
        ScopedValues = new Dictionary<string, DataValue>();
    }

    public DataValue this[string index]
    {
        get
        {
            if (ScopedValues.TryGetValue(index, out var result))
            {
                return result;
            }

            if (Parent != null)
            {
                return Parent[index];
            }
            
            throw new KeyNotFoundException(index);
        }
        set
        {
            if (value.IsDeletion)
            {
                if (ScopedValues.ContainsKey(index))
                {
                    ScopedValues.Remove(index);
                }
            
                throw new KeyNotFoundException(index);
            }
            else
            {
                ScopedValues[index] = value;
            }
        }
    }

    /// <summary>
    /// Takes a "snapshot" of this environment, for later action taken within the environment, such as in selection actions
    /// </summary>
    /// <returns>A copy of this environment, in its current state, w/ each parent in their current state as well</returns>
    public Environment Snapshot()
    {
        var scopedValues = new Dictionary<string, DataValue>(ScopedValues);
        var parent = Parent?.Snapshot();
        return new Environment(GlobalEnvironment, parent)
        {
            ScopedValues = scopedValues,
            _slotActions = _slotActions
        };
    }
}