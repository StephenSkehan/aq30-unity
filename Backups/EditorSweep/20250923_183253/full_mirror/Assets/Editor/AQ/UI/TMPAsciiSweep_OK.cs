// AQ — TMP ASCII Sweep (✓ -> "OK")
// Menu: AQ → UI → TMP → Replace ✓ with "OK" (Scenes & Prefabs)
// Safe, idempotent. Operates on open scene + all .unity/.prefab assets under Assets/.
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.EditorTools.UI
{
    public static class TMPAsciiSweep_OK
    {
        private const string MenuPath = "AQ/UI/TMP/Replace ✓ with \"OK\" (Scenes & Prefabs)";
        private static readonly string[] Kinds = { ".prefab", ".unity" };

        [MenuItem(MenuPath)]
        public static void Run()
        {
            int changed = 0;

            // 1) Process open scene first (cheap)
            changed += SweepScene(SceneManager.GetActiveScene());

            // 2) Process all scenes & prefabs in Assets/
            var guids = AssetDatabase.FindAssets("t:prefab t:scene", new[] { "Assets" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Kinds.Any(path.EndsWith)) continue;

                if (path.EndsWith(".prefab"))
                {
                    var root = PrefabUtility.LoadPrefabContents(path);
                    changed += SweepUnder(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else // .unity scene
                {
                    var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    changed += SweepScene(s);
                    EditorSceneManager.SaveScene(s);
                }
            }

            Debug.Log(changed > 0
                ? $"[AQ TMP] Replaced ✓ with \"OK\" in {changed} text fields. (Scenes & Prefabs)"
                : "[AQ TMP] No ✓ glyphs found. Nothing to change.");
        }

        private static int SweepScene(Scene scene)
        {
            int local = 0;
            foreach (var go in scene.GetRootGameObjects())
                local += SweepUnder(go);
            if (local > 0) EditorSceneManager.MarkSceneDirty(scene);
            return local;
        }

        private static int SweepUnder(GameObject root)
        {
            int edits = 0;
            // TMP present in com.unity.ugui; both TMP_Text and legacy Text may be present.
            foreach (var tmp in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (tmp.text != null && tmp.text.Contains("✓"))
                {
                    Undo.RecordObject(tmp, "[AQ TMP] Replace ✓");
                    tmp.text = tmp.text.Replace("✓", "OK");
                    EditorUtility.SetDirty(tmp);
                    edits++;
                }
            }
#if TMP_PRESENT == false // fallback if any legacy UGUI Text slipped in
            foreach (var utext in root.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                if (utext.text != null && utext.text.Contains("✓"))
                {
                    Undo.RecordObject(utext, "[AQ TMP] Replace ✓");
                    utext.text = utext.text.Replace("✓", "OK");
                    EditorUtility.SetDirty(utext);
                    edits++;
                }
            }
#endif
            return edits;
        }
    }
}
#endif
