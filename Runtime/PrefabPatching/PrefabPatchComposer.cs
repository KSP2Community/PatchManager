using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Applies a validated resolved plan once to a loaded external prefab asset.
/// The caller owns and retains the Addressables handles.
/// </summary>
public static class PrefabPatchComposer
{
    public sealed class Result
    {
        public bool Success;
        public string Failure;
        public long ElapsedMilliseconds;
        public int AppliedOperationCount;
        public Dictionary<string, GameObject> PatchOwnedObjects = new(
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
                !string.Equals(
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
                        + $"'{actualFingerprint}'. Recompile or repair the patch."
                );
            }

            var unusedDeferredDestroy = false;
            foreach (var operation in plan.Operations)
            {
                ApplyOne(
                    prefab,
                    operation,
                    references,
                    result.PatchOwnedObjects,
                    ref unusedDeferredDestroy,
                    true
                );
                result.AppliedOperationCount++;
            }

            result.Success = true;
        }
        catch (Exception exception)
        {
            result.Failure = exception.ToString();
        }
        finally
        {
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
        ref bool deferredDestroy,
        bool immediateDestroy
    )
    {
        if (operation.Kind == PrefabPatchOperationKind.AddObject)
        {
            var parentObject = operation.Target == null
                ? root
                : AsGameObject(Resolve(root, operation.Target, patchOwned));
            if (parentObject == null)
                throw MissingTarget(operation);
            CreateFragment(
                operation.PatchId,
                operation.AddedObject,
                parentObject.transform,
                references,
                patchOwned
            );
            return;
        }

        var target = Resolve(root, operation.Target, patchOwned);
        if (target == null)
            throw MissingTarget(operation);
        switch (operation.Kind)
        {
            case PrefabPatchOperationKind.SetValue:
                SetValue(target, operation.PropertyPath, operation.Value);
                return;
            case PrefabPatchOperationKind.SetObjectReference:
                if (
                    operation.ObjectReference == null
                    || !references.TryGetValue(
                        operation.ObjectReference.Address,
                        out var reference
                    )
                    || reference == null
                )
                {
                    throw new InvalidOperationException(
                        $"Operation '{operation.OperationId}' could not resolve "
                            + $"object reference "
                            + $"'{operation.ObjectReference?.Address}'."
                    );
                }

                SetRawValue(target, operation.PropertyPath, reference);
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
                    references
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
                    Object.DestroyImmediate(component);
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
        IDictionary<string, GameObject> patchOwned
    )
    {
        Transform transform;
        if (target.Kind == PrefabPatchTargetKind.Stock)
        {
            transform = PrefabPatchStructure.Resolve(
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
        IDictionary<string, GameObject> patchOwned
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

        var gameObject = new GameObject(fragment.Name ?? fragment.ObjectId);
        gameObject.transform.SetParent(parent, false);
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
        gameObject.SetActive(fragment.Active);
        gameObject.AddComponent<PrefabPatchObjectId>().Id = fragment.ObjectId;
        patchOwned.Add(key, gameObject);
        foreach (var component in fragment.Components)
            AddComponent(gameObject, component, references);
        foreach (var child in fragment.Children)
            CreateFragment(patchId, child, gameObject.transform, references, patchOwned);
        return gameObject;
    }

    private static Component AddComponent(
        GameObject target,
        PrefabPatchComponentFragment fragment,
        IReadOnlyDictionary<string, Object> references
    )
    {
        if (target == null || fragment == null)
            throw new InvalidOperationException(
                "An added component has no target or payload."
            );
        switch (fragment.Kind)
        {
            case PrefabPatchComponentKind.BoxCollider:
            {
                var value = target.AddComponent<BoxCollider>();
                value.enabled = fragment.Enabled;
                value.center = ToVector3(fragment.Center, Vector3.zero);
                value.size = ToVector3(fragment.Size, Vector3.one);
                value.isTrigger = fragment.IsTrigger;
                return value;
            }
            case PrefabPatchComponentKind.SphereCollider:
            {
                var value = target.AddComponent<SphereCollider>();
                value.enabled = fragment.Enabled;
                value.center = ToVector3(fragment.Center, Vector3.zero);
                value.radius = (float)fragment.Radius;
                value.isTrigger = fragment.IsTrigger;
                return value;
            }
            case PrefabPatchComponentKind.MeshFilter:
            {
                var value = target.AddComponent<MeshFilter>();
                if (fragment.Mesh != null)
                {
                    if (
                        !references.TryGetValue(
                            fragment.Mesh.Address,
                            out var reference
                        )
                        || reference is not Mesh mesh
                    )
                    {
                        throw new InvalidOperationException(
                            $"Could not resolve Mesh reference "
                                + $"'{fragment.Mesh.Address}'."
                        );
                    }

                    value.sharedMesh = mesh;
                }

                return value;
            }
            case PrefabPatchComponentKind.MeshRenderer:
                return target.AddComponent<MeshRenderer>();
            default:
                throw new NotSupportedException(
                    $"Added component kind '{fragment.Kind}' is unsupported."
                );
        }
    }

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

    private static void SetMemberPath(
        object root,
        string path,
        object value
    )
    {
        var segments = path.Split('.');
        SetMemberRecursive(root, segments, 0, value);
    }

    private static object SetMemberRecursive(
        object current,
        IReadOnlyList<string> segments,
        int index,
        object value
    )
    {
        if (current == null)
            throw new InvalidOperationException(
                $"Cannot traverse null while setting '{string.Join(".", segments)}'."
            );
        var member = FindMember(current.GetType(), segments[index]);
        if (member == null)
            throw new MissingMemberException(
                current.GetType().FullName,
                segments[index]
            );
        var memberType = GetMemberType(member);
        if (index == segments.Count - 1)
        {
            var converted = ConvertForType(value, memberType);
            SetMemberValue(member, current, converted);
            return current;
        }

        var child = GetMemberValue(member, current);
        var updatedChild = SetMemberRecursive(
            child,
            segments,
            index + 1,
            value
        );
        if (memberType.IsValueType)
            SetMemberValue(member, current, updatedChild);
        return current;
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
