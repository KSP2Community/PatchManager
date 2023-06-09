namespace PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="StageDefinition"/>
/// </summary>
public class StageDefinitionValidator : ParseValidator<StageDefinition>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="StageDefinition"/>
    /// </summary>
    public string StageName = "";
    
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="StageDefinition"/>
    /// </summary>
    public ulong StagePriority = 0;
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(StageDefinition node)
    {
        return node.StageName == StageName && node.StagePriority == StagePriority;
    }
}