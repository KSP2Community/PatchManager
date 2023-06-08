namespace PatchManager.SassyPatching.Exceptions;

public class InterpreterException : Exception
{
    public Coordinate Coordinate;

    public InterpreterException(Coordinate coordinate, string message) : base($"{coordinate.ToString()}: {message}")
    {
        Coordinate = coordinate;
    }
}