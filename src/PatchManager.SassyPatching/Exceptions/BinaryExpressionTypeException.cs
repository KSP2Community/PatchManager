namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// An exception thrown when the operands of a binary expression do not match together for the expression
/// </summary>
public class BinaryExpressionTypeException : InterpreterException
{
    internal BinaryExpressionTypeException(Coordinate c, string operation, string firstType, string secondType) : base(c,$"Attempting to {operation} a value of type: {firstType} and a value of type: {secondType} which is not allowed")
    {
    }
}