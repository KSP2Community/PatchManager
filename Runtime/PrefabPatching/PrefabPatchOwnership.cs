using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Applies the containing mod's runtime identity to an ownership-free prefab
/// patch manifest. Serialized manifests deliberately do not duplicate swinfo
/// metadata.
/// </summary>
public static class PrefabPatchOwnership
{
    public static PrefabPatchManifest Bind(
        PrefabPatchManifest manifest,
        string modId
    )
    {
        if (manifest == null)
            throw new ArgumentNullException(nameof(manifest));
        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException(
                "The containing mod ID is required.",
                nameof(modId)
            );
        if (string.IsNullOrWhiteSpace(manifest.PatchName))
            throw new InvalidOperationException(
                "Prefab patch manifests must define a local patchName."
            );
        if (manifest.PatchName.IndexOf(':') >= 0)
            throw new InvalidOperationException(
                $"Prefab patch name '{manifest.PatchName}' must be local to "
                    + "its containing mod and cannot contain ':'."
            );

        modId = modId.Trim();
        if (!string.IsNullOrWhiteSpace(manifest.ModId))
        {
            if (!string.Equals(manifest.ModId, modId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Prefab patch '{manifest.PatchName}' is already bound to "
                        + $"'{manifest.ModId}', not '{modId}'."
                );
            }
        }

        var authoredHash = PrefabPatchJson.CalculateManifestHash(manifest);
        if (
            !string.IsNullOrWhiteSpace(manifest.ManifestHash)
            && !string.Equals(
                authoredHash,
                manifest.ManifestHash,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                $"Prefab patch '{manifest.PatchName}' manifest hash does not "
                    + "match its authored content."
            );
        }

        manifest.ModId = modId;
        manifest.PatchId = Qualify(modId, manifest.PatchName);
        manifest.NeedsPatches = QualifyAll(modId, manifest.NeedsPatches);
        manifest.ConflictsPatches = QualifyAll(
            modId,
            manifest.ConflictsPatches
        );
        manifest.BeforePatches = QualifyAll(modId, manifest.BeforePatches);
        manifest.AfterPatches = QualifyAll(modId, manifest.AfterPatches);

        foreach (var operation in manifest.Operations ?? new())
        {
            if (operation == null)
                continue;
            operation.PatchId = manifest.PatchId;
            BindTarget(operation.Target, modId);
            BindReference(operation.ObjectReference, modId);
            BindFragment(operation.AddedObject, modId);
            BindComponent(operation.AddedComponent, modId);
        }

        manifest.ManifestHash = PrefabPatchJson.CalculateManifestHash(manifest);
        return manifest;
    }

    public static string Qualify(string modId, string patchNameOrId)
    {
        if (string.IsNullOrWhiteSpace(patchNameOrId))
            return patchNameOrId;
        var value = patchNameOrId.Trim();
        return value.IndexOf(':') >= 0 ? value : modId + ":" + value;
    }

    private static string[] QualifyAll(
        string modId,
        IEnumerable<string> values
    ) =>
        (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Qualify(modId, value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static void BindFragment(
        PrefabPatchObjectFragment fragment,
        string modId
    )
    {
        if (fragment == null)
            return;
        foreach (var component in fragment.Components ?? new())
            BindComponent(component, modId);
        foreach (var child in fragment.Children ?? new())
            BindFragment(child, modId);
    }

    private static void BindComponent(
        PrefabPatchComponentFragment component,
        string modId
    )
    {
        if (component == null)
            return;
        foreach (var reference in component.References ?? new())
            BindReference(reference?.Reference, modId);
    }

    private static void BindReference(
        PrefabPatchObjectReference reference,
        string modId
    )
    {
        if (reference?.Kind == PrefabPatchObjectReferenceKind.Target)
            BindTarget(reference.Target, modId);
    }

    private static void BindTarget(
        PrefabPatchObjectTarget target,
        string modId
    )
    {
        if (
            target == null
            || target.Kind == PrefabPatchTargetKind.Stock
            || string.IsNullOrWhiteSpace(target.OwnerPatchId)
        )
        {
            return;
        }
        target.OwnerPatchId = Qualify(modId, target.OwnerPatchId);
    }
}
