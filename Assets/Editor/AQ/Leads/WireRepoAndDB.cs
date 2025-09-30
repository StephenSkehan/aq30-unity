#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace AQ.Editor.Leads
{
    public static class WireRepoAndDB
    {
        [MenuItem("AQ/Leads/Wire Repo + DB (Auto)")]
        public static void Run()
        {
            // 1) Find or create a LeadsRepository in the open scene
            var repo = FindRepoInScene();
            if (repo == null)
            {
                var go = new GameObject("Leads_Runtime");
                repo = go.AddComponent<AQ.App.Leads.LeadsRepository>();
                Debug.Log("🆕 Created LeadsRepository on 'Leads_Runtime'.", go);
                EditorSceneManager.MarkSceneDirty(go.scene);
            }

            // 2) Find or create a LeadsDatabase asset
            var db = FindDatabaseAsset();
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<AQ.App.Leads.LeadsDatabase>();
                var path = "Assets/App/Leads/Data/LeadsDatabase.asset";
                var dir  = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(db, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"🆕 Created LeadsDatabase asset at: {path}", db);
            }

            // 3) Wire & save
            repo.database = db;
            EditorUtility.SetDirty(repo);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(repo.gameObject.scene);

            var dbPath = AssetDatabase.GetAssetPath(db);
            Debug.Log($"✅ Wired LeadsRepository.database → {dbPath}", repo);
        }

        // Convenience: open the Playground window if it exists
        [MenuItem("AQ/Leads/Open Playground Window")]
        public static void OpenPlayground()
        {
            var t = System.Type.GetType("AQ.App.Leads.LeadsPlaygroundWindow, Assembly-CSharp-Editor");
            if (t != null) EditorWindow.GetWindow(t, false, "Leads Playground", true).Show();
            else Debug.LogWarning("LeadsPlaygroundWindow type not found.");
        }

        // --- helpers ---
        static AQ.App.Leads.LeadsRepository FindRepoInScene()
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            var repo = Object.FindFirstObjectByType<AQ.App.Leads.LeadsRepository>(FindObjectsInactive.Include);
#else
            var repo = Object.FindObjectOfType<AQ.App.Leads.LeadsRepository>();
#endif
            return repo;
        }

        static AQ.App.Leads.LeadsDatabase FindDatabaseAsset()
        {
            var guids = AssetDatabase.FindAssets("t:AQ.App.Leads.LeadsDatabase");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var db = AssetDatabase.LoadAssetAtPath<AQ.App.Leads.LeadsDatabase>(path);
                if (db != null) return db;
            }
            return null;
        }
    }
}
#endif
