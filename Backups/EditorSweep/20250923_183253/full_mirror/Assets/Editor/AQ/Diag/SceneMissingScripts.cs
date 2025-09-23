// AQ — Scene Missing Script Scan
// Menu: AQ → Diag → Scan Active Scene for Missing Scripts
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AQ.EditorTools.Diag
{
    public static class SceneMissingScripts
    {
        [MenuItem("AQ/Diag/Scan Active Scene for Missing Scripts")]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) { Debug.LogWarning("[AQ Diag] No active scene."); return; }

            int objects = 0, missing = 0, comps = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    objects++;
                    var c = t.GetComponents<Component>();
                    for (int i = 0; i < c.Length; i++)
                    {
                        comps++;
                        if (c[i] == null)
                        {
                            missing++;
                            Debug.LogWarning($"[AQ Diag] Missing Script on: {GetPath(t.gameObject)}", t.gameObject);
                        }
                    }
                }
            }
            Debug.Log($"[AQ Diag] Scene scan complete. Objects={objects}, Components={comps}, MissingScripts={missing}");
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var p = go.transform.parent;
            while (p != null) { path = p.name + "/" + path; p = p.parent; }
            return path;
        }
    }
}
#endif
