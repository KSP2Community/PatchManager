using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// A binary expression which indexes the left hand side by the right hand side and returns the result
/// </summary>
public class Subscript : Binary
{
    internal Subscript(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // ReSharper disable once CognitiveComplexity
    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        try
        {
            return leftHandSide[rightHandSide];
        }
        catch (IndexOutOfRangeException indexOutOfRangeException)
        {
            throw new ListIndexOutOfRangeException(Coordinate, indexOutOfRangeException.Message);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            throw new DictionaryKeyNotFoundException(Coordinate, keyNotFoundException.Message);
        }
        catch (DataValueOperationException)
        {
            throw new BinaryExpressionTypeException(Coordinate, "subscript", leftHandSide.Type.ToString(),
                rightHandSide.Type.ToString());
        }
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}