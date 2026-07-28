using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Fluent C# frontend that generates the same declarative public manifest used
/// by visual prefab-variant compilation.
/// </summary>
public sealed class PrefabPatchBuilder
{
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

    public PrefabPatchBuilder(
        string modId,
        string patchName,
        PrefabPatchPrefabIdentity target,
        string modVersion = null
    )
    {
        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("Mod ID is required.", nameof(modId));
        if (string.IsNullOrWhiteSpace(patchName))
            throw new ArgumentException(
                "Patch name is required.",
                nameof(patchName)
            );
        _manifest = new PrefabPatchManifest
        {
            PatchId =
                patchName.IndexOf(':') >= 0
                    ? patchName
                    : modId + ":" + patchName,
            ModId = modId,
            ModVersion = modVersion,
            TargetPrefab =
                target ?? throw new ArgumentNullException(nameof(target))
        };
    }

    public PrefabPatchBuilder Early()
    {
        _manifest.Pass = PrefabPatchPass.Early;
        return this;
    }

    public PrefabPatchBuilder Late()
    {
        _manifest.Pass = PrefabPatchPass.Late;
        return this;
    }

    public PrefabPatchBuilder First()
    {
        _manifest.Ordering = PrefabPatchOrdering.First;
        return this;
    }

    public PrefabPatchBuilder Last()
    {
        _manifest.Ordering = PrefabPatchOrdering.Last;
        return this;
    }

    public PrefabPatchBuilder NeedsMod(params string[] ids) =>
        Add(_needsMods, ids);

    public PrefabPatchBuilder ConflictsMod(params string[] ids) =>
        Add(_conflictsMods, ids);

    public PrefabPatchBuilder NeedsPatch(params string[] ids) =>
        Add(_needsPatches, Normalize(ids));

    public PrefabPatchBuilder ConflictsPatch(params string[] ids) =>
        Add(_conflictsPatches, Normalize(ids));

    public PrefabPatchBuilder BeforePatch(params string[] ids) =>
        Add(_beforePatches, Normalize(ids));

    public PrefabPatchBuilder AfterPatch(params string[] ids) =>
        Add(_afterPatches, Normalize(ids));

    public PrefabPatchBuilder BeforeMod(params string[] ids) =>
        Add(_beforeMods, ids);

    public PrefabPatchBuilder AfterMod(params string[] ids) =>
        Add(_afterMods, ids);

    public PrefabPatchBuilder AddOperation(PrefabPatchOperation operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));
        operation.PatchId = _manifest.PatchId;
        _manifest.Operations.Add(operation);
        return this;
    }

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
        _manifest.Operations = _manifest.Operations
            .OrderBy(value => value.OperationId, StringComparer.Ordinal)
            .ToList();
        _manifest.DeclaredCapabilities = _manifest.Operations
            .Select(value => value.Kind.ToString())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        _manifest.ManifestHash = PrefabPatchJson.CalculateManifestHash(_manifest);
        return _manifest;
    }

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

    private IEnumerable<string> Normalize(IEnumerable<string> ids) =>
        (ids ?? Array.Empty<string>()).Select(
            id =>
                id.IndexOf(':') >= 0
                    ? id
                    : _manifest.ModId + ":" + id
        );

    private static string[] Sorted(IEnumerable<string> values) =>
        values.OrderBy(value => value, StringComparer.Ordinal).ToArray();
}
