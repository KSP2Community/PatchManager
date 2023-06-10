using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// This represents an argument passed to a patch function
/// </summary>
[PublicAPI]
public class PatchArgument
{
    /// <summary>
    /// The name of the argument if it was named by the caller
    /// </summary>
    [CanBeNull] public string ArgumentName;
    /// <summary>
    /// The value of the argument
    /// </summary>
    public DataValue ArgumentDataValue;
}