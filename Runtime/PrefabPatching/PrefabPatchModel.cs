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
    /// <summary>The current serialized manifest schema version.</summary>
    public const int Version = 1;
    /// <summary>The current runtime composition algorithm version.</summary>
    public const int ComposerVersion = 1;
    /// <summary>The suffix used for per-mod prefab manifest labels.</summary>
    public const string AddressablesLabelSuffix = "_prefab_patches";
}

/// <summary>
/// Common dependency and relative-ordering metadata shared by JSON patches
/// and declarative prefab patches.
/// </summary>
/// <remarks>
/// The two patch domains retain their own serialized models, but consumers
/// that inspect eligibility or ordering can use this interface instead of
/// duplicating a second metadata abstraction.
/// </remarks>
public interface IPatchRelationships
{
    /// <summary>Gets the mod IDs required by the patch.</summary>
    IEnumerable<string> NeedsMods { get; }

    /// <summary>Gets the mod IDs that disable the patch when present.</summary>
    IEnumerable<string> ConflictsMods { get; }

    /// <summary>Gets the patch IDs required by the patch.</summary>
    IEnumerable<string> NeedsPatches { get; }

    /// <summary>Gets the patch IDs that disable the patch when present.</summary>
    IEnumerable<string> ConflictsPatches { get; }

    /// <summary>Gets the patch IDs that this patch must precede.</summary>
    IEnumerable<string> BeforePatches { get; }

    /// <summary>Gets the patch IDs that this patch must follow.</summary>
    IEnumerable<string> AfterPatches { get; }

    /// <summary>Gets the mod IDs whose patches this patch must precede.</summary>
    IEnumerable<string> BeforeMods { get; }

    /// <summary>Gets the mod IDs whose patches this patch must follow.</summary>
    IEnumerable<string> AfterMods { get; }
}

/// <summary>
/// One active mod's Addressables discovery boundary for declarative prefab
/// patches. The mod descriptor supplies ownership; the manifest asset address
/// is deliberately not part of patch identity.
/// </summary>
public sealed class PrefabPatchManifestSource
{
    /// <summary>The SpaceWarp mod ID that owns manifests discovered from this source.</summary>
    public string OwnerModId;
    /// <summary>The Addressables label that exposes the owning mod's manifests.</summary>
    public string AddressablesLabel;
}

/// <summary>Defines the broad execution pass for a prefab patch.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchPass
{
    /// <summary>Runs before the normal patch pass.</summary>
    Early,
    /// <summary>Runs in the normal patch pass.</summary>
    Default,
    /// <summary>Runs after the normal patch pass.</summary>
    Late
}

/// <summary>Defines the ordering bucket within a patch pass.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchOrdering
{
    /// <summary>Runs before Default and Last patches in the pass.</summary>
    First,
    /// <summary>Runs after First and before Last patches in the pass.</summary>
    Default,
    /// <summary>Runs after First and Default patches in the pass.</summary>
    Last
}

/// <summary>Identifies the source domain of an operation target.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchTargetKind
{
    /// <summary>An object or component inherited from the stock prefab.</summary>
    Stock,
    /// <summary>A GameObject introduced by a patch operation.</summary>
    PatchOwned,
    /// <summary>A component introduced by a patch operation.</summary>
    PatchComponent
}

/// <summary>Identifies the Unity object kind returned by a runtime locator.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchRuntimeTargetKind
{
    /// <summary>The locator resolves a GameObject.</summary>
    GameObject,
    /// <summary>The locator resolves a Component.</summary>
    Component
}

/// <summary>Lists the normalized mutations supported by the composer.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchOperationKind
{
    /// <summary>Writes a serialized scalar or structured value.</summary>
    SetValue,
    /// <summary>Writes a Unity object reference.</summary>
    SetObjectReference,
    /// <summary>Changes a GameObject's active state.</summary>
    SetActive,
    /// <summary>Adds a patch-owned GameObject hierarchy.</summary>
    AddObject,
    /// <summary>Disables an inherited or patch-owned GameObject.</summary>
    SuppressObject,
    /// <summary>Adds a patch-owned component.</summary>
    AddComponent,
    /// <summary>Removes an inherited or patch-owned component.</summary>
    RemoveComponent
}

