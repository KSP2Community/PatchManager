namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// An exception thrown by the patch interpreter due to runtime code being bad
/// </summary>
public class InterpreterException : Exception
{
    /// <summary>
    /// The location of where the exception was thrown from
    /// </summary>
    public Coordinate Coordinate;

    internal InterpreterException(Coordinate coordinate, string message) : base($"{coordinate.ToString()}: {message}")
    {
        Coordinate = coordinate;
    }
}