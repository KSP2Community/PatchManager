using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// Used to store information in a selectable, for stuff like class capture selectors and the like
/// Makes stuff a lot more complex, but meh
/// </summary>
public class SelectableWithEnvironment
{
    /// <summary>
    /// The selectable being selected against
    /// </summary>
    public ISelectable Selectable;
    /// <summary>
    /// The environment being used
    /// </summary>
    public Environment Environment;
}