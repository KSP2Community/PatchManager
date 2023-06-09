namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// An exception thrown when a dictionary is indexed with a key that it does not have
/// </summary>
public class DictionaryKeyNotFoundException : InterpreterException
{
    internal DictionaryKeyNotFoundException(Coordinate coordinate, string message) : base(coordinate, message)
    {
    }
}