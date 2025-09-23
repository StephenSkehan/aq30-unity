#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Content
{
    public static class OverlayTidy_NoLayout
    {
        [MenuItem("AQ/Content/Overlay/Repair Overlay Layout (No LayoutGroup)")]
        public static void Repair()
        {
            // 1) Find overlay
            var root = GameObject.Find("ResolutionRoot");
            if (!root) { Debug.LogWarning("Overlay repair: ResolutionRoot not found."); return; }
            var panelT = root.transform.Find("ResolutionPanel");
            if (!panelT) panelT = root.transform; // fallback to root
            var panel = panelT as RectTransform;

            // 2) Make sure we’re visible and on top
            var cg = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
            cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true;
            root.transform.SetAsLastSibling();

            // 3) Strip layout components that fight manual anchors
            RemoveIfExists<VerticalLayoutGroup>(panel.gameObject);
            RemoveIfExists<HorizontalLayoutGroup>(panel.gameObject);
            RemoveIfExists<ContentSizeFitter>(panel.gameObject);
            RemoveIfExists<Mask>(panel.gameObject);
            RemoveIfExists<RectMask2D>(panel.gameObject);

            // 4) Apply safe rect recipe (TOP-STRETCH math using offsets)
            // ResolutionPanel: Top-stretch, Top = 200, Left/Right = 64, Height ≈ 560.
            ApplyTopStretch(panel, left:64, right:64, top:200, height:560);

            // Children (cast to RectTransform where found)
            ApplyTopStretch(FindRT(panel, "TitleText"), left:32, right:32, top:32,  height:64);
            ApplyTopStretch(FindRT(panel, "BodyText"),  left:32, right:32, top:120, height:96);
            ApplyTopStretch(FindRT(panel, "Quest_0"),   left:32, right:32, top:200, height:44);
            ApplyTopStretch(FindRT(panel, "Quest_1"),   left:32, right:32, top:248, height:44);
            ApplyTopStretch(FindRT(panel, "Quest_2"),   left:32, right:32, top:296, height:44);

            // Button: top-center within panel, about 140px below body bottom (≈ -260px from panel top)
            var btn = FindRT(panel, "ResolveButton");
            if (btn)
            {
                SetAnchors(btn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
                btn.sizeDelta = new Vector2(420, 74);
                btn.anchoredPosition = new Vector2(0, -260f);
            }

            // 5) Nudge colors (optional, dark readable palette)
            var img = panel.GetComponent<Image>();
            if (img) img.color = new Color32(0x23,0x1A,0x2C, (byte)(0.90f*255)); // #231A2C @ 90%

            // 6) Ensure labels aren’t fully transparent
            var texts = panel.GetComponentsInChildren<Component>(true)
                             .Where(c => c && (c.GetType().Name=="Text" || c.GetType().Name=="TextMeshProUGUI"));
            foreach (var t in texts)
            {
                var type = t.GetType();
                var colorProp = type.GetProperty("color");
                if (colorProp != null)
                {
                    var col = (Color)colorProp.GetValue(t,null);
                    if (col.a < 0.98f) { col.a = 1f; colorProp.SetValue(t,col,null); }
                }
            }

            Debug.Log("Overlay repair complete: removed layout components, re-anchored, ensured visibility.");
        }

        // ---------- helpers ----------

        static RectTransform FindRT(Transform parent, string name)
        {
            if (!parent) return null;
            foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                if (t && t.name == name) return t as RectTransform;
            return null;
        }

        static void RemoveIfExists<T>(GameObject go) where T:Component
        {
            var c = go.GetComponent<T>();
            if (c) Object.DestroyImmediate(c);
        }

        /// <summary>
        /// Apply TOP-STRETCH anchor preset and set offsets for left/right/top/height.
        /// For top-stretch (min=(0,1), max=(1,1), pivot=(0.5,1)):
        ///   offsetMin.x = left
        ///   offsetMax.x = -right
        ///   offsetMax.y = -top
        ///   offsetMin.y = -(top + height)
        /// </summary>
        static void ApplyTopStretch(RectTransform rt, float left, float right, float top, float height)
        {
            if (!rt) return;
            SetAnchors(rt, new Vector2(0f,1f), new Vector2(1f,1f), new Vector2(0.5f,1f));
            var offMin = rt.offsetMin;
            var offMax = rt.offsetMax;
            offMin.x = left;
            offMax.x = -right;
            offMax.y = -top;
            offMin.y = -(top + height);
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
        }

        static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot     = pivot;
        }
    }
}
#endif
