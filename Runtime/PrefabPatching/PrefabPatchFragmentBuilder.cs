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

    /// <summary>
    /// Creates a builder for one patch-owned component.
    /// </summary>
    /// <param name="componentId">Stable ID used by later patch operations.</param>
    /// <param name="componentType">Concrete Unity component type to create.</param>
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

    /// <summary>
    /// Creates a component builder for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="componentId">Stable ID used by later patch operations.</param>
    /// <typeparam name="T">Concrete Unity component type to create.</typeparam>
    /// <returns>A builder for the requested component type.</returns>
    public static PrefabPatchComponentBuilder For<T>(string componentId)
        where T : Component =>
        new(componentId, typeof(T));

    /// <summary>
    /// Adds a serialized value assignment to the component fragment.
    /// </summary>
    /// <param name="propertyPath">Unity serialized-property path.</param>
    /// <param name="value">Value to assign.</param>
    /// <returns>This builder.</returns>
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

    /// <summary>
    /// Adds a serialized object-reference assignment to the component fragment.
    /// </summary>
    /// <param name="propertyPath">Unity serialized-property path.</param>
    /// <param name="reference">Reference to assign.</param>
    /// <returns>This builder.</returns>
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

    /// <summary>
    /// Returns the component fragment represented by this builder.
    /// </summary>
    /// <returns>The mutable component fragment.</returns>
    public PrefabPatchComponentFragment Build() => _fragment;
}

/// <summary>
/// Fluent C# frontend for an inline patch-owned hierarchy.
/// </summary>
public sealed class PrefabPatchObjectBuilder
{
    private readonly PrefabPatchObjectFragment _fragment;

    /// <summary>
    /// Creates a builder for one patch-owned GameObject.
    /// </summary>
    /// <param name="objectId">Stable ID used by later patch operations.</param>
    /// <param name="name">Optional GameObject name; defaults to the object ID.</param>
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

    /// <summary>
    /// Uses a <see cref="RectTransform"/> for the object.
    /// </summary>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder RectTransform()
    {
        _fragment.TransformType = typeof(RectTransform).AssemblyQualifiedName;
        return this;
    }

    /// <summary>
    /// Sets whether the object is active by default.
    /// </summary>
    /// <param name="value">Default active state.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Active(bool value)
    {
        _fragment.Active = value;
        return this;
    }

    /// <summary>
    /// Sets the GameObject layer.
    /// </summary>
    /// <param name="value">Unity layer index.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Layer(int value)
    {
        _fragment.Layer = value;
        return this;
    }

    /// <summary>
    /// Sets the GameObject tag.
    /// </summary>
    /// <param name="value">Unity tag name.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Tag(string value)
    {
        _fragment.Tag = value;
        return this;
    }

    /// <summary>
    /// Sets the GameObject static flag.
    /// </summary>
    /// <param name="value">Whether the object is static.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Static(bool value = true)
    {
        _fragment.IsStatic = value;
        return this;
    }

    /// <summary>
    /// Sets the local transform values for the object.
    /// </summary>
    /// <param name="localPosition">Local position.</param>
    /// <param name="localRotation">Local rotation.</param>
    /// <param name="localScale">Local scale.</param>
    /// <returns>This builder.</returns>
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

    /// <summary>
    /// Configures the object as a UI rectangle.
    /// </summary>
    /// <param name="anchorMin">Minimum normalized anchor.</param>
    /// <param name="anchorMax">Maximum normalized anchor.</param>
    /// <param name="anchoredPosition">Position relative to the anchors.</param>
    /// <param name="sizeDelta">Size relative to the anchors.</param>
    /// <param name="pivot">Normalized pivot.</param>
    /// <returns>This builder.</returns>
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

    /// <summary>
    /// Adds a component fragment to the object.
    /// </summary>
    /// <param name="component">Component fragment to add.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Component(
        PrefabPatchComponentFragment component
    )
    {
        _fragment.Components.Add(
            component ?? throw new ArgumentNullException(nameof(component))
        );
        return this;
    }

    /// <summary>
    /// Adds a child object fragment.
    /// </summary>
    /// <param name="child">Child fragment to add.</param>
    /// <returns>This builder.</returns>
    public PrefabPatchObjectBuilder Child(
        PrefabPatchObjectFragment child
    )
    {
        _fragment.Children.Add(
            child ?? throw new ArgumentNullException(nameof(child))
        );
        return this;
    }

    /// <summary>
    /// Returns the object fragment represented by this builder.
    /// </summary>
    /// <returns>The mutable object fragment.</returns>
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
