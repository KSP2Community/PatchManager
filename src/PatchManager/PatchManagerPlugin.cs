using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace PatchManager;

/// <summary>
/// Main plugin class
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class PatchManagerPlugin : BaseSpaceWarpPlugin
{
    /// <summary>
    /// BepInEx GUID of the mod
    /// </summary>
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    /// <summary>
    /// Name of the mod
    /// </summary>
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    /// <summary>
    /// Version of the mod
    /// </summary>
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// <summary>
    /// Path to the folder where files are stored after patches get applied
    /// </summary>
    public static string CachePath { get; private set; }
    /// <summary>
    /// Temporary folder from where patches are loaded and applied
    /// TODO: Make patches load from all mods' `patches` folders
    /// </summary>
    public static string PatchesPath { get; private set; }

    private void Awake()
    {
        CachePath = Path.Combine(Paths.PluginPath, ModGuid, "cache");
        PatchesPath = Path.Combine(Paths.PluginPath, ModGuid, "patches");

        // Initialize logging
        Logging.Initialize(Logger);

        // Load library DLLs
        var path = new FileInfo(Assembly.GetExecutingAssembly().Location);
        var dir = path.Directory!;
        Assembly.LoadFile(Path.Combine(dir.FullName, "lib", "Antlr4.Runtime.Standard.dll"));
        Assembly.LoadFile(Path.Combine(dir.FullName, "PatchManager.SassyPatching.dll"));

        // Load module DLLs
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Core.dll"));
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Parts.dll"));
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Generic.dll"));
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Resources.dll"));
    }

    private void Start()
    {
        // Register Harmony patches
        var harmony = Harmony.CreateAndPatchAll(typeof(PatchManagerPlugin).Assembly);
        foreach (var module in ModuleManager.Modules)
        {
            harmony.PatchAll(module.GetType().Assembly);
        }

        // Preload modules
        ModuleManager.InitAll();
    }

    /// <summary>
    /// Called after the game is initialized
    /// </summary>
    public override void OnInitialized()
    {
        ModuleManager.LoadAll();
    }
}