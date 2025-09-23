// Assets/Editor/AQ/Art/FixCanvasBoardLayout.cs
// Deterministically normalizes Canvas_Board + HUD_Board.
// Builds TopBar, StatusRow, and a proper LeadsBar horizontal ScrollRect with Viewport/Content_Leads.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class FixCanvasBoardLayout
    {
        const string TAG = "[AQ Art]";
        static readonly Vector2 REF_RES = new Vector2(1080, 1920);

        // Header metrics
        const float TOPBAR_H  = 176f;
        const float STATUS_H  =  96f;
        const float HUD_SPACE =  12f;

        [MenuItem("AQ/Art/Fix Canvas_Board Layout (deterministic)")]
        public static void Run()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas) { Debug.LogWarning($"{TAG} Canvas_Board not found."); return; }

            Undo.RegisterFullObjectHierarchyUndo(canvas, "Fix Canvas_Board Layout");

            // Canvas + Scaler
            var cv = GetOrAdd<Canvas>(canvas);
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            cv.pixelPerfect = false;

            var cs = GetOrAdd<CanvasScaler>(canvas);
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = REF_RES;
            cs.matchWidthOrHeight  = 0.5f;

            FullStretch(canvas.GetComponent<RectTransform>());

            // HUD_Board
            var hudT = canvas.transform.Find("HUD_Board");
            if (!hudT)
            {
                var go = new GameObject("HUD_Board", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                hudT = go.transform;
            }
            var rtHUD = (RectTransform)hudT;
            FullStretch(rtHUD);

            // Remove conflicting layout components
            foreach (var cmp in hudT.GetComponents<Component>())
            {
                if (cmp is HorizontalLayoutGroup || cmp is ContentSizeFitter) Object.DestroyImmediate(cmp);
            }

            // Vertical flow that CONTROLS child height so LeadsBar gets the leftover space
            var vlg = GetOrAdd<VerticalLayoutGroup>(hudT.gameObject);
            vlg.spacing = HUD_SPACE;
            vlg.padding = new RectOffset(0,0,0,0);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;   // critical: VLG drives heights
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Sections (order matters)
            var top    = EnsureFixed(hudT, "TopBar",     TOPBAR_H);
            var status = EnsureFixed(hudT, "StatusRow",  STATUS_H);
            var leads  = EnsureFlexible(hudT, "LeadsBar");

            // Keep RequirementsHUD but collapse it
            var req = hudT.Find("RequirementsHUD");
            if (req)
            {
                var rt = (RectTransform)req;
                StretchRect(rt, new Vector2(0.5f, 1f), new Vector2(0f, 0f));
                var le = GetOrAdd<LayoutElement>(req.gameObject);
                le.minHeight = 0f;
                le.preferredHeight = 0f;
                le.flexibleHeight = 0f;
            }

            // Build header + labels
            RebuildTopBar.Run();
            BuildStatusRow(status);

            // Build/normalize leads scroller
            BuildLeadsScroll(leads);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"{TAG} Canvas_Board layout normalized.");
        }

        // ---------- build sections ----------

        static Transform EnsureFixed(Transform parent, string name, float height)
        {
            var t = parent.Find(name);
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                t = go.transform;
            }
            var rt = (RectTransform)t;
            StretchRect(rt, new Vector2(0.5f, 1f), new Vector2(0f, height));

            foreach (var c in t.GetComponents<Component>())
            {
                if (c is ContentSizeFitter || c is HorizontalLayoutGroup) Object.DestroyImmediate(c);
            }

            var le = GetOrAdd<LayoutElement>(t.gameObject);
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0f;

            return t;
        }

        static Transform EnsureFlexible(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (!t)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                t = go.transform;
            }
            var rt = (RectTransform)t;
            StretchRect(rt, new Vector2(0.5f, 1f), new Vector2(0f, 0f));

            var le = GetOrAdd<LayoutElement>(rt.gameObject);
            le.minHeight = 0f;
            le.preferredHeight = 0f;
            le.flexibleHeight = 1f; // fills remaining space
            return t;
        }

        static void BuildStatusRow(Transform status)
        {
            for (int i = status.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(status.GetChild(i).gameObject);

            var h = status.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 24f;
            h.padding = new RectOffset(0,0,0,0);
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = false;
            h.childControlHeight = false;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            MakeStat(status, "Text_Solved",           "Solved 0%");
            MakeStat(status, "Text_Evidence",         "Evidence 0");
            MakeStat(status, "Text_Leads",            "Leads 0");
            MakeStat(status, "Text_LastBreakthrough", "Last OK — —");
        }

        static void MakeStat(Transform parent, string name, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(240, 72);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 40;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 260;
            le.preferredHeight = 72;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;
        }

        // ---------- Leads ScrollRect ----------

        static void BuildLeadsScroll(Transform leadsBar)
        {
            // Clear awkward legacy containers but keep known good ones
            var existingScroll = leadsBar.Find("ScrollLeads");
            RectTransform rtScroll;

            if (!existingScroll)
            {
                var scrollGO = new GameObject("ScrollLeads", typeof(RectTransform), typeof(ScrollRect));
                scrollGO.transform.SetParent(leadsBar, false);
                rtScroll = (RectTransform)scrollGO.transform;
            }
            else
            {
                rtScroll = (RectTransform)existingScroll;
                if (!rtScroll.GetComponent<ScrollRect>()) rtScroll.gameObject.AddComponent<ScrollRect>();
                // remove any stray layout comps on the scroll root
                foreach (var c in rtScroll.GetComponents<Component>())
                {
                    if (c is HorizontalLayoutGroup || c is VerticalLayoutGroup || c is ContentSizeFitter) Object.DestroyImmediate(c);
                }
            }

            // Scroll root fills LeadsBar
            FullStretch(rtScroll);

            // Ensure viewport
            var viewportT = rtScroll.Find("Viewport") as RectTransform;
            if (!viewportT)
            {
                var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
                vpGO.transform.SetParent(rtScroll, false);
                viewportT = (RectTransform)vpGO.transform;
            }
            FullStretch(viewportT);
            var vpImg = viewportT.GetComponent<Image>();
            vpImg.color = new Color(1,1,1,0f); // invisible viewport mask

            // Ensure content
            var contentT = (RectTransform)viewportT.Find("Content_Leads");
            if (!contentT)
            {
                var ctGO = new GameObject("Content_Leads", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                ctGO.transform.SetParent(viewportT, false);
                contentT = (RectTransform)ctGO.transform;
            }

            // Content: anchored top-left so it grows to the right
            contentT.anchorMin = new Vector2(0f, 1f);
            contentT.anchorMax = new Vector2(0f, 1f);
            contentT.pivot     = new Vector2(0f, 1f);
            contentT.anchoredPosition = Vector2.zero;
            contentT.sizeDelta = new Vector2(0f, 380f); // row height (adjust if your card height changes)

            var hlg = contentT.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 32f;
            hlg.padding = new RectOffset(24, 24, 24, 24);
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth  = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            // Wire ScrollRect (horizontal list)
            var sr = rtScroll.GetComponent<ScrollRect>();
            sr.horizontal = true;
            sr.vertical   = false;
            sr.content    = contentT;
            sr.viewport   = viewportT;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.inertia = true;
            sr.scrollSensitivity = 24f;
        }

        // ---------- tiny local utils (so this file is standalone) ----------

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c ? c : go.AddComponent<T>();
        }

        public static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
        }

        public static void StretchRect(RectTransform rt, Vector2 pivot, Vector2 sizeDelta)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = pivot;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, sizeDelta.y);
        }
    }
}
#endif
