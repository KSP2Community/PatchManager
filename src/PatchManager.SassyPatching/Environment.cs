using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes;

namespace PatchManager.SassyPatching;

public class Environment
{
    public GlobalEnvironment GlobalEnvironment;
    [CanBeNull] public Environment Parent;
    public Dictionary<string, Value> ScopedValues;

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
        set => ScopedValues[index] = value;
    }
}