#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FixTMPCanvasRenderer
{
    [MenuItem("AQ/UI/Fix → Remove stray CanvasRenderer from 3D TextMeshPro (scenes + prefabs)")]
    public static void RemoveObsoleteCanvasRenderers()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
            return;
        }

        int sceneRemoved = 0, prefabRemoved = 0;

        // ---- 1) SCENES: open each, strip CanvasRenderer from TextMeshPro (NOT TextMeshProUGUI) ----
        var sceneGuids = AssetDatabase.FindAssets("t:scene", new[] { "Assets/Scenes" });
        string activePath = EditorSceneManager.GetActiveScene().path;

        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            foreach (var tmp3D in Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var cr = tmp3D.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    Object.DestroyImmediate(cr, allowDestroyingAssets: false);
                    sceneRemoved++;
                    EditorUtility.SetDirty(tmp3D);
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        // ---- 2) PREFABS: open, strip, save ----
        var prefabGuids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;

            // Only TextMeshPro (3D) should not have CanvasRenderer
            foreach (var tmp3D in root.GetComponentsInChildren<TextMeshPro>(true))
            {
                var cr = tmp3D.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    Object.DestroyImmediate(cr, allowDestroyingAssets: true);
                    changed = true;
                    prefabRemoved++;
                }
            }

            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        // Restore previously active scene if it existed
        if (!string.IsNullOrEmpty(activePath))
            EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        Debug.Log($"[FixTMPCanvasRenderer] Removed CanvasRenderer from 3D TMP: scenes={sceneRemoved}, prefabs={prefabRemoved}.");
    }

    // Optional helper: replace ✓ with OK across both UGUI and 3D TMP, if you still want ASCII-only
    [MenuItem("AQ/UI/Fix → Replace ✓ with OK (both TMP types)")]
    public static void ReplaceChecksASCII()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
            return;
        }

        int total = 0;
        var sceneGuids = AssetDatabase.FindAssets("t:scene", new[] { "Assets/Scenes" });
        string activePath = EditorSceneManager.GetActiveScene().path;

        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(t.text) && t.text.Contains("✓"))
                {
                    t.text = t.text.Replace("✓", "OK");
                    total++;
                    EditorUtility.SetDirty(t);
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        var prefabGuids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;

            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!string.IsNullOrEmpty(t.text) && t.text.Contains("✓"))
                {
                    t.text = t.text.Replace("✓", "OK");
                    changed = true;
                    total++;
                }
            }

            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        if (!string.IsNullOrEmpty(activePath))
            EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        Debug.Log($"[FixTMPCanvasRenderer] Replaced ✓ → OK in {total} TMP text entries.");
    }
}
#endif
