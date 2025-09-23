#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI
{
    public static class RebuildReqAndWireLeadCard_Reflect
    {
        private const string ReqItemPath  = "Assets/UI/Prefabs/ReqItem.prefab";
        private const string LeadCardPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string ExpectedTypeFullName = "AQ.App.Leads.LeadRequirementItem"; // we’ll search broadly too

        [MenuItem("AQ/UI/Fix (NUKE, Reflect) → Rebuild ReqItem & wire LeadCard")]
        public static void Run()
        {
            if (Application.isPlaying) { EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode first.", "OK"); return; }

            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/UI/Prefabs");

            // 0) Discover the LeadRequirementItem Type by reflection
            var reqType = FindTypeAnywhere(ExpectedTypeFullName) ?? FindTypeByShortName("LeadRequirementItem");
            if (reqType == null)
            {
                Debug.LogError("[AQ Reflect Fix] Could not find a MonoBehaviour type named 'LeadRequirementItem'. " +
                               "Please confirm it is defined as: `public class LeadRequirementItem : MonoBehaviour` and note its namespace.");
                return;
            }
            if (!typeof(MonoBehaviour).IsAssignableFrom(reqType))
            {
                Debug.LogError($"[AQ Reflect Fix] Found type '{reqType.FullName}', but it does not derive from MonoBehaviour.");
                return;
            }
            Debug.Log($"[AQ Reflect Fix] Using requirement type: {reqType.FullName}");

            // 1) Delete any existing ReqItem prefab, then rebuild with the reflected component type
            var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ReqItemPath);
            if (existing) AssetDatabase.DeleteAsset(ReqItemPath);

            var reqPrefab = CreateReqItemPrefabWithType(reqType);
            if (!reqPrefab)
            {
                Debug.LogError("[AQ Reflect Fix] Failed to create ReqItem.prefab");
                return;
            }
            var hasComp = reqPrefab.GetComponent(reqType) != null;
            Debug.Log($"[AQ Reflect Fix] ReqItem.prefab component present: {hasComp}");

            // 2) Wire LeadCardView.prefab → requirementItemPrefab (and requirementsRoot if null)
            var leadCard = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPath);
            if (!leadCard)
            {
                Debug.LogError($"[AQ Reflect Fix] Missing {LeadCardPath}. Build that prefab first.");
                return;
            }

            var opened = PrefabUtility.LoadPrefabContents(LeadCardPath);
            var lpv = opened.GetComponentsInChildren<Component>(true).FirstOrDefault(c => c.GetType().Name == "LeadCardView");
            if (!lpv)
            {
                Debug.LogError($"[AQ Reflect Fix] LeadCardView component not found inside {LeadCardPath}.");
                PrefabUtility.UnloadPrefabContents(opened);
                return;
            }

            var so = new SerializedObject(lpv);

            var reqField = so.FindProperty("requirementItemPrefab");
            if (reqField == null)
            {
                Debug.LogError("[AQ Reflect Fix] Field 'requirementItemPrefab' not found on LeadCardView. " +
                               "Open LeadCardView.cs and confirm the serialized field name.");
                PrefabUtility.UnloadPrefabContents(opened);
                return;
            }
            // assign the Component from the prefab (component reference, not GameObject)
            var reqCompOnPrefab = reqPrefab.GetComponent(reqType);
            if (reqCompOnPrefab == null)
            {
                Debug.LogError("[AQ Reflect Fix] ReqItem.prefab exists but still has no requirement component. Aborting.");
                PrefabUtility.UnloadPrefabContents(opened);
                return;
            }
            reqField.objectReferenceValue = reqCompOnPrefab;

            // requirementsRoot (if null, auto-find a child named "Requirements")
            var reqRootProp = so.FindProperty("requirementsRoot");
            if (reqRootProp != null && reqRootProp.objectReferenceValue == null)
            {
                var tf = opened.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                if (tf != null) reqRootProp.objectReferenceValue = tf;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(opened, LeadCardPath);
            PrefabUtility.UnloadPrefabContents(opened);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AQ Reflect Fix] Wired requirementItemPrefab on LeadCardView.prefab");

            // 3) Patch any open-scene LeadCardView instances and normalize ✓ → OK in open scenes
            int sceneFixed = 0, ascii = 0;
            foreach (var comp in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (comp.GetType().Name != "LeadCardView") continue;
                var soi = new SerializedObject(comp);
                var rp = soi.FindProperty("requirementItemPrefab");
                if (rp != null && rp.objectReferenceValue == null)
                {
                    rp.objectReferenceValue = reqCompOnPrefab;
                    soi.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(comp);
                    sceneFixed++;
                }
                var rr = soi.FindProperty("requirementsRoot");
                if (rr != null && rr.objectReferenceValue == null)
                {
                    var tf = comp.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                    if (tf != null)
                    {
                        rr.objectReferenceValue = tf;
                        soi.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(comp);
                        sceneFixed++;
                    }
                }
            }
            foreach (var tmp in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    ascii++; EditorUtility.SetDirty(tmp);
                }
            }
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[AQ Reflect Fix] Done. Scene instances fixed: {sceneFixed}. ASCII replacements: {ascii}.");
        }

        // ---------- helpers ----------

        private static Type FindTypeAnywhere(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, false);
                    if (t != null) return t;
                }
                catch { /* ignore */ }
            }
            return null;
        }

        private static Type FindTypeByShortName(string shortName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetTypes().FirstOrDefault(x => x.Name == shortName);
                    if (t != null) return t;
                }
                catch { /* ignore */ }
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static GameObject CreateReqItemPrefabWithType(Type reqType)
        {
            // Build chip: [Icon 64] [Label TMP 24] [Check 24]
            var go = new GameObject("ReqItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(180, 72);
            var bg = go.GetComponent<Image>(); bg.color = new Color(1,1,1,0.06f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(8,8,8,8); h.spacing = 8;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            var icon  = NewImage("Icon", go.transform, new Vector2(64,64));
            var label = NewTMP("Label", go.transform, "Item", 24, TextAlignmentOptions.MidlineLeft);
            var check = NewImage("Check", go.transform, new Vector2(24,24)); check.enabled = false;

            // Add the requirement component by Type (reflection-safe)
            var comp = go.AddComponent(reqType);
            // Wire its serialized fields if their names are 'icon', 'label', 'checkmark'
            var so = new SerializedObject(comp);
            var pIcon = so.FindProperty("icon");     if (pIcon != null)     pIcon.objectReferenceValue = icon;
            var pLabel = so.FindProperty("label");   if (pLabel != null)    pLabel.objectReferenceValue = label;
            var pCheck = so.FindProperty("checkmark"); if (pCheck != null)  pCheck.objectReferenceValue = check;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ReqItemPath);
            UnityEngine.Object.DestroyImmediate(go);
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