/// <summary>Identifies the payload stored in a <see cref="PrefabPatchValue" />.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchValueKind
{
    /// <summary>A Boolean value.</summary>
    Boolean,
    /// <summary>A signed integer value.</summary>
    Integer,
    /// <summary>A floating-point value.</summary>
    Float,
    /// <summary>A string value.</summary>
    String,
    /// <summary>A two-component vector.</summary>
    Vector2,
    /// <summary>A three-component vector.</summary>
    Vector3,
    /// <summary>A four-component vector.</summary>
    Vector4,
    /// <summary>A quaternion.</summary>
    Quaternion,
    /// <summary>An RGBA color.</summary>
    Color,
    /// <summary>A serialized array or list size.</summary>
    ArraySize,
    /// <summary>A managed-reference payload with an explicit CLR type.</summary>
    ManagedReference,
    /// <summary>An arbitrary JSON payload interpreted by the target property.</summary>
    Json
}

/// <summary>Identifies how a Unity object reference is resolved.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchObjectReferenceKind
{
    /// <summary>Resolve the object from an Addressables key.</summary>
    Addressable,
    /// <summary>Resolve the object from the effective prefab hierarchy.</summary>
    Target
}

/// <summary>
/// Runtime identity and optional compatibility data for one stock Addressable
/// prefab.
/// </summary>
[Serializable]
public sealed class PrefabPatchPrefabIdentity
{
    /// <summary>The stock prefab Addressables key.</summary>
    public string Address;
    /// <summary>The expected assembly-qualified asset type.</summary>
    public string AssetType;
    /// <summary>An optional authoring-time hierarchy fingerprint.</summary>
    public string StructuralFingerprint;

    /// <summary>Gets the stable cache and grouping identity.</summary>
    [JsonIgnore]
    public string CanonicalKey => $"address:{Address}";

    /// <summary>Creates an identity for a GameObject Addressables key.</summary>
    /// <param name="address">The non-empty stock prefab key.</param>
    /// <returns>A prefab identity without a structural fingerprint.</returns>
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
    /// <summary>Sibling indices from the prefab root to the target.</summary>
    public int[] SiblingIndices = Array.Empty<int>();
    /// <summary>An optional slash-delimited hierarchy path for imperative patches.</summary>
    public string HierarchyPath;
    /// <summary>The Unity object kind produced by the locator.</summary>
    public PrefabPatchRuntimeTargetKind TargetKind;
    /// <summary>The assembly-qualified component type when targeting a component.</summary>
    public string ComponentType;
    /// <summary>The zero-based component occurrence on the located GameObject.</summary>
    public int ComponentOrdinal;
    /// <summary>A diagnostic-only human-readable authoring path.</summary>
    public string DisplayPath;
}

