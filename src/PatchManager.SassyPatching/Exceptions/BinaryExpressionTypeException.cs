namespace PatchManager.SassyPatching.Exceptions;

public class BinaryExpressionTypeException : InterpreterException
{
    public BinaryExpressionTypeException(Coordinate c, string operation, string firstType, string secondType) : base(c,$"Attempting to {operation} a value of type: {firstType} and a value of type: {secondType} which is not allowed")
    {
    }
}