#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AsciiSweep
{
    [MenuItem("AQ/UI/Fix → Replace ✓ with OK (scenes + prefabs)")]
    public static void ReplaceChecks()
    {
        if (Application.isPlaying) { EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK"); return; }

        int total = 0;

        // 1) Open scenes in Assets/Scenes and replace
        var sceneGuids = AssetDatabase.FindAssets("t:scene", new[] {"Assets/Scenes"});
        var current = EditorSceneManager.GetActiveScene().path;

        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    EditorUtility.SetDirty(tmp);
                    total++;
                }
            }
            EditorSceneManager.SaveScene(scene);
        }

        // 2) Prefabs: load, replace, save
        var prefabGuids = AssetDatabase.FindAssets("t:prefab", new[] {"Assets"});
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    changed = true; total++;
                }
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        // Reopen whatever you were on
        if (!string.IsNullOrEmpty(current)) EditorSceneManager.OpenScene(current, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        Debug.Log($"[AsciiSweep] Replaced ✓ → OK in {total} TextMeshProUGUI entries.");
    }
}
#endif
