namespace PatchManager.SassyPatching.Execution;

public class Stage
{
    public List<string> RunsBefore = new();
    public List<string> RunsAfter = new();


    public void UpdateRequirements(List<string> allStages)
    {
        RunsBefore = RunsBefore.Where(x => allStages.Contains(x)).ToList();
        RunsAfter = RunsAfter.Where(x => allStages.Contains(x)).ToList();
    }
}