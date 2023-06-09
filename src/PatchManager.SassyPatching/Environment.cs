using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes;

namespace PatchManager.SassyPatching;

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
    public readonly Dictionary<string, Value> ScopedValues;

    /// <summary>
    /// Creates a new environment
    /// </summary>
    /// <param name="globalEnvironment">The global environment that this environment will be a part of</param>
    /// <param name="parent">The parent of the environment</param>
    public Environment(GlobalEnvironment globalEnvironment, [CanBeNull] Environment parent = null)
    {
        GlobalEnvironment = globalEnvironment;
        Parent = parent;
        ScopedValues = new Dictionary<string, Value>();
    }

    public Value this[string index]
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
}