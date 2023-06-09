namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="MixinInclude"/>
/// </summary>
public class MixinIncludeValidator : ParseValidator<MixinInclude>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="MixinInclude"/>
    /// </summary>
    public string MixinName = "";
    
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="MixinInclude"/> 
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<ParseValidator> Arguments = new();
    
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(MixinInclude node)
    {
        if (node.MixinName != MixinName) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        return !Arguments.Where((x, y) => !x.Validate(node.Arguments[y])).Any();
    }
}