namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public abstract class Unary : Expression
{
    public Expression Child;

    
    
    protected Unary(Coordinate c, Expression child) : base(c)
    {
        Child = child;
    }

    public abstract Value GetResult(Value child);
    
    public override Value Compute(Environment environment)
    {
        return GetResult(Child.Compute(environment));
    }
}