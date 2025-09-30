#if UNITY_EDITOR
using AQ.App.Leads;
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.Leads
{
    public static class PushLeadsToRuntime
    {
        [MenuItem("AQ/Leads/Push DB To Runtime")]
        public static void PushMenu()
        {
            var db = FindLeadsDatabaseAsset();
            if (!db)
            {
                EditorUtility.DisplayDialog("Leads", "Could not locate a LeadsDatabase asset.", "OK");
                return;
            }

            Push(db);
        }

        public static void Push(LeadsDatabase db)
        {
            var repo = Object.FindAnyObjectByType<LeadsRepository>(FindObjectsInactive.Exclude);
            if (!repo)
            {
                Debug.LogWarning("[Leads Push] No LeadsRepository found in scene.");
                return;
            }

            repo.ReplaceFromDatabase(db);

            var glue = Object.FindAnyObjectByType<LeadsRuntimeGlue>(FindObjectsInactive.Exclude);
            if (glue) glue.BindOnce();

            Debug.Log($"[Leads Push] Pushed to runtime from DB: {db.name}", db);
        }

        private static LeadsDatabase FindLeadsDatabaseAsset()
        {
            var guids = AssetDatabase.FindAssets("t:LeadsDatabase");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LeadsDatabase>(path);
            }
            return null;
        }
    }
}
#endif
