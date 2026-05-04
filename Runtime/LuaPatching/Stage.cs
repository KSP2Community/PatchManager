using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniLinq;

namespace PatchManager.LuaPatching;

[MoonSharpUserData]
public class Stage
{
    public List<string> RunsBefore = new();
    public List<string> RunsAfter = new();


    [MoonSharpHidden]
    public void UpdateRequirements(HashSet<string> allStages)
    {
        RunsBefore = RunsBefore.Where(allStages.Contains).ToList();
        RunsAfter = RunsAfter.Where(allStages.Contains).ToList();
    }

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