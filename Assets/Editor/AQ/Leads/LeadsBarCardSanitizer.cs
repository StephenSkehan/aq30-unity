#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarCardSanitizer
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";
        private static readonly string[] AllowedTopLevel = { "V", "ActorAnchor" };

        [MenuItem("AQ/Leads/Cards • Deep Sanitize (remove stray children + re-skin)")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }

            Undo.IncrementCurrentGroup();
            int scrubbed = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // 1) Remove unexpected top-level children
                for (int c = card.childCount - 1; c >= 0; c--)
                {
                    var ch = card.GetChild(c);
                    if (!AllowedTopLevel.Contains(ch.name))
                        Undo.DestroyObjectImmediate(ch.gameObject);
                }

                // 2) Ensure background + mask
                var img = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                var bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Leads/lead_card_bg_9s.png");
                if (bg != null) { img.sprite = bg; img.type = Image.Type.Sliced; img.color = Color.white; }
                else { img.sprite = null; img.type = Image.Type.Simple; img.color = new Color(0.18f,0.19f,0.20f,1f); }
                if (!card.GetComponent<Mask>() && !card.GetComponent<RectMask2D>())
                    Undo.AddComponent<RectMask2D>(card.gameObject);

                // 3) Standard layout block
                var v = EnsureChild(card, "V", out var vRT, typeof(VerticalLayoutGroup)).GetComponent<VerticalLayoutGroup>();
                v.childAlignment = TextAnchor.UpperLeft;
                v.childControlWidth = v.childControlHeight = false;
                v.childForceExpandWidth = v.childForceExpandHeight = false;
                v.spacing = 8;
                vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
                vRT.offsetMin = new Vector2(16,16); vRT.offsetMax = new Vector2(-16,-16);

                // 4) Title + Micro (stronger, readable)
                var title = EnsureTMP(vRT, "Text_Title");
                title.text = "Demo Lead";
                title.fontSize = 30; title.fontStyle = FontStyles.Bold; title.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                title.textWrappingMode = TextWrappingModes.NoWrap;
#else
                title.enableWordWrapping = false;
#endif
                title.overflowMode = TextOverflowModes.Truncate;

                var micro = EnsureTMP(vRT, "Text_Microcopy");
                micro.text = "Collect deli CCTV • 1–2 min";
                micro.fontSize = 24; micro.color = new Color(0.92f,0.94f,0.96f,1f);
#if UNITY_2022_2_OR_NEWER
                micro.textWrappingMode = TextWrappingModes.Normal;
#else
                micro.enableWordWrapping = true;
#endif
                micro.maxVisibleLines = 2; micro.overflowMode = TextOverflowModes.Truncate;

                // 5) Requirements row (3)
                var reqRow = EnsureChild(vRT, "Row_Requirements", out var reqRT, typeof(HorizontalLayoutGroup))
                                 .GetComponent<HorizontalLayoutGroup>();
                reqRow.spacing = 8; reqRow.childAlignment = TextAnchor.MiddleLeft;
                reqRow.childControlWidth = reqRow.childControlHeight = false;
                reqRow.childForceExpandWidth = reqRow.childForceExpandHeight = false;
                EnsureChip(reqRT, "Req_1"); EnsureChip(reqRT, "Req_2"); EnsureChip(reqRT, "Req_3");

                // 6) CTA
                var ctaGo = EnsureChild(vRT, "Btn_CTA", out var ctaRT, typeof(Image), typeof(Button));
                ctaRT.sizeDelta = new Vector2(160, 44);
                var ctaImg = ctaGo.GetComponent<Image>(); ctaImg.type = Image.Type.Sliced;
                ctaImg.color = new Color(0.12f,0.70f,0.70f,1f);
                var ctaTxt = ctaGo.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (!ctaTxt)
                {
                    var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    var trt = tgo.GetComponent<RectTransform>(); trt.SetParent(ctaRT, false);
                    trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
                    ctaTxt = tgo.GetComponent<TextMeshProUGUI>();
                }
                ctaTxt.text = "Resolve"; ctaTxt.fontSize = 26; ctaTxt.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                ctaTxt.textWrappingMode = TextWrappingModes.NoWrap;
#else
                ctaTxt.enableWordWrapping = false;
#endif
                // 7) Actor anchor (top center)
                if (!card.Find("ActorAnchor"))
                {
                    var anchor = new GameObject("ActorAnchor", typeof(RectTransform)).GetComponent<RectTransform>();
                    Undo.RegisterCreatedObjectUndo(anchor.gameObject, "Create ActorAnchor");
                    anchor.SetParent(card, false);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f,1f);
                    anchor.pivot = new Vector2(0.5f,0f); anchor.anchoredPosition = new Vector2(0,8);
                }

                scrubbed++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"🧽 Deep sanitized {scrubbed} card(s): stray children removed, demo structure reapplied.");
        }

        // helpers
        private static GameObject EnsureChild(RectTransform parent, string name, out RectTransform rt, params System.Type[] extras)
        {
            var t = parent.Find(name) as RectTransform;
            if (!t)
            {
                var go = new GameObject(name, new System.Type[] { typeof(RectTransform) }.Concat(extras).ToArray());
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
                t = go.GetComponent<RectTransform>(); t.SetParent(parent, false);
            }
            rt = t; return t.gameObject;
        }

        private static TextMeshProUGUI EnsureTMP(RectTransform parent, string name)
        {
            var t = parent.Find(name)?.GetComponent<TextMeshProUGUI>();
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
                var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
                rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
                rt.pivot = new Vector2(0,1); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
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
                Undo.RegisterCreatedObjectUndo(chip.gameObject, $"Create {name}"); chip.SetParent(parent, false);
            }
            chip.sizeDelta = new Vector2(90,28);
            var img = chip.GetComponent<Image>(); img.type = Image.Type.Simple;
            img.color = new Color(0.10f,0.55f,0.55f,1f);

            var txt = chip.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (!txt)
            {
                var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var rt = go.GetComponent<RectTransform>(); rt.SetParent(chip, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(6,2); rt.offsetMax = new Vector2(-6,-2);
                txt = go.GetComponent<TextMeshProUGUI>();
            }
            txt.text = "Req"; txt.fontSize = 20; txt.color = Color.white;
#if UNITY_2022_2_OR_NEWER
            txt.textWrappingMode = TextWrappingModes.NoWrap;
#else
            txt.enableWordWrapping = false;
#endif
        }
    }
}
#endif
