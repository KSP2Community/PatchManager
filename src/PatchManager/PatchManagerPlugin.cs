using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine.UIElements;

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
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Science.dll"));
        ModuleManager.Register(Path.Combine(dir.FullName, "PatchManager.Missions.dll"));
        foreach (var module in ModuleManager.Modules)
        {
            module.BindConfiguration(SWConfiguration);
        }
    }

    private void Start()
    {
        // Register Harmony patches
        var harmony = Harmony.CreateAndPatchAll(typeof(PatchManagerPlugin).Assembly);
        foreach (var module in ModuleManager.Modules)
        {
            harmony.PatchAll(module.GetType().Assembly);
        }

        // Init modules
        ModuleManager.InitAll();
    }

    /// <summary>
    /// Called before the game is initialized
    /// </summary>
    public override void OnPreInitialized()
    {
        ModuleManager.PreLoadAll();
    }

    /// <summary>
    /// Called after the game is initialized
    /// </summary>
    public override void OnInitialized()
    {
        ModuleManager.LoadAll();
    }

    /// <summary>
    /// Called after all mods are initialized
    /// </summary>
    public override void OnPostInitialized()
    {
        InitializePatchManagerDetailsFoldout();
    }
    
    private static void InitializePatchManagerDetailsFoldout()
    {
        VisualElement GeneratePatchManagerText()
        {
            var detailsContainer = new ScrollView();
            var str = "Loaded modules: ";
            var toAdd = new List<VisualElement>();
            foreach (var module in ModuleManager.Modules)
            {
                str += $"\n- {module.GetType().Assembly.GetName().Name}";
                var details = module.GetDetails();
                if (details != null)
                {
                    toAdd.Add(details);
                }
            }
            str += "\n";
            var loadedModules = new TextElement
            {
                text = str,
                visible = true,
                style =
                {
                    display = DisplayStyle.Flex
                }
            };
            detailsContainer.Add(loadedModules);
            foreach (var element in toAdd)
            {
                detailsContainer.Add(element);
            }
            detailsContainer.visible = true;
            detailsContainer.style.display = DisplayStyle.Flex;
            return detailsContainer;
        }
        SpaceWarp.API.UI.ModList.RegisterDetailsFoldoutGenerator(ModGuid, GeneratePatchManagerText);
    }
}