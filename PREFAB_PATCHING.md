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

The first implementation was never released. Schema 2 is therefore the only
accepted schema; there is no schema-1 migration path. Recompile any local
experimental manifests from their authoring variants.

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
using UnityEngine;
using UnityEngine.UI;

var button = PrefabPatchComponentBuilder
    .For<Button>("toolbar:button")
    .Value("m_Interactable", PrefabPatchValue.FromBoolean(false))
    .Reference(
        "m_TargetGraphic",
        PrefabPatchBuilder.TargetReference(
            PrefabPatchBuilder.PatchComponent(
                "my-mod:toolbar",
                "toolbar:image"
            ),
            typeof(Graphic)
        )
    )
    .Build();
var image = PrefabPatchComponentBuilder
    .For<Image>("toolbar:image")
    .Build();

new PrefabPatchBuilder("my-mod", "toolbar", targetIdentity, "1.0.0")
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
targets only need the owning patch ID plus stable object/component ID.

## Runtime and compatibility behavior

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
