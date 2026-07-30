using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Applies a validated resolved plan once to a loaded external prefab asset.
/// The caller owns and retains the Addressables handles.
/// </summary>
public static class PrefabPatchComposer
{
    private sealed class PendingReference
    {
        public Object Target;
        public string PropertyPath;
        public PrefabPatchObjectReference Reference;
        public string OperationId;
    }

    private sealed class AnimationCurvePayload
    {
        public Keyframe[] Keys;
        public int PreWrapMode;
        public int PostWrapMode;
    }

    private sealed class GradientPayload
    {
        public GradientColorKey[] ColorKeys;
        public GradientAlphaKey[] AlphaKeys;
        public int Mode;
    }

    public sealed class Result
    {
        public bool Success;
        public string Failure;
        public long ElapsedMilliseconds;
        public int AppliedOperationCount;
        public Dictionary<string, GameObject> PatchOwnedObjects = new(
            StringComparer.Ordinal
        );
        public Dictionary<string, Component> PatchOwnedComponents = new(
            StringComparer.Ordinal
        );
    }

    public static Result ApplySynchronously(
        GameObject prefab,
        PrefabPatchResolvedPlan plan,
        IReadOnlyDictionary<string, Object> references
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Result();
        Transform originalParent = null;
        var originalSiblingIndex = 0;
        GameObject safetyRoot = null;
        try
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (plan == null || !plan.IsValid)
                throw new InvalidOperationException(
                    "The prefab patch plan is missing or invalid."
                );
            var actualFingerprint = PrefabPatchStructure.Calculate(prefab);
            if (
                !string.IsNullOrWhiteSpace(
                    plan.TargetPrefab.StructuralFingerprint
                )
                && !string.Equals(
                    actualFingerprint,
                    plan.TargetPrefab.StructuralFingerprint,
                    StringComparison.Ordinal
                )
            )
            {
                throw new InvalidOperationException(
                    $"Stock prefab '{plan.TargetPrefab.Address}' structural "
                        + $"fingerprint changed. Expected "
                        + $"'{plan.TargetPrefab.StructuralFingerprint}', got "
                        + $"'{actualFingerprint}'. Expected structure: "
                        + $"'{plan.TargetPrefab.StructuralDescription}'. "
                        + $"Actual structure: "
                        + $"'{PrefabPatchStructure.Describe(prefab)}'. "
                        + "Recompile or repair the patch."
                );
            }

            if (prefab.activeInHierarchy)
            {
                originalParent = prefab.transform.parent;
                originalSiblingIndex = prefab.transform.GetSiblingIndex();
                safetyRoot = new GameObject(
                    "PatchManager Composition Root"
                );
                safetyRoot.hideFlags = HideFlags.HideAndDontSave;
                safetyRoot.SetActive(false);
                prefab.transform.SetParent(safetyRoot.transform, false);
            }
            var unusedDeferredDestroy = false;
            var pendingReferences = new List<PendingReference>();
            foreach (var operation in plan.Operations)
            {
                ApplyOne(
                    prefab,
                    operation,
                    references,
                    result.PatchOwnedObjects,
                    result.PatchOwnedComponents,
                    pendingReferences,
                    ref unusedDeferredDestroy,
                    true
                );
                result.AppliedOperationCount++;
            }

            foreach (var pending in pendingReferences)
            {
                var reference = ResolveReference(
                    prefab,
                    pending.Reference,
                    references,
                    result.PatchOwnedObjects,
                    result.PatchOwnedComponents
                );
                if (reference == null && pending.Reference != null)
                {
                    throw new InvalidOperationException(
                        $"Operation '{pending.OperationId}' could not resolve "
                            + $"object reference "
                            + $"'{DescribeReference(pending.Reference)}'."
                    );
                }

                SetRawValue(
                    pending.Target,
                    pending.PropertyPath,
                    reference
                );
            }

            result.Success = true;
        }
        catch (Exception exception)
        {
            result.Failure = exception.ToString();
        }
        finally
        {
            if (safetyRoot != null)
            {
                if (prefab != null)
                {
                    prefab.transform.SetParent(originalParent, false);
                    if (originalParent != null)
                        prefab.transform.SetSiblingIndex(originalSiblingIndex);
                }
                Object.DestroyImmediate(safetyRoot);
            }
            stopwatch.Stop();
            result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    private static void ApplyOne(
        GameObject root,
        PrefabPatchOperation operation,
        IReadOnlyDictionary<string, Object> references,
        IDictionary<string, GameObject> patchOwned,
        IDictionary<string, Component> patchComponents,
        ICollection<PendingReference> pendingReferences,
        ref bool deferredDestroy,
        bool immediateDestroy
    )
    {
        if (operation.Kind == PrefabPatchOperationKind.AddObject)
        {
            var parentObject = operation.Target == null
                ? root
                : AsGameObject(
                    Resolve(
                        root,
                        operation.Target,
                        patchOwned,
                        patchComponents
                    )
                );
            if (parentObject == null)
                throw MissingTarget(operation);
            CreateFragment(
                operation.PatchId,
                operation.AddedObject,
                parentObject.transform,
                references,
                patchOwned,
                patchComponents,
                pendingReferences,
                operation.OperationId
            );
            return;
        }

        var target = Resolve(
            root,
            operation.Target,
            patchOwned,
            patchComponents
        );
        if (target == null)
            throw MissingTarget(operation);
        switch (operation.Kind)
        {
            case PrefabPatchOperationKind.SetValue:
                SetValue(target, operation.PropertyPath, operation.Value);
                return;
            case PrefabPatchOperationKind.SetObjectReference:
                if (operation.ObjectReference == null)
                    throw new InvalidOperationException(
                        $"Operation '{operation.OperationId}' has no object "
                            + "reference payload."
                    );
                pendingReferences.Add(
                    new PendingReference
                    {
                        Target = target,
                        PropertyPath = operation.PropertyPath,
                        Reference = operation.ObjectReference,
                        OperationId = operation.OperationId
                    }
                );
                return;
            case PrefabPatchOperationKind.SetActive:
                AsGameObject(target)?.SetActive(
                    operation.Value?.Boolean ?? false
                );
                return;
            case PrefabPatchOperationKind.SuppressObject:
                AsGameObject(target)?.SetActive(false);
                return;
            case PrefabPatchOperationKind.AddComponent:
                AddComponent(
                    AsGameObject(target),
                    operation.AddedComponent,
                    references,
                    patchOwned,
                    patchComponents,
                    pendingReferences,
                    operation.PatchId,
                    operation.OperationId
                );
                return;
            case PrefabPatchOperationKind.RemoveComponent:
                if (target is not Component component)
                {
                    throw new InvalidOperationException(
                        $"RemoveComponent operation '{operation.OperationId}' "
                            + "did not resolve to a Component."
                    );
                }

                if (immediateDestroy)
                {
                    // Runtime composition operates on a session-owned clone,
                    // never directly on the read-only AssetBundle asset.
                    Object.DestroyImmediate(component);
                    if (component != null)
                    {
                        throw new InvalidOperationException(
                            $"RemoveComponent operation "
                                + $"'{operation.OperationId}' did not destroy "
                                + $"'{operation.Target.ObjectType}'."
                        );
                    }
                }
                else
                {
                    Object.Destroy(component);
                    deferredDestroy = true;
                }
                return;
            default:
                throw new NotSupportedException(
                    $"Prefab operation kind '{operation.Kind}' is not supported "
                        + $"by composer {PrefabPatchSchema.ComposerVersion}."
                );
        }
    }

    private static Object Resolve(
        GameObject root,
        PrefabPatchObjectTarget target,
        IDictionary<string, GameObject> patchOwned,
        IDictionary<string, Component> patchComponents
    )
    {
        if (target.Kind == PrefabPatchTargetKind.PatchComponent)
        {
            var componentKey =
                $"{target.OwnerPatchId}:{target.ComponentId}";
            return patchComponents.TryGetValue(
                componentKey,
                out var patchComponent
            )
                ? patchComponent
                : null;
        }

        Transform transform;
        if (target.Kind == PrefabPatchTargetKind.Stock)
        {
            transform = !string.IsNullOrWhiteSpace(
                target.RuntimeLocator?.HierarchyPath
            )
                ? PrefabPatchStructure.ResolveHierarchyPath(
                    root.transform,
                    target.RuntimeLocator.HierarchyPath
                )
                : PrefabPatchStructure.Resolve(
                    root.transform,
                    target.RuntimeLocator?.SiblingIndices
                );
        }
        else
        {
            var key = $"{target.OwnerPatchId}:{target.ObjectId}";
            if (!patchOwned.TryGetValue(key, out var owned) || owned == null)
                return null;
            transform = owned.transform;
        }

        if (transform == null)
            return null;
        if (
            target.RuntimeLocator == null
            || target.RuntimeLocator.TargetKind
            == PrefabPatchRuntimeTargetKind.GameObject
        )
        {
            return transform.gameObject;
        }

        var type = ResolveType(target.RuntimeLocator.ComponentType);
        if (type == null || !typeof(Component).IsAssignableFrom(type))
            return null;
        var components = transform.gameObject.GetComponents(type);
        var ordinal = target.RuntimeLocator.ComponentOrdinal;
        return ordinal >= 0 && ordinal < components.Length
            ? components[ordinal]
            : null;
    }

    private static GameObject CreateFragment(
        string patchId,
        PrefabPatchObjectFragment fragment,
        Transform parent,
        IReadOnlyDictionary<string, Object> references,
        IDictionary<string, GameObject> patchOwned,
        IDictionary<string, Component> patchComponents,
        ICollection<PendingReference> pendingReferences,
        string operationId
    )
    {
        if (fragment == null || string.IsNullOrWhiteSpace(fragment.ObjectId))
            throw new InvalidOperationException(
                $"Patch '{patchId}' contains an invalid added-object fragment."
            );
        var key = $"{patchId}:{fragment.ObjectId}";
        if (patchOwned.ContainsKey(key))
            throw new InvalidOperationException(
                $"Patch-owned object '{key}' already exists."
            );

        var transformType = ResolveType(fragment.TransformType);
        var gameObject =
            transformType != null
            && typeof(RectTransform).IsAssignableFrom(transformType)
                ? new GameObject(
                    fragment.Name ?? fragment.ObjectId,
                    typeof(RectTransform)
                )
                : new GameObject(fragment.Name ?? fragment.ObjectId);
        gameObject.transform.SetParent(parent, false);
        gameObject.layer = fragment.Layer;
        if (!string.IsNullOrWhiteSpace(fragment.Tag))
        {
            try
            {
                gameObject.tag = fragment.Tag;
            }
            catch (UnityException exception)
            {
                throw new InvalidOperationException(
                    $"Patch-owned object '{key}' uses unknown tag "
                        + $"'{fragment.Tag}'.",
                    exception
                );
            }
        }
        gameObject.isStatic = fragment.IsStatic;
        gameObject.transform.localPosition = ToVector3(
            fragment.LocalPosition,
            Vector3.zero
        );
        gameObject.transform.localRotation = ToQuaternion(
            fragment.LocalRotation,
            Quaternion.identity
        );
        gameObject.transform.localScale = ToVector3(
            fragment.LocalScale,
            Vector3.one
        );
        if (gameObject.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = ToVector2(
                fragment.AnchorMin,
                rectTransform.anchorMin
            );
            rectTransform.anchorMax = ToVector2(
                fragment.AnchorMax,
                rectTransform.anchorMax
            );
            rectTransform.anchoredPosition = ToVector2(
                fragment.AnchoredPosition,
                rectTransform.anchoredPosition
            );
            rectTransform.sizeDelta = ToVector2(
                fragment.SizeDelta,
                rectTransform.sizeDelta
            );
            rectTransform.pivot = ToVector2(
                fragment.Pivot,
                rectTransform.pivot
            );
        }
        gameObject.SetActive(fragment.Active);
        gameObject.AddComponent<PrefabPatchObjectId>().Id = fragment.ObjectId;
        patchOwned.Add(key, gameObject);
        foreach (var component in fragment.Components)
            AddComponent(
                gameObject,
                component,
                references,
                patchOwned,
                patchComponents,
                pendingReferences,
                patchId,
                operationId
            );
        foreach (var child in fragment.Children)
            CreateFragment(
                patchId,
                child,
                gameObject.transform,
                references,
                patchOwned,
                patchComponents,
                pendingReferences,
                operationId
            );
        return gameObject;
    }

    private static Component AddComponent(
        GameObject target,
        PrefabPatchComponentFragment fragment,
        IReadOnlyDictionary<string, Object> references,
        IDictionary<string, GameObject> patchOwned,
        IDictionary<string, Component> patchComponents,
        ICollection<PendingReference> pendingReferences,
        string patchId,
        string operationId
    )
    {
        if (target == null || fragment == null)
            throw new InvalidOperationException(
                "An added component has no target or payload."
            );
        if (string.IsNullOrWhiteSpace(fragment.ComponentType))
            throw new InvalidOperationException(
                "An added component has no assembly-qualified component type."
            );
        var type = ResolveType(fragment.ComponentType);
        if (type == null || !typeof(Component).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"Added component type '{fragment.ComponentType}' could "
                    + "not be resolved as a Unity Component."
            );
        }
        if (typeof(Transform).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"Transform type '{fragment.ComponentType}' must be "
                    + "declared by the object fragment, not added as a "
                    + "component."
            );
        }

