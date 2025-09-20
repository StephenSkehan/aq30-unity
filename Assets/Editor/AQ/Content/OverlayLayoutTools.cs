#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Content
{
    public static class OverlayLayoutTools
    {
        // Run this first to see what’s wrong
        [MenuItem("AQ/Content/Overlay: Audit Layout")]
        public static void Audit()
        {
            var root = FindResolutionRoot();
            if (!root) { Debug.LogWarning("[OverlayAudit] ResolutionRoot not found."); return; }

            Report(root, "ResolutionRoot");
            var panel = root.transform.Find("ResolutionPanel") as RectTransform;
            if (!panel) { Debug.LogWarning("[OverlayAudit] ResolutionPanel not found."); return; }
            Report(panel.gameObject, "ResolutionRoot/ResolutionPanel");

            Dump("ResolutionRoot/ResolutionPanel/TitleText");
            Dump("ResolutionRoot/ResolutionPanel/BodyText");
            Dump("ResolutionRoot/ResolutionPanel/Quest_0");
            Dump("ResolutionRoot/ResolutionPanel/Quest_1");
            Dump("ResolutionRoot/ResolutionPanel/Quest_2");
            Dump("ResolutionRoot/ResolutionPanel/ResolveButton");
            Dump("ResolutionRoot/ResolutionPanel/ResolveButton/Text");

            void Dump(string path)
            {
                var t = root.transform.Find(path.Substring("ResolutionRoot/".Length));
                if (!t) { Debug.LogWarning($"[OverlayAudit] Missing: {path}"); return; }
                Report(t.gameObject, path);
            }

            static void Report(GameObject go, string path)
            {
                var rt = go.GetComponent<RectTransform>();
                var parentRT = go.transform.parent ? go.transform.parent.GetComponent<RectTransform>() : null;

                string comps = string.Join(", ",
                    go.GetComponents<Component>()
                      .Where(c => !(c is Transform || c is RectTransform))
                      .Select(c => c.GetType().Name));

                string layoutComps = string.Join(" | ",
                    new Component[]{
                        go.GetComponent<ContentSizeFitter>(),
                        go.GetComponent<LayoutElement>(),
                        go.GetComponent<HorizontalLayoutGroup>(),
                        go.GetComponent<VerticalLayoutGroup>()}
                      .Where(c => c != null)
                      .Select(c => c.GetType().Name));

                string textType = go.GetComponent<Text>() ? "UGUI.Text"
                                 : GetTMP(go) != null ? "TMP_Text"
                                 : "--";

                float w = rt ? rt.rect.width : -1f;
                float pw = parentRT ? parentRT.rect.width : -1f;

                Debug.Log(
                    $"[OverlayAudit] {path}\n" +
                    $"  Active: {go.activeInHierarchy}\n" +
                    $"  Rect:(w={w:0.##}, h={ (rt?rt.rect.height:-1f):0.##})  ParentW={pw:0.##}\n" +
                    $"  Anchors min={V2(rt?.anchorMin)} max={V2(rt?.anchorMax)}  Pivot={V2(rt?.pivot)}\n" +
                    $"  OffMin={V2(rt?.offsetMin)} OffMax={V2(rt?.offsetMax)}  SizeDelta={V2(rt?.sizeDelta)}\n" +
                    $"  Components: {comps}\n" +
                    $"  Layout: { (string.IsNullOrEmpty(layoutComps) ? "(none)" : layoutComps) }\n" +
                    $"  Text: {textType}"
                );

                if (w >= 0 && w < 200f)
                    Debug.LogWarning($"[OverlayAudit] {path} width is very small ({w:0}). Likely constrained by a Layout component or anchors.");
            }

            static string V2(Vector2? v) => v.HasValue ? $"({v.Value.x:0.##},{v.Value.y:0.##})" : "(null)";
        }

        // Fixes the “skinny column” by stripping layout constraints and widening the text areas.
        [MenuItem("AQ/Content/Overlay: Fix Narrow Text (Normalize)")]
        public static void FixNarrowText()
        {
            var root = FindResolutionRoot();
            if (!root) { Debug.LogWarning("[OverlayFix] ResolutionRoot not found."); return; }

            var panel = root.transform.Find("ResolutionPanel") as RectTransform;
            if (!panel) { Debug.LogWarning("[OverlayFix] ResolutionPanel not found."); return; }

            // Remove layout groups / fitters on the panel that can clamp widths
            KillIf<HorizontalLayoutGroup>(panel.gameObject);
            KillIf<VerticalLayoutGroup>(panel.gameObject);
            KillIf<ContentSizeFitter>(panel.gameObject);
            KillIf<LayoutElement>(panel.gameObject);

            NormalizeText("TitleText", y: +160, height: 60, left: 36, right: 36, size: 40, align: Align.Center);
            NormalizeText("BodyText",  y:  +30, height: 84, left: 36, right: 36, size: 28, align: Align.Left);
            NormalizeText("Quest_0",   y:  -40, height: 44, left: 36, right: 36, size: 24, align: Align.Left);
            NormalizeText("Quest_1",   y:  -86, height: 44, left: 36, right: 36, size: 24, align: Align.Left);
            NormalizeText("Quest_2",   y: -132, height: 44, left: 36, right: 36, size: 24, align: Align.Left);

            // Button label — ensure single label fills the button
            var btn = panel.Find("ResolveButton") as RectTransform;
            if (btn)
            {
                KillIf<ContentSizeFitter>(btn.gameObject);
                KillIf<LayoutElement>(btn.gameObject);

                var label = btn.Find("Text");
                if (!label)
                {
                    var go = new GameObject("Text", typeof(RectTransform));
                    label = go.transform;
                    label.SetParent(btn, false);
                }

                var lblRT = label as RectTransform;
                lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one; lblRT.pivot = new Vector2(0.5f, 0.5f);
                lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;

                var txt = label.GetComponent<Text>();
                var tmp = GetTMP(label.gameObject);
                if (tmp != null) SetTMP(tmp, "Continue", 28, Color.black, Align.Center, wrap:false);
                else             SetUGUI(label.gameObject, "Continue", 28, Color.black, Align.Center, wrap:false);
            }

            Debug.Log("[OverlayFix] Normalized text rects. If anything still collapses, run Audit and paste output.");
            
            // ---------- helpers ----------
            void NormalizeText(string child, float y, float height, float left, float right, int size, Align align)
            {
                var tr = panel.Find(child) as RectTransform;
                if (!tr)
                {
                    var go = new GameObject(child, typeof(RectTransform));
                    tr = go.GetComponent<RectTransform>();
                    tr.SetParent(panel, false);
                }

                KillIf<ContentSizeFitter>(tr.gameObject);
                KillIf<LayoutElement>(tr.gameObject);

                // Stretch horizontally with padding; keep explicit line height via offsets.
                tr.anchorMin = new Vector2(0f, 0.5f);
                tr.anchorMax = new Vector2(1f, 0.5f);
                tr.pivot     = new Vector2(0.5f, 0.5f);
                tr.offsetMin = new Vector2(left,  y - height/2f);
                tr.offsetMax = new Vector2(-right, y + height/2f);
                tr.localScale = Vector3.one;

                // Ensure a text component exists and isn’t auto-sized by a fitter.
                var tmp = GetTMP(tr.gameObject);
                if (tmp != null)
                {
                    SetTMP(tmp, defaultTextFor(child), size, Color.white, align, wrap:true);
                }
                else
                {
                    SetUGUI(tr.gameObject, defaultTextFor(child), size, Color.white, align, wrap:true);
                }
            }

            static string defaultTextFor(string name) => name switch
            {
                "TitleText" => "Case Closed",
                "BodyText"  => "Your investigation cracked the trail wide open.",
                "Quest_0"   => "• Investigate new lead at City Hall",
                "Quest_1"   => "• Cross-check Marlow’s alibi records",
                "Quest_2"   => "• Tag recovered evidence in caseboard",
                _           => ""
            };
        }

        // ---------- low-level utilities ----------
        enum Align { Left, Center, Right }

        static void SetUGUI(GameObject go, string text, int size, Color color, Align align, bool wrap)
        {
            var t = go.GetComponent<Text>() ?? go.AddComponent<Text>();
            if (!t.font) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.supportRichText = true;
            t.resizeTextForBestFit = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow   = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            t.text = text;
            t.color = color;
            t.fontSize = size;
            t.alignment = align switch
            {
                Align.Left   => TextAnchor.MiddleLeft,
                Align.Center => TextAnchor.MiddleCenter,
                Align.Right  => TextAnchor.MiddleRight,
                _            => TextAnchor.MiddleCenter
            };
        }

        static Component GetTMP(GameObject go)
        {
            var tmpType = GetType("TMPro.TextMeshProUGUI");
            return tmpType != null ? go.GetComponent(tmpType) : null;
        }

        static void SetTMP(Component tmp, string text, int size, Color color, Align align, bool wrap)
        {
            var t = tmp.GetType();
            t.GetProperty("text")?.SetValue(tmp, text);
            t.GetProperty("fontSize")?.SetValue(tmp, (float)size);
            t.GetProperty("color")?.SetValue(tmp, color);
            t.GetProperty("enableAutoSizing")?.SetValue(tmp, false);
            t.GetProperty("enableWordWrapping")?.SetValue(tmp, wrap);

            // alignment (TMP & UGUI enum-safe)
            var alignProp = t.GetProperty("alignment");
            if (alignProp != null)
            {
                object val = null;
                foreach (var name in align switch
                {
                    Align.Left   => new[] { "Left", "MiddleLeft" },
                    Align.Center => new[] { "Center", "MiddleCenter" },
                    Align.Right  => new[] { "Right", "MiddleRight" },
                    _            => new[] { "Center" }
                })
                {
                    try { val = Enum.Parse(alignProp.PropertyType, name); break; } catch { }
                }
                if (val != null) alignProp.SetValue(tmp, val);
            }

            // overflow = Overflow
            var ov = t.GetProperty("overflowMode");
            if (ov != null)
            {
                try
                {
                    var val = Enum.Parse(ov.PropertyType, "Overflow");
                    ov.SetValue(tmp, val);
                } catch { /* ignore */ }
            }

            // margins zero
            var margin = t.GetProperty("margin");
            if (margin != null) margin.SetValue(tmp, new Vector4(0,0,0,0));
        }

        static void KillIf<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c) Undo.DestroyObjectImmediate(c);
        }

        static GameObject FindResolutionRoot()
        {
            var go = GameObject.Find("ResolutionRoot");
            if (go) return go;

            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t && t.name == "ResolutionRoot" && (t.hideFlags & HideFlags.HideInHierarchy) == 0)
                    return t.gameObject;
            }

            // fallback: top-most ScreenSpaceOverlay canvas
            var canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var candidate = canvases
                .Where(c => c && c.renderMode == RenderMode.ScreenSpaceOverlay)
                .OrderByDescending(c => c.sortingOrder)
                .FirstOrDefault();
            return candidate ? candidate.gameObject : null;
        }

        static Type GetType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
#endif
