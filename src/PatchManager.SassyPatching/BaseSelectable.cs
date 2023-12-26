using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching;

/// <summary>
/// A base class for selectables that implements most of the selection functions
/// </summary>
[PublicAPI]
public abstract class BaseSelectable : ISelectable
{
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
    public bool MatchesName(string name) => Name.MatchesPattern(name);

    /// <summary>
    /// The classes that this selectable has, usually corresponds to the field names of the children (except for parts, in which its that + module type)
    /// </summary>
    public abstract List<string> Classes { get; }

    /// <inheritdoc />
    public bool MatchesClass(string @class) => Classes.Contains(@class);

    /// <summary>
    /// Test if this selectable has a class, and if so, output the value of that class
    /// </summary>
    /// <param name="class">The class</param>
    /// <param name="classValue">The value of the class</param>
    /// <returns>True if it has the class, false otherwise</returns>
    public abstract bool MatchesClass(string @class, out DataValue classValue);

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

    /// <inheritdoc />

    public abstract DataValue GetValue();
}