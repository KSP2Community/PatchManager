using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Deterministic dependency, ordering, target, and conflict resolver for the
/// prefab asset domain. It does not execute Unity mutations.
/// </summary>
public static class PrefabPatchResolver
{
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

        plan.TargetPrefab = manifests[0].TargetPrefab;
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

            if (
                !string.Equals(
                    manifest.TargetPrefab.CanonicalKey,
                    plan.TargetPrefab.CanonicalKey,
                    StringComparison.Ordinal
                )
            )
            {
                Add(
                    plan,
                    PrefabPatchDiagnosticSeverity.Error,
                    "PM-PREFAB-MIXED-TARGET",
                    manifest.PatchId,
                    null,
                    $"Patch '{manifest.PatchId}' targets "
                        + $"'{manifest.TargetPrefab.CanonicalKey}', not "
                        + $"'{plan.TargetPrefab.CanonicalKey}'."
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
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-PATCH-ID",
                manifest.PatchId,
                null,
                "Prefab patch IDs must be namespaced as 'mod-id:patch-id'."
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
            || string.IsNullOrWhiteSpace(
                manifest.TargetPrefab.SourceSerializedFileName
            )
            || manifest.TargetPrefab.SourcePathId == 0
            || string.IsNullOrWhiteSpace(
                manifest.TargetPrefab.StructuralFingerprint
            )
        )
        {
            Add(
                plan,
                PrefabPatchDiagnosticSeverity.Error,
                "PM-PREFAB-TARGET-IDENTITY",
                manifest.PatchId,
                null,
                $"Patch '{manifest.PatchId}' has an incomplete canonical "
                    + "prefab identity or structural fingerprint."
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
                var members = enabled
                    .Select(id => byId[id])
                    .Where(
                        manifest =>
                            manifest.Pass == pass && manifest.Ordering == bucket
                    )
                    .ToDictionary(
                        manifest => manifest.PatchId,
                        StringComparer.Ordinal
                    );
                var incoming = members.Keys.ToDictionary(
                    id => id,
                    _ => new HashSet<string>(StringComparer.Ordinal),
                    StringComparer.Ordinal
                );
                var outgoing = members.Keys.ToDictionary(
                    id => id,
                    _ => new HashSet<string>(StringComparer.Ordinal),
                    StringComparer.Ordinal
                );

                foreach (var manifest in members.Values)
                {
                    foreach (var dependency in Safe(manifest.NeedsPatches))
                    {
                        if (!enabled.Contains(dependency))
                            continue;
                        var dependencyManifest = byId[dependency];
                        if (
                            Rank(dependencyManifest) > Rank(manifest)
                        )
                        {
                            Add(
                                plan,
                                PrefabPatchDiagnosticSeverity.Error,
                                "PM-PREFAB-DEPENDENCY-ORDER",
                                manifest.PatchId,
                                null,
                                $"Patch '{manifest.PatchId}' requires later "
                                    + $"patch '{dependency}'."
                            );
                            fatal = true;
                        }
                        else if (members.ContainsKey(dependency))
                        {
                            AddEdge(
                                dependency,
                                manifest.PatchId,
                                incoming,
                                outgoing
                            );
                        }
                    }

                    AddSameBucketEdges(
                        manifest,
                        manifest.AfterPatches,
                        after: true,
                        members,
                        incoming,
                        outgoing,
                        plan
                    );
                    AddSameBucketEdges(
                        manifest,
                        manifest.BeforePatches,
                        after: false,
                        members,
                        incoming,
                        outgoing,
                        plan
                    );
                    AddModEdges(
                        manifest,
                        manifest.AfterMods,
                        after: true,
                        members,
                        incoming,
                        outgoing
                    );
                    AddModEdges(
                        manifest,
                        manifest.BeforeMods,
                        after: false,
                        members,
                        incoming,
                        outgoing
                    );
                }

                var ready = new SortedSet<string>(
                    incoming
                        .Where(pair => pair.Value.Count == 0)
                        .Select(pair => pair.Key),
                    StringComparer.Ordinal
                );
                var emitted = new HashSet<string>(StringComparer.Ordinal);
                while (ready.Count > 0)
                {
                    var id = ready.Min;
                    ready.Remove(id);
                    emitted.Add(id);
                    result.Add(members[id]);
                    foreach (var next in outgoing[id].OrderBy(
                                 value => value,
                                 StringComparer.Ordinal
                             ))
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
            }
        }

        return result;
    }

    private static void ValidateAndFlattenOperations(
        IReadOnlyList<PrefabPatchManifest> ordered,
        PrefabPatchResolvedPlan plan,
        ref bool fatal
    )
    {
        var patchOrder = ordered
            .Select((manifest, index) => (manifest.PatchId, index))
            .ToDictionary(pair => pair.PatchId, pair => pair.index);
        var introduced = new Dictionary<string, string>(StringComparer.Ordinal);
        var writes = new Dictionary<string, PrefabPatchOperation>(
            StringComparer.Ordinal
        );

        foreach (var manifest in ordered)
        {
            var needs = new HashSet<string>(
                Safe(manifest.NeedsPatches),
                StringComparer.Ordinal
            );
            foreach (
                var operation in manifest.Operations
                    .Where(value => value != null)
                    .OrderBy(value => value.OperationId, StringComparer.Ordinal)
            )
            {
                operation.PatchId = manifest.PatchId;
                if (string.IsNullOrWhiteSpace(operation.OperationId))
                {
                    Add(
                        plan,
                        PrefabPatchDiagnosticSeverity.Error,
                        "PM-PREFAB-OPERATION-ID",
                        manifest.PatchId,
                        null,
                        $"Patch '{manifest.PatchId}' contains an operation "
                            + "without an ID."
                    );
                    fatal = true;
                    continue;
                }

                if (
                    operation.Kind != PrefabPatchOperationKind.AddObject
                    && operation.Target == null
                )
                {
                    Add(
                        plan,
                        PrefabPatchDiagnosticSeverity.Error,
                        "PM-PREFAB-MISSING-TARGET",
                        manifest.PatchId,
                        operation.OperationId,
                        $"Operation '{operation.OperationId}' has no target."
                    );
                    fatal = true;
                    continue;
                }

                if (
                    operation.Target?.Kind
                    == PrefabPatchTargetKind.PatchOwned
                )
                {
                    var owner = operation.Target.OwnerPatchId;
                    var objectKey = $"{owner}:{operation.Target.ObjectId}";
                    if (
                        !string.Equals(
                            owner,
                            manifest.PatchId,
                            StringComparison.Ordinal
                        )
                        && !needs.Contains(owner)
                    )
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Error,
                            "PM-PREFAB-OWNER-NOT-REQUIRED",
                            manifest.PatchId,
                            operation.OperationId,
                            $"Operation '{operation.OperationId}' targets "
                                + $"'{objectKey}' but does not require owning "
                                + $"patch '{owner}'."
                        );
                        fatal = true;
                        continue;
                    }

                    if (
                        !introduced.ContainsKey(objectKey)
                        || patchOrder[owner] >= patchOrder[manifest.PatchId]
                    )
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Error,
                            "PM-PREFAB-PATCH-OWNED-TARGET",
                            manifest.PatchId,
                            operation.OperationId,
                            $"Patch-owned target '{objectKey}' is not introduced "
                                + "by an earlier required operation."
                        );
                        fatal = true;
                        continue;
                    }
                }

