namespace PatchManager.SassyPatching.Exceptions;

public class UnaryTypeException : InterpreterException
{
    public UnaryTypeException(Coordinate coordinate, string operation, string firstType) : base(coordinate,$"Attempting to {operation} a value of type: {firstType} which is not allowed")
    {
    }
}