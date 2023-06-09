using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;

/// <summary>
/// Represents a function return statement
/// </summary>
public class Return : Node
{
    /// <summary>
    /// The value to be returned from the function
    /// </summary>
    public readonly Expression ReturnedValue;
    internal Return(Coordinate c, Expression returnedValue) : base(c)
    {
        ReturnedValue = returnedValue;
    }
}