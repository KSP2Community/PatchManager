namespace PatchManager.SassyPatching;

/// <summary>
/// Defines a location in a file
/// </summary>
/// <param name="Filename">The file the location is in</param>
/// <param name="Line">The line of the location</param>
/// <param name="Column">The column of the location</param>
public record struct Coordinate(string Filename, int Line, int Column)
{
    /// <summary>
    /// Get the string representation of the location
    /// </summary>
    /// <returns>A string representation in file:line:column format</returns>
    public override string ToString()
    {
        return $"{Filename}:{Line}:{Column}";
    }
}