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
    public static class FixLeadPrefabs_Strong
    {
        private const string LeadCardPrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string ReqItemPrefabPath  = "Assets/UI/Prefabs/ReqItem.prefab";

        [MenuItem("AQ/UI/Fix (STRONG) → Wire LeadCardView prefab + scene & ASCII text")]
        public static void FixStrong()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before running the fixer.", "OK");
                return;
            }

            EnsureFolders("Assets/UI", "Assets/UI/Prefabs");

            // 0) Ensure ReqItem.prefab exists & valid
            var reqItem = AssetDatabase.LoadAssetAtPath<GameObject>(ReqItemPrefabPath);
            if (reqItem == null || reqItem.GetComponent<LeadRequirementItem>() == null)
            {
                reqItem = RecreateReqItemPrefab(); // safe rebuild
                if (reqItem == null) { Debug.LogError("[FixLeadPrefabs] Could not create ReqItem.prefab."); return; }
                Debug.Log("[FixLeadPrefabs] Ensured Assets/UI/Prefabs/ReqItem.prefab");
            }
            var reqItemComp = reqItem.GetComponent<LeadRequirementItem>();

            // 1) Edit the LeadCardView PREFAB via Prefab API (this guarantees write)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPrefabPath);
            if (cardPrefab == null)
            {
                Debug.LogError($"[FixLeadPrefabs] Missing {LeadCardPrefabPath}. Build it first (AQ → UI → Build Prefabs → Build LeadCardView only).");
            }
            else
            {
                var opened = PrefabUtility.LoadPrefabContents(LeadCardPrefabPath); // a temporary hierarchy
                var lpv = opened.GetComponent<LeadCardView>() ??
                          opened.GetComponentsInChildren<LeadCardView>(true).FirstOrDefault();
                if (lpv == null)
                {
                    Debug.LogError($"[FixLeadPrefabs] {LeadCardPrefabPath} has no LeadCardView component.");
                }
                else
                {
                    // requirementItemPrefab
                    var so = new SerializedObject(lpv);
                    so.FindProperty("requirementItemPrefab").objectReferenceValue = reqItemComp;

                    // requirementsRoot (child named "Requirements")
                    var reqRoot = lpv.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                    if (reqRoot != null)
                        so.FindProperty("requirementsRoot").objectReferenceValue = reqRoot;

                    so.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(opened, LeadCardPrefabPath);
                }
                PrefabUtility.UnloadPrefabContents(opened);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[FixLeadPrefabs] LeadCardView.prefab wired.");
            }

            // 2) Fix any LeadCardView instances already sitting in open scenes
            int sceneCardsFixed = 0;
            foreach (var lpv in Object.FindObjectsByType<LeadCardView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var so = new SerializedObject(lpv);
                if (so.FindProperty("requirementItemPrefab").objectReferenceValue == null)
                {
                    so.FindProperty("requirementItemPrefab").objectReferenceValue = reqItemComp;
                    sceneCardsFixed++;
                }

                var rootProp = so.FindProperty("requirementsRoot");
                if (rootProp.objectReferenceValue == null)
                {
                    var reqRoot = lpv.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Requirements");
                    if (reqRoot != null) { rootProp.objectReferenceValue = reqRoot; sceneCardsFixed++; }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(lpv);
            }

            // 3) Replace any lingering ✓ with ASCII "OK" in open scenes
            int asciiFix = 0;
            foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(tmp.text) && tmp.text.Contains("✓"))
                {
                    tmp.text = tmp.text.Replace("✓", "OK");
                    asciiFix++;
                    EditorUtility.SetDirty(tmp);
                }
            }

            // Save scenes so changes persist
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[FixLeadPrefabs] Done. Scene LeadCardView fields fixed: {sceneCardsFixed}. TMP texts normalized: {asciiFix}.");
        }

        // ---------- helpers ----------
        private static void EnsureFolders(params string[] folders)
        {
            foreach (var path in folders)
            {
                if (AssetDatabase.IsValidFolder(path)) continue;
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
        }

        private static GameObject RecreateReqItemPrefab()
        {
            var go = new GameObject("ReqItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(180, 72);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.06f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(8,8,8,8);
            h.spacing = 8;
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
