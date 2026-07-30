using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Versioned public schema for declarative prefab patches.
/// </summary>
public static class PrefabPatchSchema
{
    public const int Version = 1;
    public const int ComposerVersion = 1;
    public const string AddressablesLabelSuffix = "_prefab_patches";
}

/// <summary>
/// One active mod's Addressables discovery boundary for declarative prefab
/// patches. The mod descriptor supplies ownership; the manifest asset address
/// is deliberately not part of patch identity.
/// </summary>
public sealed class PrefabPatchManifestSource
{
    public string OwnerModId;
    public string AddressablesLabel;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchPass
{
    Early,
    Default,
    Late
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchOrdering
{
    First,
    Default,
    Last
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchTargetKind
{
    Stock,
    PatchOwned,
    PatchComponent
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchRuntimeTargetKind
{
    GameObject,
    Component
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchOperationKind
{
    SetValue,
    SetObjectReference,
    SetActive,
    AddObject,
    SuppressObject,
    AddComponent,
    RemoveComponent
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchValueKind
{
    Boolean,
    Integer,
    Float,
    String,
    Vector2,
    Vector3,
    Vector4,
    Quaternion,
    Color,
    ArraySize,
    ManagedReference,
    Json
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchObjectReferenceKind
{
    Addressable,
    Target
}

/// <summary>
/// Runtime identity and optional compatibility data for one stock Addressable
/// prefab.
/// </summary>
[Serializable]
public sealed class PrefabPatchPrefabIdentity
{
    public string Address;
    public string AssetType;
    public string StructuralFingerprint;

    [JsonIgnore]
    public string CanonicalKey => $"address:{Address}";

    public static PrefabPatchPrefabIdentity FromAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException(
                "Addressables key is required.",
                nameof(address)
            );
        return new PrefabPatchPrefabIdentity
        {
            Address = address.Trim(),
            AssetType = typeof(UnityEngine.GameObject).AssemblyQualifiedName
        };
    }
}

/// <summary>
/// Runtime traversal data. Visual compilation uses sibling indices backed by
/// canonical source identity; key-first C#/Lua authoring uses a hierarchy path.
/// </summary>
[Serializable]
public sealed class PrefabPatchRuntimeLocator
{
    public int[] SiblingIndices = Array.Empty<int>();
    public string HierarchyPath;
    public PrefabPatchRuntimeTargetKind TargetKind;
    public string ComponentType;
    public int ComponentOrdinal;
    public string DisplayPath;
}

/// <summary>
/// An inherited source object or an object introduced by a named patch.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectTarget
{
    public PrefabPatchTargetKind Kind;
    public string ObjectType;
    public string OwnerPatchId;
    public string ObjectId;
    public string ComponentId;
    public PrefabPatchRuntimeLocator RuntimeLocator;

    [JsonIgnore]
    public string CanonicalKey
    {
        get
        {
            if (Kind == PrefabPatchTargetKind.PatchComponent)
                return $"patch-component:{OwnerPatchId}:{ComponentId}";
            if (Kind == PrefabPatchTargetKind.PatchOwned)
                return $"patch:{OwnerPatchId}:{ObjectId}";

            var path = !string.IsNullOrWhiteSpace(
                RuntimeLocator?.HierarchyPath
            )
                ? RuntimeLocator.HierarchyPath
                : RuntimeLocator?.DisplayPath;
            if (!string.IsNullOrWhiteSpace(path))
                return $"path:{path}:{RuntimeLocator.TargetKind}:"
                    + $"{RuntimeLocator.ComponentType}:"
                    + $"{RuntimeLocator.ComponentOrdinal}";

            var indices = RuntimeLocator?.SiblingIndices == null
                ? ""
                : string.Join(",", RuntimeLocator.SiblingIndices);
            return $"indices:{indices}:{RuntimeLocator?.TargetKind}:"
                + $"{RuntimeLocator?.ComponentType}:"
                + $"{RuntimeLocator?.ComponentOrdinal}";
        }
    }
}

/// <summary>
/// JSON-safe typed value. Numeric vectors use X/Y/Z/W in their normal Unity
/// component order.
/// </summary>
[Serializable]
public sealed class PrefabPatchValue
{
    public PrefabPatchValueKind Kind;
    public bool Boolean;
    public long Integer;
    public double Float;
    public string String;
    public string SerializedType;
    public double X;
    public double Y;
    public double Z;
    public double W;

    public static PrefabPatchValue FromBoolean(bool value) =>
        new() { Kind = PrefabPatchValueKind.Boolean, Boolean = value };

    public static PrefabPatchValue FromInteger(long value) =>
        new() { Kind = PrefabPatchValueKind.Integer, Integer = value };

    public static PrefabPatchValue FromFloat(double value) =>
        new() { Kind = PrefabPatchValueKind.Float, Float = value };

    public static PrefabPatchValue FromString(string value) =>
        new() { Kind = PrefabPatchValueKind.String, String = value };
}

/// <summary>
/// Addressable or target-local Unity object reference used by
/// SetObjectReference or a component fragment.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectReference
{
    public PrefabPatchObjectReferenceKind Kind;
    public string Address;
    public string ExpectedType;
    public PrefabPatchObjectTarget Target;

    public static PrefabPatchObjectReference FromAddress(
        string address,
        string expectedType = null
    ) =>
        new()
        {
            Kind = PrefabPatchObjectReferenceKind.Addressable,
            Address = address,
            ExpectedType = expectedType
        };

    public static PrefabPatchObjectReference FromTarget(
        PrefabPatchObjectTarget target,
        string expectedType = null
    ) =>
        new()
        {
            Kind = PrefabPatchObjectReferenceKind.Target,
            Target = target,
            ExpectedType = expectedType
        };
}

/// <summary>
/// One serialized field/property value captured from an arbitrary component.
/// Visual, C#, and Lua authoring all emit this property-stream representation.
/// </summary>
[Serializable]
public sealed class PrefabPatchSerializedValue
{
    public string PropertyPath;
    public PrefabPatchValue Value;
}

/// <summary>
/// One Unity object reference removed from a serialized component payload and
/// restored after every patch-owned object and component has been created.
/// </summary>
[Serializable]
public sealed class PrefabPatchSerializedReference
{
    public string PropertyPath;
    public PrefabPatchObjectReference Reference;
}

/// <summary>
/// Serialized payload used for added patch-owned objects and AddComponent
/// operations.
/// </summary>
[Serializable]
public sealed class PrefabPatchComponentFragment
{
    public string ComponentId;
    public string ComponentType;
    public List<PrefabPatchSerializedValue> Values = new();
    public List<PrefabPatchSerializedReference> References = new();
}

/// <summary>
/// An inline, patch-owned hierarchy fragment. Every object has an explicit ID,
/// allowing later required patches to target it without hierarchy-name lookup.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectFragment
{
    public string ObjectId;
    public string Name;
    public string TransformType;
    public bool Active = true;
    public int Layer;
    public string Tag = "Untagged";
    public bool IsStatic;
    public PrefabPatchValue LocalPosition;
    public PrefabPatchValue LocalRotation;
    public PrefabPatchValue LocalScale;
    public PrefabPatchValue AnchorMin;
    public PrefabPatchValue AnchorMax;
    public PrefabPatchValue AnchoredPosition;
    public PrefabPatchValue SizeDelta;
    public PrefabPatchValue Pivot;
    public List<PrefabPatchComponentFragment> Components = new();
    public List<PrefabPatchObjectFragment> Children = new();
}

/// <summary>
/// One normalized declarative operation.
/// </summary>
[Serializable]
public sealed class PrefabPatchOperation
{
    public string OperationId;
    [JsonIgnore]
    public string PatchId;
    public PrefabPatchOperationKind Kind;
    public PrefabPatchObjectTarget Target;
    public string PropertyPath;
    public PrefabPatchValue Value;
    public PrefabPatchObjectReference ObjectReference;
    public PrefabPatchObjectFragment AddedObject;
    public PrefabPatchComponentFragment AddedComponent;
    public string ExpectedOriginalFingerprint;
    public string AuthoringAssetPath;
    public string AuthoringPropertyPath;

    [JsonIgnore]
    public string ConflictKey
    {
        get
        {
            var target = Target?.CanonicalKey ?? "<missing-target>";
            return Kind switch
            {
                PrefabPatchOperationKind.SetValue =>
                    $"{target}:value:{PropertyPath}",
                PrefabPatchOperationKind.SetObjectReference =>
                    $"{target}:object:{PropertyPath}",
                PrefabPatchOperationKind.SetActive =>
                    $"{target}:active",
                PrefabPatchOperationKind.SuppressObject =>
                    $"{target}:suppressed",
                PrefabPatchOperationKind.RemoveComponent =>
                    $"{target}:removed",
                PrefabPatchOperationKind.AddObject =>
                    $"patch:{PatchId}:{AddedObject?.ObjectId}:introduced",
                PrefabPatchOperationKind.AddComponent =>
                    $"{target}:component:{AddedComponent?.ComponentId}",
                _ => $"{target}:{Kind}:{OperationId}"
            };
        }
    }
}

/// <summary>
/// One independently distributable prefab patch manifest.
/// </summary>
[Serializable]
public sealed class PrefabPatchManifest
{
    public int SchemaVersion = PrefabPatchSchema.Version;
    public int ComposerVersion = PrefabPatchSchema.ComposerVersion;
    public string PatchName;
    [JsonIgnore]
    public string PatchId;
    [JsonIgnore]
    public string ModId;
    public PrefabPatchPrefabIdentity TargetPrefab;
    public PrefabPatchPass Pass = PrefabPatchPass.Default;
    public PrefabPatchOrdering Ordering = PrefabPatchOrdering.Default;
    public string[] NeedsMods = Array.Empty<string>();
    public string[] ConflictsMods = Array.Empty<string>();
    public string[] NeedsPatches = Array.Empty<string>();
    public string[] ConflictsPatches = Array.Empty<string>();
    public string[] BeforePatches = Array.Empty<string>();
    public string[] AfterPatches = Array.Empty<string>();
    public string[] BeforeMods = Array.Empty<string>();
    public string[] AfterMods = Array.Empty<string>();
    public string[] ConfigurationInputs = Array.Empty<string>();
    public string[] DeclaredCapabilities = Array.Empty<string>();
    public List<PrefabPatchOperation> Operations = new();
    public string ManifestHash;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Machine-readable ordering, compatibility, conflict, cache, or composition
/// diagnostic.
/// </summary>
[Serializable]
public sealed class PrefabPatchDiagnostic
{
    public PrefabPatchDiagnosticSeverity Severity;
    public string Code;
    public string TargetAddress;
    public string PatchId;
    public string OperationId;
    public string Message;
}

/// <summary>
/// Cacheable result of discovery, dependency resolution, ordering, conflict
/// analysis, and normalized operation validation for one stock prefab.
/// </summary>
[Serializable]
public sealed class PrefabPatchResolvedPlan
{
    public int SchemaVersion = PrefabPatchSchema.Version;
    public int ComposerVersion = PrefabPatchSchema.ComposerVersion;
    public string CacheKey;
    public string SourceFingerprint;
    public string InputHash;
    public PrefabPatchPrefabIdentity TargetPrefab;
    public string[] OrderedPatchIds = Array.Empty<string>();
    public List<PrefabPatchOperation> Operations = new();
    public List<PrefabPatchDiagnostic> Diagnostics = new();
    public bool IsValid;
    public long ResolvedUtcTicks;
}
