namespace PatchManager.SassyPatching;

/// <summary>
/// An exception that is thrown when a <see cref="Value"/> is read as a type which it is not
/// </summary>
public class IncorrectTypeException : Exception
{
    internal IncorrectTypeException(string message) : base(message)
    {
    }
}