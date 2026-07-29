using PatchManager.PrefabPatching;
using UnityEditor;

namespace PatchManager.Editor;

[InitializeOnLoad]
internal static class PrefabPatchPlayModeCleanup
{
    static PrefabPatchPlayModeCleanup()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            PrefabPatchRuntime.ReleaseSessionResources();
    }
}
