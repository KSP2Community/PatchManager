using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Public registry and lazy effective-prefab runtime for the prefab domain.
/// </summary>
public static class PrefabPatchRuntime
{
    public sealed class Metrics
    {
        public int DiscoveredManifestCount;
        public int ResolvedPlanCount;
        public int CacheHitCount;
        public int CacheMissCount;
        public long StartupMilliseconds;
        public long FirstCompositionMilliseconds;
        public long RepeatedRequestMilliseconds;
        public int RepeatedRequestCount;
        public long MemoryBeforeBytes;
        public long MemoryAfterBytes;
        public int RetainedAddressablesHandles;
    }

    internal sealed class Entry
    {
        public PrefabPatchResolvedPlan Plan;
        public GameObject EffectivePrefab;
        public AsyncOperationHandle<GameObject> StockHandle;
        public List<AsyncOperationHandle<Object>> ReferenceHandles = new();
        public Dictionary<string, Object> References = new(
            StringComparer.Ordinal
        );
        public bool Failed;
        public Exception Failure;
        public int RequestCount;
    }

    private const string PlanCacheDirectory = "./pm_cache/prefabs";
    private const string SummaryPath = "./pm_prefab_summary.log";
    private static readonly List<PrefabPatchManifest> Registered = new();
    private static readonly Dictionary<string, Entry> Entries = new(
        StringComparer.Ordinal
    );
    private static PrefabPatchResourceLocator _locator;

    public static bool RegistrationOpen { get; private set; } = true;
    public static Metrics CurrentMetrics { get; private set; } = new();
    public static IReadOnlyDictionary<string, PrefabPatchResolvedPlan> Plans =>
        Entries.ToDictionary(pair => pair.Key, pair => pair.Value.Plan);

    /// <summary>
    /// Registers a fluent or generated C# manifest before registration closes.
    /// Visual manifests are normally discovered through the public label.
    /// </summary>
    public static void Register(PrefabPatchManifest manifest)
    {
        if (!RegistrationOpen)
            throw new InvalidOperationException(
                "Prefab patch registration is closed for this run."
            );
        if (manifest == null)
            throw new ArgumentNullException(nameof(manifest));
        Registered.Add(manifest);
    }

    public static void CloseRegistration()
    {
        RegistrationOpen = false;
    }

    /// <summary>
    /// Discovers independently built TextAsset manifests, resolves per-prefab
    /// plans through the atomic cache, and writes a deterministic summary.
    /// </summary>
    public static void DiscoverAndResolve(
        ISet<string> activeModIds,
        Action resolve,
        Action<string> reject
    )
    {
        var stopwatch = Stopwatch.StartNew();
        CurrentMetrics = new Metrics
        {
            MemoryBeforeBytes = Profiler.GetTotalAllocatedMemoryLong()
        };
        try
        {
            var manifests = new List<PrefabPatchManifest>(Registered);
            var locations = FindManifestLocations();
            if (locations.Count > 0)
            {
                var manifestHandle = Addressables.LoadAssetsAsync<TextAsset>(
                    locations,
                    null
                );
                var assets = manifestHandle.WaitForCompletion();
                if (manifestHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw manifestHandle.OperationException
                        ?? new InvalidOperationException(
                            "Prefab patch manifest load failed."
                        );
                }

                foreach (var asset in assets.Where(value => value != null))
                {
                    try
                    {
                        manifests.Add(
                            PrefabPatchJson.Deserialize<PrefabPatchManifest>(
                                asset.text
                            )
                        );
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidDataException(
                            $"Could not parse prefab patch manifest "
                                + $"'{asset.name}'.",
                            exception
                        );
                    }
                }

                Addressables.Release(manifestHandle);
            }

            CurrentMetrics.DiscoveredManifestCount = manifests.Count;
            var cache = new PrefabPatchPlanCache(PlanCacheDirectory);
            Entries.Clear();
            foreach (
                var group in manifests
                    .Where(value => value?.TargetPrefab != null)
                    .GroupBy(
                        value => value.TargetPrefab.CanonicalKey,
                        StringComparer.Ordinal
                    )
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
            )
            {
                var result = cache.LoadOrResolve(
                    group,
                    activeModIds,
                    Application.unityVersion,
                    Application.platform.ToString()
                );
                if (result.CacheHit)
                    CurrentMetrics.CacheHitCount++;
                else
                    CurrentMetrics.CacheMissCount++;
                if (
                    result.Plan?.TargetPrefab != null
                    && result.Plan.IsValid
                )
                {
                    Entries[result.Plan.TargetPrefab.Address] = new Entry
                    {
                        Plan = result.Plan
                    };
                }
            }

            CurrentMetrics.ResolvedPlanCount = Entries.Count;
            WriteSummary();
            stopwatch.Stop();
            CurrentMetrics.StartupMilliseconds = stopwatch.ElapsedMilliseconds;
            CurrentMetrics.MemoryAfterBytes =
                Profiler.GetTotalAllocatedMemoryLong();
            resolve();
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            CurrentMetrics.StartupMilliseconds = stopwatch.ElapsedMilliseconds;
            UnityEngine.Debug.LogException(exception);
            reject(
                "Patch Manager prefab discovery failed: "
                    + exception.Message
            );
        }
    }

