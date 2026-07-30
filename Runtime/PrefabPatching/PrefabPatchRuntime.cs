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
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Coordinates the ordinary and prefab-patch sections of Patch Manager's
/// single human-readable summary log.
/// </summary>
public static class PatchManagerSummaryLog
{
    private const string SummaryPath = "./pm_summary.log";
    private static readonly object Gate = new();
    private static string _coreSummary;
    private static string _prefabSummary;

    public static void UpdateCoreSummary(string summary)
    {
        lock (Gate)
        {
            _coreSummary = Normalize(summary);
            Write();
        }
    }

    public static void UpdatePrefabSummary(string summary)
    {
        lock (Gate)
        {
            _prefabSummary = Normalize(summary);
            Write();
        }
    }

    public static void Reset()
    {
        lock (Gate)
        {
            _coreSummary = null;
            _prefabSummary = null;
        }
    }

    private static string Normalize(string summary)
    {
        return string.IsNullOrWhiteSpace(summary)
            ? null
            : summary.TrimEnd();
    }

    private static void Write()
    {
        var sections = new[] { _coreSummary, _prefabSummary }
            .Where(value => !string.IsNullOrEmpty(value));
        File.WriteAllText(
            SummaryPath,
            string.Join(Environment.NewLine + Environment.NewLine, sections)
                + Environment.NewLine
        );
    }
}

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

    private sealed class ManifestLocation
    {
        public IResourceLocation Location;
        public string OwnerModId;
        public string LocatorId;
    }

    private const string PlanCacheDirectory = "./pm_cache/prefabs";
    private static readonly List<PrefabPatchManifest> Registered = new();
    private static readonly Dictionary<string, Entry> Entries = new(
        StringComparer.Ordinal
    );
    private static PrefabPatchResourceLocator _locator;
    private static GameObject _effectivePrefabRoot;

    public static bool RegistrationOpen { get; private set; } = true;
    public static Metrics CurrentMetrics { get; private set; } = new();
    public static IReadOnlyDictionary<string, PrefabPatchResolvedPlan> Plans =>
        Entries.ToDictionary(pair => pair.Key, pair => pair.Value.Plan);
#if UNITY_EDITOR
    /// <summary>
    /// Editor integration hook that maps a compiled manifest asset path to
    /// the Mod authoring asset (and therefore swinfo ID) that owns it.
    /// </summary>
    public static Func<string, string> EditorManifestOwnerResolver { get; set; }
#endif

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
    ) =>
        DiscoverAndResolve(
            activeModIds,
            new Dictionary<string, string>(StringComparer.Ordinal),
            resolve,
            reject
        );

    public static void DiscoverAndResolve(
        ISet<string> activeModIds,
        IReadOnlyDictionary<string, string> manifestCatalogOwners,
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
#if UNITY_EDITOR
            manifests.AddRange(LoadEditorProjectManifests());
            ResolveDiscoveredManifests(
                manifests,
                activeModIds,
                stopwatch,
                resolve
            );
            return;
#else
            var locations = FindManifestLocations(manifestCatalogOwners);
            if (locations.Count > 0)
            {
                foreach (var source in locations)
                {
                    AsyncOperationHandle<TextAsset> manifestHandle = default;
                    try
                    {
                        manifestHandle = Addressables.LoadAssetAsync<TextAsset>(
                            source.Location
                        );
                        var asset = manifestHandle.WaitForCompletion();
                        if (
                            manifestHandle.Status
                                != AsyncOperationStatus.Succeeded
                            || asset == null
                        )
                        {
                            throw manifestHandle.OperationException
                                ?? new InvalidOperationException(
                                    "Prefab patch manifest load failed."
                                );
                        }

                        var manifest =
                            PrefabPatchJson.Deserialize<PrefabPatchManifest>(
                                asset.text
                            );
                        manifests.Add(
                            PrefabPatchOwnership.Bind(
                                manifest,
                                source.OwnerModId
                            )
                        );
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidDataException(
                            $"Could not load prefab patch manifest "
                                + $"'{source.Location.PrimaryKey}' from "
                                + $"catalog '{source.LocatorId}'.",
                            exception
                        );
                    }
                    finally
                    {
                        if (manifestHandle.IsValid())
                            Addressables.Release(manifestHandle);
                    }
                }
            }

            ResolveDiscoveredManifests(
                manifests,
                activeModIds,
                stopwatch,
                resolve
            );
#endif
        }
        catch (Exception exception)
        {
            RejectDiscovery(stopwatch, reject, exception);
        }
    }

