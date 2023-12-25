using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;

/// <summary>
/// Represents a loop that iterates over a list/dictionary/string
/// </summary>
public class Each : Node
{
    /// <summary>
    /// The variable to store the keys of iterations of this list node (optional)
    /// </summary>
    [CanBeNull] public readonly string KeyName;
    /// <summary>
    /// Where to store the values of the iterations of this list node
    /// </summary>
    public readonly string ValueName;
    /// <summary>
    /// The expression that returns what is being iterated over
    /// </summary>
    public readonly Expression Iterator;
    public readonly List<Node> Children;
    
    internal Each(Coordinate c, [CanBeNull] string keyName, string valueName, Expression iterator, List<Node> children) : base(c)
    {
        Iterator = iterator;
        Children = children;
        ValueName = valueName;
        KeyName = keyName;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        var value = Iterator.Compute(environment);
        if (value.IsList)
        {
            EachList(environment, value);
            return;
        }

        if (value.IsString)
        {
            EachString(environment, value);
            return;
        }

        if (value.IsDictionary)
        {
            EachDictionary(environment, value);
            return;
        }

        throw new InterpreterException(Coordinate,
            $"cannot iterate over a value of type {value.Type.ToString().ToLowerInvariant()}");
    }

    private void EachDictionary(Environment environment, DataValue value)
    {
        foreach (var (k, v) in value.Dictionary)
        {
            if (KeyName != null)
            {
                environment[KeyName] = k;
            }

            environment[ValueName] = v;
            ExecuteChildren(environment);
        }
    }

    private void EachString(Environment environment, DataValue value)
    {
        if (KeyName != null)
        {
            for (var i = 0; i < value.String.Length; i++)
            {
                environment[KeyName] = i;
                environment[ValueName] = value.String[i];
                ExecuteChildren(environment);
            }
        }
        else
        {
            foreach (var v in value.String)
            {
                environment[ValueName] = v;
                ExecuteChildren(environment);
            }
        }
    }

    private void EachList(Environment environment, DataValue value)
    {
        if (KeyName != null)
        {
            for (var i = 0; i < value.List.Count; i++)
            {
                environment[KeyName] = i;
                environment[ValueName] = value.List[i];
                ExecuteChildren(environment);
            }
        }
        else
        {
            foreach (var v in value.List)
            {
                environment[ValueName] = v;
                ExecuteChildren(environment);
            }
        }
    }

    private void ExecuteChildren(Environment environment)
    {
        foreach (var child in Children)
        {
            child.ExecuteIn(environment);
        }
    }
}