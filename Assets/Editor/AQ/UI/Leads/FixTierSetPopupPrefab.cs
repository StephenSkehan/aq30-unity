#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Repairs the prefab asset at Assets/UI/Prefabs/TierSetPopup.prefab:
    /// - Remove missing MonoBehaviours (deep)
    /// - Ensure IconGrid (RectTransform + GridLayoutGroup)
    /// - Ensure Icon1..Icon6 (Image) and a disabled "Highlight" (Image)
    /// Idempotent and surgical; touches only that prefab.
    /// </summary>
    public static class FixTierSetPopupPrefab
    {
        private const string DefaultPath = "Assets/UI/Prefabs/TierSetPopup.prefab";

        [MenuItem("AQ/UI/Leads/Fix TierSetPopup Prefab (Remove Missing + Grid & Icons)")]
        public static void Run()
        {
            // Resolve prefab path
            string path = AssetDatabase.AssetPathToGUID(DefaultPath) != string.Empty
                ? DefaultPath
                : ResolveFirstPrefabPath();

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[AQ FixPopupPrefab] No TierSetPopup prefab found.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                Debug.LogWarning("[AQ FixPopupPrefab] Could not load prefab at: " + path);
                return;
            }

            // 1) Remove missing scripts (deep)
            int removed = 0;
            RemoveMissingDeep(root, ref removed);

            // 2) Ensure IconGrid is a RectTransform with GridLayoutGroup
            var t = root.transform;
            Transform gridTr = t.Find("IconGrid");
            if (gridTr != null && !(gridTr is RectTransform))
            {
                gridTr.gameObject.name = "IconGrid (legacy)";
                gridTr = null;
            }
            if (gridTr == null)
            {
                GameObject gridGO = new GameObject("IconGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                gridTr = gridGO.transform;
                gridTr.SetParent(t, false);

                var rt = (RectTransform)gridTr;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(480f, 120f);
                rt.anchoredPosition = new Vector2(0f, 10f);

                var layout = gridGO.GetComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(64f, 64f);
                layout.spacing = new Vector2(12f, 12f);
                layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                layout.constraintCount = 1;
            }
            else
            {
                var layout = gridTr.GetComponent<GridLayoutGroup>();
                if (layout == null) layout = gridTr.gameObject.AddComponent<GridLayoutGroup>();
                if (layout.cellSize == Vector2.zero) layout.cellSize = new Vector2(64f, 64f);
            }

            // 3) Ensure Icon1..Icon6 exist
            int iconsFound = 0;
            foreach (Transform ch in gridTr) if (ch.name.StartsWith("Icon")) iconsFound++;
            if (iconsFound == 0)
            {
                for (int i = 1; i <= 6; i++)
                {
                    GameObject iconGO = new GameObject("Icon" + i, typeof(RectTransform), typeof(Image));
                    var rt = iconGO.GetComponent<RectTransform>();
                    rt.SetParent(gridTr, false);
                    rt.sizeDelta = new Vector2(64f, 64f);
                    var img = iconGO.GetComponent<Image>();
                    img.raycastTarget = false;
                    img.enabled = true;
                }
            }

            // 4) Ensure Highlight exists (disabled)
            if (gridTr.Find("Highlight") == null)
            {
                GameObject hlGO = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
                var rt = hlGO.GetComponent<RectTransform>();
                rt.SetParent(gridTr, false);
                rt.sizeDelta = new Vector2(72f, 72f);
                var img = hlGO.GetComponent<Image>();
                img.raycastTarget = false;
                img.enabled = false;
            }

            // Save prefab and exit
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();

            Debug.Log("[AQ FixPopupPrefab] Repaired prefab: " + path + " | Missing scripts removed: " + removed);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static string ResolveFirstPrefabPath()
        {
            string[] guids = AssetDatabase.FindAssets("TierSetPopup t:prefab");
            if (guids == null || guids.Length == 0) return null;
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        private static void RemoveMissingDeep(GameObject go, ref int removed)
        {
            // Remove on this object
            int before = CountMissing(go);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            int after = CountMissing(go);
            removed += (before - after);

            // Recurse
            foreach (Transform ch in go.transform)
                RemoveMissingDeep(ch.gameObject, ref removed);
        }

        private static int CountMissing(GameObject go)
        {
            int count = 0;
            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null) count++;
            }
            return count;
        }
    }
}
#endif
