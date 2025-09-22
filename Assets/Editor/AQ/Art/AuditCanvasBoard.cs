// Assets/Editor/AQ/Art/AuditCanvasBoard.cs
// Full-scene audit for Canvas_Board / HUD_Board / TopBar / StatusRow / LeadsBar.

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class AuditCanvasBoard
    {
        const string TAG = "[AQ Art]";

        [MenuItem("AQ/Art/Audit Canvas_Board (Deep)")]
        public static void Run()
        {
            var sb = new StringBuilder();
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas)
            {
                Debug.LogWarning($"{TAG} Canvas_Board not found.");
                return;
            }

            sb.AppendLine($"{TAG} --- Canvas_Board Deep Audit ---");

            // Canvas + Scaler
            var cv = canvas.GetComponent<Canvas>();
            var cs = canvas.GetComponent<CanvasScaler>();
            var rtCanvas = canvas.GetComponent<RectTransform>();
            sb.AppendLine(DescribeCanvas(cv, cs, rtCanvas));

            // HUD_Board
            var hud = canvas.transform.Find("HUD_Board") ? canvas.transform.Find("HUD_Board").gameObject : null;
            if (!hud)
            {
                sb.AppendLine("HUD_Board: (missing)");
                Debug.Log(sb.ToString());
                return;
            }

            var rtHUD = hud.GetComponent<RectTransform>();
            sb.AppendLine(DescribeRect(rtHUD, "HUD_Board"));
            var vlg = hud.GetComponent<VerticalLayoutGroup>();
            sb.AppendLine(vlg ? DescribeVLG(vlg) : "  (no VerticalLayoutGroup)");

            // Children of HUD_Board
            foreach (Transform child in hud.transform)
            {
                var name = child.name;
                if (name == "TopBar") DumpTopBar(child, sb);
                else if (name == "StatusRow") DumpStatusRow(child, sb);
                else if (name == "LeadsBar") DumpLeadsBar(child, sb);
                else DumpGeneric(child, sb);
            }

            Debug.Log(sb.ToString());
        }

        static void DumpTopBar(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"TopBar: {DescribeRect(t.GetComponent<RectTransform>())}");
            var hlg = t.GetComponent<HorizontalLayoutGroup>();
            sb.AppendLine(hlg ? "  " + DescribeHLG(hlg) : "  (no HorizontalLayoutGroup)");

            foreach (Transform c in t) DumpGeneric(c, sb, 2);

            // Expected children
            RequireChild(t, "Btn_Home", sb);
            RequireChild(t, "AvatarChip", sb);
            RequireChild(t, "EpisodeChip", sb);
            RequireChild(t, "Meter_Energy", sb);
            RequireChild(t, "Meter_Soft", sb);
            RequireChild(t, "Meter_Premium", sb);
        }

        static void DumpStatusRow(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"StatusRow: {DescribeRect(t.GetComponent<RectTransform>())}");
            var hlg = t.GetComponent<HorizontalLayoutGroup>();
            sb.AppendLine(hlg ? "  " + DescribeHLG(hlg) : "  (no HorizontalLayoutGroup)");

            foreach (Transform c in t) DumpGeneric(c, sb, 2);

            // Expected children
            RequireChild(t, "Text_Solved", sb);
            RequireChild(t, "Text_Evidence", sb);
            RequireChild(t, "Text_Leads", sb);
            RequireChild(t, "Text_LastBreakthrough", sb);
        }

        static void DumpLeadsBar(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"LeadsBar: {DescribeRect(t.GetComponent<RectTransform>())}");
            var sr = t.GetComponent<ScrollRect>();
            if (!sr)
                sb.AppendLine("  (no ScrollRect)");
            else
            {
                sb.AppendLine($"  ScrollRect horiz={sr.horizontal} vert={sr.vertical} inertia={sr.inertia}");
                if (!sr.viewport) sb.AppendLine("  (viewport missing)");
                if (!sr.content) sb.AppendLine("  (content missing)");
            }
            foreach (Transform c in t) DumpGeneric(c, sb, 2);
        }

        static void DumpGeneric(Transform t, StringBuilder sb, int depth = 1)
        {
            var rt = t.GetComponent<RectTransform>();
            var img = t.GetComponent<Image>();
            var tmp = t.GetComponent<TextMeshProUGUI>();
            var le = t.GetComponent<LayoutElement>();
            var pad = new string(' ', depth * 2);
            sb.AppendLine($"{pad}{t.name}: {DescribeRect(rt, inline:true)}");
            if (img) sb.AppendLine($"{pad}  Image sprite={(img.sprite ? img.sprite.name : "(none)")} type={img.type}");
            if (tmp) sb.AppendLine($"{pad}  TMP text=\"{tmp.text}\" size={tmp.fontSize}");
            if (le) sb.AppendLine($"{pad}  LayoutElement pref=({le.preferredWidth},{le.preferredHeight}) min=({le.minWidth},{le.minHeight})");
        }

        static void RequireChild(Transform parent, string child, StringBuilder sb)
        {
            if (!parent.Find(child))
                sb.AppendLine($"  !! Missing expected child: {child}");
        }

        // ---- describe helpers ----
        static string DescribeCanvas(Canvas cv, CanvasScaler cs, RectTransform rt)
        {
            var mode = cv ? cv.renderMode.ToString() : "(no Canvas)";
            var scaler = cs ? $"Scaler: UI Scale Mode={cs.uiScaleMode} Ref=({cs.referenceResolution.x}x{cs.referenceResolution.y}) Match={cs.matchWidthOrHeight:F2}" : "(no CanvasScaler)";
            return $"Canvas: {mode}, {scaler}, {DescribeRect(rt, "Rect", inline: true)}";
        }

        static string DescribeRect(RectTransform rt, string label = null, bool inline = false)
        {
            if (!rt) return label == null ? "(no RectTransform)" : $"{label}: (no RectTransform)";
            var aMin = rt.anchorMin; var aMax = rt.anchorMax; var sd = rt.sizeDelta; var ap = rt.anchoredPosition; var pv = rt.pivot;
            var s = $"{(label ?? "Rect")}: anchors=({aMin.x:F2},{aMin.y:F2})-({aMax.x:F2},{aMax.y:F2}) pivot=({pv.x:F2},{pv.y:F2}) size=({sd.x:F0},{sd.y:F0}) pos=({ap.x:F0},{ap.y:F0})";
            return inline ? s : "  " + s;
        }

        static string DescribeHLG(HorizontalLayoutGroup h)
        {
            return $"HLG padding L{h.padding.left}/R{h.padding.right}/T{h.padding.top}/B{h.padding.bottom} spacing={h.spacing} childCtrlW={h.childControlWidth} childCtrlH={h.childControlHeight} forceExpandW={h.childForceExpandWidth} forceExpandH={h.childForceExpandHeight}";
        }

        static string DescribeVLG(VerticalLayoutGroup v)
        {
            return $"VLG padding L{v.padding.left}/R{v.padding.right}/T{v.padding.top}/B{v.padding.bottom} spacing={v.spacing} childCtrlW={v.childControlWidth} childCtrlH={v.childControlHeight} forceExpandW={v.childForceExpandWidth} forceExpandH={v.childForceExpandHeight} align={v.childAlignment}";
        }
    }
}
#endif
