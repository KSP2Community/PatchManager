using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Indexers;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a variable declaration
/// </summary>
public class VariableDeclaration : Node
{
    /// <summary>
    /// The name of the variable being declared
    /// </summary>
    public readonly string Variable;

    /// <summary>
    /// The list of indexers for list creation/indexing
    /// </summary>
    public readonly List<Indexer> Indexers;

    /// <summary>
    /// The value being assigned to the variable
    /// </summary>
    public readonly Expression Value;


    internal VariableDeclaration(Coordinate c, string variable, List<Indexer> indexers, Expression value) : base(c)
    {
        Variable = variable;
        Indexers = indexers;
        Value = value;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        var subEnv = new Environment(environment.GlobalEnvironment, environment);
        var current = DataValue.Null;
        try
        {
            current = environment[Variable];
        }
        catch
        {
            // Ignored
        }

        var result = ComputeValues(subEnv, current);
        environment[Variable] = result;
    }

    private DataValue ComputeValues(Environment subEnv, DataValue current, int layer = 0)
    {
        if (Indexers.Count == layer)
        {
            subEnv["value"] = current;
            return Value.Compute(subEnv);
        }

        var indexer = Indexers[layer];
        switch (indexer)
        {
            case SingleIndexer singleIndexer:
            {
                var index = singleIndexer.Index.Compute(subEnv);
                if (index.IsInteger)
                {
                    return ComputeIntegerIndex(subEnv, current, layer, index, indexer);
                }

                if (index.IsString)
                {
                    return ComputeStringIndex(subEnv, current, layer, index, indexer);
                }

                throw new InterpreterException(indexer.Coordinate,
                    $"Invalid index type {index.Type}, expected Integer or String");
            }
            case EverythingIndexer everythingIndexer:
                return current.Type switch
                {
                    DataValue.DataType.List => current.List.Select(value => ComputeValues(subEnv, value, layer + 1))
                        .ToList(),
                    DataValue.DataType.Dictionary => current.Dictionary
                        .Select(kv => (kv.Key, ComputeValues(subEnv, kv.Value, layer + 1)))
                        .ToDictionary(kv => kv.Key, kv => kv.Item2),
                    DataValue.DataType.None => DataValue.Null,
                    _ => throw new InterpreterException(everythingIndexer.Coordinate,
                        $"Attempting to use a `*` indexer on a value of type {current.Type}")
                };
            default:
                throw new InterpreterException(indexer.Coordinate, $"Unknown indexer type: {indexer.GetType()}");
        }
    }

    private DataValue ComputeStringIndex(
        Environment subEnv,
        DataValue current,
        int layer,
        DataValue index,
        Node indexer
    )
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return current.Type switch
        {
            DataValue.DataType.None => new Dictionary<string, DataValue>
            {
                [index.String] = ComputeValues(subEnv, DataValue.Null, layer + 1)
            },
            DataValue.DataType.Dictionary => new Dictionary<string, DataValue>(current.Dictionary)
            {
                [index.String] = ComputeValues(subEnv,
                    current.Dictionary.TryGetValue(index.String, out var currentValue)
                        ? currentValue
                        : DataValue.Null, layer + 1),
            },
            _ => throw new InterpreterException(indexer.Coordinate,
                $"Attempting to index into a value of type {current.Type} with an indexer that is a String")
        };
    }

    private DataValue ComputeIntegerIndex(
        Environment subEnv,
        DataValue current,
        int layer,
        DataValue index,
        Node indexer
    )
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (current.Type)
        {
            case DataValue.DataType.None:
            {
                var list = new List<DataValue>();
                for (var i = 0; i < index.Integer; i++)
                {
                    list.Add(DataValue.Null);
                }

                list.Add(ComputeValues(subEnv, DataValue.Null, layer + 1));
                return list;
            }
            case DataValue.DataType.List:
            {
                var list = new List<DataValue>(current.List);
                while (list.Count <= (int)index.Integer)
                {
                    list.Add(DataValue.Null);
                }

                list[(int)index.Integer] = ComputeValues(subEnv, list[(int)index.Integer], layer + 1);
                return list;
            }
            default:
                throw new InterpreterException(indexer.Coordinate,
                    $"Attempting to index into a value of type {current.Type} with an indexer that is an Integer");
        }
    }
}