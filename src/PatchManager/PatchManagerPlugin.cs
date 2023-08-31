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

    private void Awake()
    {
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