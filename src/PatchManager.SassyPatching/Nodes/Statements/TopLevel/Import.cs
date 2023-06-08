namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class Import : Node
{
    public string Library;

    public Import(Coordinate c, string library) : base(c)
    {
        Library = library;
    }
}