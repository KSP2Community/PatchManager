using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Deterministic dependency, ordering, target, and conflict resolver for the
/// prefab asset domain. It does not execute Unity mutations.
/// </summary>
/// <remarks>
/// Resolution validates ownership and target compatibility, removes patches
/// whose mod or patch constraints are not satisfied, topologically sorts each
/// pass and ordering bucket, validates patch-owned target lifetimes, and
/// flattens operations into one cacheable plan. All tie-breaking uses ordinal
/// patch IDs so identical inputs produce identical output.
/// </remarks>
public static class PrefabPatchResolver
{
    /// <summary>Resolves manifests for one stock prefab into an executable plan.</summary>
    /// <param name="source">The owned manifests targeting the prefab.</param>
    /// <param name="activeModIds">The active SpaceWarp mod IDs.</param>
    /// <param name="unityVersion">The Unity version included in the cache input.</param>
    /// <param name="targetPlatform">The runtime platform included in the cache input.</param>
    /// <returns>A plan containing ordered operations and all diagnostics.</returns>
    public static PrefabPatchResolvedPlan Resolve(
        IEnumerable<PrefabPatchManifest> source,
        ISet<string> activeModIds,
        string unityVersion,
        string targetPlatform
    )
    {
        var manifests = source
            .Where(manifest => manifest != null)
            .OrderBy(manifest => manifest.PatchId, StringComparer.Ordinal)
            .ToList();
        var plan = new PrefabPatchResolvedPlan
        {
            ResolvedUtcTicks = DateTime.UtcNow.Ticks
        };
        if (manifests.Count == 0)
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-NO-PATCHES",
                null,
                null,
                "No prefab patch manifests were supplied."
            );
            return plan;
        }

        plan.TargetPrefab = manifests
            .Select(manifest => manifest.TargetPrefab)
            .Where(target => target != null)
            .OrderByDescending(
                target =>
                    !string.IsNullOrWhiteSpace(
                        target.StructuralFingerprint
                    )
            )
            .FirstOrDefault();
        var address = plan.TargetPrefab?.Address;
        var fatal = false;
        var byId = new Dictionary<string, PrefabPatchManifest>(
            StringComparer.Ordinal
        );
        foreach (var manifest in manifests)
        {
            if (!ValidateManifest(manifest, plan))
            {
                fatal = true;
                continue;
            }

            if (!TargetsMatch(manifest.TargetPrefab, plan.TargetPrefab))
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-MIXED-TARGET",
                    manifest.PatchId,
                    null,
                    string.Equals(
                        manifest.TargetPrefab.Address,
                        plan.TargetPrefab.Address,
                        StringComparison.Ordinal
                    )
                        ? $"Patch '{manifest.PatchId}' was compiled against "
                            + "a different structural version of "
                            + $"'{plan.TargetPrefab.Address}'."
                        : $"Patch '{manifest.PatchId}' targets "
                            + $"'{manifest.TargetPrefab.Address}', not "
                            + $"'{plan.TargetPrefab.Address}'."
                );
                fatal = true;
                continue;
            }

            if (!byId.TryAdd(manifest.PatchId, manifest))
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-DUPLICATE-PATCH-ID",
                    manifest.PatchId,
                    null,
                    $"Patch ID '{manifest.PatchId}' is registered more than once."
                );
                fatal = true;
            }
        }

        var enabled = new HashSet<string>(byId.Keys, StringComparer.Ordinal);
        FilterModConstraints(byId, enabled, activeModIds, plan);
        FilterPatchConstraints(byId, enabled, plan);

        var ordered = Order(byId, enabled, plan, ref fatal);
        plan.OrderedPatchIds = ordered
            .Select(manifest => manifest.PatchId)
            .ToArray();

        ValidateAndFlattenOperations(ordered, plan, ref fatal);
        plan.InputHash = BuildInputHash(
            ordered,
            plan.TargetPrefab,
            plan.Diagnostics,
            unityVersion,
            targetPlatform
        );
        plan.CacheKey = PrefabPatchJson.Sha256(
            $"{address}|{plan.TargetPrefab?.CanonicalKey}|{plan.InputHash}"
        );
        plan.IsValid = !fatal;
        return plan;
    }

    private static bool ValidateManifest(
        PrefabPatchManifest manifest,
        PrefabPatchResolvedPlan plan
    )
    {
        if (
            manifest.SchemaVersion != PrefabPatchSchema.Version
            || manifest.ComposerVersion != PrefabPatchSchema.ComposerVersion
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-SCHEMA",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' uses schema "
                    + $"{manifest.SchemaVersion}/composer "
                    + $"{manifest.ComposerVersion}; runtime requires "
                    + $"{PrefabPatchSchema.Version}/"
                    + $"{PrefabPatchSchema.ComposerVersion}."
            );
            return false;
        }

        if (
            string.IsNullOrWhiteSpace(manifest.PatchId)
            || manifest.PatchId.IndexOf(':') <= 0
            || string.IsNullOrWhiteSpace(manifest.PatchName)
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-PATCH-ID",
                manifest.PatchId,
                null,
                "Prefab patch ownership has not been bound from its "
                    + "containing mod."
            );
            return false;
        }

        if (
            string.IsNullOrWhiteSpace(manifest.ModId)
            || !manifest.PatchId.StartsWith(
                manifest.ModId + ":",
                StringComparison.Ordinal
            )
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-MOD-NAMESPACE",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' is not namespaced to owning mod "
                    + $"'{manifest.ModId}'."
            );
            return false;
        }

        if (
            manifest.TargetPrefab == null
            || string.IsNullOrWhiteSpace(manifest.TargetPrefab.Address)
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-TARGET-IDENTITY",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' has no target Addressables key."
            );
            return false;
        }

        var calculatedHash = PrefabPatchJson.CalculateManifestHash(manifest);
        if (
            !string.IsNullOrWhiteSpace(manifest.ManifestHash)
            && !string.Equals(
                calculatedHash,
                manifest.ManifestHash,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-MANIFEST-HASH",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' manifest hash does not match its "
                    + "normalized content."
            );
            return false;
        }

        manifest.ManifestHash = calculatedHash;
        return true;
    }

    private static bool TargetsMatch(
        PrefabPatchPrefabIdentity left,
        PrefabPatchPrefabIdentity right
    )
    {
        if (
            left == null
            || right == null
            || !string.Equals(
                left.Address,
                right.Address,
                StringComparison.Ordinal
            )
        )
            return false;
        return string.IsNullOrWhiteSpace(left.StructuralFingerprint)
            || string.IsNullOrWhiteSpace(right.StructuralFingerprint)
            || string.Equals(
                left.StructuralFingerprint,
                right.StructuralFingerprint,
                StringComparison.Ordinal
            );
    }

    private static void FilterModConstraints(
        IReadOnlyDictionary<string, PrefabPatchManifest> byId,
        ISet<string> enabled,
        ISet<string> activeModIds,
        PrefabPatchResolvedPlan plan
    )
    {
        foreach (var manifest in byId.Values.OrderBy(
                     value => value.PatchId,
                     StringComparer.Ordinal
                 ))
        {
            var missing = Safe(manifest.NeedsMods)
                .Where(id => !activeModIds.Contains(id))
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            if (missing.Length > 0)
            {
                enabled.Remove(manifest.PatchId);
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-MISSING-MOD",
                    manifest.PatchId,
                    null,
                    $"Disabled '{manifest.PatchId}': missing required mod(s) "
                        + string.Join(", ", missing) + "."
                );
                continue;
            }

            var conflicts = Safe(manifest.ConflictsMods)
                .Where(activeModIds.Contains)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            if (conflicts.Length > 0)
            {
                enabled.Remove(manifest.PatchId);
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-CONFLICTING-MOD",
                    manifest.PatchId,
                    null,
                    $"Disabled '{manifest.PatchId}': conflicting mod(s) "
                        + string.Join(", ", conflicts) + " are active."
                );
            }
        }
    }

    private static void FilterPatchConstraints(
        IReadOnlyDictionary<string, PrefabPatchManifest> byId,
        ISet<string> enabled,
        PrefabPatchResolvedPlan plan
    )
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var patchId in enabled.OrderBy(
                         id => id,
                         StringComparer.Ordinal
                     ).ToArray())
            {
                var manifest = byId[patchId];
                var missing = Safe(manifest.NeedsPatches)
                    .Where(id => !enabled.Contains(id))
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToArray();
                if (missing.Length > 0)
                {
                    enabled.Remove(patchId);
                    changed = true;
                    Add(
                        plan,
                        PrefabPatchDiagnosticSeverity.Error,
                        "PM-PREFAB-MISSING-PATCH",
                        patchId,
                        null,
                        $"Disabled '{patchId}': missing required patch(es) "
                            + string.Join(", ", missing) + "."
                    );
                    continue;
                }

                var conflicts = Safe(manifest.ConflictsPatches)
                    .Where(enabled.Contains)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToArray();
                if (conflicts.Length > 0)
                {
                    enabled.Remove(patchId);
                    changed = true;
                    Add(
                        plan,
                        PrefabPatchDiagnosticSeverity.Error,
                        "PM-PREFAB-CONFLICTING-PATCH",
                        patchId,
                        null,
                        $"Disabled '{patchId}': conflicting patch(es) "
                            + string.Join(", ", conflicts) + " are active."
                    );
                }
            }
        } while (changed);
    }

    private static List<PrefabPatchManifest> Order(
        IReadOnlyDictionary<string, PrefabPatchManifest> byId,
        ISet<string> enabled,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        var result = new List<PrefabPatchManifest>();
        foreach (PrefabPatchPass pass in Enum.GetValues(typeof(PrefabPatchPass)))
        {
            foreach (
                PrefabPatchOrdering bucket in Enum.GetValues(
                    typeof(PrefabPatchOrdering)
                )
            )
            {
                result.AddRange(
                    OrderBucket(byId, enabled, pass, bucket, plan, ref fatal)
                );
            }
        }

        return result;
    }

    private static IEnumerable<PrefabPatchManifest> OrderBucket(
        IReadOnlyDictionary<string, PrefabPatchManifest> byId,
        ISet<string> enabled,
        PrefabPatchPass pass,
        PrefabPatchOrdering bucket,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        var members = enabled
            .Select(id => byId[id])
            .Where(manifest => manifest.Pass == pass && manifest.Ordering == bucket)
            .ToDictionary(manifest => manifest.PatchId, StringComparer.Ordinal);
        var incoming = CreateEdgeMap(members.Keys);
        var outgoing = CreateEdgeMap(members.Keys);

        foreach (var manifest in members.Values)
        {
            AddDependencyEdges(
                manifest,
                byId,
                enabled,
                members,
                incoming,
                outgoing,
                plan,
                ref fatal
            );
            AddOrderingEdges(manifest, members, incoming, outgoing, plan);
        }

        return TopologicalSort(members, incoming, outgoing, plan, ref fatal);
    }

    private static Dictionary<string, HashSet<string>> CreateEdgeMap(
        IEnumerable<string> patchIds
    ) =>
        patchIds.ToDictionary(
            id => id,
            _ => new HashSet<string>(StringComparer.Ordinal),
            StringComparer.Ordinal
        );

    private static void AddDependencyEdges(
        PrefabPatchManifest manifest,
        IReadOnlyDictionary<string, PrefabPatchManifest> byId,
        ISet<string> enabled,
        IReadOnlyDictionary<string, PrefabPatchManifest> members,
        IDictionary<string, HashSet<string>> incoming,
        IDictionary<string, HashSet<string>> outgoing,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        foreach (var dependency in Safe(manifest.NeedsPatches))
        {
            if (!enabled.Contains(dependency))
                continue;
            if (Rank(byId[dependency]) > Rank(manifest))
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-DEPENDENCY-ORDER",
                    manifest.PatchId,
                    null,
                    $"Patch '{manifest.PatchId}' requires later patch "
                        + $"'{dependency}'."
                );
                fatal = true;
            }
            else if (members.ContainsKey(dependency))
            {
                AddEdge(dependency, manifest.PatchId, incoming, outgoing);
            }
        }
    }

    private static void AddOrderingEdges(
        PrefabPatchManifest manifest,
        IReadOnlyDictionary<string, PrefabPatchManifest> members,
        IDictionary<string, HashSet<string>> incoming,
        IDictionary<string, HashSet<string>> outgoing,
        PrefabPatchResolvedPlan plan
    )
    {
        AddSameBucketEdges(
            manifest,
            manifest.AfterPatches,
            true,
            members,
            incoming,
            outgoing,
            plan
        );
        AddSameBucketEdges(
            manifest,
            manifest.BeforePatches,
            false,
            members,
            incoming,
            outgoing,
            plan
        );
        AddModEdges(
            manifest,
            manifest.AfterMods,
            true,
            members,
            incoming,
            outgoing
        );
        AddModEdges(
            manifest,
            manifest.BeforeMods,
            false,
            members,
            incoming,
            outgoing
        );
    }

    private static IEnumerable<PrefabPatchManifest> TopologicalSort(
        IReadOnlyDictionary<string, PrefabPatchManifest> members,
        IDictionary<string, HashSet<string>> incoming,
        IReadOnlyDictionary<string, HashSet<string>> outgoing,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        var result = new List<PrefabPatchManifest>();
        var ready = new SortedSet<string>(
            incoming.Where(pair => pair.Value.Count == 0).Select(pair => pair.Key),
            StringComparer.Ordinal
        );
        var emitted = new HashSet<string>(StringComparer.Ordinal);
        while (ready.Count > 0)
        {
            var id = ready.Min;
            ready.Remove(id);
            emitted.Add(id);
            result.Add(members[id]);
            foreach (var next in outgoing[id].OrderBy(value => value, StringComparer.Ordinal))
            {
                incoming[next].Remove(id);
                if (incoming[next].Count == 0)
                    ready.Add(next);
            }
        }

        var cyclic = members.Keys
            .Where(id => !emitted.Contains(id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        if (cyclic.Length > 0)
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-ORDER-CYCLE",
                null,
                null,
                "Ordering cycle among prefab patches: "
                    + string.Join(", ", cyclic) + "."
            );
            fatal = true;
        }

        return result;
    }

    private static void ValidateAndFlattenOperations(
        IReadOnlyList<PrefabPatchManifest> ordered,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        var state = new OperationValidationState(ordered, plan);
        foreach (var manifest in ordered)
        {
            foreach (
                var operation in manifest.Operations
                    .Where(value => value != null)
            )
            {
                ValidateOperation(manifest, operation, state);
            }
        }

        fatal |= state.Fatal;
    }

    private sealed class OperationValidationState
    {
        public readonly IReadOnlyDictionary<string, int> PatchOrder;
        public readonly Dictionary<string, string> IntroducedObjects = new(
            StringComparer.Ordinal
        );
        public readonly Dictionary<string, string> IntroducedComponents = new(
            StringComparer.Ordinal
        );
        public readonly Dictionary<string, PrefabPatchOperation> Writes = new(
            StringComparer.Ordinal
        );
        public readonly PrefabPatchResolvedPlan Plan;
        public bool Fatal;

        public OperationValidationState(
            IReadOnlyList<PrefabPatchManifest> ordered,
            PrefabPatchResolvedPlan plan
        )
        {
            PatchOrder = ordered
                .Select((manifest, index) => (manifest.PatchId, index))
                .ToDictionary(pair => pair.PatchId, pair => pair.index);
            Plan = plan;
        }
    }

    private static void ValidateOperation(
        PrefabPatchManifest manifest,
        PrefabPatchOperation operation,
        OperationValidationState state
    )
    {
        operation.PatchId = manifest.PatchId;
        if (!ValidateOperationHeader(manifest, operation, state))
            return;
        if (!ValidatePatchOwnedTarget(manifest, operation, state))
            return;
        if (!RegisterIntroducedContent(manifest, operation, state))
            return;

        RecordWriteConflict(manifest, operation, state);
        state.Plan.Operations.Add(operation);
    }

    private static bool ValidateOperationHeader(
        PrefabPatchManifest manifest,
        PrefabPatchOperation operation,
        OperationValidationState state
    )
    {
        if (string.IsNullOrWhiteSpace(operation.OperationId))
        {
            Add(
                state.Plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-OPERATION-ID",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' contains an operation without an ID."
            );
            state.Fatal = true;
            return false;
        }

        if (
            operation.Kind != PrefabPatchOperationKind.AddObject
            && operation.Target == null
        )
        {
            Add(
                state.Plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-MISSING-TARGET",
                manifest.PatchId,
                operation.OperationId,
                $"Operation '{operation.OperationId}' has no target."
            );
            state.Fatal = true;
            return false;
        }

        return true;
    }

    private static bool ValidatePatchOwnedTarget(
        PrefabPatchManifest manifest,
        PrefabPatchOperation operation,
        OperationValidationState state
    )
    {
        var target = operation.Target;
        if (
            target?.Kind != PrefabPatchTargetKind.PatchOwned
            && target?.Kind != PrefabPatchTargetKind.PatchComponent
        )
        {
            return true;
        }

        var owner = target.OwnerPatchId;
        var isComponent = target.Kind == PrefabPatchTargetKind.PatchComponent;
        var ownedId = isComponent ? target.ComponentId : target.ObjectId;
        var targetKey = $"{owner}:{ownedId}";
        var ownerIsCurrentPatch = string.Equals(
            owner,
            manifest.PatchId,
            StringComparison.Ordinal
        );
        var needs = new HashSet<string>(
            Safe(manifest.NeedsPatches),
            StringComparer.Ordinal
        );
        if (!ownerIsCurrentPatch && !needs.Contains(owner))
        {
            Add(
                state.Plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-OWNER-NOT-REQUIRED",
                manifest.PatchId,
                operation.OperationId,
                $"Operation '{operation.OperationId}' targets '{targetKey}' but "
                    + $"does not require owning patch '{owner}'."
            );
            state.Fatal = true;
            return false;
        }

        var introduced = isComponent
            ? state.IntroducedComponents.ContainsKey(targetKey)
            : state.IntroducedObjects.ContainsKey(targetKey);
        var ownerRunsEarlier = ownerIsCurrentPatch
            || (
                state.PatchOrder.TryGetValue(owner, out var ownerOrder)
                && ownerOrder < state.PatchOrder[manifest.PatchId]
            );
        if (introduced && ownerRunsEarlier)
            return true;

        Add(
            state.Plan,
            PrefabPatchDiagnosticSeverity.Error,
            "PM-PREFAB-PATCH-OWNED-TARGET",
            manifest.PatchId,
            operation.OperationId,
            "Patch-owned "
                + (isComponent ? "component" : "object")
                + $" target '{targetKey}' is not introduced by an earlier "
                + "required operation."
        );
        state.Fatal = true;
        return false;
    }

    private static bool RegisterIntroducedContent(
        PrefabPatchManifest manifest,
        PrefabPatchOperation operation,
        OperationValidationState state
    )
    {
        if (operation.Kind == PrefabPatchOperationKind.AddObject)
        {
            var valid = RegisterFragment(
                operation.AddedObject,
                manifest.PatchId,
                operation.OperationId,
                state.IntroducedObjects,
                state.IntroducedComponents,
                state.Plan
            );
            state.Fatal |= !valid;
            return true;
        }

        if (operation.Kind != PrefabPatchOperationKind.AddComponent)
            return true;

        var componentId = operation.AddedComponent?.ComponentId;
        var componentKey = $"{manifest.PatchId}:{componentId}";
        if (
            string.IsNullOrWhiteSpace(componentId)
            || string.IsNullOrWhiteSpace(operation.AddedComponent?.ComponentType)
        )
        {
            Add(
                state.Plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-ADDED-COMPONENT-ID",
                manifest.PatchId,
                operation.OperationId,
                $"Added component operation '{operation.OperationId}' needs a "
                    + "stable component ID and assembly-qualified type."
            );
            state.Fatal = true;
            return false;
        }
        if (state.IntroducedComponents.TryAdd(componentKey, operation.OperationId))
            return true;

        Add(
            state.Plan,
            PrefabPatchDiagnosticSeverity.Error,
            "PM-PREFAB-DUPLICATE-COMPONENT-ID",
            manifest.PatchId,
            operation.OperationId,
            $"Patch-owned component '{componentKey}' is introduced more than once."
        );
        state.Fatal = true;
        return false;
    }

    private static void RecordWriteConflict(
        PrefabPatchManifest manifest,
        PrefabPatchOperation operation,
        OperationValidationState state
    )
    {
        if (!IsWrite(operation.Kind))
            return;

        var conflictKey = operation.ConflictKey;
        if (state.Writes.TryGetValue(conflictKey, out var previous))
        {
            var identical = string.Equals(
                OperationPayload(previous),
                OperationPayload(operation),
                StringComparison.Ordinal
            );
            Add(
                state.Plan,
                identical
                    ? PrefabPatchDiagnosticSeverity.Info
                    : PrefabPatchDiagnosticSeverity.Warning,
                identical ? "PM-PREFAB-IDENTICAL-WRITE" : "PM-PREFAB-SOFT-CONFLICT",
                manifest.PatchId,
                operation.OperationId,
                identical
                    ? $"'{manifest.PatchId}' repeats the identical write from "
                        + $"'{previous.PatchId}' to '{conflictKey}'."
                    : $"Different writes target '{conflictKey}'. Deterministic "
                        + $"order selects '{manifest.PatchId}' over "
                        + $"'{previous.PatchId}'."
            );
        }

        state.Writes[conflictKey] = operation;
    }

    private static string BuildInputHash(
        IReadOnlyList<PrefabPatchManifest> ordered,
        PrefabPatchPrefabIdentity target,
        IReadOnlyList<PrefabPatchDiagnostic> diagnostics,
        string unityVersion,
        string targetPlatform
    )
    {
        var value = new
        {
            UnityVersion = unityVersion,
            TargetPlatform = targetPlatform,
            SchemaVersion = PrefabPatchSchema.Version,
            ComposerVersion = PrefabPatchSchema.ComposerVersion,
            Target = target,
            OrderedPatches = ordered.Select(
                manifest => new
                {
                    manifest.PatchId,
                    manifest.ManifestHash,
                    manifest.ConfigurationInputs
                }
            ),
            Resolution = diagnostics.Select(
                diagnostic => new
                {
                    diagnostic.Severity,
                    diagnostic.Code,
                    diagnostic.PatchId,
                    diagnostic.OperationId,
                    diagnostic.Message
                }
            )
        };
        return PrefabPatchJson.Sha256(PrefabPatchJson.Serialize(value));
    }

    private static bool RegisterFragment(
        PrefabPatchObjectFragment fragment,
        string patchId,
        string operationId,
        IDictionary<string, string> objects,
        IDictionary<string, string> components,
        PrefabPatchResolvedPlan plan
    )
    {
        if (fragment == null || string.IsNullOrWhiteSpace(fragment.ObjectId))
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-ADDED-OBJECT-ID",
                patchId,
                operationId,
                $"Added object operation '{operationId}' contains an object "
                    + "without a stable patch-local ID."
            );
            return false;
        }

        var valid = true;
        var objectKey = $"{patchId}:{fragment.ObjectId}";
        if (!objects.TryAdd(objectKey, operationId))
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-DUPLICATE-OBJECT-ID",
                patchId,
                operationId,
                $"Patch-owned object '{objectKey}' is introduced more than "
                    + "once."
            );
            valid = false;
        }

        foreach (
            var component in fragment.Components
                ?? new List<PrefabPatchComponentFragment>()
        )
        {
            var componentId = component?.ComponentId;
            var componentKey = $"{patchId}:{componentId}";
            if (
                component == null
                || string.IsNullOrWhiteSpace(componentId)
                || string.IsNullOrWhiteSpace(component.ComponentType)
            )
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-ADDED-COMPONENT-ID",
                    patchId,
                    operationId,
                    $"Patch-owned object '{objectKey}' contains a component "
                        + "without a stable ID or assembly-qualified type."
                );
                valid = false;
                continue;
            }
            if (!components.TryAdd(componentKey, operationId))
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-DUPLICATE-COMPONENT-ID",
                    patchId,
                    operationId,
                    $"Patch-owned component '{componentKey}' is introduced "
                        + "more than once."
                );
                valid = false;
            }
        }

        foreach (
            var child in fragment.Children
                ?? new List<PrefabPatchObjectFragment>()
        )
        {
            if (
                !RegisterFragment(
                    child,
                    patchId,
                    operationId,
                    objects,
                    components,
                    plan
                )
            )
                valid = false;
        }

        return valid;
    }

    private static void AddSameBucketEdges(
        PrefabPatchManifest manifest,
        IEnumerable<string> ids,
        bool after,
        IReadOnlyDictionary<string, PrefabPatchManifest> members,
        IDictionary<string, HashSet<string>> incoming,
        IDictionary<string, HashSet<string>> outgoing,
        PrefabPatchResolvedPlan plan
    )
    {
        foreach (var id in Safe(ids))
        {
            if (!members.ContainsKey(id))
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Info,
                    "PM-PREFAB-CROSS-BUCKET-ORDER-IGNORED",
                    manifest.PatchId,
                    null,
                    $"Ordering relation between '{manifest.PatchId}' and "
                        + $"'{id}' is absent or crosses a pass/bucket and was "
                        + "ignored."
                );
                continue;
            }

            AddEdge(
                after ? id : manifest.PatchId,
                after ? manifest.PatchId : id,
                incoming,
                outgoing
            );
        }
    }

    private static void AddModEdges(
        PrefabPatchManifest manifest,
        IEnumerable<string> modIds,
        bool after,
        IReadOnlyDictionary<string, PrefabPatchManifest> members,
        IDictionary<string, HashSet<string>> incoming,
        IDictionary<string, HashSet<string>> outgoing
    )
    {
        foreach (var modId in Safe(modIds))
        {
            foreach (
                var other in members.Values.Where(
                    value =>
                        string.Equals(
                            value.ModId,
                            modId,
                            StringComparison.Ordinal
                        )
                        && value.PatchId != manifest.PatchId
                )
            )
            {
                AddEdge(
                    after ? other.PatchId : manifest.PatchId,
                    after ? manifest.PatchId : other.PatchId,
                    incoming,
                    outgoing
                );
            }
        }
    }

    private static void AddEdge(
        string before,
        string after,
        IDictionary<string, HashSet<string>> incoming,
        IDictionary<string, HashSet<string>> outgoing
    )
    {
        if (before == after)
            return;
        if (outgoing[before].Add(after))
            incoming[after].Add(before);
    }

    private static int Rank(PrefabPatchManifest manifest) =>
        ((int)manifest.Pass * 3) + (int)manifest.Ordering;

    private static bool IsWrite(PrefabPatchOperationKind kind) =>
        kind == PrefabPatchOperationKind.SetValue
        || kind == PrefabPatchOperationKind.SetObjectReference
        || kind == PrefabPatchOperationKind.SetActive
        || kind == PrefabPatchOperationKind.SuppressObject
        || kind == PrefabPatchOperationKind.RemoveComponent;

    private static string OperationPayload(PrefabPatchOperation operation) =>
        PrefabPatchJson.Serialize(
            new
            {
                operation.Kind,
                operation.PropertyPath,
                operation.Value,
                operation.ObjectReference
            }
        );

    private static IEnumerable<string> Safe(IEnumerable<string> values) =>
        values ?? Array.Empty<string>();

    private static void Add(
        PrefabPatchResolvedPlan plan,
        PrefabPatchDiagnosticSeverity severity,
        string code,
        string patchId,
        string operationId,
        string message
    )
    {
        plan.Diagnostics.Add(
            new PrefabPatchDiagnostic
            {
                Severity = severity,
                Code = code,
                TargetAddress = plan.TargetPrefab?.Address,
                PatchId = patchId,
                OperationId = operationId,
                Message = message
            }
        );
    }
}
