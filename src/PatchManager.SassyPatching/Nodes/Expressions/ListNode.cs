namespace PatchManager.SassyPatching.Nodes.Expressions;

public class ListNode : Expression
{
    public List<Expression> Expressions;
    public ListNode(Coordinate c, List<Expression> expressions) : base(c)
    {
        Expressions = expressions;
    }

    public override Value Compute(Environment environment)
    {
        return Expressions.Select(x => x.Compute(environment)).ToList();
    }
}