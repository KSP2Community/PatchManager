namespace PatchManager.Core.Flow;

/// <summary>
/// Represents an action to be executed during game loading.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Name of the action.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="resolve">Action to be executed on success.</param>
    /// <param name="reject">Action to be executed on failure.</param>
    public void DoAction(Action resolve, Action<string> reject);
}