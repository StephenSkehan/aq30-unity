#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarForceDarkSkin
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";
        private static readonly string[] KeepUnderV = { "Text_Title", "Text_Microcopy", "Row_Requirements", "Btn_CTA" };

        [MenuItem("AQ/Leads/Cards • FORCE Dark Skin (no sprite, high-contrast)")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }

            Undo.IncrementCurrentGroup();
            int n = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // 0) Size sanity (so text isn’t cramped)
                var le = card.GetComponent<LayoutElement>() ?? Undo.AddComponent<LayoutElement>(card.gameObject);
                le.minWidth = le.preferredWidth = 360;
                le.minHeight = le.preferredHeight = 220;

                // 1) Background: NO SPRITE — flat charcoal
                var img = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                img.sprite = null;
                img.type = Image.Type.Simple;
                img.color = new Color(0.12f, 0.12f, 0.13f, 1f); // dark
                if (!card.GetComponent<Mask>() && !card.GetComponent<RectMask2D>())
                    Undo.AddComponent<RectMask2D>(card.gameObject);

                // 2) Ensure vertical stack
                var v = card.Find("V") as RectTransform;
                if (!v)
                {
                    v = new GameObject("V", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
                    Undo.RegisterCreatedObjectUndo(v.gameObject, "Create V");
                    v.SetParent(card, false);
                }
                var vlg = v.GetComponent<VerticalLayoutGroup>() ?? Undo.AddComponent<VerticalLayoutGroup>(v.gameObject);
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlWidth = vlg.childControlHeight = false;
                vlg.childForceExpandWidth = vlg.childForceExpandHeight = false;
                vlg.spacing = 10f;
                v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one;
                v.offsetMin = new Vector2(16, 16); v.offsetMax = new Vector2(-16, -16);

                // 3) Purge unknown children under V (kills any leftover capsules)
                for (int c = v.childCount - 1; c >= 0; c--)
                {
                    var ch = v.GetChild(c);
                    if (!KeepUnderV.Contains(ch.name))
                        Undo.DestroyObjectImmediate(ch.gameObject);
                }

                // 4) Title
                var title = EnsureTMP(v, "Text_Title");
                if (string.IsNullOrWhiteSpace(title.text)) title.text = "Demo Lead";
                title.fontSize = 32;
                title.fontStyle = FontStyles.Bold;
                title.fontWeight = FontWeight.Heavy;
                title.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                title.textWrappingMode = TextWrappingModes.NoWrap;
#else
                title.enableWordWrapping = false;
#endif
                title.overflowMode = TextOverflowModes.Truncate;
                EnsureShadow(title, new Color(0f,0f,0f,0.70f), new Vector2(1f,-1f));

                // 5) Microcopy (2 lines)
                var micro = EnsureTMP(v, "Text_Microcopy");
                if (string.IsNullOrWhiteSpace(micro.text)) micro.text = "Collect deli CCTV • 1–2 min";
                micro.fontSize = 24;
                micro.fontStyle = FontStyles.Normal;
                micro.color = new Color(0.92f, 0.96f, 1f, 1f);
#if UNITY_2022_2_OR_NEWER
                micro.textWrappingMode = TextWrappingModes.Normal;
#else
                micro.enableWordWrapping = true;
#endif
                micro.maxVisibleLines = 2;
                micro.overflowMode = TextOverflowModes.Truncate;
                EnsureShadow(micro, new Color(0f,0f,0f,0.65f), new Vector2(0.8f,-0.8f));

                // 6) Requirements row (3 chips)
                var row = EnsureChild(v, "Row_Requirements", out var rowRT, typeof(HorizontalLayoutGroup)).GetComponent<HorizontalLayoutGroup>();
                row.spacing = 8; row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = row.childControlHeight = false;
                row.childForceExpandWidth = row.childForceExpandHeight = false;

                EnsureChip(rowRT, "Req_1");
                EnsureChip(rowRT, "Req_2");
                EnsureChip(rowRT, "Req_3");

                // 7) CTA
                var cta = EnsureChild(v, "Btn_CTA", out var ctaRT, typeof(Image), typeof(Button));
                ctaRT.sizeDelta = new Vector2(160, 44);
                var ctaImg = cta.GetComponent<Image>(); ctaImg.type = Image.Type.Sliced;
                ctaImg.color = new Color(0.14f, 0.78f, 0.78f, 1f);
                var ctaTxt = cta.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (!ctaTxt)
                {
                    var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    var trt = tgo.GetComponent<RectTransform>(); trt.SetParent(ctaRT, false);
                    trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
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
                EnsureShadow(ctaTxt, new Color(0f,0f,0f,0.65f), new Vector2(0.8f,-0.8f));

                n++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"✅ Force Dark Skin applied to {n} card(s).");
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

        private static TextMeshProUGUI EnsureTMP(Transform parent, string name)
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

        private static void EnsureShadow(TMP_Text tmp, Color col, Vector2 dist)
        {
            var sh = (tmp as Component).GetComponent<Shadow>();
            if (!sh) sh = Undo.AddComponent<Shadow>((tmp as Component).gameObject);
            sh.effectColor = col; sh.effectDistance = dist; sh.useGraphicAlpha = true;
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
            var img = chip.GetComponent<Image>(); img.type = Image.Type.Sliced;
            img.color = new Color(0.10f, 0.60f, 0.60f, 1f);

            var txt = chip.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (!txt)
            {
                var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var rt = go.GetComponent<RectTransform>(); rt.SetParent(chip, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(8,2); rt.offsetMax = new Vector2(-8,-2);
                txt = go.GetComponent<TextMeshProUGUI>();
            }
            txt.text = "Req"; txt.fontSize = 20; txt.color = Color.white;
#if UNITY_2022_2_OR_NEWER
            txt.textWrappingMode = TextWrappingModes.NoWrap;
#else
            txt.enableWordWrapping = false;
#endif
            var sh = txt.GetComponent<Shadow>() ?? Undo.AddComponent<Shadow>(txt.gameObject);
            sh.effectColor = new Color(0f,0f,0f,0.55f); sh.effectDistance = new Vector2(0.6f,-0.6f);
        }
    }
}
#endif
