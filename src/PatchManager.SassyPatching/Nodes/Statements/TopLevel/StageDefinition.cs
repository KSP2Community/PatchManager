using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class StageDefinition : Node
{

    public string Name;
    public bool Implicit;
    public bool Global;
    public List<string> Before;
    public List<string> After;
    public StageDefinition(Coordinate c, string name) : base(c)
    {
        Name = name;
        Implicit = true;
        Global = false;
        Before = new();
        After = new();
    }

    public StageDefinition(Coordinate c, string name, bool global) : base(c)
    {
        Name = name;
        Global = global;
        Implicit = !global;
        Before = new();
        After = new();
    }

    public StageDefinition(Coordinate c, string name, List<StageDefinitionAttribute> attributes) : base(c)
    {
        Name = name;
        Global = false;
        Implicit = false;
        Before = new();
        After = new();
        foreach (var attribute in attributes)
        {
            if (attribute.After)
                After.Add(attribute.Relative);
            else
                Before.Add(attribute.Relative);
        }
    }
    
    public override void ExecuteIn(Environment environment)
    {
        var universe = environment.GlobalEnvironment.Universe;
        var id = environment.GlobalEnvironment.ModGuid;
        var interpName = "";
        try
        {
            interpName = Name.Interpolate(environment);
        }
        catch (Exception e)
        {
            throw new InterpolationException(Coordinate, e.Message);
        }
        var name = $"{id}:{interpName}";
        var stage = new Stage();
        if (Global) // @global
        {
            stage.RunsAfter.Add(universe.LastImplicitGlobal);
            universe.LastImplicitGlobal = name;
        } else if (Implicit) // implicit
        {
            stage.RunsAfter.Add(universe.LastImplicitWithinMod[id]);
            var post = universe.UnsortedStages[$"{id}:post"];
            post.RunsAfter.Clear();
            post.RunsAfter.Add(name);
            universe.LastImplicitWithinMod[id] = name;
        }
        else // defined relations
        {
            stage.RunsAfter.AddRange(After.Select(x =>
            {
                var interp = "";
                try
                {
                    interp = x.Interpolate(environment);
                }
                catch (Exception e)
                {
                    throw new InterpolationException(Coordinate, e.Message);
                }
                return (interp.Contains(":") || universe.AllMods.Contains(interp)) ? interp : $"{id}:{interp}";
            }));
            stage.RunsBefore.AddRange(Before.Select(x =>
            {
                var interp = "";
                try
                {
                    interp = x.Interpolate(environment);
                }
                catch (Exception e)
                {
                    throw new InterpolationException(Coordinate, e.Message);
                }
                return (interp.Contains(":") || universe.AllMods.Contains(interp)) ? interp : $"{id}:{interp}";
            }));
        }
        universe.UnsortedStages[name] = stage;
    }
}