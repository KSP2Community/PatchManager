using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Runtime-computable hierarchy fingerprint and sibling-index traversal.
/// Canonical identity still comes from BundleKit CAB/path IDs.
/// </summary>
public static class PrefabPatchStructure
{
    /// <summary>
    /// Calculates a stable hierarchy fingerprint for a prefab root.
    /// </summary>
    /// <param name="root">Prefab root.</param>
    /// <returns>The structural hash, or null for a null root.</returns>
    public static string Calculate(GameObject root)
    {
        var description = Describe(root);
        return description == null
            ? null
            : PrefabPatchJson.Sha256(description);
    }

    /// <summary>
    /// Describes hierarchy names, sibling indices, active states, and component types.
    /// </summary>
    /// <param name="root">Prefab root.</param>
    /// <returns>The canonical description, or null for a null root.</returns>
    public static string Describe(GameObject root)
    {
        if (root == null)
            return null;
        var builder = new StringBuilder();
        Append(root.transform, builder, true);
        return builder.ToString();
    }

    /// <summary>
    /// Gets the sibling-index path from a root to a descendant transform.
    /// </summary>
    /// <param name="root">Ancestor transform.</param>
    /// <param name="target">Descendant transform.</param>
    /// <returns>The sibling-index path relative to <paramref name="root"/>.</returns>
    public static int[] GetSiblingPath(Transform root, Transform target)
    {
        var reverse = new List<int>();
        var current = target;
        while (current != null && current != root)
        {
            reverse.Add(current.GetSiblingIndex());
            current = current.parent;
        }

        if (current != root)
            throw new InvalidOperationException(
                $"Transform '{target?.name}' is not beneath '{root?.name}'."
            );
        reverse.Reverse();
        return reverse.ToArray();
    }

    /// <summary>
    /// Resolves a sibling-index path beneath a root transform.
    /// </summary>
    /// <param name="root">Starting transform.</param>
    /// <param name="path">Sibling indices to traverse.</param>
    /// <returns>The resolved transform, or null when the path is invalid.</returns>
    public static Transform Resolve(Transform root, IEnumerable<int> path)
    {
        var current = root;
        foreach (var index in path ?? Array.Empty<int>())
        {
            if (index < 0 || index >= current.childCount)
                return null;
            current = current.GetChild(index);
        }

        return current;
    }

    /// <summary>
    /// Resolves an unambiguous slash-separated hierarchy path.
    /// </summary>
    /// <param name="root">Starting transform.</param>
    /// <param name="hierarchyPath">Path of GameObject names.</param>
    /// <returns>The resolved transform, or null when no object matches.</returns>
    public static Transform ResolveHierarchyPath(
        Transform root,
        string hierarchyPath
    )
    {
        if (root == null)
            return null;
        var parts = (hierarchyPath ?? string.Empty)
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var index = 0;
        if (
            parts.Length > 0
            && string.Equals(
                parts[0],
                root.name,
                StringComparison.Ordinal
            )
        )
        {
            index = 1;
        }

        var current = root;
        for (; index < parts.Length; index++)
        {
            var name = parts[index];
            Transform match = null;
            for (var childIndex = 0; childIndex < current.childCount; childIndex++)
            {
                var child = current.GetChild(childIndex);
                if (!string.Equals(child.name, name, StringComparison.Ordinal))
                    continue;
                if (match != null)
                {
                    throw new InvalidOperationException(
                        $"Hierarchy path '{hierarchyPath}' is ambiguous: "
                            + $"'{current.name}' has multiple children named "
                            + $"'{name}'. Use visual authoring for this target."
                    );
                }
                match = child;
            }

            if (match == null)
                return null;
            current = match;
        }

        return current;
    }

    private static void Append(
        Transform transform,
        StringBuilder builder,
        bool isRoot
    )
    {
        builder.Append('[')
            .Append(isRoot ? 0 : transform.GetSiblingIndex())
            .Append('|')
            .Append(isRoot ? "<root>" : transform.name)
            .Append('|')
            .Append(transform.gameObject.activeSelf ? '1' : '0');
        foreach (
            var component in transform.gameObject
                .GetComponents<Component>()
                .Where(value => value != null)
        )
        {
            builder.Append('|')
                .Append(component.GetType().FullName);
        }

        builder.Append(']');
        for (var i = 0; i < transform.childCount; i++)
            Append(transform.GetChild(i), builder, false);
    }
}
