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
    public const string AddressablesLabel = "patch-manager-prefab-patches";
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
    PatchOwned
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
    Color
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchComponentKind
{
    BoxCollider,
    SphereCollider,
    MeshFilter,
    MeshRenderer
}

/// <summary>
/// Canonical identity and compatibility data for one stock Addressable prefab.
/// </summary>
[Serializable]
public sealed class PrefabPatchPrefabIdentity
{
    public string Address;
    public string CatalogId;
    public string CatalogHash;
    public string SourceBundleFileName;
    public string SourceBundleHash;
    public string SourceSerializedFileName;
    public long SourcePathId;
    public string AssetType;
    public string StructuralDescription;
    public string StructuralFingerprint;

    [JsonIgnore]
    public string CanonicalKey =>
        $"{CatalogId}|{SourceSerializedFileName}|{SourcePathId}|{AssetType}";
}

/// <summary>
/// Deterministic runtime traversal hint. It is validated against the manifest's
/// source identity and structural fingerprint; it is never the canonical ID.
/// </summary>
[Serializable]
public sealed class PrefabPatchRuntimeLocator
{
    public int[] SiblingIndices = Array.Empty<int>();
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
    public string SourceSerializedFileName;
    public long SourcePathId;
    public string ObjectType;
    public string OwnerPatchId;
    public string ObjectId;
    public PrefabPatchRuntimeLocator RuntimeLocator;

    [JsonIgnore]
    public string CanonicalKey =>
        Kind == PrefabPatchTargetKind.Stock
            ? $"stock:{SourceSerializedFileName}:{SourcePathId}:{ObjectType}"
            : $"patch:{OwnerPatchId}:{ObjectId}";
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
/// Addressable Unity object reference used by SetObjectReference or a component
/// fragment. Source identity is retained for compatibility diagnostics.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectReference
{
    public string Address;
    public string ExpectedType;
    public string CatalogId;
    public string SourceBundleFileName;
    public string SourceSerializedFileName;
    public long SourcePathId;
}

/// <summary>
/// Constrained component payload used for added patch-owned objects and
/// AddComponent operations.
/// </summary>
[Serializable]
public sealed class PrefabPatchComponentFragment
{
    public PrefabPatchComponentKind Kind;
    public string ComponentId;
    public bool Enabled = true;
    public PrefabPatchValue Center;
    public PrefabPatchValue Size;
    public double Radius;
    public bool IsTrigger;
    public PrefabPatchObjectReference Mesh;
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
    public bool Active = true;
    public PrefabPatchValue LocalPosition;
    public PrefabPatchValue LocalRotation;
    public PrefabPatchValue LocalScale;
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
    public string PatchId;
    public string ModId;
    public string ModVersion;
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
