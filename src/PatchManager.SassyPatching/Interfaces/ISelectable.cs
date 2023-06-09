namespace PatchManager.SassyPatching.Interfaces;

/// <summary>
/// Represents a selectable object
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Select from children by name pattern
    /// </summary>
    /// <param name="name">The name pattern being matched against</param>
    /// <returns>All children that match the name pattern</returns>
    public List<ISelectable> SelectByName(string name);
    /// <summary>
    /// Select from children by class
    /// </summary>
    /// <param name="class">The class being selected by</param>
    /// <returns>All children that have the class</returns>
    public List<ISelectable> SelectByClass(string @class);
    /// <summary>
    /// Select from children by class
    /// </summary>
    /// <param name="class">The class being selected by</param>
    /// <returns>All children that don't have the class</returns>
    public List<ISelectable> SelectWithoutClass(string @class);
    
    /// <summary>
    /// Select from children by name pattern
    /// </summary>
    /// <param name="name">The name pattern being matched against</param>
    /// <returns>All children that don't match the name pattern</returns>
    public List<ISelectable> SelectWithoutName(string name);
    /// <summary>
    /// Select from children by element
    /// </summary>
    /// <param name="element">The element being selected by</param>
    /// <returns>All children that are the element</returns>
    public List<ISelectable> SelectByElement(string element);
    
    /// <summary>
    /// Select all children
    /// </summary>
    /// <returns>All children</returns>
    public List<ISelectable> SelectEverything();

    
    /// <summary>
    /// Test if this selectable matches a name pattern
    /// </summary>
    /// <param name="name">The pattern being matched against</param>
    /// <returns>True if it matches the name pattern</returns>
    public bool MatchesName(string name);
    /// <summary>
    /// Test if this selectable has a class
    /// </summary>
    /// <param name="class">The class</param>
    /// <returns>True if it has the class</returns>
    public bool MatchesClass(string @class);
    /// <summary>
    /// Test if this selectable is an element
    /// </summary>
    /// <param name="element">The element</param>
    /// <returns>True if it is the element</returns>
    public bool MatchesElement(string element);
    
    /// <summary>
    /// Checks if this selectable is the same as another selectable
    /// </summary>
    /// <param name="other">The selectable to be checked against</param>
    /// <returns>True if they are the same</returns>
    public bool IsSameAs(ISelectable other);

    /// <summary>
    /// Opens up this selectable for modification
    /// </summary>
    /// <returns>The modifiable state of this selector</returns>
    /// <exception cref="Exceptions.NotModifiableException">Thrown if this selectable cannot be modified</exception>
    public IModifiable OpenModification();

    /// <summary>
    /// Adds an element to this selectable and returns the selectable for it
    /// </summary>
    /// <exception cref="Exceptions.NotModifiableException">Thrown if this selectable cannot be modified</exception>
    public ISelectable AddElement(string elementType);
}