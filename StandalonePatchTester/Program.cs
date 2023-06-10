// See https://aka.ms/new-console-template for more information

if (Directory.Exists("json") && Directory.Exists("patches"))
{
    if (Directory.Exists("patched"))
    {
        Directory.Delete("patched",true);
    }
}
else
{
    Console.WriteLine("Usage: run this program in a folder that has a json/ subfolder and a patches/ subfolder and it will run every patch in the patches folder and output patched files into patched/");
}