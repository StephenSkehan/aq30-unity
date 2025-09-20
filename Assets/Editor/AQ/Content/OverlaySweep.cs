#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
#if TMP_PRESENT
using TMPro;
#endif

namespace AQ.EditorTools.Content
{
    public static class OverlaySweep
    {
        // ===== MENUS =====

        [MenuItem("AQ/Content/Overlay: Sweep → Report Text Nodes")]
        public static void ReportTexts()
        {
            var root = FindOverlayRoot();
            if (!root) { Debug.LogWarning("[OverlaySweep] ResolutionRoot not found (searched inactive too)."); return; }

            var sb = new StringBuilder();
            sb.AppendLine("[OverlaySweep] Text nodes under ResolutionRoot:");
            foreach (var t in Snapshot(root.transform))
            {
                if (!t) continue;
                var rt = t as RectTransform;
                if (IsTextish(t.gameObject))
                {
                    var rect = rt ? rt.rect : new Rect();
                    sb.AppendLine($"  • {GetPath(t)}  rect=(w={rect.width:F0}, h={rect.height:F0})  anchors=({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})→({rt.anchorMax.x:F2},{rt.anchorMax.y:F2})");
                }
            }
            Debug.Log(sb.ToString());
        }

        // Remove text that lives under ResolutionRoot but NOT under ResolutionPanel
        [MenuItem("AQ/Content/Overlay: Sweep → Remove Stray Text (outside panel)")]
        public static void RemoveStraysOutsidePanel()
        {
            var root = FindOverlayRoot();
            if (!root) { Debug.LogWarning("[OverlaySweep] ResolutionRoot not found."); return; }
            var panel = root.transform.Find("ResolutionPanel");
            if (!panel) { Debug.LogWarning("[OverlaySweep] ResolutionPanel not found under ResolutionRoot."); return; }

            Undo.RegisterFullObjectHierarchyUndo(root, "Overlay Remove Strays Outside");

            int removed = 0;
            foreach (var t in Snapshot(root.transform))
            {
                if (!t) continue;
                if (IsTextish(t.gameObject) && !IsChildOf(t, panel))
                {
                    Debug.Log($"[OverlaySweep] Removed stray text: {GetPath(t)}");
                    Object.DestroyImmediate(t.gameObject, true);
                    removed++;
                }
            }
            Debug.Log($"[OverlaySweep] Done. Removed {removed} stray text object(s) outside panel.");
        }

        // Remove legacy/duplicate text under the panel that isn't one of our expected nodes
        [MenuItem("AQ/Content/Overlay: Sweep → Cull Legacy Panel Text (Title/Body/QuestList)")]
        public static void CullLegacyPanelText()
        {
            var root = FindOverlayRoot();
            if (!root) { Debug.LogWarning("[OverlaySweep] ResolutionRoot not found."); return; }
            var panel = root.transform.Find("ResolutionPanel");
            if (!panel) { Debug.LogWarning("[OverlaySweep] ResolutionPanel not found."); return; }

            Undo.RegisterFullObjectHierarchyUndo(root, "Overlay Cull Legacy Panel Text");

            bool Allowed(Transform t)
            {
                // Keep only these:
                if (t.name == "TitleText") return true;
                if (t.name == "BodyText") return true;
                if (t.name == "Quest_0" || t.name == "Quest_1" || t.name == "Quest_2") return true;
                if (t.name == "Text" && t.parent && t.parent.name == "ResolveButton") return true;
                return false;
            }

            int removed = 0;
            foreach (var t in Snapshot(panel))
            {
                if (!t) continue;

                // If it's a whole container we know is legacy, nuke it entirely.
                if (t.name == "QuestList" || t.name == "Title" || t.name == "Body")
                {
                    Debug.Log($"[OverlaySweep] Removed legacy container: {GetPath(t)}");
                    Object.DestroyImmediate(t.gameObject, true);
                    removed++;
                    continue;
                }

                // Delete any text components under panel that are not allowed.
                if (IsTextish(t.gameObject) && !Allowed(t))
                {
                    Debug.Log($"[OverlaySweep] Removed legacy/duplicate text: {GetPath(t)}");
                    Object.DestroyImmediate(t.gameObject, true);
                    removed++;
                }
            }

            Debug.Log($"[OverlaySweep] Done. Removed {removed} legacy text object(s) under panel.");
        }

        // Heuristic kill for the “picket-fence” vertical line
        [MenuItem("AQ/Content/Overlay: Sweep → Kill Centerline Skinny Text")]
        public static void KillCenterlineSkinny()
        {
            var root = FindOverlayRoot();
            if (!root) { Debug.LogWarning("[OverlaySweep] ResolutionRoot not found."); return; }

            Undo.RegisterFullObjectHierarchyUndo(root, "Overlay Kill Centerline Skinny");

            int removed = 0;
            foreach (var t in Snapshot(root.transform))
            {
                if (!t) continue;
                if (!IsTextish(t.gameObject)) continue;
                var rt = t as RectTransform;
                if (!rt) continue;

                bool skinny = rt.rect.width > 0 && rt.rect.width <= 160f;
                bool centered =
                    Mathf.Abs(rt.anchorMin.x - 0.5f) < 0.01f &&
                    Mathf.Abs(rt.anchorMax.x - 0.5f) < 0.01f;

                if (skinny && centered)
                {
                    Debug.Log($"[OverlaySweep] Removed skinny centered text: {GetPath(t)} (w={rt.rect.width:F0})");
                    Object.DestroyImmediate(t.gameObject, true);
                    removed++;
                }
            }
            Debug.Log($"[OverlaySweep] Done. Removed {removed} skinny centered text object(s).");
        }

