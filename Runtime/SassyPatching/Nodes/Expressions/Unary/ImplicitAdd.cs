using System;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary
{
    /// <summary>
    /// Represents an implicit addition, which adds the child to $value
    /// </summary>
    public class ImplicitAdd : Implicit
    {
        internal ImplicitAdd(Coordinate c, Expression child) : base(c, child)
        {
        }

        internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
        {
            try
            {
                return leftHandSide + rightHandSide;
            }
            catch (DataValueOperationException)
            {
                throw new BinaryExpressionTypeException(Coordinate, "add", leftHandSide.Type.ToString(),
                    rightHandSide.Type.ToString());
            }
        }
    
    }
}