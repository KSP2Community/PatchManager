namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// Thrown when a selectable is trying to be modified that can't be modified
/// </summary>
public class NotModifiableException : Exception
{
    /// <inheritdoc />
    public NotModifiableException(string message) : base(message)
    {
    }
}