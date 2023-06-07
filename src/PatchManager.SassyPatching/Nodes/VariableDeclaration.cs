namespace PatchManager.SassyPatching.Nodes;

public class VariableDeclaration : Node
{ 
    public string Variable;
    public Node Value;

    public VariableDeclaration(Coordinate c, string variable, Node value) : base(c)
    {
        Variable = variable;
        Value = value;
    }
}