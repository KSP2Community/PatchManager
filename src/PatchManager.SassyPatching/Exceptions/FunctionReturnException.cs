namespace PatchManager.SassyPatching.Exceptions;

internal class FunctionReturnException : Exception
{
    internal DataValue FunctionResult;

    public FunctionReturnException(DataValue functionResult)
    {
        FunctionResult = functionResult;
    }
}