namespace PatchManager.SassyPatching.Exceptions;

/// <summary>
/// Thrown by the Managed invoker when a type cannot be converted either way
/// </summary>
public class TypeConversionException : Exception
{
    public TypeConversionException(string from, string to) : base($"Cannot convert a value of type {from} to type {to}")
    {
    }
}