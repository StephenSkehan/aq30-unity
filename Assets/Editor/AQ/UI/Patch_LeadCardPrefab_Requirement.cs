#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using AQ.App.Leads; // LeadCardView, LeadRequirementItem

namespace AQ.EditorTools.UI
{
    public static class Patch_LeadCardPrefab_Requirement
    {
        private const string LeadCardPrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string ReqItemPrefabPath  = "Assets/UI/Prefabs/ReqItem.prefab";

        [MenuItem("AQ/UI/Patch → Wire requirementItemPrefab on LeadCardView.prefab")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before running the patch.", "OK");
                return;
            }

            // Ensure the ReqItem prefab exists and has the right component
            var reqGo = AssetDatabase.LoadAssetAtPath<GameObject>(ReqItemPrefabPath);
            if (!reqGo)
            {
                // create a minimal req item if it's missing
                reqGo = CreateMinimalReqItemPrefab();
                Debug.Log("[LeadCard Patch] Created missing ReqItem.prefab");
            }
            var reqComp = reqGo.GetComponent<LeadRequirementItem>();
            if (!reqComp)
            {
                Debug.LogError($"[LeadCard Patch] {ReqItemPrefabPath} exists but has no LeadRequirementItem component.");
                return;
            }

            // Open LeadCardView prefab, set the field, save
            var root = PrefabUtility.LoadPrefabContents(LeadCardPrefabPath);
            if (!root)
            {
                Debug.LogError($"[LeadCard Patch] Missing prefab at {LeadCardPrefabPath}. Build it first.");
                return;
            }

            var lpv = root.GetComponent<LeadCardView>() ?? root.GetComponentsInChildren<LeadCardView>(true).FirstOrDefault();
            if (!lpv)
            {
                Debug.LogError($"[LeadCard Patch] LeadCardView component not found inside {LeadCardPrefabPath}.");
                PrefabUtility.UnloadPrefabContents(root);
                return;
            }

            var so = new SerializedObject(lpv);
            so.FindProperty("requirementItemPrefab").objectReferenceValue = reqComp;

            // sanity: auto-find Requirements container if not set
            var rootProp = so.FindProperty("requirementsRoot");
            if (rootProp != null && rootProp.objectReferenceValue == null)
            {
                var rr = lpv.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                if (rr) rootProp.objectReferenceValue = rr;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, LeadCardPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Patch any open-scene instances as well (nice to have)
            int fixedCount = 0;
            foreach (var inst in Object.FindObjectsByType<LeadCardView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var soi = new SerializedObject(inst);
                if (soi.FindProperty("requirementItemPrefab").objectReferenceValue == null)
                {
                    soi.FindProperty("requirementItemPrefab").objectReferenceValue = reqComp;
                    soi.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(inst);
                    fixedCount++;
                }
            }

            // ASCII normalize lingering ✓ in open scenes
            int ascii = 0;
            foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    ascii++;
                    EditorUtility.SetDirty(tmp);
                }
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[LeadCard Patch] requirementItemPrefab wired on prefab. Scene instances fixed: {fixedCount}. ASCII fixes: {ascii}.");
        }

        private static GameObject CreateMinimalReqItemPrefab()
        {
            var go = new GameObject("ReqItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(180, 72);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.06f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(8,8,8,8); h.spacing = 8;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            var icon = NewImage("Icon", go.transform, new Vector2(64,64));
            var label = NewTMP("Label", go.transform, "Item", 24, TextAlignmentOptions.MidlineLeft);
            var check = NewImage("Check", go.transform, new Vector2(24,24)); check.enabled = false;

            var comp = go.AddComponent<LeadRequirementItem>();
            var so = new SerializedObject(comp);
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("checkmark").objectReferenceValue = check;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Ensure folder
            if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
            if (!AssetDatabase.IsValidFolder("Assets/UI/Prefabs")) AssetDatabase.CreateFolder("Assets/UI", "Prefabs");

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ReqItemPrefabPath);
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
