namespace PatchManager.SassyPatching.Nodes;

public abstract class Node
{
    public readonly Coordinate Coordinate;

    protected Node(Coordinate c)
    {
        Coordinate = c;
    }
}