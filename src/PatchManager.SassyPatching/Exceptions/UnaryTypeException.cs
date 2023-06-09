namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// An exception thrown when a unary expression is being run on a type that which it cannot
/// </summary>
public class UnaryTypeException : InterpreterException
{
    internal UnaryTypeException(Coordinate coordinate, string operation, string firstType) : base(coordinate,$"Attempting to {operation} a value of type: {firstType} which is not allowed")
    {
    }
}