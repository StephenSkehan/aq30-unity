#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.Leads; // LeadCardView, LeadRequirementItem

namespace AQ.EditorTools.UI
{
    public static class RebuildReqAndWireLeadCard
    {
        private const string ReqItemPath  = "Assets/UI/Prefabs/ReqItem.prefab";
        private const string LeadCardPath = "Assets/UI/Prefabs/LeadCardView.prefab";

        [MenuItem("AQ/UI/Fix (NUKE) → Rebuild ReqItem.prefab and wire LeadCardView.prefab")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before running the fixer.", "OK");
                return;
            }

            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/UI/Prefabs");

            // 1) Delete any existing/broken ReqItem prefab
            if (AssetDatabase.LoadAssetAtPath<Object>(ReqItemPath) != null)
            {
                AssetDatabase.DeleteAsset(ReqItemPath);
            }

            // 2) Recreate ReqItem.prefab (with LeadRequirementItem wired)
            var reqPrefab = CreateReqItemPrefab();
            if (!reqPrefab)
            {
                Debug.LogError("[AQ Fix] Failed to create ReqItem.prefab");
                return;
            }
            var reqComp = reqPrefab.GetComponent<LeadRequirementItem>();
            if (!reqComp)
            {
                Debug.LogError("[AQ Fix] ReqItem.prefab did not get LeadRequirementItem component.");
                return;
            }
            Debug.Log("[AQ Fix] Rebuilt ReqItem.prefab");

            // 3) Wire LeadCardView.prefab -> requirementItemPrefab (and requirementsRoot if missing)
            var leadCard = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPath);
            if (!leadCard)
            {
                Debug.LogError($"[AQ Fix] Missing {LeadCardPath}. Build that prefab first.");
                return;
            }

            var opened = PrefabUtility.LoadPrefabContents(LeadCardPath); // temp instance
            var lpv = opened.GetComponent<LeadCardView>() ?? opened.GetComponentsInChildren<LeadCardView>(true).FirstOrDefault();
            if (!lpv)
            {
                Debug.LogError($"[AQ Fix] LeadCardView component not found inside {LeadCardPath}.");
                PrefabUtility.UnloadPrefabContents(opened);
                return;
            }

            var so = new SerializedObject(lpv);
            so.FindProperty("requirementItemPrefab").objectReferenceValue = reqComp;

            var reqRootProp = so.FindProperty("requirementsRoot");
            if (reqRootProp != null && reqRootProp.objectReferenceValue == null)
            {
                var rr = lpv.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                if (rr != null) reqRootProp.objectReferenceValue = rr;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(opened, LeadCardPath);
            PrefabUtility.UnloadPrefabContents(opened);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AQ Fix] Wired requirementItemPrefab on LeadCardView.prefab");

            // 4) Patch any open-scene instances & normalize ASCII (✓ → OK)
            int sceneFixed = 0, ascii = 0;
            foreach (var inst in Object.FindObjectsByType<LeadCardView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var soi = new SerializedObject(inst);
                if (soi.FindProperty("requirementItemPrefab").objectReferenceValue == null)
                {
                    soi.FindProperty("requirementItemPrefab").objectReferenceValue = reqComp;
                    soi.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(inst);
                    sceneFixed++;
                }
                var rrProp = soi.FindProperty("requirementsRoot");
                if (rrProp != null && rrProp.objectReferenceValue == null)
                {
                    var rr = inst.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                    if (rr != null)
                    {
                        rrProp.objectReferenceValue = rr;
                        soi.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(inst);
                        sceneFixed++;
                    }
                }
            }
            foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    ascii++; EditorUtility.SetDirty(tmp);
                }
            }
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[AQ Fix] Done. Scene instances fixed: {sceneFixed}. ASCII replacements: {ascii}.");
        }

        // --- helpers ---
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static GameObject CreateReqItemPrefab()
        {
            var go = new GameObject("ReqItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(180, 72);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.06f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(8,8,8,8); h.spacing = 8;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            var icon  = NewImage("Icon", go.transform, new Vector2(64,64));
            var label = NewTMP("Label", go.transform, "Item", 24, TextAlignmentOptions.MidlineLeft);
            var check = NewImage("Check", go.transform, new Vector2(24,24)); check.enabled = false;

            var comp = go.AddComponent<LeadRequirementItem>();
            var so = new SerializedObject(comp);
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("checkmark").objectReferenceValue = check;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ReqItemPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Image NewImage(string name, Transform parent, Vector2 size)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(parent, false);
            g.GetComponent<RectTransform>().sizeDelta = size;
            return g.GetComponent<Image>();
        }
        private static TextMeshProUGUI NewTMP(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            g.transform.SetParent(parent, false);
            var t = g.GetComponent<TextMeshProUGUI>(); t.text = text; t.fontSize = size; t.alignment = align;
            return t;
        }
    }
}
#endif
