using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Fluent C# frontend that generates the same declarative public manifest used
/// by visual prefab-variant compilation.
/// </summary>
/// <remarks>
/// Calls only mutate an in-memory <see cref="PrefabPatchManifest" />. Call
/// <see cref="Build" /> to inspect or serialize it, or <see cref="Register" />
/// during mod initialization to add it to the current runtime registration
/// window. Dependency and ordering methods share the
/// <see cref="IPatchRelationships" /> model used by ordinary JSON patches.
/// </remarks>
public sealed class PrefabPatchBuilder
{
    private readonly string _modId;
    private readonly PrefabPatchManifest _manifest;
    private readonly HashSet<string> _needsMods = new(StringComparer.Ordinal);
    private readonly HashSet<string> _conflictsMods = new(
        StringComparer.Ordinal
    );
    private readonly HashSet<string> _needsPatches = new(StringComparer.Ordinal);
    private readonly HashSet<string> _conflictsPatches = new(
        StringComparer.Ordinal
    );
    private readonly HashSet<string> _beforePatches = new(
        StringComparer.Ordinal
    );
    private readonly HashSet<string> _afterPatches = new(StringComparer.Ordinal);
    private readonly HashSet<string> _beforeMods = new(StringComparer.Ordinal);
    private readonly HashSet<string> _afterMods = new(StringComparer.Ordinal);

    /// <summary>Creates a builder for a prefab identified by a complete target descriptor.</summary>
    /// <param name="modId">The owning SpaceWarp mod ID.</param>
    /// <param name="patchName">A mod-local patch name without a colon.</param>
    /// <param name="target">The stock prefab identity to patch.</param>
    public PrefabPatchBuilder(
        string modId,
        string patchName,
        PrefabPatchPrefabIdentity target
    )
    {
        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("Mod ID is required.", nameof(modId));
        if (string.IsNullOrWhiteSpace(patchName))
            throw new ArgumentException(
                "Patch name is required.",
                nameof(patchName)
            );
        if (patchName.IndexOf(':') >= 0)
            throw new ArgumentException(
                "Patch name must be local to the mod and cannot contain ':'.",
                nameof(patchName)
            );
        _modId = modId.Trim();
        _manifest = new PrefabPatchManifest
        {
            PatchName = patchName.Trim(),
            TargetPrefab =
                target ?? throw new ArgumentNullException(nameof(target))
        };
    }

    /// <summary>Creates a builder for a stock prefab Addressables key.</summary>
    /// <param name="modId">The owning SpaceWarp mod ID.</param>
    /// <param name="patchName">A mod-local patch name without a colon.</param>
    /// <param name="address">The stock prefab Addressables key.</param>
    public PrefabPatchBuilder(
        string modId,
        string patchName,
        string address
    ) : this(
        modId,
        patchName,
        PrefabPatchPrefabIdentity.FromAddress(address)
    ) { }

    /// <summary>Places the patch in the Early pass.</summary>
    public PrefabPatchBuilder Early()
    {
        _manifest.Pass = PrefabPatchPass.Early;
        return this;
    }

    /// <summary>Places the patch in the Late pass.</summary>
    public PrefabPatchBuilder Late()
    {
        _manifest.Pass = PrefabPatchPass.Late;
        return this;
    }

    /// <summary>Places the patch in the First bucket of its pass.</summary>
    public PrefabPatchBuilder First()
    {
        _manifest.Ordering = PrefabPatchOrdering.First;
        return this;
    }

    /// <summary>Places the patch in the Last bucket of its pass.</summary>
    public PrefabPatchBuilder Last()
    {
        _manifest.Ordering = PrefabPatchOrdering.Last;
        return this;
    }

    /// <summary>Requires the supplied mod IDs to be active.</summary>
    public PrefabPatchBuilder NeedsMod(params string[] ids) =>
        Add(_needsMods, ids);

    /// <summary>Disables the patch when any supplied mod ID is active.</summary>
    public PrefabPatchBuilder ConflictsMod(params string[] ids) =>
        Add(_conflictsMods, ids);

    /// <summary>Requires the supplied patch IDs to be enabled.</summary>
    public PrefabPatchBuilder NeedsPatch(params string[] ids) =>
        Add(_needsPatches, Normalize(ids));

    /// <summary>Disables the patch when any supplied patch ID is enabled.</summary>
    public PrefabPatchBuilder ConflictsPatch(params string[] ids) =>
        Add(_conflictsPatches, Normalize(ids));

    /// <summary>Orders this patch before the supplied patch IDs in the same bucket.</summary>
    public PrefabPatchBuilder BeforePatch(params string[] ids) =>
        Add(_beforePatches, Normalize(ids));

