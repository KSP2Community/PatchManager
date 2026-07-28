using UnityEngine;

namespace PatchManager.PrefabPatching
{
    /// <summary>
    /// Explicit stable ID for a GameObject introduced by a visual prefab patch.
    /// Later patches address it as owning patch ID plus this patch-local ID.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrefabPatchObjectId : MonoBehaviour
    {
        [SerializeField] private string _id;

        public string Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
