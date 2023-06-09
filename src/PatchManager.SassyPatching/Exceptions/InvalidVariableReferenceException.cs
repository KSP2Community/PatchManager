namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// An exception thrown when a variable is being referenced that does not exist
/// </summary>
public class InvalidVariableReferenceException : InterpreterException
{
    internal InvalidVariableReferenceException(Coordinate coordinate, string message) : base(coordinate, message)
    {
    }
}