    /// <summary>Orders this patch after the supplied patch IDs in the same bucket.</summary>
    public PrefabPatchBuilder AfterPatch(params string[] ids) =>
        Add(_afterPatches, Normalize(ids));

    /// <summary>Orders this patch before patches owned by the supplied mods.</summary>
    public PrefabPatchBuilder BeforeMod(params string[] ids) =>
        Add(_beforeMods, ids);

    /// <summary>Orders this patch after patches owned by the supplied mods.</summary>
    public PrefabPatchBuilder AfterMod(params string[] ids) =>
        Add(_afterMods, ids);

    /// <summary>Adds a normalized operation and binds it to this patch.</summary>
    /// <param name="operation">The operation to append in call order.</param>
    public PrefabPatchBuilder AddOperation(PrefabPatchOperation operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));
        operation.PatchId = PrefabPatchOwnership.Qualify(
            _modId,
            _manifest.PatchName
        );
        _manifest.Operations.Add(operation);
        return this;
    }

    /// <summary>Writes a serialized value on the target object or component.</summary>
    public PrefabPatchBuilder SetValue(
        string operationId,
        PrefabPatchObjectTarget target,
        string propertyPath,
        PrefabPatchValue value
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.SetValue,
                Target = target,
                PropertyPath = propertyPath,
                Value = value
            }
        );

    /// <summary>Changes a target GameObject's active state.</summary>
    public PrefabPatchBuilder SetActive(
        string operationId,
        PrefabPatchObjectTarget target,
        bool active
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.SetActive,
                Target = target,
                Value = PrefabPatchValue.FromBoolean(active)
            }
        );

    /// <summary>Writes an Addressable or target-local Unity object reference.</summary>
    public PrefabPatchBuilder SetObjectReference(
        string operationId,
        PrefabPatchObjectTarget target,
        string propertyPath,
        PrefabPatchObjectReference reference
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.SetObjectReference,
                Target = target,
                PropertyPath = propertyPath,
                ObjectReference = reference
            }
        );

    /// <summary>Suppresses a target GameObject in the effective prefab.</summary>
    public PrefabPatchBuilder SuppressObject(
        string operationId,
        PrefabPatchObjectTarget target
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.SuppressObject,
                Target = target
            }
        );

    /// <summary>Adds an inline patch-owned hierarchy beneath a target parent.</summary>
    public PrefabPatchBuilder AddObject(
        string operationId,
        PrefabPatchObjectTarget parent,
        PrefabPatchObjectFragment fragment
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.AddObject,
                Target = parent,
                AddedObject = fragment
            }
        );

    /// <summary>Adds a serialized component fragment to a target GameObject.</summary>
    public PrefabPatchBuilder AddComponent(
        string operationId,
        PrefabPatchObjectTarget target,
        PrefabPatchComponentFragment component
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.AddComponent,
                Target = target,
                AddedComponent = component
            }
        );

    /// <summary>Removes the targeted component from the effective prefab.</summary>
    public PrefabPatchBuilder RemoveComponent(
        string operationId,
        PrefabPatchObjectTarget target
    ) =>
        AddOperation(
            new PrefabPatchOperation
            {
                OperationId = operationId,
                Kind = PrefabPatchOperationKind.RemoveComponent,
                Target = target
            }
        );

    /// <summary>Adds configuration values that participate in cache invalidation.</summary>
    public PrefabPatchBuilder Configuration(params string[] inputs)
    {
        _manifest.ConfigurationInputs = Sorted(
            (_manifest.ConfigurationInputs ?? Array.Empty<string>())
                .Concat(inputs ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
        );
        return this;
    }

    /// <summary>Targets an object introduced by this or a required patch.</summary>
    public static PrefabPatchObjectTarget PatchObject(
        string ownerPatchId,
        string objectId
    ) =>
        new()
        {
            Kind = PrefabPatchTargetKind.PatchOwned,
            OwnerPatchId = ownerPatchId,
            ObjectId = objectId,
            RuntimeLocator = new PrefabPatchRuntimeLocator
            {
                TargetKind = PrefabPatchRuntimeTargetKind.GameObject,
                SiblingIndices = Array.Empty<int>()
            }
        };

    /// <summary>Targets a component introduced by this or a required patch.</summary>
    public static PrefabPatchObjectTarget PatchComponent(
        string ownerPatchId,
        string componentId
    ) =>
        new()
        {
            Kind = PrefabPatchTargetKind.PatchComponent,
            OwnerPatchId = ownerPatchId,
            ComponentId = componentId,
            RuntimeLocator = new PrefabPatchRuntimeLocator
            {
                TargetKind = PrefabPatchRuntimeTargetKind.Component,
                SiblingIndices = Array.Empty<int>()
            }
        };

    /// <summary>Targets a stock GameObject by hierarchy path.</summary>
    public static PrefabPatchObjectTarget GameObjectAt(
        string hierarchyPath
    ) =>
        StockTarget(
            hierarchyPath,
            PrefabPatchRuntimeTargetKind.GameObject,
            typeof(UnityEngine.GameObject).AssemblyQualifiedName,
            0
        );

    /// <summary>Targets a typed stock component by hierarchy path and ordinal.</summary>
    public static PrefabPatchObjectTarget ComponentAt<TComponent>(
        string hierarchyPath,
        int componentOrdinal = 0
    ) where TComponent : UnityEngine.Component =>
        ComponentAt(
            hierarchyPath,
            typeof(TComponent).AssemblyQualifiedName,
            componentOrdinal
        );

    /// <summary>Targets a stock component by assembly-qualified type and ordinal.</summary>
    public static PrefabPatchObjectTarget ComponentAt(
        string hierarchyPath,
        string componentType,
        int componentOrdinal = 0
    )
    {
        if (string.IsNullOrWhiteSpace(componentType))
            throw new ArgumentException(
                "Component type is required.",
                nameof(componentType)
            );
        return StockTarget(
            hierarchyPath,
            PrefabPatchRuntimeTargetKind.Component,
            componentType,
            componentOrdinal
        );
    }

    /// <summary>Creates a reference to an Addressable asset.</summary>
    public static PrefabPatchObjectReference Addressable(
        string address,
        Type expectedType = null
    ) =>
        PrefabPatchObjectReference.FromAddress(
            address,
            expectedType?.AssemblyQualifiedName
        );

    /// <summary>Creates a reference to another object in the composed prefab.</summary>
    public static PrefabPatchObjectReference TargetReference(
        PrefabPatchObjectTarget target,
        Type expectedType = null
    ) =>
        PrefabPatchObjectReference.FromTarget(
            target,
            expectedType?.AssemblyQualifiedName
        );

    /// <summary>Normalizes relationships, capabilities, ownership, and the manifest hash.</summary>
    public PrefabPatchManifest Build()
    {
        _manifest.NeedsMods = Sorted(_needsMods);
        _manifest.ConflictsMods = Sorted(_conflictsMods);
        _manifest.NeedsPatches = Sorted(_needsPatches);
        _manifest.ConflictsPatches = Sorted(_conflictsPatches);
        _manifest.BeforePatches = Sorted(_beforePatches);
        _manifest.AfterPatches = Sorted(_afterPatches);
        _manifest.BeforeMods = Sorted(_beforeMods);
        _manifest.AfterMods = Sorted(_afterMods);
        _manifest.DeclaredCapabilities = _manifest.Operations
            .Select(value => value.Kind.ToString())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        _manifest.ManifestHash = PrefabPatchJson.CalculateManifestHash(_manifest);
        return PrefabPatchOwnership.Bind(_manifest, _modId);
    }

    /// <summary>Builds and registers the manifest for the current play session.</summary>
    public PrefabPatchManifest Register()
    {
        var manifest = Build();
        PrefabPatchRuntime.Register(manifest);
        return manifest;
    }

    private PrefabPatchBuilder Add(
        ISet<string> destination,
        IEnumerable<string> ids
    )
    {
        foreach (
            var id in (ids ?? Array.Empty<string>()).Where(
                value => !string.IsNullOrWhiteSpace(value)
            )
        )
        {
            destination.Add(id);
        }

        return this;
    }

    private static IEnumerable<string> Normalize(IEnumerable<string> ids) =>
        ids ?? Array.Empty<string>();

    private static string[] Sorted(IEnumerable<string> values) =>
        values.OrderBy(value => value, StringComparer.Ordinal).ToArray();

    private static PrefabPatchObjectTarget StockTarget(
        string hierarchyPath,
        PrefabPatchRuntimeTargetKind targetKind,
        string objectType,
        int componentOrdinal
    )
    {
        if (hierarchyPath == null)
            throw new ArgumentNullException(nameof(hierarchyPath));
        if (componentOrdinal < 0)
            throw new ArgumentOutOfRangeException(
                nameof(componentOrdinal)
            );
        var normalizedPath = string.Join(
            "/",
            hierarchyPath.Split(
                new[] { '/' },
                StringSplitOptions.RemoveEmptyEntries
            )
        );
        return new PrefabPatchObjectTarget
        {
            Kind = PrefabPatchTargetKind.Stock,
            ObjectType = objectType,
            RuntimeLocator = new PrefabPatchRuntimeLocator
            {
                HierarchyPath = normalizedPath,
                TargetKind = targetKind,
                ComponentType =
                    targetKind == PrefabPatchRuntimeTargetKind.Component
                        ? objectType
                        : null,
                ComponentOrdinal = componentOrdinal,
                DisplayPath = normalizedPath,
                SiblingIndices = Array.Empty<int>()
            }
        };
    }
}
