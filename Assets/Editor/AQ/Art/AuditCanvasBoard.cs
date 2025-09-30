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
            if (!canvas) { Debug.LogWarning($"{TAG} Canvas_Board not found."); return; }

            sb.AppendLine($"{TAG} --- Canvas_Board Deep Audit ---");

            // Canvas + Scaler
            var cv = canvas.GetComponent<Canvas>();
            var cs = canvas.GetComponent<CanvasScaler>();
            var rtCanvas = canvas.GetComponent<RectTransform>();
            sb.AppendLine(DescribeCanvas(cv, cs, rtCanvas));

            var hud = canvas.transform.Find("HUD_Board");
            if (!hud) { sb.AppendLine("HUD_Board: (missing)"); Debug.Log(sb.ToString()); return; }

            sb.AppendLine(DescribeRect(hud.GetComponent<RectTransform>(), "HUD_Board"));
            var vlg = hud.GetComponent<VerticalLayoutGroup>();
            sb.AppendLine(vlg ? DescribeVLG(vlg) : "VLG: (none)");

            foreach (Transform child in hud)
            {
                switch (child.name)
                {
                    case "TopBar":    DumpTopBar(child, sb); break;
                    case "StatusRow": DumpStatusRow(child, sb); break;
                    case "LeadsBar":  DumpLeadsBar(child, sb); break;
                    default:          DumpGeneric(child, sb, 1); break;
                }
            }

            Debug.Log(sb.ToString());
        }

        static void DumpTopBar(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"TopBar:   {DescribeRect(t.GetComponent<RectTransform>(), inline:true)}");
            var hlg = t.GetComponent<HorizontalLayoutGroup>();
            sb.AppendLine("  " + (hlg ? DescribeHLG(hlg) : "(no HLG)"));
            foreach (Transform c in t) DumpGeneric(c, sb, 2);

            RequireChild(t, "EpisodeChip", sb);
        }

        static void DumpStatusRow(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"StatusRow:{DescribeRect(t.GetComponent<RectTransform>(), inline:true)}");
            var hlg = t.GetComponent<HorizontalLayoutGroup>();
            sb.AppendLine("  " + (hlg ? DescribeHLG(hlg) : "(no HLG)"));
            foreach (Transform c in t) DumpGeneric(c, sb, 2);
        }

        static void DumpLeadsBar(Transform t, StringBuilder sb)
        {
            sb.AppendLine($"LeadsBar: {DescribeRect(t.GetComponent<RectTransform>(), inline:true)}");
            var sr = t.GetComponent<ScrollRect>();
            if (!sr)
                sb.AppendLine("  (no ScrollRect)");
            else
            {
                sb.AppendLine($"  ScrollRect horiz={sr.horizontal} vert={sr.vertical} inertia={sr.inertia}");
                sb.AppendLine($"  viewport={(sr.viewport ? sr.viewport.name : "(null)")} content={(sr.content ? sr.content.name : "(null)")}");
            }

            // Viewport + Content
            var viewport = t.Find("Viewport");
            if (viewport) DumpGeneric(viewport, sb, 2);
            var content  = t.Find("Viewport/Content_Leads");
            if (content)
            {
                DumpGeneric(content, sb, 2);
                int idx = 0;
                foreach (Transform card in content)
                {
                    sb.AppendLine($"    [Card {idx}] {DescribeRect(card.GetComponent<RectTransform>(), inline:true)}");
                    foreach (Transform k in card) DumpGeneric(k, sb, 3);
                    idx++;
                }
            }

            // Float layer (actors)
            var floats = t.Find("FloatLayer_Actors");
            if (floats)
            {
                sb.AppendLine($"  FloatLayer_Actors: children={floats.childCount}");
                int i = 0;
                foreach (Transform a in floats)
                {
                    var img = a.GetComponent<Image>();
                    sb.AppendLine($"    [Actor {i}] {a.name} sprite={(img && img.sprite ? img.sprite.name : "(none)")} {DescribeRect(a.GetComponent<RectTransform>(), inline:true)}");
                    i++;
                }
            }
        }

        static void DumpGeneric(Transform t, StringBuilder sb, int depth)
        {
            var pad = new string(' ', depth * 2);
            var rt = t.GetComponent<RectTransform>();
            var img = t.GetComponent<Image>();
            var tmp = t.GetComponent<TextMeshProUGUI>();
            var le = t.GetComponent<LayoutElement>();
            var btn = t.GetComponent<Button>();
            var cg = t.GetComponent<CanvasGroup>();
            var mask = t.GetComponent<Mask>();
            var rmask = t.GetComponent<RectMask2D>();

            sb.AppendLine($"{pad}{t.name}: {DescribeRect(rt, inline:true)} children={t.childCount}");

            if (img)
                sb.AppendLine($"{pad}  Image sprite={(img.sprite ? img.sprite.name : "(none)")} type={img.type} raycast={img.raycastTarget} color={RGBA(img.color)}");
            if (tmp)
            {
#if UNITY_2022_2_OR_NEWER
                var wrap = tmp.textWrappingMode.ToString();
#else
                var wrap = tmp.enableWordWrapping ? "WordWrap" : "NoWrap";
#endif
                var fontName = tmp.font ? tmp.font.name : "(null)";
                sb.AppendLine($"{pad}  TMP text=\"{San(tmp.text)}\" size={tmp.fontSize} font={fontName} align={tmp.alignment} wrap={wrap} overflow={tmp.overflowMode} maxLines={tmp.maxVisibleLines} color={RGBA(tmp.color)} raycast={tmp.raycastTarget}");
            }
            if (btn)  sb.AppendLine($"{pad}  Button interactable={btn.interactable} transition={btn.transition}");
            if (cg)   sb.AppendLine($"{pad}  CanvasGroup alpha={cg.alpha:F2} blocksRaycasts={cg.blocksRaycasts} interactable={cg.interactable}");
            if (mask || rmask) sb.AppendLine($"{pad}  Mask={(mask ? "Mask" : "RectMask2D")}");
            if (le)   sb.AppendLine($"{pad}  LayoutElement pref=({le.preferredWidth},{le.preferredHeight}) min=({le.minWidth},{le.minHeight}) flex=({le.flexibleWidth},{le.flexibleHeight})");
        }

        // ---- helpers ----
        static void RequireChild(Transform parent, string child, StringBuilder sb)
        {
            if (!parent.Find(child))
                sb.AppendLine($"  !! Missing expected child: {child}");
        }

        static string DescribeCanvas(Canvas cv, CanvasScaler cs, RectTransform rt)
        {
            var mode = cv ? cv.renderMode.ToString() : "(no Canvas)";
            var scaler = cs ? $"Scaler: UI Scale Mode={cs.uiScaleMode} Ref=({cs.referenceResolution.x}x{cs.referenceResolution.y}) Match={cs.matchWidthOrHeight:F2}" : "(no CanvasScaler)";
            return $"Canvas: {mode}, {scaler}, {DescribeRect(rt, "Rect", inline:true)}";
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
            return $"VLG padding L{v.padding.left}/R{v.padding.right}/T{v.padding.bottom} spacing={v.spacing} childCtrlW={v.childControlWidth} childCtrlH={v.childControlHeight} forceExpandW={v.childForceExpandWidth} forceExpandH={v.childForceExpandHeight} align={v.childAlignment}";
        }

        static string RGBA(Color c) => $"{c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2}";
        static string San(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\n","\\n");
    }
}
#endif
