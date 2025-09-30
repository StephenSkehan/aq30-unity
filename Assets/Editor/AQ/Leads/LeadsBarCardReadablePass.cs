#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarCardReadablePass
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";

        // canonical child names we keep under the vertical stack
        private static readonly string[] KeepUnderV = { "Text_Title", "Text_Microcopy", "Row_Requirements", "Btn_CTA" };

        [MenuItem("AQ/Leads/Cards • Readable Pass (contrast + tidy + purge V)")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }

            Undo.IncrementCurrentGroup();
            int fixedCards = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // --- Background: prefer your 9-slice, else charcoal ---
                var img = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                var bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Leads/lead_card_bg_9s.png");
                if (bg != null) { img.sprite = bg; img.type = Image.Type.Sliced; img.color = Color.white; }
                else { img.sprite = null; img.type = Image.Type.Simple; img.color = new Color(0.12f, 0.12f, 0.13f, 1f); }

                // mask so text can’t spill
                if (!card.GetComponent<Mask>() && !card.GetComponent<RectMask2D>())
                    Undo.AddComponent<RectMask2D>(card.gameObject);

                // --- Ensure vertical stack V exists and is padded properly ---
                var v = card.Find("V") as RectTransform;
                if (!v)
                {
                    v = new GameObject("V", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
                    Undo.RegisterCreatedObjectUndo(v.gameObject, "Create V");
                    v.SetParent(card, false);
                }
                var vlg = v.GetComponent<VerticalLayoutGroup>() ?? Undo.AddComponent<VerticalLayoutGroup>(v.gameObject);
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlWidth = false; vlg.childControlHeight = false;
                vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;
                vlg.spacing = 10f;
                v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one;
                v.offsetMin = new Vector2(16, 16); v.offsetMax = new Vector2(-16, -16);

                // --- PURGE any unknown children under V (kills cream capsule etc.) ---
                for (int c = v.childCount - 1; c >= 0; c--)
                {
                    var ch = v.GetChild(c);
                    if (!KeepUnderV.Contains(ch.name))
                        Undo.DestroyObjectImmediate(ch.gameObject);
                }

                // --- Title (single line, bright) ---
                var title = EnsureTMP(v, "Text_Title");
                title.text = string.IsNullOrEmpty(title.text) ? "Demo Lead" : title.text;
                title.fontSize = 32;
                title.fontStyle = FontStyles.Bold;                 // <- fixed
                title.fontWeight = FontWeight.Heavy;               // <- heavier weight (TMP property)
                title.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                title.textWrappingMode = TextWrappingModes.NoWrap;
#else
                title.enableWordWrapping = false;
#endif
                title.overflowMode = TextOverflowModes.Truncate;
                EnsureShadow(title,  new Color(0f,0f,0f,0.65f), new Vector2(1.0f,-1.0f));

                // --- Microcopy (two lines max, high contrast) ---
                var micro = EnsureTMP(v, "Text_Microcopy");
                if (string.IsNullOrEmpty(micro.text)) micro.text = "Collect deli CCTV • 1–2 min";
                micro.fontSize = 24;
                micro.fontStyle = FontStyles.Normal;
                micro.color = new Color(0.92f, 0.96f, 1f, 0.95f); // icy white-blue for pop
#if UNITY_2022_2_OR_NEWER
                micro.textWrappingMode = TextWrappingModes.Normal;
#else
                micro.enableWordWrapping = true;
#endif
                micro.maxVisibleLines = 2;
                micro.overflowMode = TextOverflowModes.Truncate;
                EnsureShadow(micro, new Color(0f,0f,0f,0.6f), new Vector2(0.8f,-0.8f));

                // --- Requirements row (3 tidy teal chips) ---
                var row = EnsureChild(v, "Row_Requirements", out var rowRT, typeof(HorizontalLayoutGroup)).GetComponent<HorizontalLayoutGroup>();
                row.spacing = 8; row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = row.childControlHeight = false;
                row.childForceExpandWidth = row.childForceExpandHeight = false;

                EnsureChip(rowRT, "Req_1");
                EnsureChip(rowRT, "Req_2");
                EnsureChip(rowRT, "Req_3");

                // --- CTA button (teal) ---
                var ctaGo = EnsureChild(v, "Btn_CTA", out var ctaRT, typeof(Image), typeof(Button));
                ctaRT.sizeDelta = new Vector2(160, 44);
                var ctaImg = ctaGo.GetComponent<Image>(); ctaImg.type = Image.Type.Sliced;
                ctaImg.color = new Color(0.14f, 0.78f, 0.78f, 1f);
                var ctaTxt = ctaGo.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
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

                fixedCards++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"✅ Readable pass applied to {fixedCards} card(s): contrast boosted, format normalized, stray UI purged.");
        }

        // ---------- helpers ----------
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
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(8,2); rt.offsetMax = new Vector2(-8,-2);
                txt = go.GetComponent<TextMeshProUGUI>();
            }
            txt.text = "Req"; txt.fontSize = 20; txt.color = Color.white;
#if UNITY_2022_2_OR_NEWER
            txt.textWrappingMode = TextWrappingModes.NoWrap;
#else
            txt.enableWordWrapping = false;
#endif
            EnsureShadow(txt, new Color(0f,0f,0f,0.55f), new Vector2(0.6f,-0.6f));
        }
    }
}
#endif
