// See https://aka.ms/new-console-template for more information

using PatchManager.SassyPatching.Execution;
using PatchManager.Shared.Interfaces;

if (Directory.Exists("json") && Directory.Exists("patches"))
{
    if (Directory.Exists("patched"))
    {
        Directory.Delete("patched",true);
    }

    List<ITextPatcher> patchers = new();
    var universe = new Universe(patchers.Add, Console.WriteLine);
    universe.LoadPatchesInDirectory(new DirectoryInfo("patches"), "test");

    Console.WriteLine($"{patchers.Count} patcher(s) registered!");
}
else
{
    Console.WriteLine("Usage: run this program in a folder that has a json/ subfolder and a patches/ subfolder and it will run every patch in the patches folder and output patched files into patched/");
}