        var component = target.AddComponent(type);
        foreach (var value in fragment.Values ?? new())
        {
            if (
                value == null
                || string.IsNullOrWhiteSpace(value.PropertyPath)
            )
                continue;
            SetValue(component, value.PropertyPath, value.Value);
        }
        QueueComponentReferences(
            component,
            fragment,
            pendingReferences,
            operationId
        );
        RegisterPatchComponent(
            patchId,
            fragment,
            component,
            patchComponents
        );
        return component;
    }

    private static void QueueComponentReferences(
        Component component,
        PrefabPatchComponentFragment fragment,
        ICollection<PendingReference> pendingReferences,
        string operationId
    )
    {
        foreach (var reference in fragment.References ?? new())
        {
            if (
                reference == null
                || string.IsNullOrWhiteSpace(reference.PropertyPath)
            )
                continue;
            pendingReferences.Add(
                new PendingReference
                {
                    Target = component,
                    PropertyPath = reference.PropertyPath,
                    Reference = reference.Reference,
                    OperationId = operationId
                }
            );
        }
    }

    private static void RegisterPatchComponent(
        string patchId,
        PrefabPatchComponentFragment fragment,
        Component component,
        IDictionary<string, Component> patchComponents
    )
    {
        if (string.IsNullOrWhiteSpace(fragment.ComponentId))
            return;
        var key = $"{patchId}:{fragment.ComponentId}";
        if (patchComponents.ContainsKey(key))
            throw new InvalidOperationException(
                $"Patch-owned component '{key}' already exists."
            );
        patchComponents.Add(key, component);
    }

    private static Object ResolveReference(
        GameObject root,
        PrefabPatchObjectReference reference,
        IReadOnlyDictionary<string, Object> references,
        IDictionary<string, GameObject> patchOwned,
        IDictionary<string, Component> patchComponents
    )
    {
        if (reference == null)
            return null;
        if (
            reference.Kind == PrefabPatchObjectReferenceKind.Target
            || reference.Target != null
        )
        {
            return Resolve(
                root,
                reference.Target,
                patchOwned,
                patchComponents
            );
        }

        return !string.IsNullOrWhiteSpace(reference.Address)
            && references.TryGetValue(reference.Address, out var value)
                ? value
                : null;
    }

    private static string DescribeReference(
        PrefabPatchObjectReference reference
    ) =>
        reference == null
            ? "<null>"
            : reference.Kind == PrefabPatchObjectReferenceKind.Target
                || reference.Target != null
                ? reference.Target?.CanonicalKey ?? "<missing-target>"
                : reference.Address ?? "<missing-address>";

    private static void SetValue(
        Object target,
        string propertyPath,
        PrefabPatchValue value
    )
    {
        if (value == null)
            throw new InvalidOperationException(
                $"Property '{propertyPath}' has no typed value."
            );
        if (value.Kind == PrefabPatchValueKind.ArraySize)
        {
            SetCollectionSize(
                target,
                propertyPath,
                checked((int)value.Integer)
            );
            return;
        }
        SetRawValue(target, propertyPath, ConvertValue(value));
    }

    private static void SetRawValue(
        Object target,
        string propertyPath,
        object value
    )
    {
        if (target is Transform transform)
        {
            if (TrySetTransform(transform, propertyPath, value))
                return;
        }

        if (
            target is GameObject gameObject
            && (
                propertyPath == "m_IsActive"
                || propertyPath == "activeSelf"
            )
        )
        {
            gameObject.SetActive(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
            return;
        }

        SetMemberPath(target, propertyPath, value);
    }

    private static bool TrySetTransform(
        Transform transform,
        string propertyPath,
        object value
    )
    {
        var segments = propertyPath.Split('.');
        if (segments.Length != 2)
            return false;
        var component = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        if (
            transform is RectTransform rectTransform
            && TrySetRectTransform(
                rectTransform,
                segments[0],
                segments[1],
                component
            )
        )
        {
            return true;
        }

        switch (segments[0])
        {
            case "m_LocalPosition":
            case "localPosition":
            {
                var vector = transform.localPosition;
                SetVectorComponent(ref vector, segments[1], component);
                transform.localPosition = vector;
                return true;
            }
            case "m_LocalScale":
            case "localScale":
            {
                var vector = transform.localScale;
                SetVectorComponent(ref vector, segments[1], component);
                transform.localScale = vector;
                return true;
            }
            case "m_LocalRotation":
            case "localRotation":
            {
                var quaternion = transform.localRotation;
                SetQuaternionComponent(
                    ref quaternion,
                    segments[1],
                    component
                );
                transform.localRotation = quaternion;
                return true;
            }
            default:
                return false;
        }
    }

    private static bool TrySetRectTransform(
        RectTransform transform,
        string property,
        string componentName,
        float componentValue
    )
    {
        switch (property)
        {
            case "m_AnchoredPosition":
            case "anchoredPosition":
            {
                var value = transform.anchoredPosition;
                SetVector2Component(
                    ref value,
                    componentName,
                    componentValue
                );
                transform.anchoredPosition = value;
                return true;
            }
            case "m_SizeDelta":
            case "sizeDelta":
            {
                var value = transform.sizeDelta;
                SetVector2Component(
                    ref value,
                    componentName,
                    componentValue
                );
                transform.sizeDelta = value;
                return true;
            }
            case "m_AnchorMin":
            case "anchorMin":
            {
                var value = transform.anchorMin;
                SetVector2Component(
                    ref value,
                    componentName,
                    componentValue
                );
                transform.anchorMin = value;
                return true;
            }
            case "m_AnchorMax":
            case "anchorMax":
            {
                var value = transform.anchorMax;
                SetVector2Component(
                    ref value,
                    componentName,
                    componentValue
                );
                transform.anchorMax = value;
                return true;
            }
            case "m_Pivot":
            case "pivot":
            {
                var value = transform.pivot;
                SetVector2Component(
                    ref value,
                    componentName,
                    componentValue
                );
                transform.pivot = value;
                return true;
            }
            default:
                return false;
        }
    }

    private static void SetVector2Component(
        ref Vector2 vector,
        string component,
        float value
    )
    {
        switch (component)
        {
            case "x":
                vector.x = value;
                return;
            case "y":
                vector.y = value;
                return;
            default:
                throw new InvalidOperationException(
                    $"Unknown Vector2 component '{component}'."
                );
        }
    }

    private static void SetMemberPath(
        object root,
        string path,
        object value
    )
    {
        var segments = ParsePath(path);
        SetMemberRecursive(root, segments, 0, value);
    }

    private static void SetCollectionSize(
        object root,
        string propertyPath,
        int size
    )
    {
        const string suffix = ".Array.size";
        if (
            string.IsNullOrWhiteSpace(propertyPath)
            || !propertyPath.EndsWith(suffix, StringComparison.Ordinal)
        )
        {
            throw new InvalidOperationException(
                $"Collection-size path '{propertyPath}' does not end in "
                    + $"'{suffix}'."
            );
        }
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        var collectionPath = propertyPath.Substring(
            0,
            propertyPath.Length - suffix.Length
        );
        ResizeCollectionRecursive(
            root,
            ParsePath(collectionPath),
            0,
            size
        );
    }

    private static object ResizeCollectionRecursive(
        object current,
        IReadOnlyList<MemberPathSegment> segments,
        int index,
        int size
    )
    {
        if (current == null)
            throw new InvalidOperationException(
                "Cannot resize a collection through a null serialized value."
            );
        var segment = segments[index];
        var member = FindMember(current.GetType(), segment.Name)
            ?? throw new MissingMemberException(
                current.GetType().FullName,
                segment.Name
            );
        var memberType = GetMemberType(member);
        var memberValue = GetMemberValue(member, current);
        if (segment.HasIndex)
        {
            if (memberValue is not IList list)
                throw new InvalidOperationException(
                    $"Member '{segment.Name}' is not an indexed collection."
                );
            var element = list[segment.Index];
            var updated = ResizeCollectionRecursive(
                element,
                segments,
                index + 1,
                size
            );
            var elementType = GetCollectionElementType(memberType);
            if (elementType.IsValueType)
                list[segment.Index] = updated;
            return current;
        }

        if (index < segments.Count - 1)
        {
            var updated = ResizeCollectionRecursive(
                memberValue,
                segments,
                index + 1,
                size
            );
            if (memberType.IsValueType)
                SetMemberValue(member, current, updated);
            return current;
        }

        if (memberType.IsArray)
        {
            var elementType =
                memberType.GetElementType() ?? typeof(object);
            var previous = memberValue as Array;
            var replacement = Array.CreateInstance(elementType, size);
            if (previous != null)
            {
                Array.Copy(
                    previous,
                    replacement,
                    Math.Min(previous.Length, size)
                );
            }
            for (
                var itemIndex = previous?.Length ?? 0;
                itemIndex < size;
                itemIndex++
            )
            {
                replacement.SetValue(
                    CreateCollectionElement(elementType),
                    itemIndex
                );
            }
            SetMemberValue(member, current, replacement);
            return current;
        }

        if (memberValue is not IList mutableList)
        {
            if (
                memberType.IsInterface
                || memberType.IsAbstract
            )
            {
                throw new InvalidOperationException(
                    $"Collection member '{segment.Name}' of type "
                        + $"'{memberType.FullName}' is null and cannot be "
                        + "constructed."
                );
            }
            mutableList = (IList)Activator.CreateInstance(memberType);
            SetMemberValue(member, current, mutableList);
        }
        var itemType = GetCollectionElementType(memberType);
        while (mutableList.Count > size)
            mutableList.RemoveAt(mutableList.Count - 1);
        while (mutableList.Count < size)
            mutableList.Add(CreateCollectionElement(itemType));
        return current;
    }

    private static object CreateCollectionElement(Type itemType)
    {
        if (
            itemType == typeof(string)
            || itemType.IsAbstract
            || itemType.IsInterface
        )
            return null;
        try
        {
            return Activator.CreateInstance(itemType);
        }
        catch (MissingMethodException)
        {
            return System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(itemType);
        }
    }

    private readonly struct MemberPathSegment
    {
        public readonly string Name;
        public readonly int Index;
        public readonly bool HasIndex;

        public MemberPathSegment(string name, int index, bool hasIndex)
        {
            Name = name;
            Index = index;
            HasIndex = hasIndex;
        }

        public override string ToString() =>
            HasIndex ? $"{Name}[{Index}]" : Name;
    }

    private static IReadOnlyList<MemberPathSegment> ParsePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(
                "A serialized property path is required.",
                nameof(path)
            );
        var normalized = path.Replace(".Array.data[", "[");
        var result = new List<MemberPathSegment>();
        foreach (var raw in normalized.Split('.'))
        {
            var bracket = raw.LastIndexOf('[');
            if (
                bracket > 0
                && raw.EndsWith("]", StringComparison.Ordinal)
                && int.TryParse(
                    raw.Substring(bracket + 1, raw.Length - bracket - 2),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var index
                )
            )
            {
                result.Add(
                    new MemberPathSegment(
                        raw.Substring(0, bracket),
                        index,
                        true
                    )
                );
            }
            else
            {
                result.Add(new MemberPathSegment(raw, 0, false));
            }
        }

        return result;
    }

    private static object SetMemberRecursive(
        object current,
        IReadOnlyList<MemberPathSegment> segments,
        int index,
        object value
    )
    {
        if (current == null)
            throw new InvalidOperationException(
                $"Cannot traverse null while setting "
                    + $"'{string.Join(".", segments)}'."
            );
        var segment = segments[index];
        var member = FindMember(current.GetType(), segment.Name);
        if (member == null)
            throw new MissingMemberException(
                current.GetType().FullName,
                segment.Name
            );
        var memberType = GetMemberType(member);
        var memberValue = GetMemberValue(member, current);
        if (segment.HasIndex)
        {
            if (memberValue is not IList list)
            {
                throw new InvalidOperationException(
                    $"Member '{segment.Name}' on "
                        + $"'{current.GetType().FullName}' is not an indexed "
                        + "serialized collection."
                );
            }
            if (segment.Index < 0 || segment.Index >= list.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Serialized collection '{segment.Name}' has "
                        + $"{list.Count} item(s), but index {segment.Index} "
                        + "was requested."
                );
            }

            var elementType = GetCollectionElementType(memberType);
            if (index == segments.Count - 1)
            {
                list[segment.Index] = ConvertForType(value, elementType);
                return current;
            }

            var element = list[segment.Index];
            var updatedElement = SetMemberRecursive(
                element,
                segments,
                index + 1,
                value
            );
            if (elementType.IsValueType)
                list[segment.Index] = updatedElement;
            return current;
        }

        if (index == segments.Count - 1)
        {
            var converted = ConvertForType(value, memberType);
            SetMemberValue(member, current, converted);
            return current;
        }

        var updatedChild = SetMemberRecursive(
            memberValue,
            segments,
            index + 1,
            value
        );
        if (memberType.IsValueType)
            SetMemberValue(member, current, updatedChild);
        return current;
    }

    private static Type GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType() ?? typeof(object);
        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments()[0];
        return typeof(object);
    }

    private static MemberInfo FindMember(Type type, string name)
    {
        const BindingFlags flags =
            BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;
        for (var current = type; current != null; current = current.BaseType)
        {
            var field = current.GetField(name, flags);
            if (field != null)
                return field;
            var property = current.GetProperty(name, flags);
            if (property != null && property.CanRead && property.CanWrite)
                return property;
        }

        if (
            name.StartsWith("m_", StringComparison.Ordinal)
            && name.Length > 2
        )
        {
            var serializedName =
                char.ToLowerInvariant(name[2]) + name.Substring(3);
            var property = type.GetProperty(serializedName, flags);
            if (property != null && property.CanRead && property.CanWrite)
                return property;

            var alias = name switch
            {
                "m_Mesh" => "sharedMesh",
                "m_Material" => "sharedMaterial",
                "m_Materials" => "sharedMaterials",
                _ => null
            };
            if (alias != null)
            {
                property = type.GetProperty(alias, flags);
                if (
                    property != null
                    && property.CanRead
                    && property.CanWrite
                )
                    return property;
            }
        }

        return null;
    }

    private static Type GetMemberType(MemberInfo member) =>
        member is FieldInfo field
            ? field.FieldType
            : ((PropertyInfo)member).PropertyType;

    private static object GetMemberValue(MemberInfo member, object target) =>
        member is FieldInfo field
            ? field.GetValue(target)
            : ((PropertyInfo)member).GetValue(target);

    private static void SetMemberValue(
        MemberInfo member,
        object target,
        object value
    )
    {
        if (member is FieldInfo field)
            field.SetValue(target, value);
        else
            ((PropertyInfo)member).SetValue(target, value);
    }

    private static object ConvertForType(object value, Type targetType)
    {
        if (value is PrefabPatchValue patchValue)
        {
            if (
                patchValue.Kind
                == PrefabPatchValueKind.ManagedReference
            )
            {
                var concreteType = ResolveType(
                    patchValue.SerializedType
                );
                if (
                    concreteType == null
                    || !targetType.IsAssignableFrom(concreteType)
                )
                {
                    throw new InvalidOperationException(
                        $"Managed-reference type "
                            + $"'{patchValue.SerializedType}' is not "
                            + $"assignable to '{targetType.FullName}'."
                    );
                }
                return CreateCollectionElement(concreteType);
            }
            if (patchValue.Kind != PrefabPatchValueKind.Json)
                value = ConvertValue(patchValue);
            else
                return DeserializeJsonValue(patchValue, targetType);
        }
        if (value == null || targetType.IsInstanceOfType(value))
            return value;
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, value);
        return Convert.ChangeType(
            value,
            targetType,
            CultureInfo.InvariantCulture
        );
    }

    private static object DeserializeJsonValue(
        PrefabPatchValue value,
        Type targetType
    )
    {
        if (targetType == typeof(AnimationCurve))
        {
            var payload = JsonConvert.DeserializeObject<
                AnimationCurvePayload
            >(value.String, PrefabPatchJson.Settings);
            var curve = new AnimationCurve(
                payload?.Keys ?? Array.Empty<Keyframe>()
            )
            {
                preWrapMode =
                    (WrapMode)(payload?.PreWrapMode ?? (int)WrapMode.Default),
                postWrapMode =
                    (WrapMode)(payload?.PostWrapMode ?? (int)WrapMode.Default)
            };
            return curve;
        }
        if (targetType == typeof(Gradient))
        {
            var payload = JsonConvert.DeserializeObject<GradientPayload>(
                value.String,
                PrefabPatchJson.Settings
            );
            var gradient = new Gradient
            {
                mode = (GradientMode)(
                    payload?.Mode ?? (int)GradientMode.Blend
                )
            };
            gradient.SetKeys(
                payload?.ColorKeys ?? Array.Empty<GradientColorKey>(),
                payload?.AlphaKeys ?? Array.Empty<GradientAlphaKey>()
            );
            return gradient;
        }
        if (targetType == typeof(Hash128))
            return Hash128.Parse(
                JsonConvert.DeserializeObject<string>(value.String)
            );
        return JsonConvert.DeserializeObject(
            value.String ?? "null",
            targetType,
            PrefabPatchJson.Settings
        );
    }

    private static object ConvertValue(PrefabPatchValue value) =>
        value.Kind switch
        {
            PrefabPatchValueKind.Boolean => value.Boolean,
            PrefabPatchValueKind.Integer => value.Integer,
            PrefabPatchValueKind.Float => value.Float,
            PrefabPatchValueKind.String => value.String,
            PrefabPatchValueKind.Vector2 =>
                new Vector2((float)value.X, (float)value.Y),
            PrefabPatchValueKind.Vector3 =>
                new Vector3((float)value.X, (float)value.Y, (float)value.Z),
            PrefabPatchValueKind.Vector4 =>
                new Vector4(
                    (float)value.X,
                    (float)value.Y,
                    (float)value.Z,
                    (float)value.W
                ),
            PrefabPatchValueKind.Quaternion =>
                new Quaternion(
                    (float)value.X,
                    (float)value.Y,
                    (float)value.Z,
                    (float)value.W
                ),
            PrefabPatchValueKind.Color =>
                new Color(
                    (float)value.X,
                    (float)value.Y,
                    (float)value.Z,
                    (float)value.W
                ),
            PrefabPatchValueKind.Json => value,
            _ => throw new NotSupportedException(
                $"Typed value kind '{value.Kind}' is unsupported."
            )
        };

    private static Vector3 ToVector3(
        PrefabPatchValue value,
        Vector3 fallback
    ) =>
        value == null
            ? fallback
            : new Vector3((float)value.X, (float)value.Y, (float)value.Z);

    private static Vector2 ToVector2(
        PrefabPatchValue value,
        Vector2 fallback
    ) =>
        value == null
            ? fallback
            : new Vector2((float)value.X, (float)value.Y);

    private static Quaternion ToQuaternion(
        PrefabPatchValue value,
        Quaternion fallback
    ) =>
        value == null
            ? fallback
            : new Quaternion(
                (float)value.X,
                (float)value.Y,
                (float)value.Z,
                (float)value.W
            );

    private static void SetVectorComponent(
        ref Vector3 value,
        string component,
        float replacement
    )
    {
        switch (component)
        {
            case "x":
                value.x = replacement;
                break;
            case "y":
                value.y = replacement;
                break;
            case "z":
                value.z = replacement;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(component));
        }
    }

    private static void SetQuaternionComponent(
        ref Quaternion value,
        string component,
        float replacement
    )
    {
        switch (component)
        {
            case "x":
                value.x = replacement;
                break;
            case "y":
                value.y = replacement;
                break;
            case "z":
                value.z = replacement;
                break;
            case "w":
                value.w = replacement;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(component));
        }
    }

    private static GameObject AsGameObject(Object target) =>
        target switch
        {
            GameObject gameObject => gameObject,
            Component component => component.gameObject,
            _ => null
        };

    private static Type ResolveType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        var type = Type.GetType(name, false);
        if (type != null)
            return type;
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(name, false))
            .FirstOrDefault(value => value != null);
    }

    private static Exception MissingTarget(PrefabPatchOperation operation) =>
        new InvalidOperationException(
            $"Operation '{operation.OperationId}' from '{operation.PatchId}' "
                + $"could not resolve target '{operation.Target?.CanonicalKey}'."
        );
}
