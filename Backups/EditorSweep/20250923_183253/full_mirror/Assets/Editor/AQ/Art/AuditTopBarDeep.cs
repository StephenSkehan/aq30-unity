// Assets/Editor/AQ/Art/AuditTopBarDeep.cs
// Deep, deterministic audit of Canvas_Board/HUD_Board/TopBar.
// Prints full child tree, RectTransform data, and key component summaries.

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class AuditTopBarDeep
    {
        const string Tag = "[AQ Art]";

        [MenuItem("AQ/Art/Audit TopBar (Deep)")]
        public static void Run()
        {
            var topBar = FindTopBar();
            if (topBar == null)
            {
                Debug.LogWarning($"{Tag} TopBar not found at Canvas_Board/HUD_Board/TopBar");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{Tag} --- TopBar Deep Audit ---");
            sb.AppendLine(DescribeRect(topBar.GetComponent<RectTransform>(), "TopBar"));

            var hlg = topBar.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) sb.AppendLine("  (no HorizontalLayoutGroup)");
            else sb.AppendLine(DescribeHLG(hlg));

            // Recurse children
            foreach (Transform child in topBar.transform)
                Dump(child, 1, sb);

            Debug.Log(sb.ToString());
        }

        static GameObject FindTopBar()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (canvas == null) return null;
            var hud = canvas.transform.Find("HUD_Board");
            if (hud == null) return null;
            var top = hud.Find("TopBar");
            return top ? top.gameObject : null;
        }

        static void Dump(Transform t, int depth, StringBuilder sb)
        {
            var rt = t.GetComponent<RectTransform>();
            sb.AppendLine(DescribeRect(rt, t.name, depth));

            var img = t.GetComponent<Image>();
            if (img) sb.AppendLine(Indent(depth + 1) + DescribeImage(img));

            var btn = t.GetComponent<Button>();
            if (btn) sb.AppendLine(Indent(depth + 1) + "(Button)");

            var le = t.GetComponent<LayoutElement>();
            if (le) sb.AppendLine(Indent(depth + 1) + DescribeLE(le));

            var hlg = t.GetComponent<HorizontalLayoutGroup>();
            if (hlg) sb.AppendLine(Indent(depth + 1) + DescribeHLG(hlg));

            var txt = t.GetComponent<TextMeshProUGUI>();
            if (txt) sb.AppendLine(Indent(depth + 1) + $"TMP text=\"{txt.text}\" size={txt.fontSize}");

            foreach (Transform c in t)
                Dump(c, depth + 1, sb);
        }

        static string DescribeRect(RectTransform rt, string label, int depth = 0)
        {
            if (!rt) return Indent(depth) + $"{label}: (no RectTransform?)";
            var aMin = rt.anchorMin; var aMax = rt.anchorMax;
            var sd = rt.sizeDelta; var ap = rt.anchoredPosition;
            return Indent(depth) + $"{label}: anchors=({aMin.x:F2},{aMin.y:F2})-({aMax.x:F2},{aMax.y:F2}) pivot={rt.pivot} sizeDelta=({sd.x:F0},{sd.y:F0}) anchored=({ap.x:F0},{ap.y:F0})";
        }

        static string DescribeImage(Image img)
        {
            var s = img.sprite ? img.sprite.name : "(none)";
            var type = img.type.ToString();
            var border = img.sprite ? img.sprite.border : Vector4.zero;
            return $"Image sprite={s} type={type} border=({border.x},{border.y},{border.z},{border.w}) preserveAspect={img.preserveAspect}";
        }

        static string DescribeHLG(HorizontalLayoutGroup h)
        {
            return $"HLG padding L{h.padding.left}/R{h.padding.right}/T{h.padding.top}/B{h.padding.bottom} spacing={h.spacing} childCtrlW={h.childControlWidth} childCtrlH={h.childControlHeight} forceExpandW={h.childForceExpandWidth} forceExpandH={h.childForceExpandHeight}";
        }

        static string DescribeLE(LayoutElement le)
        {
            return $"LayoutElement min=({le.minWidth:F0},{le.minHeight:F0}) pref=({le.preferredWidth:F0},{le.preferredHeight:F0}) flex=({le.flexibleWidth:F1},{le.flexibleHeight:F1})";
        }

        static string Indent(int n) => new string(' ', n * 2);
    }
}
#endif
