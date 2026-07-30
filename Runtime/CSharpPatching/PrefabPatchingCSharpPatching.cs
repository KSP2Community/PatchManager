using PatchManager.PrefabPatching;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Mod-scoped C# frontend for declarative prefab patches.
    /// </summary>
    public static class PrefabPatchingCSharpPatching
    {
        /// <summary>
        /// Creates a prefab patch owned by the calling mod's swinfo identity.
        /// </summary>
        public static PrefabPatchBuilder PatchPrefab(
            this PmScope scope,
            string name,
            PrefabPatchPrefabIdentity target
        ) => new(scope.ModId, name, target);

        /// <summary>
        /// Creates a prefab patch targeting a stock Addressables key.
        /// Canonical bundle and CAB metadata are not part of imperative
        /// authoring.
        /// </summary>
        public static PrefabPatchBuilder PatchPrefab(
            this PmScope scope,
            string name,
            string address
        ) => new(scope.ModId, name, address);
    }
}