                if (operation.Kind == PrefabPatchOperationKind.AddObject)
                {
                    var objectId = operation.AddedObject?.ObjectId;
                    var objectKey = $"{manifest.PatchId}:{objectId}";
                    if (string.IsNullOrWhiteSpace(objectId))
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Error,
                            "PM-PREFAB-ADDED-OBJECT-ID",
                            manifest.PatchId,
                            operation.OperationId,
                            $"Added object operation '{operation.OperationId}' "
                                + "has no patch-local object ID."
                        );
                        fatal = true;
                        continue;
                    }

                    if (!introduced.TryAdd(objectKey, operation.OperationId))
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Error,
                            "PM-PREFAB-DUPLICATE-OBJECT-ID",
                            manifest.PatchId,
                            operation.OperationId,
                            $"Patch-owned object '{objectKey}' is introduced "
                                + "more than once."
                        );
                        fatal = true;
                        continue;
                    }
                }

                var conflictKey = operation.ConflictKey;
                if (
                    IsWrite(operation.Kind)
                    && writes.TryGetValue(conflictKey, out var previous)
                )
                {
                    if (
                        string.Equals(
                            OperationPayload(previous),
                            OperationPayload(operation),
                            StringComparison.Ordinal
                        )
                    )
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Info,
                            "PM-PREFAB-IDENTICAL-WRITE",
                            manifest.PatchId,
                            operation.OperationId,
                            $"'{manifest.PatchId}' repeats the identical write "
                                + $"from '{previous.PatchId}' to '{conflictKey}'."
                        );
                    }
                    else
                    {
                        Add(
                            plan,
                            PrefabPatchDiagnosticSeverity.Warning,
                            "PM-PREFAB-SOFT-CONFLICT",
                            manifest.PatchId,
                            operation.OperationId,
                            $"Different writes target '{conflictKey}'. "
                                + $"Deterministic order selects "
                                + $"'{manifest.PatchId}' over "
                                + $"'{previous.PatchId}'."
                        );
                    }
                }

                if (IsWrite(operation.Kind))
                    writes[conflictKey] = operation;
                plan.Operations.Add(operation);
            }
        }
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
