namespace PatchManager.SassyPatching;

/// <summary>
/// An exception that is thrown when a <see cref="Value"/> is read as a type which it is not
/// </summary>
public class IncorrectTypeException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="IncorrectTypeException" /> class with a specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public IncorrectTypeException(string message) : base(message)
    {
    }
}