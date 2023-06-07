namespace PatchManager.SassyPatching.Nodes;

public class Import : Node
{
    public string Library;

    public Import(Coordinate c, string library) : base(c)
    {
        Library = library;
    }
}