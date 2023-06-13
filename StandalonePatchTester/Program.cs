// See https://aka.ms/new-console-template for more information

using System.Reflection;
using PatchManager.SassyPatching.Execution;
using PatchManager.Shared.Interfaces;

//Assert that the assembly we want is loaded for testing
Assembly.Load("PatchManager.Parts");

if (Directory.Exists("json") && Directory.Exists("patches"))
{
    if (Directory.Exists("patched"))
    {
        Directory.Delete("patched",true);
    }

    Directory.CreateDirectory("patched");

    List<ITextPatcher> patchers = new();

    void Add(ITextPatcher patcher)
    {
        bool inserted = false;
        for (int index = 0; index < patchers.Count; index++)
        {
            if (patchers[index].Priority > patcher.Priority)
            {
                patchers.Insert(index, patcher);
                inserted = true;
                break;
            }
        }

        if (!inserted)
        {
            patchers.Add(patcher);
        }
    }
    
    var universe = new Universe(Add, Console.WriteLine, Console.WriteLine);
    universe.LoadPatchesInDirectory(new DirectoryInfo("patches"), "test");

    Console.WriteLine($"{universe.AllLibraries.Count} libraries loaded!");
    universe.RegisterAllPatches();
    Console.WriteLine($"{patchers.Count} patcher(s) registered!");
    var numPatchesRan = 0;
    var jsonInfo = new DirectoryInfo("json");
    foreach (var part in jsonInfo.EnumerateFiles("*.json"))
    {
        // if (!part.Name.StartsWith("fueltank")) continue;
        var localPatchesRan = 0;
        Console.WriteLine($"Running patchers on {part.Name}");
        string text = File.ReadAllText(part.FullName);
        foreach (var patcher in patchers)
        {
            var copy = new string(text);
            try
            {
                var patched = patcher.TryPatch("parts_data",Path.GetFileNameWithoutExtension(part.Name), ref text);
                if (patched) localPatchesRan++;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Patch errored due to: {e.Message}");
                text = copy;
            }
        }
        Console.WriteLine($"{localPatchesRan} patch(es) ran on {part.Name}");
        if (localPatchesRan > 0)
        {
            File.WriteAllText(Path.Combine("patched", part.Name), text);
        }
        numPatchesRan += localPatchesRan;

    }
    Console.WriteLine($"{numPatchesRan} patch(es) ran in total");
}
else
{
    Console.WriteLine("Usage: run this program in a folder that has a json/ subfolder and a patches/ subfolder and it will run every patch in the patches folder and output patched files into patched/");
}