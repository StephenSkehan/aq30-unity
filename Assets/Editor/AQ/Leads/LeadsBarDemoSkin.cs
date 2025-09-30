#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarDemoSkin
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";
        private const string CardTitle   = "Text_Title";
        private const string CardMicro   = "Text_Microcopy";
        private const string CardStack   = "V";
        private const string ReqRow      = "Row_Requirements";
        private const string CTA         = "Btn_CTA";
        private const string ActorAnchor = "ActorAnchor";

        [MenuItem("AQ/Leads/Apply Demo Skin (cards, type, chips, CTA)")]
        public static void Apply()
        {
            var content = GameObject.Find(ContentPath)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found. Run the LeadsBar conform step first."); return; }

            // Try to load a 9-slice if you’ve imported it already
            Sprite leadBG = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Leads/lead_card_bg_9s.png");

            Undo.IncrementCurrentGroup();
            int fixedCards = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // Size & border sanity
                EnsureCardRect(card, 360, 220);

                // Mask to prevent text spill
                if (!card.GetComponent<Mask>() && !card.GetComponent<RectMask2D>())
                    Undo.AddComponent<RectMask2D>(card.gameObject);

                // Background
                var img = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                if (leadBG != null)
                {
                    img.sprite = leadBG;
                    img.type   = Image.Type.Sliced;
                    img.color  = Color.white;
                }
                else
                {
                    img.sprite = null;
                    img.type   = Image.Type.Simple;
                    img.color  = new Color(0.18f, 0.19f, 0.20f, 1f); // fallback charcoal
                }
                img.raycastTarget = false;

                // Vertical stack (padding)
                var v = EnsureChild(card, CardStack, out var vRT, typeof(VerticalLayoutGroup)).GetComponent<VerticalLayoutGroup>();
                v.childAlignment       = TextAnchor.UpperLeft;
                v.childControlWidth    = false;
                v.childControlHeight   = false;
                v.childForceExpandWidth  = false;
                v.childForceExpandHeight = false;
                v.spacing = 8f;
                vRT.anchorMin = new Vector2(0, 0); vRT.anchorMax = new Vector2(1, 1);
                vRT.offsetMin = new Vector2(16, 16); vRT.offsetMax = new Vector2(-16, -16);

                // Title (1 line, bold)
                var title = EnsureTMP(vRT, CardTitle);
                title.fontSize = 30;
                title.fontStyle = FontStyles.Bold;
                title.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                title.textWrappingMode = TextWrappingModes.NoWrap;
#else
                title.enableWordWrapping = false;
#endif
                title.overflowMode = TextOverflowModes.Ellipsis;
                title.text = "Demo Lead";

                // Microcopy (2 lines, ellipsis)
                var micro = EnsureTMP(vRT, CardMicro);
                micro.fontSize = 22;
                micro.fontStyle = FontStyles.Normal;
                micro.color = new Color(0.85f, 0.85f, 0.88f, 1f);
#if UNITY_2022_2_OR_NEWER
                micro.textWrappingMode = TextWrappingModes.Normal;
#else
                micro.enableWordWrapping = true;
#endif
                micro.overflowMode = TextOverflowModes.Ellipsis;
                micro.maxVisibleLines = 2;
                micro.text = "Collect deli CCTV • 1–2 min";

                // Requirements row (3 teal chips)
                var reqRow = EnsureChild(vRT, ReqRow, out var reqRT, typeof(HorizontalLayoutGroup)).GetComponent<HorizontalLayoutGroup>();
                reqRow.spacing = 8;
                reqRow.childAlignment = TextAnchor.MiddleLeft;
                reqRow.childControlWidth = false;
                reqRow.childControlHeight = false;
                reqRow.childForceExpandWidth = false;
                reqRow.childForceExpandHeight = false;

                EnsureChip(reqRT, "Req_1");
                EnsureChip(reqRT, "Req_2");
                EnsureChip(reqRT, "Req_3");

                // CTA
                var ctaGO = EnsureChild(vRT, CTA, out var ctaRT, typeof(Image), typeof(Button));
                ctaRT.sizeDelta = new Vector2(160, 44);
                var ctaImg = ctaGO.GetComponent<Image>();
                ctaImg.type = Image.Type.Sliced;
                ctaImg.color = new Color(0.12f, 0.7f, 0.7f, 1f);

                var ctaTxt = ctaGO.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (!ctaTxt)
                {
                    var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    var trt = tgo.GetComponent<RectTransform>();
                    trt.SetParent(ctaRT, false);
                    trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                    trt.offsetMin = Vector2.zero;  trt.offsetMax = Vector2.zero;
                    ctaTxt = tgo.GetComponent<TextMeshProUGUI>();
                }
                ctaTxt.text = "Resolve";
                ctaTxt.fontSize = 26;
                ctaTxt.alignment = TextAlignmentOptions.Center;
                ctaTxt.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                ctaTxt.textWrappingMode = TextWrappingModes.NoWrap;
#else
                ctaTxt.enableWordWrapping = false;
#endif

                // Actor anchor (top center, above card)
                if (!card.Find(ActorAnchor))
                {
                    var anchor = new GameObject(ActorAnchor, typeof(RectTransform)).GetComponent<RectTransform>();
                    Undo.RegisterCreatedObjectUndo(anchor.gameObject, "Create ActorAnchor");
                    anchor.SetParent(card, false);
                    anchor.anchorMin = new Vector2(0.5f, 1f);
                    anchor.anchorMax = new Vector2(0.5f, 1f);
                    anchor.pivot     = new Vector2(0.5f, 0f);
                    anchor.anchoredPosition = new Vector2(0, 8);
                    anchor.sizeDelta = Vector2.zero;
                }

                fixedCards++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Debug.Log($"✅ Demo skin applied to {fixedCards} card(s).");
        }

        // --- helpers ---
        private static void EnsureCardRect(RectTransform rt, float w, float h)
        {
            var le = rt.GetComponent<LayoutElement>() ?? Undo.AddComponent<LayoutElement>(rt.gameObject);
            le.minWidth = le.preferredWidth = w;
            le.minHeight = le.preferredHeight = h;

            rt.pivot = new Vector2(0, 0.5f);
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static GameObject EnsureChild(RectTransform parent, string name, out RectTransform rt, params System.Type[] extra)
        {
            var t = parent.Find(name) as RectTransform;
            if (!t)
            {
                var go = new GameObject(name, new System.Type[] { typeof(RectTransform) }.Concat(extra).ToArray());
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
            }
            rt = t;
            return t.gameObject;
        }

        private static TextMeshProUGUI EnsureTMP(RectTransform parent, string name)
        {
            var t = parent.Find(name)?.GetComponent<TextMeshProUGUI>();
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(parent, false);
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.offsetMin = new Vector2(0, 0); rt.offsetMax = new Vector2(0, 0);
                t = go.GetComponent<TextMeshProUGUI>();
            }
            return t;
        }

        private static void EnsureChip(RectTransform parent, string name)
        {
            var chip = parent.Find(name) as RectTransform;
            if (!chip)
            {
                chip = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(chip.gameObject, $"Create {name}");
                chip.SetParent(parent, false);
            }
            chip.sizeDelta = new Vector2(90, 28);
            var img = chip.GetComponent<Image>();
            img.type = Image.Type.Simple;
            img.color = new Color(0.10f, 0.55f, 0.55f, 1f);

            var lbl = chip.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (!lbl)
            {
                var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(chip, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(6, 2); rt.offsetMax = new Vector2(-6, -2);
                lbl = go.GetComponent<TextMeshProUGUI>();
            }
            lbl.text = "Req";
            lbl.fontSize = 20;
            lbl.color = Color.white;
#if UNITY_2022_2_OR_NEWER
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
#else
            lbl.enableWordWrapping = false;
#endif
        }
    }
}
#endif
