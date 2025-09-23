// Copyright Indigo Chimp
// Editor-only helper to apply safe padding/spacing on LeadCardView prefab.
// Menu: AQ → UI → Polish → Lead Cards (Safe Defaults)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI
{
    public static class PolishLeadCards
    {
        private const string LeadCardPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string RequirementsName = "Requirements";
        private const float CardPadding = 16f;
        private const float TitleMinSize = 24f;
        private const float OneLinerMinSize = 18f;
        private const float ReqItemSpacing = 8f;

        [MenuItem("AQ/UI/Polish/Lead Cards (Safe Defaults)")]
        public static void Run()
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPath);
            if (!go) { Debug.LogWarning($"[AQ Polish] Missing prefab at {LeadCardPath}"); return; }

            var root = PrefabUtility.LoadPrefabContents(LeadCardPath);
            int changes = 0;

            // 1) Add/normalize a LayoutElement on root to allow flexible height
            var le = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            if (!le.flexibleHeight.Equals(1f)) { le.flexibleHeight = 1f; changes++; }

            // 2) Ensure a top-level VerticalLayoutGroup for safe padding/spacing (non-destructive)
            var vlg = root.GetComponent<VerticalLayoutGroup>() ?? root.AddComponent<VerticalLayoutGroup>();
            if (vlg.padding.left != CardPadding || vlg.padding.right != CardPadding ||
                vlg.padding.top != CardPadding || vlg.padding.bottom != CardPadding ||
                vlg.spacing != 10f)
            {
                vlg.padding = new RectOffset((int)CardPadding, (int)CardPadding, (int)CardPadding, (int)CardPadding);
                vlg.spacing = 10f;
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
                changes++;
            }

            // 3) Requirements panel enforcement: VerticalLayoutGroup only
            var req = FindChild(root.transform, RequirementsName);
            if (req)
            {
                // remove Grid/Horizontal if present
                var grid = req.GetComponent<GridLayoutGroup>();
                var hlg = req.GetComponent<HorizontalLayoutGroup>();
                if (grid) { Object.DestroyImmediate(grid, true); changes++; }
                if (hlg) { Object.DestroyImmediate(hlg, true); changes++; }

                var reqVlg = req.GetComponent<VerticalLayoutGroup>() ?? req.gameObject.AddComponent<VerticalLayoutGroup>();
                reqVlg.spacing = ReqItemSpacing;
                reqVlg.childControlHeight = true;
                reqVlg.childForceExpandHeight = false;

                var csf = req.GetComponent<ContentSizeFitter>() ?? req.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            // 4) Gentle minimum font sizes on common text fields (if present)
            BumpMinSize(root.transform, "Title", TitleMinSize);
            BumpMinSize(root.transform, "OneLiner", OneLinerMinSize);

            if (changes > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, LeadCardPath);
                Debug.Log($"[AQ Polish] LeadCardView updated ({changes} structural tweaks).");
            }
            else
            {
                Debug.Log("[AQ Polish] LeadCardView already matches safe defaults.");
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Transform FindChild(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var c = FindChild(t.GetChild(i), name);
                if (c) return c;
            }
            return null;
        }

        private static void BumpMinSize(Transform root, string name, float minSize)
        {
            var target = FindChild(root, name);
#if TMP_PRESENT || UNITY_TEXTMESHPRO
            if (target)
            {
                var tmp = target.GetComponent<TMPro.TMP_Text>();
                if (tmp && tmp.fontSize < minSize) tmp.fontSize = minSize;
            }
#else
            if (target)
            {
                var txt = target.GetComponent<UnityEngine.UI.Text>();
                if (txt && txt.fontSize < minSize) txt.fontSize = (int)minSize;
            }
#endif
        }
    }
}
#endif
