# Declarative prefab patches

Patch Manager prefab patches modify stock Addressable prefabs before the game
instantiates them. Visual prefab-variant authoring, C#, and Lua all generate the
same versioned JSON manifest and use the same resolver, ordering rules, cache,
and runtime composer.

The schema is type-agnostic for Unity `Component` types. An added component is
identified by its assembly-qualified CLR type and contains:

- every serialized value, using Unity `SerializedProperty` paths;
- every Unity object reference as a separate fixup;
- a stable patch-local component ID.

The runtime constructs all patch-owned objects and components beneath an
inactive root, applies collection sizes and serialized values, and then resolves
references. This permits references between sibling objects, between arbitrary
components, into nested serialized arrays/lists, to stock prefab objects, and
to mod or stock Addressables. It also prevents `OnEnable` from observing a
partially hydrated effective prefab.

This is still the initial prerelease schema (`schemaVersion: 1`,
`composerVersion: 1`). There is no compatibility or migration layer. Recompile
local experimental manifests from their authoring variants whenever the schema
changes.

Compiled JSON contains the local `patchName`, operations, dependencies, and
target identity. It does not serialize a mod ID or mod version. Each SpaceWarp
descriptor declares an `addressable_prefab_patch_label`; Patch Manager queries
that label in both editor Play Mode and players and assigns the descriptor's
`swinfo` ID to every returned manifest. The manifest asset's own Addressables
address is not patch identity. The fully-qualified `mod-id:patch-name` exists
only in the resolved runtime model.

KSP2UnityTools-generated mods default to `<mod-id>_prefab_patches`. Redux uses
`redux_prefab_patches`. Script TextAssets use the parallel
`<mod-id>_patches`/`redux_patches` convention. Loose Lua files remain supported.

## Visual authoring

In Redux SDK:

1. Import the stock prefab with BundleKit's Linked Addressables Browser.
2. Right-click the linked prefab and choose **Redux SDK > Create Prefab Patch
   from Linked Prefab**.
3. Edit the generated prefab variant normally.
4. Add `PrefabPatchAuthoringObjectId` to every newly added GameObject and give
   each one a stable ID.
5. Save the variant. With auto-compile enabled, its `.prefabpatch.json` manifest
   is rebuilt immediately.

New `GameObject`, `RectTransform`, UGUI, TMP, game, and mod component types use
the same general compiler. The compiler records the complete serialized
component state; it does not select from a component whitelist. Local
GameObject/component references are translated to patch-owned IDs. Linked game
assets and mod-owned Addressables remain address references and are loaded
before composition.

## C# authoring

Register C# patches before `PrefabPatchRuntime.CloseRegistration()`. Handwritten
patches target a stock Addressables key and select objects by their ordinary
prefab hierarchy path:

```csharp
using PatchManager.PrefabPatching;
using PatchManager.CSharpPatching;
using UnityEngine.UI;

var background = PrefabPatchBuilder.ComponentAt<Image>(
    "KSP2UIWindow/Root/Window-App/Background"
);

Patching.Mod.PatchPrefab("toolbar", "SomeWindow.prefab")
    .SetValue(
        "tint-background",
        background,
        "m_Color.r",
        PrefabPatchValue.FromFloat(0.25)
    )
    .Register();
```

`PrefabPatchBuilder` also exposes value/reference writes, active/suppress,
add/remove component, ordering, dependency, conflict, and configuration-input
methods. `PatchObject` and `PatchComponent` create stable targets for objects
introduced by this or a required patch. Addressable references are created with
`Addressable`.

Operation order is the fluent call order. This is significant when a later
operation targets an object or component introduced earlier in the same patch.

## Lua authoring

Lua uses `PM:Prefab`. Tables use the camel-case names from the public JSON
schema; empty Lua tables are normalized to empty arrays where the model expects
a collection.

```lua
local patch = PM:Prefab("toolbar", "SomeWindow.prefab")
    :Needs("some-required-mod")
    :SetComponent(
        "tint-background",
        "KSP2UIWindow/Root/Window-App/Background",
        "UnityEngine.UI.Image",
        "m_Color.r",
        0.25
    )

patch:Register()
```

The returned builder supports `Early`, `Late`, `First`, `Last`, dependency and
ordering methods, `Set`, `Reference`, `Active`, `Suppress`, `AddObject`,
`AddComponent`, `RemoveComponent`, `Build`, and `Register`. `Build` is useful
for tools/tests; normal mods call `Register`. `GameObject(path)` and
`Component(path, type, ordinal)` return key-first targets that can be passed to
any generic operation method; `SetComponent` is shorthand for the common
scalar-component case.

Visual, C#, and Lua manifests all identify the stock prefab by its Addressables
key. Compiled visual manifests additionally retain a structural fingerprint
and sibling-index runtime locators, but do not serialize catalog, bundle, CAB,
path-ID, or full structural-description metadata. Imperative patches resolve
named hierarchy paths at runtime. Duplicate child names at one hierarchy level
are rejected as ambiguous rather than resolved arbitrarily.

Patch-owned targets use the local owning patch name plus stable
object/component ID. An explicit `other-mod:patch-name` is used only when
targeting another mod's required patch.

## Runtime and compatibility behavior

- Prefab discovery, plan-cache, ordering, diagnostics, and composition failures
  are written as a `Prefab Patches` section in the existing
  `pm_summary.log`; prefab updates preserve the ordinary JSON-patch summary.
- Composition occurs once per resolved stock prefab and the effective prefab is
  cached for repeated provider requests.
- Resolver ordering, required/conflicting mods and patches, and field conflicts
  are identical for all three frontends.
- The player loads Addressable references before applying the plan. Target
  references are resolved locally after every object/component has been
  created.
- A missing CLR component type, missing Addressable, stale stock fingerprint,
  invalid property path, or unknown project tag fails the patch with a precise
  diagnostic instead of silently dropping data.
- Patch manifests and mod-owned assets are distributable. Linked stock game
  assets remain external references and are not copied into a mod build by the
  prefab patch schema.
