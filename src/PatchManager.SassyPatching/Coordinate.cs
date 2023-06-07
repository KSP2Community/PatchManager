namespace PatchManager.SassyPatching;

public record struct Coordinate(string Filename, int Line, int Column)
{
    public override string ToString()
    {
        return $"{Filename}:{Line}:{Column}";
    }
}