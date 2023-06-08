namespace PatchManager.SassyPatching.Exceptions;

public class ListIndexOutOfRangeException : InterpreterException
{
    public ListIndexOutOfRangeException(Coordinate coordinate, string message) : base(coordinate, message)
    {
    }
}