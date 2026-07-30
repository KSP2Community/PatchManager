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
target identity. It does not serialize a mod ID or mod version. In editor Play
Mode, the owning Mod authoring asset supplies the ID. In a player, Patch
Manager associates each manifest's Addressables catalog with the SpaceWarp
descriptor that loaded it and uses that descriptor's `swinfo` ID. The
fully-qualified `mod-id:patch-name` exists only in the resolved runtime model.

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

Register C# patches before `PrefabPatchRuntime.CloseRegistration()`:

```csharp
using PatchManager.PrefabPatching;
using PatchManager.CSharpPatching;
using UnityEngine;
using UnityEngine.UI;

var button = PrefabPatchComponentBuilder
    .For<Button>("toolbar:button")
    .Value("m_Interactable", PrefabPatchValue.FromBoolean(false))
    .Reference(
        "m_TargetGraphic",
        PrefabPatchBuilder.TargetReference(
            PrefabPatchBuilder.PatchComponent(
                "toolbar",
                "toolbar:image"
            ),
            typeof(Graphic)
        )
    )
    .Build();
var image = PrefabPatchComponentBuilder
    .For<Image>("toolbar:image")
    .Build();

Patching.Mod.PatchPrefab("toolbar", targetIdentity)
    .AddObject(
        "01-add-toolbar",
        stockParentTarget,
        new PrefabPatchObjectBuilder("toolbar", "Toolbar")
            .Rect(
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                new Vector2(320, 80),
                new Vector2(0.5f, 0.5f)
            )
            .Component(image)
            .Component(button)
            .Build()
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

Write `sourcePathId` values as quoted decimal strings when copying canonical
identities or stock targets into Lua. Unity path IDs are signed 64-bit integers,
while Lua numbers cannot exactly represent every value in that range. Patch
Manager accepts either form and converts decimal strings to `Int64` without
losing precision.

```lua
local patch = PM:Prefab("toolbar", targetIdentity)
    :Needs("some-required-mod")
    :AddObject("01-add-toolbar", stockParent, {
        objectId = "toolbar",
        name = "Toolbar",
        transformType = UnityRectTransformType,
        active = true,
        tag = "Untagged",
        components = {
            {
                componentId = "toolbar:button",
                componentType = UnityButtonType,
                values = {
                    {
                        propertyPath = "m_Interactable",
                        value = { kind = "Boolean", boolean = false }
                    }
                },
                references = {}
            }
        },
        children = {}
    })

patch:Register()
```

The returned builder supports `Early`, `Late`, `First`, `Last`, dependency and
ordering methods, `Set`, `Reference`, `Active`, `Suppress`, `AddObject`,
`AddComponent`, `RemoveComponent`, `Build`, and `Register`. `Build` is useful
for tools/tests; normal mods call `Register`.

Pure Lua/C# patches still need the stock prefab's canonical identity and
structural fingerprint, plus canonical stock-object targets. Those values are
source-build-specific safety data, not name-based hierarchy paths. They can be
copied from a visual compiler manifest for the same linked prefab. Patch-owned
targets only need the local owning patch name plus stable object/component ID.
An explicit `other-mod:patch-name` is used only when targeting another mod's
required patch.

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
