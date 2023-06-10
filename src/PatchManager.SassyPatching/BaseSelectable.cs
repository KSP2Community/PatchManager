using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching;

/// <summary>
/// A base class for selectables that implements most of the selection functions
/// </summary>
[PublicAPI]
public abstract class BaseSelectable : ISelectable
{
    /// <inheritdoc />
    public List<ISelectable> SelectByName(string name) =>
        Children.Where(child => child.MatchesName(name)).ToList();

    /// <inheritdoc />
    public List<ISelectable> SelectByClass(string @class)=> 
        Children.Where(child => child.MatchesClass(@class)).ToList();

    /// <inheritdoc />
    public List<ISelectable> SelectWithoutClass(string @class) => 
        Children.Where(child => !child.MatchesClass(@class)).ToList();

    /// <inheritdoc />
    public List<ISelectable> SelectWithoutName(string name) =>
        Children.Where(child => !child.MatchesName(name)).ToList();

    /// <inheritdoc />
    public List<ISelectable> SelectByElement(string element) => Children.Where(child => child.MatchesElement(element)).ToList();

    /// <summary>
    /// The children of this selectable
    /// </summary>
    public abstract List<ISelectable> Children { get; }

    /// <inheritdoc />
    public List<ISelectable> SelectEverything() => Children;

    /// <summary>
    /// The name of this selectable element
    /// </summary>
    public abstract string Name { get; }

    /// <inheritdoc />
    public bool MatchesName(string name) => name.MatchesPattern(name);

    /// <summary>
    /// The classes that this selectable has, usually corresponds to the field names of the children (except for parts, in which its that + module type)
    /// </summary>
    public abstract List<string> Classes { get; }

    /// <inheritdoc />
    public bool MatchesClass(string @class) => Classes.Contains(@class);

    /// <summary>
    /// The type of this selectable element (usually corresponds to the field name it is defined as, or the module type)
    /// </summary>
    public abstract string ElementType { get; }

    /// <inheritdoc />
    public bool MatchesElement(string element) => ElementType == element;

    /// <inheritdoc />
    public abstract bool IsSameAs(ISelectable other);

    /// <inheritdoc />
    public abstract IModifiable OpenModification();

    /// <inheritdoc />
    public abstract ISelectable AddElement(string elementType);


    /// <inheritdoc />
    public abstract string Serialize();
}