namespace PatchManager.SassyPatching.Exceptions;

public class DictionaryKeyNotFoundException : InterpreterException
{
    public DictionaryKeyNotFoundException(Coordinate coordinate, string message) : base(coordinate, message)
    {
    }
}