namespace PatchManager.SassyPatching.Exceptions;

public class InterpolationException : InterpreterException
{
    internal InterpolationException(Coordinate coordinate, string message) : base(coordinate, message) { }
}