namespace PatchManager.SassyPatching.Exceptions;

public class InvalidVariableReferenceException : InterpreterException
{
    public InvalidVariableReferenceException(Coordinate coordinate, string message) : base(coordinate, message)
    {
    }
}