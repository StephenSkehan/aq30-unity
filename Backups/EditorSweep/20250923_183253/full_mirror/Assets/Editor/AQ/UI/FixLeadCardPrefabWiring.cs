#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

using AQ.App.Leads; // LeadCardView, LeadRequirementItem

namespace AQ.EditorTools.UI
{
    public static class FixLeadCardPrefabWiring
    {
        private const string LeadCardPrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string ReqItemPrefabPath  = "Assets/UI/Prefabs/ReqItem.prefab";

        [MenuItem("AQ/UI/Fix → LeadCard requirement wiring + rebuild ReqItem + ASCII text")]
        public static void FixAll()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before running the fixer.", "OK");
                return;
            }

            EnsureFolders("Assets/UI", "Assets/UI/Prefabs");

            // 1) Ensure ReqItem.prefab exists and has LeadRequirementItem
            var reqItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ReqItemPrefabPath);
            bool recreateReqItem = false;

            if (reqItemPrefab == null) recreateReqItem = true;
            else
            {
                var comp = reqItemPrefab.GetComponent<LeadRequirementItem>();
                if (comp == null) recreateReqItem = true;
            }

            if (recreateReqItem)
            {
                reqItemPrefab = RecreateReqItemPrefab();
                if (reqItemPrefab == null)
                {
                    Debug.LogError("[FixLeadCard] Failed to create ReqItem.prefab.");
                    return;
                }
                else
                {
                    Debug.Log("[FixLeadCard] Rebuilt Assets/UI/Prefabs/ReqItem.prefab");
                }
            }

            // 2) Fix LeadCardView.prefab wiring
            var leadCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPrefabPath);
            if (leadCardPrefab == null)
            {
                Debug.LogError($"[FixLeadCard] Missing prefab: {LeadCardPrefabPath}. Build it first (AQ → UI → Build Prefabs → Build LeadCardView only).");
            }
            else
            {
                var lpv = leadCardPrefab.GetComponent<LeadCardView>();
                if (lpv == null)
                {
                    Debug.LogError($"[FixLeadCard] {LeadCardPrefabPath} has no LeadCardView component.");
                }
                else
                {
                    var so = new SerializedObject(lpv);

                    // requirementItemPrefab
                    var reqPrefabProp = so.FindProperty("requirementItemPrefab");
                    var reqItemComp = reqItemPrefab.GetComponent<LeadRequirementItem>();
                    if (reqPrefabProp != null && reqItemComp != null)
                        reqPrefabProp.objectReferenceValue = reqItemComp;

                    // requirementsRoot (auto-find child named "Requirements")
                    var rootProp = so.FindProperty("requirementsRoot");
                    if (rootProp != null && (rootProp.objectReferenceValue == null))
                    {
                        var reqRoot = leadCardPrefab.GetComponentsInChildren<Transform>(true)
                                                    .FirstOrDefault(t => t.name == "Requirements");
                        if (reqRoot != null) rootProp.objectReferenceValue = reqRoot;
                    }

                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(lpv);
                }
            }

            // 3) Fix any scene instances
            int sceneCardsFixed = 0;
            foreach (var lpv in Object.FindObjectsByType<LeadCardView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var so = new SerializedObject(lpv);

                var reqPrefabProp = so.FindProperty("requirementItemPrefab");
                var reqItemComp = reqItemPrefab.GetComponent<LeadRequirementItem>();
                if (reqPrefabProp != null && reqItemComp != null && reqPrefabProp.objectReferenceValue == null)
                {
                    reqPrefabProp.objectReferenceValue = reqItemComp;
                    sceneCardsFixed++;
                }

                var rootProp = so.FindProperty("requirementsRoot");
                if (rootProp != null && (rootProp.objectReferenceValue == null))
                {
                    var reqRoot = lpv.GetComponentsInChildren<Transform>(true)
                                     .FirstOrDefault(t => t.name == "Requirements");
                    if (reqRoot != null)
                    {
                        rootProp.objectReferenceValue = reqRoot;
                        sceneCardsFixed++;
                    }
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(lpv);
            }

            // 4) Normalize TMP texts in open scenes (✓ → OK)
            int sceneTextsFixed = 0;
            foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    sceneTextsFixed++;
                    EditorUtility.SetDirty(tmp);
                }
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[FixLeadCard] Done. Scene LeadCardView fields fixed: {sceneCardsFixed}. TMP texts normalized: {sceneTextsFixed}.");
        }

        // ---------- helpers ----------

        private static void EnsureFolders(params string[] folders)
        {
            for (int i = 0; i < folders.Length; i++)
            {
                var path = folders[i];
                if (string.IsNullOrEmpty(path)) continue;
                var parts = path.Split('/');
                string cur = parts[0];
                for (int p = 1; p < parts.Length; p++)
                {
                    var next = cur + "/" + parts[p];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(cur, parts[p]);
                    cur = next;
                }
            }
        }

        private static GameObject RecreateReqItemPrefab()
        {
            // Build a simple horizontal chip: Icon (64), Label (TMP 24), Check (24)
            var go = new GameObject("ReqItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 72);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.02f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(8,8,8,8);
            h.spacing = 8;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            var icon = CreateImage("Icon", go.transform, new Vector2(64,64));
            var label = CreateTMP("Label", go.transform, "Item", 24, TextAlignmentOptions.MidlineLeft);
            label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 28;
            var check = CreateImage("Check", go.transform, new Vector2(24,24));
            check.enabled = false;

            var comp = go.AddComponent<LeadRequirementItem>();
            // Wire serialized fields
            var so = new SerializedObject(comp);
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("checkmark").objectReferenceValue = check;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ReqItemPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = size;
            return go.GetComponent<Image>();
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.alignment = align;
            return tmp;
        }
    }
}
#endif
