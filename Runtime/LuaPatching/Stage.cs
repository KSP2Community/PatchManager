using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniLinq;

namespace PatchManager.LuaPatching;

/// <summary>
/// A patching stage with optional ordering constraints relative to other stages.
/// </summary>
[MoonSharpUserData]
public class Stage
{
    /// <summary>
    /// Stage names this stage must run before.
    /// </summary>
    public List<string> RunsBefore = new();

    /// <summary>
    /// Stage names this stage must run after.
    /// </summary>
    public List<string> RunsAfter = new();


    /// <summary>
    /// Drops any references in <see cref="RunsBefore" /> and <see cref="RunsAfter" /> that name a stage not in
    /// <paramref name="allStages" />.
    /// </summary>
    /// <remarks>
    /// Called by <see cref="Universe" /> before topological sort so stale or misspelled dependencies do not deadlock the sort.
    /// </remarks>
    /// <param name="allStages">The names of every stage known to the universe.</param>
    [MoonSharpHidden]
    public void UpdateRequirements(HashSet<string> allStages)
    {
        RunsBefore = RunsBefore.Where(allStages.Contains).ToList();
        RunsAfter = RunsAfter.Where(allStages.Contains).ToList();
    }

    /// <summary>
    /// Adds each given stage name to <see cref="RunsBefore" /> and returns this stage for chaining.
    /// </summary>
    /// <param name="args">The stage names this stage should run before.</param>
    /// <returns>This stage.</returns>
    public Stage Before(CallbackArguments args)
    {
        foreach (var value in args.GetArray())
        {
            var result = value.CastToString();
            if (!string.IsNullOrEmpty(result))
            {
                RunsBefore.Add(result);
            }
        }
        return this;
    }

    /// <summary>
    /// Adds each given stage name to <see cref="RunsAfter" /> and returns this stage for chaining.
    /// </summary>
    /// <param name="args">The stage names this stage should run after.</param>
    /// <returns>This stage.</returns>
    public Stage After(CallbackArguments args)
    {
        foreach (var value in args.GetArray())
        {
            var result = value.CastToString();
            if (!string.IsNullOrEmpty(result))
            {
                RunsAfter.Add(result);
            }
        }
        return this;
    }
}