/// <summary>
/// An inherited source object or an object introduced by a named patch.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectTarget
{
    /// <summary>The source domain of the target.</summary>
    public PrefabPatchTargetKind Kind;
    /// <summary>The expected assembly-qualified Unity object type.</summary>
    public string ObjectType;
    /// <summary>The namespaced patch ID that introduced a patch-owned target.</summary>
    public string OwnerPatchId;
    /// <summary>The stable local ID of a patch-owned GameObject.</summary>
    public string ObjectId;
    /// <summary>The stable local ID of a patch-owned component.</summary>
    public string ComponentId;
    /// <summary>The traversal information for stock targets.</summary>
    public PrefabPatchRuntimeLocator RuntimeLocator;

    /// <summary>Gets the deterministic conflict and lookup key for the target.</summary>
    [JsonIgnore]
    public string CanonicalKey
    {
        get
        {
            if (Kind == PrefabPatchTargetKind.PatchComponent)
                return $"patch-component:{OwnerPatchId}:{ComponentId}";
            if (Kind == PrefabPatchTargetKind.PatchOwned)
                return $"patch:{OwnerPatchId}:{ObjectId}";

            // Match runtime traversal. Display names are diagnostic only and
            // can repeat between siblings, particularly in engine clusters.
            var path = RuntimeLocator?.HierarchyPath;
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
    /// <summary>The active payload representation.</summary>
    public PrefabPatchValueKind Kind;
    /// <summary>The Boolean payload.</summary>
    public bool Boolean;
    /// <summary>The integer payload.</summary>
    public long Integer;
    /// <summary>The floating-point payload.</summary>
    public double Float;
    /// <summary>The string or raw JSON payload.</summary>
    public string String;
    /// <summary>The assembly-qualified type for managed-reference payloads.</summary>
    public string SerializedType;
    /// <summary>The first vector, quaternion, or color component.</summary>
    public double X;
    /// <summary>The second vector, quaternion, or color component.</summary>
    public double Y;
    /// <summary>The third vector, quaternion, or color component.</summary>
    public double Z;
    /// <summary>The fourth vector, quaternion, or color component.</summary>
    public double W;

    /// <summary>Creates a Boolean payload.</summary>
    public static PrefabPatchValue FromBoolean(bool value) =>
        new() { Kind = PrefabPatchValueKind.Boolean, Boolean = value };

    /// <summary>Creates an integer payload.</summary>
    public static PrefabPatchValue FromInteger(long value) =>
        new() { Kind = PrefabPatchValueKind.Integer, Integer = value };

    /// <summary>Creates a floating-point payload.</summary>
    public static PrefabPatchValue FromFloat(double value) =>
        new() { Kind = PrefabPatchValueKind.Float, Float = value };

    /// <summary>Creates a string payload.</summary>
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
    /// <summary>The resolution strategy.</summary>
    public PrefabPatchObjectReferenceKind Kind;
    /// <summary>The Addressables key for an external reference.</summary>
    public string Address;
    /// <summary>The optional assembly-qualified type expected after resolution.</summary>
    public string ExpectedType;
    /// <summary>The effective-prefab target for a local reference.</summary>
    public PrefabPatchObjectTarget Target;

    /// <summary>Creates an Addressables-backed reference.</summary>
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

    /// <summary>Creates a reference to an object in the effective prefab.</summary>
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
    /// <summary>The Unity SerializedProperty path.</summary>
    public string PropertyPath;
    /// <summary>The value written at the property path.</summary>
    public PrefabPatchValue Value;
}

/// <summary>
/// One Unity object reference removed from a serialized component payload and
/// restored after every patch-owned object and component has been created.
/// </summary>
[Serializable]
public sealed class PrefabPatchSerializedReference
{
    /// <summary>The Unity SerializedProperty path.</summary>
    public string PropertyPath;
    /// <summary>The reference restored after object creation.</summary>
    public PrefabPatchObjectReference Reference;
}

/// <summary>
/// Serialized payload used for added patch-owned objects and AddComponent
/// operations.
/// </summary>
[Serializable]
public sealed class PrefabPatchComponentFragment
{
    /// <summary>The stable patch-local component ID.</summary>
    public string ComponentId;
    /// <summary>The assembly-qualified concrete component type.</summary>
    public string ComponentType;
    /// <summary>The serialized non-reference values.</summary>
    public List<PrefabPatchSerializedValue> Values = new();
    /// <summary>The deferred Unity object references.</summary>
    public List<PrefabPatchSerializedReference> References = new();
}

/// <summary>
/// An inline, patch-owned hierarchy fragment. Every object has an explicit ID,
/// allowing later required patches to target it without hierarchy-name lookup.
/// </summary>
[Serializable]
public sealed class PrefabPatchObjectFragment
{
    /// <summary>The stable patch-local GameObject ID.</summary>
    public string ObjectId;
    /// <summary>The created GameObject name.</summary>
    public string Name;
    /// <summary>The assembly-qualified Transform or RectTransform type.</summary>
    public string TransformType;
    /// <summary>Whether the object is active after composition.</summary>
    public bool Active = true;
    /// <summary>The Unity layer assigned to the object.</summary>
    public int Layer;
    /// <summary>The Unity tag assigned to the object.</summary>
    public string Tag = "Untagged";
    /// <summary>Whether the object uses Unity's static flag.</summary>
    public bool IsStatic;
    /// <summary>The local-position payload.</summary>
    public PrefabPatchValue LocalPosition;
    /// <summary>The local-rotation payload.</summary>
    public PrefabPatchValue LocalRotation;
    /// <summary>The local-scale payload.</summary>
    public PrefabPatchValue LocalScale;
    /// <summary>The RectTransform anchor-min payload.</summary>
    public PrefabPatchValue AnchorMin;
    /// <summary>The RectTransform anchor-max payload.</summary>
    public PrefabPatchValue AnchorMax;
    /// <summary>The RectTransform anchored-position payload.</summary>
    public PrefabPatchValue AnchoredPosition;
    /// <summary>The RectTransform size-delta payload.</summary>
    public PrefabPatchValue SizeDelta;
    /// <summary>The RectTransform pivot payload.</summary>
    public PrefabPatchValue Pivot;
    /// <summary>Components created on this object.</summary>
    public List<PrefabPatchComponentFragment> Components = new();
    /// <summary>Child objects created beneath this object.</summary>
    public List<PrefabPatchObjectFragment> Children = new();
}

/// <summary>
/// One normalized declarative operation.
/// </summary>
[Serializable]
public sealed class PrefabPatchOperation
{
    /// <summary>The stable patch-local operation ID.</summary>
    public string OperationId;
    /// <summary>The owning namespaced patch ID, assigned during resolution.</summary>
    [JsonIgnore]
    public string PatchId;
    /// <summary>The mutation performed by the operation.</summary>
    public PrefabPatchOperationKind Kind;
    /// <summary>The object or component receiving the operation.</summary>
    public PrefabPatchObjectTarget Target;
    /// <summary>The Unity SerializedProperty path for value/reference writes.</summary>
    public string PropertyPath;
    /// <summary>The payload for value and active-state writes.</summary>
    public PrefabPatchValue Value;
    /// <summary>The payload for object-reference writes.</summary>
    public PrefabPatchObjectReference ObjectReference;
    /// <summary>The hierarchy created by an AddObject operation.</summary>
    public PrefabPatchObjectFragment AddedObject;
    /// <summary>The component created by an AddComponent operation.</summary>
    public PrefabPatchComponentFragment AddedComponent;
    /// <summary>An optional authoring-time value fingerprint used for stale-data checks.</summary>
    public string ExpectedOriginalFingerprint;
    /// <summary>The authoring asset path used in diagnostics.</summary>
    public string AuthoringAssetPath;
    /// <summary>The authoring property path used in diagnostics.</summary>
    public string AuthoringPropertyPath;

    /// <summary>Gets the deterministic key used to detect competing writes.</summary>
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
public sealed class PrefabPatchManifest : IPatchRelationships
{
    /// <summary>The serialized schema version.</summary>
    public int SchemaVersion = PrefabPatchSchema.Version;
    /// <summary>The required composer version.</summary>
    public int ComposerVersion = PrefabPatchSchema.ComposerVersion;
    /// <summary>The mod-local patch name stored in distributable JSON.</summary>
    public string PatchName;
    /// <summary>The runtime namespaced patch ID assigned from ownership.</summary>
    [JsonIgnore]
    public string PatchId;
    /// <summary>The runtime owning mod ID assigned during discovery.</summary>
    [JsonIgnore]
    public string ModId;
    /// <summary>The stock prefab targeted by this manifest.</summary>
    public PrefabPatchPrefabIdentity TargetPrefab;
    /// <summary>The broad execution pass.</summary>
    public PrefabPatchPass Pass = PrefabPatchPass.Default;
    /// <summary>The ordering bucket within the pass.</summary>
    public PrefabPatchOrdering Ordering = PrefabPatchOrdering.Default;
    /// <summary>Required active mod IDs.</summary>
    public string[] NeedsMods = Array.Empty<string>();
    /// <summary>Conflicting active mod IDs.</summary>
    public string[] ConflictsMods = Array.Empty<string>();
    /// <summary>Required namespaced patch IDs.</summary>
    public string[] NeedsPatches = Array.Empty<string>();
    /// <summary>Conflicting namespaced patch IDs.</summary>
    public string[] ConflictsPatches = Array.Empty<string>();
    /// <summary>Patch IDs that this patch must precede in its bucket.</summary>
    public string[] BeforePatches = Array.Empty<string>();
    /// <summary>Patch IDs that this patch must follow in its bucket.</summary>
    public string[] AfterPatches = Array.Empty<string>();
    /// <summary>Mod IDs whose patches this patch must precede.</summary>
    public string[] BeforeMods = Array.Empty<string>();
    /// <summary>Mod IDs whose patches this patch must follow.</summary>
    public string[] AfterMods = Array.Empty<string>();
    /// <summary>Configuration values included in the plan cache key.</summary>
    public string[] ConfigurationInputs = Array.Empty<string>();
    /// <summary>Operation capabilities declared for diagnostics and compatibility.</summary>
    public string[] DeclaredCapabilities = Array.Empty<string>();
    /// <summary>Operations applied in their serialized order.</summary>
    public List<PrefabPatchOperation> Operations = new();
    /// <summary>The normalized content hash, excluding runtime ownership.</summary>
    public string ManifestHash;

    IEnumerable<string> IPatchRelationships.NeedsMods => NeedsMods;
    IEnumerable<string> IPatchRelationships.ConflictsMods => ConflictsMods;
    IEnumerable<string> IPatchRelationships.NeedsPatches => NeedsPatches;
    IEnumerable<string> IPatchRelationships.ConflictsPatches => ConflictsPatches;
    IEnumerable<string> IPatchRelationships.BeforePatches => BeforePatches;
    IEnumerable<string> IPatchRelationships.AfterPatches => AfterPatches;
    IEnumerable<string> IPatchRelationships.BeforeMods => BeforeMods;
    IEnumerable<string> IPatchRelationships.AfterMods => AfterMods;
}

/// <summary>Defines the impact level of a resolver or composer diagnostic.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PrefabPatchDiagnosticSeverity
{
    /// <summary>Informational behavior that does not invalidate a plan.</summary>
    Info,
    /// <summary>A recoverable conflict resolved by deterministic ordering.</summary>
    Warning,
    /// <summary>An error that invalidates the resolved plan.</summary>
    Error
}

/// <summary>
/// Machine-readable ordering, compatibility, conflict, cache, or composition
/// diagnostic.
/// </summary>
[Serializable]
public sealed class PrefabPatchDiagnostic
{
    /// <summary>The diagnostic impact level.</summary>
    public PrefabPatchDiagnosticSeverity Severity;
    /// <summary>The stable machine-readable diagnostic code.</summary>
    public string Code;
    /// <summary>The affected stock prefab address.</summary>
    public string TargetAddress;
    /// <summary>The affected patch ID, when applicable.</summary>
    public string PatchId;
    /// <summary>The affected operation ID, when applicable.</summary>
    public string OperationId;
    /// <summary>The human-readable explanation.</summary>
    public string Message;
}

/// <summary>
/// Cacheable result of discovery, dependency resolution, ordering, conflict
/// analysis, and normalized operation validation for one stock prefab.
/// </summary>
[Serializable]
public sealed class PrefabPatchResolvedPlan
{
    /// <summary>The manifest schema version used to build the plan.</summary>
    public int SchemaVersion = PrefabPatchSchema.Version;
    /// <summary>The composer version required by the plan.</summary>
    public int ComposerVersion = PrefabPatchSchema.ComposerVersion;
    /// <summary>The stable cache filename identity.</summary>
    public string CacheKey;
    /// <summary>The hash of all source manifests and environment inputs.</summary>
    public string SourceFingerprint;
    /// <summary>The hash of enabled, ordered plan inputs.</summary>
    public string InputHash;
    /// <summary>The common stock prefab target.</summary>
    public PrefabPatchPrefabIdentity TargetPrefab;
    /// <summary>The final deterministic patch order.</summary>
    public string[] OrderedPatchIds = Array.Empty<string>();
    /// <summary>The validated flattened operation stream.</summary>
    public List<PrefabPatchOperation> Operations = new();
    /// <summary>Diagnostics produced while resolving the plan.</summary>
    public List<PrefabPatchDiagnostic> Diagnostics = new();
    /// <summary>Whether the plan can be composed.</summary>
    public bool IsValid;
    /// <summary>The UTC creation time represented as <see cref="DateTime.Ticks" />.</summary>
    public long ResolvedUtcTicks;
}
