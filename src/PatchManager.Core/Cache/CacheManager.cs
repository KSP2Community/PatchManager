using System.Reflection;
using PatchManager.Core.Cache.Json;
using PatchManager.Shared;

namespace PatchManager.Core.Cache;

internal static class CacheManager
{
    private static readonly string CacheDirectory = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        "cache"
    );

    private static readonly string InventoryPath = Path.Combine(CacheDirectory, "inventory.json");

    private static readonly Dictionary<string, Archive> OpenArchives = new();
    public static readonly List<string> CacheValidLabels = new();

    private static Inventory _inventory;
    public static Inventory Inventory => _inventory ??= Inventory.Load(InventoryPath);

    public static void CreateCacheFolderIfNotExists()
    {
        if (Directory.Exists(CacheDirectory))
        {
            return;
        }

        Logging.LogDebug("Cache directory does not exist, creating a new one.");
        Directory.CreateDirectory(CacheDirectory);
    }

    public static Archive CreateArchive(string archiveFilename)
    {
        var archivePath = Path.Combine(CacheDirectory, archiveFilename);
        if (File.Exists(archivePath))
        {
            throw new ArgumentException($"Archive '{archivePath}' already exists!");
        }

        var archive = new Archive(archivePath, true);
        OpenArchives.Add(archiveFilename, archive);
        return archive;
    }

    public static Archive GetArchive(string archiveFilename)
    {
        var archivePath = Path.Combine(CacheDirectory, archiveFilename);
        if (!File.Exists(archivePath))
        {
            throw new FileNotFoundException($"Archive '{archivePath}' does not exist!");
        }

        if (!OpenArchives.ContainsKey(archiveFilename))
        {
            OpenArchives.Add(archiveFilename, new Archive(archivePath));
        }

        return OpenArchives[archiveFilename];
    }

    public static void InvalidateCache()
    {
        CacheValidLabels.Clear();

        _inventory = Inventory.Create();

        foreach (var archive in OpenArchives.Values)
        {
            archive.Dispose();
        }

        OpenArchives.Clear();

        Directory.Delete(CacheDirectory, true);
        CreateCacheFolderIfNotExists();
    }

    public static void SaveInventory()
    {
        Inventory.Save(InventoryPath);
    }
}