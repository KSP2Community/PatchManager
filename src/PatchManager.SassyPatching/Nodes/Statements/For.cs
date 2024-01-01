using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a loop that iterates over a range of numbers
/// </summary>
public class For : Node, ISelectionAction
{
    /// <summary>
    /// The variable that is set to the current iteration value
    /// </summary>
    public readonly string VariableName;
    /// <summary>
    /// The starting value of the iteration
    /// </summary>
    public readonly Expression InitialValue;
    /// <summary>
    /// Whether or not the loop is inclusive in its range
    /// </summary>
    public readonly bool Inclusive;
    /// <summary>
    /// The ending value of the iteration
    /// </summary>
    public readonly Expression EndingValue;
    /// <summary>
    /// The body of the iteration
    /// </summary>
    public readonly List<Node> Children;

    internal For(Coordinate c, string variableName, Expression initialValue, bool inclusive, Expression endingValue, List<Node> children) : base(c)
    {
        VariableName = variableName;
        InitialValue = initialValue;
        Inclusive = inclusive;
        EndingValue = endingValue;
        Children = children;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        var start = InitialValue.Compute(environment);
        var end = EndingValue.Compute(environment);
        if (start.IsInteger)
        {
            SetupIntegerIteration(environment, end, start);
            return;
        }

        if (start.IsReal)
        {
            SetupRealIteration(environment, end, start);
            return;
        }

        throw new InterpreterException(Coordinate, $"cannot do a for loop w/ a value that is of type {start.Type.ToString().ToLowerInvariant()}");
    }

    private void SetupRealIteration(Environment environment, DataValue end, DataValue start)
    {
        Func<double, bool> endCheck;
        bool inverted;
        if (end.IsReal)
        {
            inverted = SetupRealRealIteration(end, start, out endCheck);
        }
        else if (end.IsInteger)
        {
            inverted = SetupRealIntegerIteration(end, start, out endCheck);
        }
        else
        {
            throw new InterpreterException(Coordinate, $"cannot do a for loop through/to a value of type {end.Type.ToString().ToLowerInvariant()}");
        }

        IterateReal(environment, start.Real, inverted, endCheck);
    }

    private bool SetupRealIntegerIteration(DataValue end, DataValue start, out Func<double, bool> endCheck)
    {
        bool inverted;
        inverted = start.Integer > end.Integer;
        if (Inclusive)
        {
            if (inverted)
            {
                endCheck = l => l < end.Integer;
            }
            else
            {
                endCheck = l => l > end.Integer;
            }
        }
        else
        {
            if (inverted)
            {
                endCheck = l => l <= end.Integer;
            }
            else
            {
                endCheck = l => l >= end.Integer;
            }
        }

        return inverted;
    }

    private bool SetupRealRealIteration(DataValue end, DataValue start, out Func<double, bool> endCheck)
    {
        bool inverted;
        inverted = start.Integer > end.Real;
        if (Inclusive)
        {
            if (inverted)
            {
                endCheck = l => l < end.Real;
            }
            else
            {
                endCheck = l => l > end.Real;
            }
        }
        else
        {
            if (inverted)
            {
                endCheck = l => l <= end.Real;
            }
            else
            {
                endCheck = l => l >= end.Real;
            }
        }

        return inverted;
    }

    private void SetupIntegerIteration(Environment environment, DataValue end, DataValue start)
    {
        Func<long, bool> endCheck;
        bool inverted;
        if (end.IsReal)
        {
            inverted = SetupIntegerRealIteration(end, start, out endCheck);
        }
        else if (end.IsInteger)
        {
            inverted = SetupIntegerIntegerIteration(end, start, out endCheck);
        }
        else
        {
            throw new InterpreterException(Coordinate, $"cannot do a for loop through/to a value of type {end.Type.ToString().ToLowerInvariant()}");
        }

        IterateInteger(environment, start.Integer, inverted, endCheck);
    }

    private bool SetupIntegerIntegerIteration(DataValue end, DataValue start, out Func<long, bool> endCheck)
    {
        bool inverted;
        inverted = start.Integer > end.Integer;
        if (Inclusive)
        {
            if (inverted)
            {
                endCheck = l => l < end.Integer;
            }
            else
            {
                endCheck = l => l > end.Integer;
            }
        }
        else
        {
            if (inverted)
            {
                endCheck = l => l <= end.Integer;
            }
            else
            {
                endCheck = l => l >= end.Integer;
            }
        }

        return inverted;
    }