    private static List<IResourceLocation> FindManifestLocations()
    {
        return Addressables.ResourceLocators
            .SelectMany(locator =>
                locator.Locate(
                    PrefabPatchSchema.AddressablesLabel,
                    typeof(TextAsset),
                    out var locations
                )
                    ? locations
                    : Array.Empty<IResourceLocation>()
            )
            .Where(location => location != null)
            .GroupBy(
                location =>
                    $"{location.ProviderId}\0{location.InternalId}\0"
                    + $"{location.PrimaryKey}\0"
                    + $"{location.ResourceType?.AssemblyQualifiedName}",
                StringComparer.Ordinal
            )
            .Select(group => group.First())
            .OrderBy(location => location.PrimaryKey, StringComparer.Ordinal)
            .ThenBy(location => location.InternalId, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Adds the public GameObject provider and locator to Patch Manager's
    /// supported KSP AssetProvider interception boundary.
    /// </summary>
    public static UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator
        RegisterResourceProvider()
    {
        var providers = Addressables.ResourceManager.ResourceProviders;
        if (
            !providers.Any(
                provider => provider is PrefabPatchResourceProvider
            )
        )
        {
            providers.Add(new PrefabPatchResourceProvider());
        }

        _locator = new PrefabPatchResourceLocator(Entries);
        return _locator;
    }

    internal static bool TryProvide(
        string address,
        out GameObject prefab,
        out Exception failure
    )
    {
        prefab = null;
        failure = null;
        if (!Entries.TryGetValue(address, out var entry))
        {
            failure = new KeyNotFoundException(
                $"No resolved prefab patch plan exists for '{address}'."
            );
            return false;
        }

        var stopwatch = Stopwatch.StartNew();
        entry.RequestCount++;
        if (entry.EffectivePrefab != null)
        {
            stopwatch.Stop();
            CurrentMetrics.RepeatedRequestCount++;
            CurrentMetrics.RepeatedRequestMilliseconds +=
                stopwatch.ElapsedMilliseconds;
            prefab = entry.EffectivePrefab;
            return true;
        }

        if (entry.Failed)
        {
            failure = entry.Failure;
            return false;
        }

        try
        {
            var stockLocation = ResolveOriginalLocation(
                address,
                typeof(GameObject)
            );
            entry.StockHandle =
                Addressables.LoadAssetAsync<GameObject>(stockLocation);
            var stock = entry.StockHandle.WaitForCompletion();
            if (
                entry.StockHandle.Status != AsyncOperationStatus.Succeeded
                || stock == null
            )
            {
                throw entry.StockHandle.OperationException
                    ?? new InvalidOperationException(
                        $"Could not load stock prefab '{address}'."
                    );
            }

            foreach (
                var reference in entry.Plan.Operations
                    .SelectMany(GetReferences)
                    .Where(value => value != null)
                    .GroupBy(value => value.Address, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .OrderBy(value => value.Address, StringComparer.Ordinal)
            )
            {
                var location = ResolveOriginalLocation(
                    reference.Address,
                    typeof(Object)
                );
                var handle = Addressables.LoadAssetAsync<Object>(location);
                var value = handle.WaitForCompletion();
                if (
                    handle.Status != AsyncOperationStatus.Succeeded
                    || value == null
                )
                {
                    throw handle.OperationException
                        ?? new InvalidOperationException(
                            $"Could not load prefab patch reference "
                                + $"'{reference.Address}'."
                        );
                }

                entry.ReferenceHandles.Add(handle);
                entry.References.Add(reference.Address, value);
            }

            var result = PrefabPatchComposer.ApplySynchronously(
                stock,
                entry.Plan,
                entry.References
            );
            if (!result.Success)
                throw new InvalidOperationException(result.Failure);
            entry.EffectivePrefab = stock;
            stopwatch.Stop();
            CurrentMetrics.FirstCompositionMilliseconds +=
                stopwatch.ElapsedMilliseconds;
            CurrentMetrics.RetainedAddressablesHandles =
                Entries.Values.Sum(
                    value =>
                        (value.StockHandle.IsValid() ? 1 : 0)
                        + value.ReferenceHandles.Count(
                            handle => handle.IsValid()
                        )
                );
            CurrentMetrics.MemoryAfterBytes =
                Profiler.GetTotalAllocatedMemoryLong();
            prefab = stock;
            return true;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            entry.Failed = true;
            entry.Failure = exception;
            failure = exception;
            UnityEngine.Debug.LogError(
                $"Prefab composition failed for '{address}': {exception}"
            );
            WriteSummary();
            return false;
        }
    }

    private static IResourceLocation ResolveOriginalLocation(
        string address,
        Type type
    )
    {
        var handle =
            type == typeof(Object)
                ? Addressables.LoadResourceLocationsAsync(address)
                : Addressables.LoadResourceLocationsAsync(address, type);
        var locations = handle.WaitForCompletion();
        try
        {
            if (
                handle.Status != AsyncOperationStatus.Succeeded
                || locations == null
                || locations.Count == 0
            )
            {
                throw handle.OperationException
                    ?? new InvalidOperationException(
                        $"No original Addressables location exists for "
                            + $"'{address}' as '{type.FullName}'."
                    );
            }

            return locations
                .Where(
                    location =>
                        !string.Equals(
                            location.ProviderId,
                            typeof(PrefabPatchResourceProvider).FullName,
                            StringComparison.Ordinal
                        )
                )
                .OrderBy(location => location.PrimaryKey, StringComparer.Ordinal)
                .ThenBy(location => location.InternalId, StringComparer.Ordinal)
                .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    $"Only recursive prefab-patch locations exist for "
                        + $"'{address}'."
                );
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    private static IEnumerable<PrefabPatchObjectReference> GetReferences(
        PrefabPatchOperation operation
    )
    {
        if (operation.ObjectReference != null)
            yield return operation.ObjectReference;
        if (operation.AddedComponent?.Mesh != null)
            yield return operation.AddedComponent.Mesh;
        if (operation.AddedObject == null)
            yield break;
        foreach (var reference in GetReferences(operation.AddedObject))
            yield return reference;
    }

    private static IEnumerable<PrefabPatchObjectReference> GetReferences(
        PrefabPatchObjectFragment fragment
    )
    {
        foreach (var component in fragment.Components)
        {
            if (component.Mesh != null)
                yield return component.Mesh;
        }

        foreach (var child in fragment.Children)
        {
            foreach (var reference in GetReferences(child))
                yield return reference;
        }
    }

    private static void WriteSummary()
    {
        var lines = new List<string>
        {
            "Patch Manager prefab patch summary",
            $"Schema: {PrefabPatchSchema.Version}",
            $"Composer: {PrefabPatchSchema.ComposerVersion}",
            $"Discovered manifests: {CurrentMetrics.DiscoveredManifestCount}",
            $"Resolved plans: {CurrentMetrics.ResolvedPlanCount}",
            $"Plan cache hits: {CurrentMetrics.CacheHitCount}",
            $"Plan cache misses: {CurrentMetrics.CacheMissCount}"
        };
        foreach (
            var pair in Entries.OrderBy(
                value => value.Key,
                StringComparer.Ordinal
            )
        )
        {
            lines.Add("");
            lines.Add($"Target: {pair.Key}");
            lines.Add($"Cache key: {pair.Value.Plan.CacheKey}");
            lines.Add(
                "Ordered patches: "
                    + string.Join(
                        ", ",
                        pair.Value.Plan.OrderedPatchIds
                    )
            );
            foreach (var diagnostic in pair.Value.Plan.Diagnostics)
            {
                lines.Add(
                    $"[{diagnostic.Severity}] {diagnostic.Code} "
                        + $"{diagnostic.PatchId} {diagnostic.OperationId}: "
                        + diagnostic.Message
                );
            }

            if (pair.Value.Failed)
                lines.Add("Composition failure: " + pair.Value.Failure);
        }

        File.WriteAllLines(SummaryPath, lines);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Registered.Clear();
        Entries.Clear();
        _locator = null;
        RegistrationOpen = true;
        CurrentMetrics = new Metrics();
    }
}

internal sealed class PrefabPatchResourceLocator :
    UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator
{
    private readonly IReadOnlyDictionary<string, PrefabPatchRuntime.Entry> _entries;

    public PrefabPatchResourceLocator(
        IReadOnlyDictionary<string, PrefabPatchRuntime.Entry> entries
    )
    {
        _entries = entries;
    }

    public string LocatorId => GetType().FullName;
    public IEnumerable<object> Keys => _entries.Keys;

    public bool Locate(
        object key,
        Type type,
        out IList<IResourceLocation> locations
    )
    {
        var address = key?.ToString();
        if (
            string.IsNullOrWhiteSpace(address)
            || !_entries.ContainsKey(address)
            || (
                type != typeof(object)
                && type != typeof(Object)
                && type != typeof(GameObject)
                && !typeof(Component).IsAssignableFrom(type)
            )
        )
        {
            locations = Array.Empty<IResourceLocation>();
            return false;
        }

        locations = new IResourceLocation[]
        {
            new ResourceLocationBase(
                "prefab-patch:" + address,
                address,
                typeof(PrefabPatchResourceProvider).FullName,
                typeof(GameObject)
            )
        };
        return true;
    }
}

internal sealed class PrefabPatchResourceProvider : ResourceProviderBase
{
    public override void Provide(ProvideHandle provideHandle)
    {
        if (
            PrefabPatchRuntime.TryProvide(
                provideHandle.Location.InternalId,
                out var prefab,
                out var failure
            )
        )
        {
            provideHandle.Complete(prefab, true, null);
        }
        else
        {
            provideHandle.Complete<GameObject>(null, false, failure);
        }
    }

    public override Type GetDefaultType(IResourceLocation location) =>
        typeof(GameObject);

    public override void Release(IResourceLocation location, object obj)
    {
        // Effective prefabs and their stock/mod Addressables handles are retained
        // for the game session. They are cleared by SubsystemRegistration.
    }
}