        [MenuItem("AQ/Content/Overlay: Sweep → Apply Safe Layout")]
        public static void ApplySafeLayout()
        {
            var root = FindOverlayRoot();
            if (!root) { Debug.LogWarning("[OverlaySweep] ResolutionRoot not found."); return; }
            var panel = root.transform.Find("ResolutionPanel") as RectTransform;
            if (!panel) { Debug.LogWarning("[OverlaySweep] ResolutionPanel not found."); return; }

            Undo.RegisterFullObjectHierarchyUndo(root, "Overlay Safe Layout");

            // Root covers screen
            var rootRT = root.GetComponent<RectTransform>() ?? root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one; rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;

            // Panel with margins
            panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.pivot = new Vector2(0.5f, 0.5f);
            panel.offsetMin = new Vector2(48, 420);
            panel.offsetMax = new Vector2(-48, -540);

            // Button lower in panel
            var btn = panel.Find("ResolveButton") as RectTransform;
            if (btn)
            {
                btn.anchorMin = btn.anchorMax = new Vector2(0.5f, 1f);
                btn.pivot = new Vector2(0.5f, 1f);
                const float top = 200f; const float w = 420f; const float h = 74f;
                btn.offsetMin = new Vector2(-w * 0.5f, -top - h);
                btn.offsetMax = new Vector2(+w * 0.5f, -top);
            }

            // Title / body / quests
            LayoutText(panel, "TitleText",  top: 36,  height: 80, size: 48, anchorTop:true, bold:true);
            LayoutText(panel, "BodyText",   top: 132, height: 90, size: 26, anchorTop:true, bold:false);
            LayoutText(panel, "Quest_0",    top: 232, height: 50, size: 24, anchorTop:true, bold:false);
            LayoutText(panel, "Quest_1",    top: 282, height: 50, size: 24, anchorTop:true, bold:false);
            LayoutText(panel, "Quest_2",    top: 332, height: 50, size: 24, anchorTop:true, bold:false);

            Debug.Log("[OverlaySweep] Safe layout applied.");
        }

        // ===== helpers =====

        static GameObject FindOverlayRoot()
        {
            var go = GameObject.Find("ResolutionRoot");
            if (go) return go;

            foreach (var c in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (!c) continue;
                if (c.GetType().Name == "ResolutionContinueMB")
                {
                    var g = c.gameObject;
                    if (!EditorUtility.IsPersistent(g) && g.scene.IsValid()) return g;
                }
            }
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (!t) continue;
                if (t.name == "ResolutionRoot")
                {
                    var g = t.gameObject;
                    if (!EditorUtility.IsPersistent(g) && g.scene.IsValid()) return g;
                }
            }
            return null;
        }

        static List<Transform> Snapshot(Transform root)
        {
            var list = new List<Transform>(128);
            if (!root) return list;
            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (!t) continue;
                list.Add(t);
                // snapshot children now (avoid edit-time invalidation)
                for (int i = 0; i < t.childCount; i++)
                    stack.Push(t.GetChild(i));
            }
            return list;
        }

        static string GetPath(Transform t)
        {
            var sb = new StringBuilder();
            while (t != null) { sb.Insert(0, "/" + t.name); t = t.parent; }
            return sb.ToString();
        }

        static bool IsChildOf(Transform t, Transform parent)
        {
            var p = t;
            while (p != null) { if (p == parent) return true; p = p.parent; }
            return false;
        }

        static bool IsTextish(GameObject go)
        {
            if (!go) return false;
            if (go.GetComponent<Text>() != null) return true;
#if TMP_PRESENT
            if (go.GetComponent<TMP_Text>() != null) return true;
#endif
            return false;
        }

        static void LayoutText(Transform parent, string name, float top, float height, int size, bool anchorTop, bool bold)
        {
            var tr = parent.Find(name) as RectTransform;
            if (!tr) return;

            if (anchorTop)
            {
                tr.anchorMin = new Vector2(0f, 1f);
                tr.anchorMax = new Vector2(1f, 1f);
                tr.pivot     = new Vector2(0.5f, 1f);
                tr.offsetMin = new Vector2(36f, -top - height);
                tr.offsetMax = new Vector2(-36f, -top);
            }

            var color = Color.white;

            var ugui = tr.GetComponent<Text>();
            if (ugui)
            {
                ugui.alignment = TextAnchor.UpperLeft;
                ugui.fontSize = size;
                ugui.color = color;
                ugui.horizontalOverflow = HorizontalWrapMode.Wrap;
                ugui.verticalOverflow = VerticalWrapMode.Overflow;
                ugui.resizeTextForBestFit = false;
                ugui.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            }
#if TMP_PRESENT
            var tmp = tr.GetComponent<TMP_Text>();
            if (tmp)
            {
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.fontSize = size;
                tmp.color = color;
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.margin = Vector4.zero;
                tmp.fontStyle = bold ? (tmp.fontStyle | FontStyles.Bold) : (tmp.fontStyle & ~FontStyles.Bold);
            }
#endif
        }
    }
}
#endif