    private bool SetupIntegerRealIteration(DataValue end, DataValue start, out Func<long, bool> endCheck)
    {
        bool inverted;
        inverted = start.Integer > end.Real;
        if (Inclusive)
        {
            if (inverted)
            {
                endCheck = l => l < end.Real;
            }
            else
            {
                endCheck = l => l > end.Real;
            }
        }
        else
        {
            if (inverted)
            {
                endCheck = l => l <= end.Real;
            }
            else
            {
                endCheck = l => l >= end.Real;
            }
        }

        return inverted;
    }


    private void IterateInteger(Environment environment, long start, bool inverted, Func<long, bool> end)
    {
        while (!end(start))
        {
            environment[VariableName] = start;
            ExecuteChildren(environment);
            start += inverted ? -1 : 1;
        }
    }

    private void IterateReal(Environment environment, double start, bool inverted, Func<double, bool> end)
    {
        while (!end(start))
        {
            environment[VariableName] = start;
            ExecuteChildren(environment);
            start += inverted ? -1 : 1;
        }
    }


    private void ExecuteChildren(Environment environment)
    {
        foreach (var child in Children)
        {
            child.ExecuteIn(environment);
        }
    }

    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        var start = InitialValue.Compute(environment);
        var end = EndingValue.Compute(environment);
        if (start.IsInteger)
        {
            SetupIntegerIteration(environment, selectable, modifiable, end, start);
            return;
        }

        if (start.IsReal)
        {
            SetupRealIteration(environment, selectable, modifiable, end, start);
            return;
        }

        throw new InterpreterException(Coordinate, $"cannot do a for loop w/ a value that is of type {start.Type.ToString().ToLowerInvariant()}");
    }
    private void SetupRealIteration(Environment environment, ISelectable selectable, IModifiable modifiable, DataValue end, DataValue start)
    {
        Func<double, bool> endCheck;
        bool inverted;
        if (end.IsReal)
        {
            inverted = SetupRealRealIteration(end, start, out endCheck);
        }
        else if (end.IsInteger)
        {
            inverted = SetupRealIntegerIteration(end, start, out endCheck);
        }
        else
        {
            throw new InterpreterException(Coordinate, $"cannot do a for loop through/to a value of type {end.Type.ToString().ToLowerInvariant()}");
        }

        IterateReal(environment, selectable, modifiable, start.Real, inverted, endCheck);
    }

    private void SetupIntegerIteration(Environment environment, ISelectable selectable, IModifiable modifiable, DataValue end, DataValue start)
    {
        Func<long, bool> endCheck;
        bool inverted;
        if (end.IsReal)
        {
            inverted = SetupIntegerRealIteration(end, start, out endCheck);
        }
        else if (end.IsInteger)
        {
            inverted = SetupIntegerIntegerIteration(end, start, out endCheck);
        }
        else
        {
            throw new InterpreterException(Coordinate, $"cannot do a for loop through/to a value of type {end.Type.ToString().ToLowerInvariant()}");
        }

        IterateInteger(environment, selectable, modifiable, start.Integer, inverted, endCheck);
    }


    private void IterateInteger(Environment environment, ISelectable selectable, IModifiable modifiable, long start, bool inverted, Func<long, bool> end)
    {
        while (!end(start))
        {
            environment[VariableName] = start;
            ExecuteChildren(environment, selectable, modifiable);
            start += inverted ? -1 : 1;
        }
    }

    private void IterateReal(Environment environment, ISelectable selectable, IModifiable modifiable, double start, bool inverted, Func<double, bool> end)
    {
        while (!end(start))
        {
            environment[VariableName] = start;
            ExecuteChildren(environment, selectable, modifiable);
            start += inverted ? -1 : 1;
        }
    }
    private void ExecuteChildren(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        foreach (var child in Children)
        {
            if (child is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(environment,selectable,modifiable);
            }
            else
            {
                child.ExecuteIn(environment);
            }
        }
    }
}