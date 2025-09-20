#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Scenes
{
    public static class WireLeadsBarAndSeed
    {
        [MenuItem("AQ/Scenes/Board → Auto-wire LeadsBar & Seed Cards (Robust)")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
                return;
            }

            // -------- 0) Ensure/Find Canvas_Board --------
            GameObject canvas = GameObject.Find("Canvas_Board");
            if (!canvas)
            {
                foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (c && c.transform.Find("HUD_Board")) { canvas = c.gameObject; break; }
            }
            if (!canvas)
            {
                var go = new GameObject("Canvas_Board", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var cv = go.GetComponent<Canvas>();
                cv.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvas = go;
                Debug.Log("[WireLeads] Created Canvas_Board.");
            }
            else
            {
                Debug.Log($"[WireLeads] Using canvas: {canvas.name}");
            }

            // -------- 1) Ensure/Find HUD_Board --------
            var hud = canvas.transform.Find("HUD_Board");
            if (!hud)
            {
                var go = new GameObject("HUD_Board", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;  rt.offsetMax = Vector2.zero;
                hud = go.transform;
                Debug.Log("[WireLeads] Created HUD_Board.");
            }

            // -------- 2) StatusRow (visible life) --------
            var statusRow = hud.Find("StatusRow") as RectTransform;
            if (!statusRow)
            {
                statusRow = new GameObject("StatusRow", typeof(RectTransform)).GetComponent<RectTransform>();
                statusRow.transform.SetParent(hud, false);
                AnchorTopStretch(statusRow, 24, 120, 24, 72);
                MakeTMP(statusRow, "Text_Solved", "Solved 0%");
                MakeTMP(statusRow, "Text_Evidence", "Evidence 0");
                MakeTMP(statusRow, "Text_Leads", "Leads 3");
                MakeTMP(statusRow, "Text_LastBreakthrough", "Last OK — —");
                Debug.Log("[WireLeads] Created StatusRow with texts.");
            }
            else
            {
                AnchorTopStretch(statusRow, 24, 120, 24, 72);
            }

            // -------- 3) LeadsBar + Scroll internals --------
            var leadsBar = hud.Find("LeadsBar") as RectTransform;
            if (!leadsBar)
            {
                leadsBar = new GameObject("LeadsBar", typeof(RectTransform)).GetComponent<RectTransform>();
                leadsBar.transform.SetParent(hud, false);
                Debug.Log("[WireLeads] Created LeadsBar.");
            }
            AnchorTopStretch(leadsBar, 24, 210, 24, 340);

            var scroll = leadsBar.Find("ScrollLeads") as RectTransform;
            if (!scroll)
            {
                var go = new GameObject("ScrollLeads", typeof(RectTransform));
                go.transform.SetParent(leadsBar, false);
                scroll = (RectTransform)go.transform;
                Debug.Log("[WireLeads] Created ScrollLeads.");
            }
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = Vector2.zero;  scroll.offsetMax = Vector2.zero;
            var scrollImg  = EnsureComponent<Image>(scroll.gameObject);
            var scrollMask = EnsureComponent<Mask>(scroll.gameObject);
            scrollImg.color = new Color(1,1,1,0.06f);
            scrollMask.showMaskGraphic = false;
            var sr = EnsureComponent<ScrollRect>(scroll.gameObject);

            var viewport = scroll.Find("Viewport") as RectTransform;
            if (!viewport)
            {
                var go = new GameObject("Viewport", typeof(RectTransform));
                go.transform.SetParent(scroll, false);
                viewport = (RectTransform)go.transform;
                Debug.Log("[WireLeads] Created Viewport.");
            }
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;  viewport.offsetMax = Vector2.zero;
            var vpImg  = EnsureComponent<Image>(viewport.gameObject);
            var vpMask = EnsureComponent<Mask>(viewport.gameObject);
            vpImg.color = new Color(1,1,1,0.04f);
            vpMask.showMaskGraphic = false;

            var content = viewport.Find("Content_Leads") as RectTransform;
            if (!content)
            {
                var go = new GameObject("Content_Leads", typeof(RectTransform));
                go.transform.SetParent(viewport, false);
                content = (RectTransform)go.transform;
                Debug.Log("[WireLeads] Created Content_Leads.");
            }
            var h = EnsureComponent<HorizontalLayoutGroup>(content.gameObject);
            h.padding = new RectOffset(24,24,24,24);
            h.spacing = 24;
            h.childControlWidth = true;  h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            var csf = EnsureComponent<ContentSizeFitter>(content.gameObject);
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit   = ContentSizeFitter.FitMode.MinSize;

            sr.horizontal = true; sr.vertical = false;
            sr.viewport = viewport;
            sr.content  = content;
            sr.movementType = ScrollRect.MovementType.Elastic; sr.inertia = true;

            // -------- 4) LeadsBarView (reflection-safe) --------
            var lbv = leadsBar.GetComponent("AQ.App.Leads.LeadsBarView") as Component;
            if (!lbv)
            {
                var type = System.Type.GetType("AQ.App.Leads.LeadsBarView, Assembly-CSharp") ??
                           System.Type.GetType("AQ.App.Leads.LeadsBarView");
                if (type == null) { EditorUtility.DisplayDialog("Missing Script", "Type AQ.App.Leads.LeadsBarView not found.", "OK"); return; }
                lbv = leadsBar.gameObject.AddComponent(type);
                Debug.Log("[WireLeads] Added LeadsBarView.");
            }
            SetField(lbv, "scrollRect", sr);
            SetField(lbv, "contentRoot", content);

            // -------- 5) Prefabs — card & req --------
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/LeadCardView.prefab");
            var reqPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/ReqItem.prefab");
            if (!cardPrefab || !reqPrefab)
            {
                EditorUtility.DisplayDialog("Missing Prefab",
                    "Expected:\n- Assets/UI/Prefabs/LeadCardView.prefab\n- Assets/UI/Prefabs/ReqItem.prefab",
                    "OK");
                return;
            }
            var lcv = cardPrefab.GetComponent("AQ.App.Leads.LeadCardView") as Component;
            if (!lcv) { EditorUtility.DisplayDialog("Missing Script", "LeadCardView prefab has no LeadCardView component.", "OK"); return; }
            var reqField = lcv.GetType().GetField("requirementItemPrefab");
            if (reqField != null && reqField.GetValue(lcv) == null)
            {
                reqField.SetValue(lcv, reqPrefab);
                EditorUtility.SetDirty(cardPrefab);
                AssetDatabase.SaveAssets();
                Debug.Log("[WireLeads] Patched LeadCardView.requirementItemPrefab → ReqItem.prefab");
            }
            SetField(lbv, "cardPrefab", cardPrefab);

            // -------- 6) Seed initial leads --------
            var lab  = AssetDatabase.LoadAssetAtPath<Object>("Assets/AQ_Seed/Leads/Lead_LabSetup.asset");
            var surv = AssetDatabase.LoadAssetAtPath<Object>("Assets/AQ_Seed/Leads/Lead_Surveillance_KP_CAM_12.asset");
            var recs = AssetDatabase.LoadAssetAtPath<Object>("Assets/AQ_Seed/Leads/Lead_RecordsPull.asset");
            var arrField = lbv.GetType().GetField("initialLeads");
            if (arrField != null)
            {
                var elemType = arrField.FieldType.GetElementType();
                var arr = System.Array.CreateInstance(elemType, 3);
                arr.SetValue(lab, 0);
                arr.SetValue(surv, 1);
                arr.SetValue(recs, 2);
                arrField.SetValue(lbv, arr);
                Debug.Log("[WireLeads] Seeded initialLeads with 3 SOs.");
            }

            // -------- 7) BG comfort layer --------
            if (!canvas.transform.Find("BG"))
            {
                var go = new GameObject("BG", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(canvas.transform, false);
                go.transform.SetSiblingIndex(0);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;  rt.offsetMax = Vector2.zero;
                go.GetComponent<Image>().color = new Color(0.13f, 0.18f, 0.26f, 1f);
                Debug.Log("[WireLeads] Added BG.");
            }

            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(canvas.scene);
            Debug.Log("[WireLeads] ✅ Done. LeadsBar wired, prefabs linked, leads seeded. Press Play.");
        }

        // ---------- helpers ----------
        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (!c) { c = go.AddComponent<T>(); Debug.Log($"[WireLeads] Added missing component: {typeof(T).Name} on {go.name}."); }
            return c;
        }

        private static void SetField(Component target, string field, object value)
        {
            if (!target) return;
            var f = target.GetType().GetField(field);
            if (f != null) f.SetValue(target, value);
        }

        private static void AnchorTopStretch(RectTransform rt, float left, float top, float right, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(left, -top - height);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void MakeTMP(Transform parent, string name, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(240, 88);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18; tmp.fontSizeMax = 34;
            tmp.color = Color.white;
        }
    }
}
#endif
