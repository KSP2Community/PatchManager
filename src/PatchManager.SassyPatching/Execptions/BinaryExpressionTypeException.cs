namespace PatchManager.SassyPatching.Execptions;

public class BinaryExpressionTypeException : Exception
{
    public BinaryExpressionTypeException(string operation, string firstType, string secondType) : base($"Attempting to {operation} a value of type: {firstType} and a value of type: {secondType} which is not allowed")
    {
    }
}