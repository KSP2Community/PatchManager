using System;
using UnityEngine;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Fluent, type-agnostic C# frontend for one serialized component fragment.
/// It produces the same public model as visual and Lua authoring.
/// </summary>
public sealed class PrefabPatchComponentBuilder
{
    private readonly PrefabPatchComponentFragment _fragment;

    public PrefabPatchComponentBuilder(string componentId, Type componentType)
    {
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException(
                "A stable component ID is required.",
                nameof(componentId)
            );
        if (
            componentType == null
            || componentType.IsAbstract
            || !typeof(Component).IsAssignableFrom(componentType)
            || typeof(Transform).IsAssignableFrom(componentType)
        )
        {
            throw new ArgumentException(
                "The component type must be a concrete, non-Transform Unity "
                    + "Component.",
                nameof(componentType)
            );
        }

        _fragment = new PrefabPatchComponentFragment
        {
            ComponentId = componentId,
            ComponentType = componentType.AssemblyQualifiedName
        };
    }

    public static PrefabPatchComponentBuilder For<T>(string componentId)
        where T : Component =>
        new(componentId, typeof(T));

    public PrefabPatchComponentBuilder Value(
        string propertyPath,
        PrefabPatchValue value
    )
    {
        _fragment.Values.Add(
            new PrefabPatchSerializedValue
            {
                PropertyPath = propertyPath,
                Value = value
            }
        );
        return this;
    }

    public PrefabPatchComponentBuilder Reference(
        string propertyPath,
        PrefabPatchObjectReference reference
    )
    {
        _fragment.References.Add(
            new PrefabPatchSerializedReference
            {
                PropertyPath = propertyPath,
                Reference = reference
            }
        );
        return this;
    }

    public PrefabPatchComponentFragment Build() => _fragment;
}

/// <summary>
/// Fluent C# frontend for an inline patch-owned hierarchy.
/// </summary>
public sealed class PrefabPatchObjectBuilder
{
    private readonly PrefabPatchObjectFragment _fragment;

    public PrefabPatchObjectBuilder(string objectId, string name = null)
    {
        if (string.IsNullOrWhiteSpace(objectId))
            throw new ArgumentException(
                "A stable object ID is required.",
                nameof(objectId)
            );
        _fragment = new PrefabPatchObjectFragment
        {
            ObjectId = objectId,
            Name = name ?? objectId,
            TransformType = typeof(Transform).AssemblyQualifiedName,
            LocalPosition = Vector3Value(Vector3.zero),
            LocalRotation = QuaternionValue(Quaternion.identity),
            LocalScale = Vector3Value(Vector3.one)
        };
    }

    public PrefabPatchObjectBuilder RectTransform()
    {
        _fragment.TransformType = typeof(RectTransform).AssemblyQualifiedName;
        return this;
    }

    public PrefabPatchObjectBuilder Active(bool value)
    {
        _fragment.Active = value;
        return this;
    }

    public PrefabPatchObjectBuilder Layer(int value)
    {
        _fragment.Layer = value;
        return this;
    }

    public PrefabPatchObjectBuilder Tag(string value)
    {
        _fragment.Tag = value;
        return this;
    }

    public PrefabPatchObjectBuilder Static(bool value = true)
    {
        _fragment.IsStatic = value;
        return this;
    }

    public PrefabPatchObjectBuilder Transform(
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale
    )
    {
        _fragment.LocalPosition = Vector3Value(localPosition);
        _fragment.LocalRotation = QuaternionValue(localRotation);
        _fragment.LocalScale = Vector3Value(localScale);
        return this;
    }

    public PrefabPatchObjectBuilder Rect(
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2 pivot
    )
    {
        RectTransform();
        _fragment.AnchorMin = Vector2Value(anchorMin);
        _fragment.AnchorMax = Vector2Value(anchorMax);
        _fragment.AnchoredPosition = Vector2Value(anchoredPosition);
        _fragment.SizeDelta = Vector2Value(sizeDelta);
        _fragment.Pivot = Vector2Value(pivot);
        return this;
    }

    public PrefabPatchObjectBuilder Component(
        PrefabPatchComponentFragment component
    )
    {
        _fragment.Components.Add(
            component ?? throw new ArgumentNullException(nameof(component))
        );
        return this;
    }

    public PrefabPatchObjectBuilder Child(
        PrefabPatchObjectFragment child
    )
    {
        _fragment.Children.Add(
            child ?? throw new ArgumentNullException(nameof(child))
        );
        return this;
    }

    public PrefabPatchObjectFragment Build() => _fragment;

    private static PrefabPatchValue Vector2Value(Vector2 value) =>
        new()
        {
            Kind = PrefabPatchValueKind.Vector2,
            X = value.x,
            Y = value.y
        };

    private static PrefabPatchValue Vector3Value(Vector3 value) =>
        new()
        {
            Kind = PrefabPatchValueKind.Vector3,
            X = value.x,
            Y = value.y,
            Z = value.z
        };

    private static PrefabPatchValue QuaternionValue(Quaternion value) =>
        new()
        {
            Kind = PrefabPatchValueKind.Quaternion,
            X = value.x,
            Y = value.y,
            Z = value.z,
            W = value.w
        };
}