#if UNITY_EDITOR
    private static IEnumerable<PrefabPatchManifest> LoadEditorProjectManifests()
    {
        foreach (
            var path in AssetDatabase
                .GetAllAssetPaths()
                .Where(path =>
                    path.EndsWith(
                        ".prefabpatch.json",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderBy(path => path, StringComparer.Ordinal)
        )
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
                continue;

            PrefabPatchManifest manifest;
            try
            {
                manifest =
                    PrefabPatchJson.Deserialize<PrefabPatchManifest>(
                        asset.text
                    );
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                    $"Could not parse prefab patch manifest '{path}'.",
                    exception
                );
            }

            if (manifest == null)
                continue;
            var owner = EditorManifestOwnerResolver?.Invoke(path);
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new InvalidDataException(
                    $"Could not determine the owning Mod asset for prefab "
                        + $"patch manifest '{path}'."
                );
            }
            yield return PrefabPatchOwnership.Bind(manifest, owner);
        }
    }
#endif

    private static void ResolveDiscoveredManifests(
        IReadOnlyCollection<PrefabPatchManifest> manifests,
        ISet<string> activeModIds,
        Stopwatch stopwatch,
        Action resolve
    )
    {
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

    private static void RejectDiscovery(
        Stopwatch stopwatch,
        Action<string> reject,
        Exception exception
    )
    {
        stopwatch.Stop();
        CurrentMetrics.StartupMilliseconds = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.LogException(exception);
        reject(
            "Patch Manager prefab discovery failed: "
                + exception.Message
        );
    }

    private static List<ManifestLocation> FindManifestLocations(
        IReadOnlyDictionary<string, string> catalogOwners
    )
    {
        return Addressables.ResourceLocators
            .SelectMany(locator =>
            {
                if (
                    locator == null
                    || !locator.Locate(
                    PrefabPatchSchema.AddressablesLabel,
                    typeof(TextAsset),
                    out var locations
                )
                )
                {
                    return Array.Empty<ManifestLocation>();
                }

                string ownerModId = null;
                catalogOwners?.TryGetValue(
                    locator.LocatorId,
                    out ownerModId
                );
                if (string.IsNullOrWhiteSpace(ownerModId))
                {
                    throw new InvalidDataException(
                        $"Addressables catalog '{locator.LocatorId}' contains "
                            + "prefab patch manifests but is not associated "
                            + "with a loaded mod swinfo descriptor."
                    );
                }
                return locations
                    .Where(location => location != null)
                    .Select(location => new ManifestLocation
                    {
                        Location = location,
                        OwnerModId = ownerModId,
                        LocatorId = locator.LocatorId
                    });
            })
            .GroupBy(
                source =>
                    $"{source.Location.ProviderId}\0"
                    + $"{source.Location.InternalId}\0"
                    + $"{source.Location.PrimaryKey}\0"
                    + $"{source.Location.ResourceType?.AssemblyQualifiedName}",
                StringComparer.Ordinal
            )
            .Select(group =>
            {
                var owners = group
                    .Select(value => value.OwnerModId)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (owners.Length != 1)
                {
                    throw new InvalidDataException(
                        $"Prefab patch location '{group.Key}' is exposed by "
                            + "multiple owning mods: "
                            + string.Join(", ", owners)
                    );
                }
                return group.First();
            })
            .OrderBy(
                source => source.Location.PrimaryKey,
                StringComparer.Ordinal
            )
            .ThenBy(
                source => source.Location.InternalId,
                StringComparer.Ordinal
            )
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
                    .Where(
                        value =>
                            value != null
                            && value.Kind
                            == PrefabPatchObjectReferenceKind.Addressable
                            && !string.IsNullOrWhiteSpace(value.Address)
                    )
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

            var effectivePrefab = CreateEffectivePrefab(stock);
            var result = PrefabPatchComposer.ApplySynchronously(
                effectivePrefab,
                entry.Plan,
                entry.References
            );
            if (!result.Success)
            {
                Object.DestroyImmediate(effectivePrefab);
                throw new InvalidOperationException(result.Failure);
            }

            entry.EffectivePrefab = effectivePrefab;
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
            prefab = effectivePrefab;
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

    private static GameObject CreateEffectivePrefab(GameObject stock)
    {
        if (_effectivePrefabRoot == null)
        {
            _effectivePrefabRoot = new GameObject(
                "PatchManager Effective Prefabs"
            );
            _effectivePrefabRoot.hideFlags = HideFlags.HideAndDontSave;
            _effectivePrefabRoot.SetActive(false);
            Object.DontDestroyOnLoad(_effectivePrefabRoot);
        }

        // AssetBundle assets are read-only native objects. Patching them in
        // place is not stable: Unity can restore removed components before a
        // later instantiation. Keep an inactive, session-owned template
        // instead. Parenting under an inactive root prevents prefab
        // MonoBehaviours from initializing while the patch is composed.
        var effectivePrefab = Object.Instantiate(
            stock,
            _effectivePrefabRoot.transform,
            false
        );
        effectivePrefab.name = stock.name;
        return effectivePrefab;
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
        foreach (
            var reference in operation.AddedComponent?.References
                ?? Enumerable.Empty<PrefabPatchSerializedReference>()
        )
        {
            if (reference?.Reference != null)
                yield return reference.Reference;
        }
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
            foreach (
                var reference in component.References
                    ?? Enumerable.Empty<PrefabPatchSerializedReference>()
            )
            {
                if (reference?.Reference != null)
                    yield return reference.Reference;
            }
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
            "Prefab Patches:",
            $"    Schema: {PrefabPatchSchema.Version}",
            $"    Composer: {PrefabPatchSchema.ComposerVersion}",
            $"    Discovered Manifests: {CurrentMetrics.DiscoveredManifestCount}",
            $"    Resolved Plans: {CurrentMetrics.ResolvedPlanCount}",
            $"    Plan Cache Hits: {CurrentMetrics.CacheHitCount}",
            $"    Plan Cache Misses: {CurrentMetrics.CacheMissCount}"
        };
        foreach (
            var pair in Entries.OrderBy(
                value => value.Key,
                StringComparer.Ordinal
            )
        )
        {
            lines.Add("");
            lines.Add($"    Target - {pair.Key}:");
            lines.Add($"        Cache Key: {pair.Value.Plan.CacheKey}");
            lines.Add(
                "        Ordered Patches: "
                    + string.Join(
                        ", ",
                        pair.Value.Plan.OrderedPatchIds
                    )
            );
            foreach (var diagnostic in pair.Value.Plan.Diagnostics)
            {
                lines.Add(
                    $"        [{diagnostic.Severity}] {diagnostic.Code} "
                        + $"{diagnostic.PatchId} {diagnostic.OperationId}: "
                        + diagnostic.Message
                );
            }

            if (pair.Value.Failed)
                lines.Add(
                    "        Composition Failure: " + pair.Value.Failure
                );
        }

        PatchManagerSummaryLog.UpdatePrefabSummary(
            string.Join(Environment.NewLine, lines)
        );
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        PatchManagerSummaryLog.Reset();
        ReleaseSessionResources();
        Registered.Clear();
        Entries.Clear();
        _locator = null;
        RegistrationOpen = true;
        CurrentMetrics = new Metrics();
    }

    /// <summary>
    /// Releases effective prefab templates and the Addressables handles that
    /// keep their stock assets and referenced objects alive.
    /// </summary>
    public static void ReleaseSessionResources()
    {
        foreach (var entry in Entries.Values)
        {
            if (entry.EffectivePrefab != null)
                Object.DestroyImmediate(entry.EffectivePrefab);
            entry.EffectivePrefab = null;

            foreach (var handle in entry.ReferenceHandles)
            {
                if (handle.IsValid())
                    handle.Release();
            }

            entry.ReferenceHandles.Clear();
            entry.References.Clear();
            if (entry.StockHandle.IsValid())
                entry.StockHandle.Release();
            entry.StockHandle = default;
        }

        if (_effectivePrefabRoot != null)
            Object.DestroyImmediate(_effectivePrefabRoot);
        _effectivePrefabRoot = null;
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

        if (_entries.ContainsKey(address))
        {
            locations = new IResourceLocation[] { CreatePatchLocation(address) };
            return true;
        }

        var resolved = new List<IResourceLocation>();
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var replacedAny = false;
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (
                !locator.Locate(key, typeof(GameObject), out var sourceLocations)
                || sourceLocations == null
            )
                continue;

            foreach (var sourceLocation in sourceLocations)
            {
                var sourceAddress = sourceLocation?.PrimaryKey;
                IResourceLocation resolvedLocation = sourceLocation;
                if (
                    !string.IsNullOrWhiteSpace(sourceAddress)
                    && _entries.ContainsKey(sourceAddress)
                )
                {
                    resolvedLocation = CreatePatchLocation(sourceAddress);
                    replacedAny = true;
                }

                if (
                    resolvedLocation != null
                    && identities.Add(GetLocationIdentity(resolvedLocation))
                )
                    resolved.Add(resolvedLocation);
            }
        }

        locations = replacedAny
            ? resolved
            : Array.Empty<IResourceLocation>();
        return replacedAny;
    }

    private static IResourceLocation CreatePatchLocation(string address)
    {
        return new ResourceLocationBase(
            "prefab-patch:" + address,
            address,
            typeof(PrefabPatchResourceProvider).FullName,
            typeof(GameObject)
        );
    }

    private static string GetLocationIdentity(IResourceLocation location)
    {
        if (
            string.Equals(
                location.ProviderId,
                typeof(PrefabPatchResourceProvider).FullName,
                StringComparison.Ordinal
            )
        )
            return "patch|" + location.PrimaryKey;

        var dependencies = location.Dependencies == null
            ? string.Empty
            : string.Join(
                ";",
                location.Dependencies.Select(dependency =>
                    (dependency?.PrimaryKey ?? string.Empty)
                    + "|"
                    + (dependency?.InternalId ?? string.Empty)
                )
            );
        return (location.PrimaryKey ?? string.Empty)
            + "|"
            + (location.InternalId ?? string.Empty)
            + "|"
            + (location.ResourceType?.AssemblyQualifiedName ?? string.Empty)
            + "|"
            + dependencies;
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
