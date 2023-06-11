namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// Thrown when invoking a managed function goes awry
/// </summary>
public class InvocationException : Exception
{
    internal InvocationException(string message) : base(message)
    {
    }
}