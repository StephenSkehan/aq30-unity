#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class EnableLeadsRuntime
    {
        [MenuItem("AQ/Leads/Enable Runtime & Save", priority = 502)]
        public static void Run()
        {
            int changes = 0;

#if UNITY_2022_3_OR_NEWER || UNITY_6000_0_OR_NEWER
            var bar  = Object.FindFirstObjectByType<AQ.App.Leads.LeadsBarView>(FindObjectsInactive.Include);
            var repo = Object.FindFirstObjectByType<AQ.App.Leads.LeadsRepository>(FindObjectsInactive.Include);
#else
            var bar  = Object.FindObjectOfType<AQ.App.Leads.LeadsBarView>(true);
            var repo = Object.FindObjectOfType<AQ.App.Leads.LeadsRepository>(true);
#endif

            // Ensure bar active/enabled
            if (bar != null)
            {
                if (!bar.gameObject.activeInHierarchy) { bar.gameObject.SetActive(true); changes++; }
                if (!bar.enabled) { bar.enabled = true; changes++; }

                // Wire common fields if present (SerializedObject avoids compile-time names)
                var so = new SerializedObject(bar);

                var spScroll = so.FindProperty("scrollRect");
                if (spScroll != null && spScroll.objectReferenceValue == null)
                {
                    var sr = bar.GetComponentInChildren<ScrollRect>(true);
                    if (sr != null) { spScroll.objectReferenceValue = sr; changes++; }
                }

                var spContent = so.FindProperty("contentRoot");
                if (spContent != null && spContent.objectReferenceValue == null)
                {
                    Transform content = null;
                    var t = bar.transform.Find("Viewport/Content_Leads");
                    if (t == null) t = bar.transform.Find("ScrollLeads/Viewport/Content_Leads");
                    if (t != null) content = t;
                    if (content != null)
                    {
                        spContent.objectReferenceValue = content.GetComponent<RectTransform>();
                        changes++;
                    }
                }

                var spCardPrefab = so.FindProperty("cardPrefab");
                if (spCardPrefab != null && spCardPrefab.objectReferenceValue == null)
                {
                    var guids = AssetDatabase.FindAssets("LeadCardView t:prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null) { spCardPrefab.objectReferenceValue = prefab; changes++; }
                    }
                }

                if (changes > 0) so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Ensure repository exists + active + DB
            if (repo == null)
            {
                var go = new GameObject("LeadsRepository");
                repo = go.AddComponent<AQ.App.Leads.LeadsRepository>();
                Undo.RegisterCreatedObjectUndo(go, "Create LeadsRepository");
                changes++;
            }

            if (!repo.gameObject.activeInHierarchy) { repo.gameObject.SetActive(true); changes++; }
            if (!repo.enabled) { repo.enabled = true; changes++; }

            var soRepo = new SerializedObject(repo);
            var spDb   = soRepo.FindProperty("database");
            if (spDb != null && spDb.objectReferenceValue == null)
            {
                // find existing DB (any)
                var dbGuids = AssetDatabase.FindAssets("t:AQ.App.Leads.LeadsDatabase");
                if (dbGuids != null && dbGuids.Length > 0)
                {
                    var dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
                    var dbObj  = AssetDatabase.LoadAssetAtPath<Object>(dbPath);
                    if (dbObj != null) { spDb.objectReferenceValue = dbObj; changes++; }
                }
            }
            if (changes > 0) soRepo.ApplyModifiedPropertiesWithoutUndo();

            if (changes > 0)
            {
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }

            Debug.Log($"✅ EnableLeadsRuntime: bar={(bar ? "OK" : "MISSING")} repo={(repo ? "OK" : "MISSING")} changes={changes}.", bar ? (Object)bar : (Object)repo);
        }
    }
}
#endif
