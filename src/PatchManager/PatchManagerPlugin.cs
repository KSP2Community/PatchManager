using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using PatchManager.Core.Cache;
using PatchManager.Core.Utility;
using PatchManager.Core.Preload;
using PatchManager.PreloadPatcher;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine.AddressableAssets;

namespace PatchManager;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class PatchManagerPlugin : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    internal static PatchManagerPlugin Instance { get; private set; }
    public new static ManualLogSource Logger { get; set; }

    internal string CachePath;
    internal string PatchesPath;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        CachePath = Path.Combine(Paths.PluginPath, ModGuid, "cache");
        PatchesPath = Path.Combine(Paths.PluginPath, ModGuid, "patches");

        // Clean up cache folder (for debugging purposes)
        if (Directory.Exists(CachePath))
        {
            Directory.Delete(CachePath, true);
        }

        Directory.CreateDirectory(CachePath);

        // Initialize logging
        Logging.Initialize(Logger);

        // Load plugin DLLs
        Assembly.LoadFile(Path.Combine(Paths.PluginPath, ModGuid, "PatchManager.Core.dll"));

        // Register Harmony patches
        Harmony.CreateAndPatchAll(typeof(PatchManagerPlugin).Assembly);

        Patcher.LoadAssetsDelegate = AssetProviderPatch.LoadByLabel;
    }

    public override void OnInitialized()
    {
        Logger.LogInfo("Registering resource locator");
        Addressables.ResourceManager.ResourceProviders.Add(new FileResourceProvider());
        Game.Assets.RegisterResourceLocator(new FileResourceLocator());
    }
}