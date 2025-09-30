#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Leads
{
    public static class LeadsBarEnsure
    {
        private const string RootGOName = "LeadsBar";
        private const string ScrollName = "ScrollLeads";
        private const string ViewportName = "Viewport";
        private const string ContentName = "Content_Leads";

        [MenuItem("AQ/Leads/Ensure LeadsBar (ScrollRect + Content)")]
        public static void EnsureLeadsBar()
        {
            var leadsBar = GameObject.Find(RootGOName);
            if (!leadsBar)
            {
                EditorUtility.DisplayDialog("LeadsBar not found",
                    $"Couldn’t find a GameObject named \"{RootGOName}\" at the scene root. " +
                    "Open the Merge scene and make sure the HUD_Board hierarchy exists.", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            // Ensure/locate child ScrollLeads
            var scrollTF = leadsBar.transform.Find(ScrollName);
            if (!scrollTF)
            {
                var go = new GameObject(ScrollName, typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
                Undo.RegisterCreatedObjectUndo(go, "Create ScrollLeads");
                go.transform.SetParent(leadsBar.transform, false);
                scrollTF = go.transform;

                var img = go.GetComponent<Image>();
                img.sprite = null; // keep invisible; your scene already has a dark HUD strip
                img.color = new Color(0, 0, 0, 0); // fully transparent

                var mask = go.GetComponent<Mask>();
                mask.showMaskGraphic = false;

                var rt = (RectTransform)scrollTF;
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Ensure/locate Viewport
            var viewportTF = scrollTF.Find(ViewportName) as RectTransform;
            if (!viewportTF)
            {
                var go = new GameObject(ViewportName, typeof(RectTransform), typeof(Image), typeof(Mask));
                Undo.RegisterCreatedObjectUndo(go, "Create Viewport");
                go.transform.SetParent(scrollTF, false);
                viewportTF = go.GetComponent<RectTransform>();

                var img = go.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 0); // invisible

                var mask = go.GetComponent<Mask>();
                mask.showMaskGraphic = false;

                viewportTF.anchorMin = new Vector2(0, 0);
                viewportTF.anchorMax = new Vector2(1, 1);
                viewportTF.offsetMin = Vector2.zero;
                viewportTF.offsetMax = Vector2.zero;
            }

            // Ensure/locate Content
            var contentTF = viewportTF.Find(ContentName) as RectTransform;
            if (!contentTF)
            {
                var go = new GameObject(ContentName, typeof(RectTransform), typeof(ContentSizeFitter), typeof(HorizontalLayoutGroup));
                Undo.RegisterCreatedObjectUndo(go, "Create Content_Leads");
                go.transform.SetParent(viewportTF, false);
                contentTF = go.GetComponent<RectTransform>();
            }

            // Layout settings (HLG for horizontal strip of cards)
            var hlg = contentTF.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;   // each card has its own fixed width
            hlg.childControlHeight = false;  // fixed height
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(24, 24, 0, 0);

            var fit = contentTF.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Wire ScrollRect
            var sr = scrollTF.GetComponent<ScrollRect>();
            sr.horizontal = true;
            sr.vertical = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.viewport = viewportTF;
            sr.content = contentTF;

            // Content Rect: pivot left-center, size auto in X, fixed height ~220
            contentTF.pivot = new Vector2(0, 0.5f);
            contentTF.anchorMin = new Vector2(0, 0.5f);
            contentTF.anchorMax = new Vector2(0, 0.5f);
            contentTF.anchoredPosition = Vector2.zero;
            contentTF.sizeDelta = new Vector2(0, 220); // card height budget

            // Viewport anchors already full; ensure content starts at left
            viewportTF.pivot = new Vector2(0.5f, 0.5f);

            Undo.CollapseUndoOperations(group);
            Debug.Log("✅ LeadsBar ensured: ScrollRect + Viewport + Content_Leads wired.");
        }

        [MenuItem("AQ/Leads/Spawn Demo LeadCards (x3)")]
        public static void SpawnDemoCards()
        {
            var leadsBar = GameObject.Find(RootGOName);
            if (!leadsBar)
            {
                EditorUtility.DisplayDialog("LeadsBar not found", "Run 'Ensure LeadsBar' first.", "OK");
                return;
            }
            var content = leadsBar.transform.Find($"{ScrollName}/{ViewportName}/{ContentName}") as RectTransform;
            if (!content)
            {
                EditorUtility.DisplayDialog("Content not found", "Run 'Ensure LeadsBar' first.", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            for (int i = 0; i < 3; i++)
            {
                var card = CreateDemoCard($"LeadCard_{i+1}");
                Undo.RegisterCreatedObjectUndo(card, "Create Demo LeadCard");
                card.transform.SetParent(content, false);
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log("🧪 Spawned 3 demo LeadCards under Content_Leads.");
        }

        // Minimal LeadCard structure that matches your spec and gives us anchor points.
        private static GameObject CreateDemoCard(string name)
        {
            // Root
            var root = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(Image));
            var rt = root.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(360, 220); // width ~360, height 220 (tweak later)

            var le = root.GetComponent<LayoutElement>();
            le.preferredWidth = 360;
            le.preferredHeight = 220;
            le.minWidth = 360;
            le.minHeight = 220;

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 1f); // dark HUD-like placeholder
            bg.type = Image.Type.Sliced; // if you later drop in lead_card_bg_9s

            // Vertical stack inside card
            var v = new GameObject("V", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            v.SetParent(rt, false);
            v.anchorMin = new Vector2(0, 0);
            v.anchorMax = new Vector2(1, 1);
            v.offsetMin = new Vector2(16, 16);
            v.offsetMax = new Vector2(-16, -16);

            var vlg = v.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Title
            var title = CreateTMP("Text_Title", "Interview Beth", 32, FontStyles.Bold);
            title.transform.SetParent(v, false);

            // Microcopy
            var micro = CreateTMP("Text_Microcopy", "Collect deli CCTV • 1–2 min", 22, FontStyles.Normal, new Color(0.85f,0.85f,0.85f,1));
            micro.transform.SetParent(v, false);

            // Requirements row (tiny chips)
            var reqRow = new GameObject("Row_Requirements", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            reqRow.transform.SetParent(v, false);
            var reqHLG = reqRow.GetComponent<HorizontalLayoutGroup>();
            reqHLG.spacing = 8;
            reqHLG.childControlWidth = false;
            reqHLG.childControlHeight = false;
            reqHLG.childForceExpandWidth = false;
            reqHLG.childForceExpandHeight = false;
            reqHLG.childAlignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < 3; i++)
            {
                var chip = CreateChip($"Req_{i+1}");
                chip.transform.SetParent(reqRow.transform, false);
            }

            // CTA
            var cta = CreateButton("Btn_CTA", "Resolve");
            cta.transform.SetParent(v, false);

            // Actor anchor (top center, for floating portrait badge)
            var actorAnchor = new GameObject("ActorAnchor", typeof(RectTransform)).GetComponent<RectTransform>();
            actorAnchor.SetParent(rt, false);
            actorAnchor.anchorMin = new Vector2(0.5f, 1f);
            actorAnchor.anchorMax = new Vector2(0.5f, 1f);
            actorAnchor.pivot = new Vector2(0.5f, 0f);
            actorAnchor.anchoredPosition = new Vector2(0, 8); // slightly above the card
            actorAnchor.sizeDelta = new Vector2(0, 0);

            return root;
        }

        private static GameObject CreateTMP(string name, string text, int size, FontStyles style, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
#if UNITY_2022_2_OR_NEWER
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
#else
            // Older Unity/TMP fallback; okay if this logs a deprecation in older branches.
            tmp.enableWordWrapping = false;
#endif
            tmp.color = color ?? Color.white;
            return go;
        }

        private static GameObject CreateChip(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 28);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.10f, 0.55f, 0.55f, 1f); // teal-ish placeholder

            var label = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(go.transform, false);
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = new Vector2(6, 2); lrt.offsetMax = new Vector2(-6, -2);
            label.text = "Req";
            label.fontSize = 20;
#if UNITY_2022_2_OR_NEWER
            label.textWrappingMode = TextWrappingModes.NoWrap;
#else
            label.enableWordWrapping = false;
#endif
            label.color = Color.white;

            return go;
        }

        private static GameObject CreateButton(string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 44);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.10f, 0.70f, 0.70f, 1f); // teal CTA placeholder
            img.type = Image.Type.Sliced;

            var txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            txt.transform.SetParent(go.transform, false);
            var trt = txt.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            txt.text = label;
            txt.fontSize = 26;
            txt.alignment = TextAlignmentOptions.Center;
#if UNITY_2022_2_OR_NEWER
            txt.textWrappingMode = TextWrappingModes.NoWrap;
#else
            txt.enableWordWrapping = false;
#endif
            txt.color = Color.white;

            return go;
        }
    }
}
#endif
