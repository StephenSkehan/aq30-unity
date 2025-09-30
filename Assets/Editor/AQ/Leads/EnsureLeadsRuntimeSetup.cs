#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// App runtime types
using AQ.App.Leads;

namespace AQ.Editor.Leads
{
    /// <summary>
    /// One-click scene bootstrap for the Leads runtime.
    /// - Ensures a LeadsRepository exists and has a LeadsDatabase assigned (creates one if missing).
    /// - Finds "LeadsBar" (with ScrollRect + Content_Leads) and ensures a LeadsBarView component is present.
    /// - Wires LeadsBarView references via SerializedObject using tolerant field-name matching.
    /// - Ensures a tiny "Leads_RuntimeGlue" dispatcher exists.
    /// This script avoids hard-coded field access, so it compiles even if your LeadsBarView uses different field names.
    /// </summary>
    public static class EnsureLeadsRuntimeSetup
    {
        [MenuItem("AQ/Leads/One-Click: Ensure Runtime Setup", priority = 0)]
        public static void EnsureAll()
        {
            var scene = EditorSceneManager.GetActiveScene();

            // ------- 1) Ensure LeadsRepository -------
            LeadsRepository repo =
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
                Object.FindFirstObjectByType<LeadsRepository>(FindObjectsInactive.Include);
#else
                Object.FindObjectOfType<LeadsRepository>();
#endif
            if (repo == null)
            {
                var go = new GameObject("LeadsRepository");
                repo = go.AddComponent<LeadsRepository>();
                Debug.Log("➕ Created LeadsRepository in scene.", go);
            }

            // ------- 2) Ensure LeadsDatabase asset + assign -------
            if (repo.database == null)
            {
                LeadsDatabase db = null;
                var guids = AssetDatabase.FindAssets("t:LeadsDatabase");
                if (guids != null && guids.Length > 0)
                {
                    db = AssetDatabase.LoadAssetAtPath<LeadsDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                else
                {
                    var path = "Assets/App/Leads/Data/LeadsDatabase.asset";
                    System.IO.Directory.CreateDirectory("Assets/App/Leads/Data");
                    db = ScriptableObject.CreateInstance<LeadsDatabase>();
                    AssetDatabase.CreateAsset(db, path);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"🆕 Created default LeadsDatabase at {path}", db);
                }

                repo.database = db;
                EditorUtility.SetDirty(repo);
            }

            // ------- 3) Ensure LeadsBarView component on "LeadsBar" GO -------
            var barGO = GameObject.Find("LeadsBar");
            if (barGO == null)
            {
                Debug.LogError("LeadsBar GameObject not found (expected object named 'LeadsBar'). Run your LeadsBar ensure tool first.");
                return;
            }

            var scroll = barGO.GetComponent<ScrollRect>();
            if (scroll == null)
            {
                Debug.LogError("LeadsBar has no ScrollRect. Please run your LeadsBar verify/repair tool.");
                return;
            }

            // Find Content_Leads (prefer under Viewport)
            RectTransform content = null;
            var viewport = barGO.transform.Find("Viewport") as RectTransform;
            if (viewport != null)
                content = viewport.Find("Content_Leads") as RectTransform;
            if (content == null)
                content = barGO.transform.Find("Content_Leads") as RectTransform; // fallback

            if (content == null)
            {
                Debug.LogError("Could not find 'Content_Leads' under LeadsBar. Please conform LeadsBar hierarchy.");
                return;
            }

            // Add LeadsBarView if missing
            var view = barGO.GetComponent<LeadsBarView>();
            if (view == null) view = barGO.AddComponent<LeadsBarView>();

            // ------- 4) Wire references by SerializedObject (name-tolerant) -------
            var so = new SerializedObject(view);

            // Helper local to set a ref if the property exists
            bool TrySetRef(string[] candidateNames, Object obj)
            {
                foreach (var name in candidateNames)
                {
                    var prop = so.FindProperty(name);
                    if (prop != null)
                    {
                        prop.objectReferenceValue = obj;
                        return true;
                    }
                }
                return false;
            }

            // Common field-name candidates used across our variants
            var contentNames   = new[] { "contentRoot", "content", "m_Content", "contentRootTransform" };
            var scrollNames    = new[] { "scrollRect", "m_ScrollRect", "scroll" };
            var cardPrefabNames= new[] { "cardPrefab", "leadCardPrefab", "card", "m_CardPrefab" };

            bool setContent = TrySetRef(contentNames, content);
            bool setScroll  = TrySetRef(scrollNames,  scroll);

            // Try find a LeadCard prefab if card field exists and is empty
            bool cardFieldExists = false;
            foreach (var n in cardPrefabNames)
                if (so.FindProperty(n) != null) { cardFieldExists = true; break; }

            if (cardFieldExists)
            {
                // Load any prefab whose main component name contains "LeadCardView"
                GameObject cardAsset = null;
                var pGuids = AssetDatabase.FindAssets("t:Prefab LeadCardView");
                if (pGuids != null && pGuids.Length > 0)
                {
                    cardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(pGuids[0]));
                }
                TrySetRef(cardPrefabNames, cardAsset);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);

            if (!setContent || !setScroll)
            {
                var msg = $"⚠️ LeadsBarView wired with partial success. content set: {setContent}, scroll set: {setScroll}. " +
                          $"If either is false, open LeadsBarView in the Inspector and assign manually (field names may differ).";
                Debug.LogWarning(msg, view);
            }
            else
            {
                Debug.Log("✅ LeadsBarView wired (content + scroll).", view);
            }

            // ------- 5) Ensure minimal runtime glue exists (idempotent) -------
            var glue = GameObject.Find("Leads_RuntimeGlue");
            if (glue == null) glue = new GameObject("Leads_RuntimeGlue");
            if (glue.GetComponent<LeadsRuntimeGlue>() == null)
                glue.AddComponent<LeadsRuntimeGlue>();
            EditorUtility.SetDirty(glue);

            // Persist scene
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("✅ Leads runtime setup complete. Repo + DB + Bar wired. Press Play, then AQ → Leads → Probe Runtime.");
        }
    }
}
#endif
