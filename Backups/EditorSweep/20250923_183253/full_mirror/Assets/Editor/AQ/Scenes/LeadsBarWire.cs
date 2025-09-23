#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class LeadsBarWire
    {
        private const string CanvasName = "Canvas_Board";
        private const string HudBoard = "HUD_Board";
        private const string TopBar = "TopBar";
        private const string StatusRow = "StatusRow";
        private const string RequirementsHUD = "RequirementsHUD";
        private const string LeadsBar = "LeadsBar";

        private const string ScrollLeads = "ScrollLeads";
        private const string Viewport = "Viewport";
        private const string ContentLeads = "Content_Leads";

        [MenuItem("AQ/Leads/Wire LeadsBar (HLG + Horizontal Scroll)")]
        public static void WireLeadsBarHLG()
        {
            var root = EnsureCanvas();
            var hud = EnsureChild(root.transform, HudBoard, addVlg: true);

            // Ensure row order: TopBar, StatusRow, RequirementsHUD, LeadsBar (don’t create TopBar/StatusRow here)
            EnsureSibling(hud, TopBar, 0, createIfMissing:false);
            EnsureSibling(hud, StatusRow, 1, createIfMissing:false);
            var req = EnsureSibling(hud, RequirementsHUD, 2, createIfMissing:true); // size will be fixed by Requirements script later
            var leads = EnsureSibling(hud, LeadsBar, 3, createIfMissing:true);

            // LeadsBar should be a fixed-height row in the VLG stack
            var le = leads.GetComponent<LayoutElement>() ?? leads.gameObject.AddComponent<LayoutElement>();
            if (le.minHeight < 200f) le.minHeight = 220f;               // sane visual row
            if (le.preferredHeight < 200f) le.preferredHeight = 220f;   // avoids vertical blow-up
            le.flexibleHeight = 0f;

            // Build ScrollRect hierarchy
            var scroll = leads.Find(ScrollLeads)?.GetComponent<RectTransform>();
            if (!scroll)
            {
                var go = new GameObject(ScrollLeads, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
                go.transform.SetParent(leads, false);
                scroll = go.GetComponent<RectTransform>();
            }
            Stretch(scroll);

            var img = scroll.GetComponent<Image>();
            var c = img.color; c.a = 0f; img.color = c; // invisible bg to host the mask

            var sr = scroll.GetComponent<ScrollRect>();
            sr.horizontal = true;
            sr.vertical = false;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 20f;

            var viewport = scroll.Find(Viewport)?.GetComponent<RectTransform>();
            if (!viewport)
            {
                var go = new GameObject(Viewport, typeof(RectTransform), typeof(Image), typeof(Mask));
                go.transform.SetParent(scroll, false);
                viewport = go.GetComponent<RectTransform>();
                var vpImg = go.GetComponent<Image>(); var cc = vpImg.color; cc.a = 0f; vpImg.color = cc;
                var mask = go.GetComponent<Mask>(); mask.showMaskGraphic = false;
            }
            Stretch(viewport);

            var content = viewport.Find(ContentLeads)?.GetComponent<RectTransform>();
            if (!content)
            {
                var go = new GameObject(ContentLeads, typeof(RectTransform));
                go.transform.SetParent(viewport, false);
                content = go.GetComponent<RectTransform>();
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(0, 1);
                content.pivot     = new Vector2(0, 1);
                content.anchoredPosition = Vector2.zero;
            }

            sr.viewport = viewport;
            sr.content  = content;

            // Ensure HLG + Fitter on content (contract)
            var glg = content.GetComponent<GridLayoutGroup>();
            if (glg) Object.DestroyImmediate(glg, true);

            var hlg = content.GetComponent<HorizontalLayoutGroup>() ?? content.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(0, 0, 0, 0);

            var fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit   = ContentSizeFitter.FitMode.MinSize;

            // Normalize names
            scroll.name   = ScrollLeads;
            viewport.name = Viewport;
            content.name  = ContentLeads;

            MarkSceneDirtyAndPing(leads.gameObject, "✅ LeadsBar wired: ScrollRect + HLG content (24 spacing).");
        }

        // ---------- helpers ----------
        private static GameObject EnsureCanvas()
        {
            var go = GameObject.Find(CanvasName);
            if (!go)
            {
                go = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }
            return go;
        }

        private static Transform EnsureChild(Transform parent, string name, bool addVlg)
        {
            var child = parent.Find(name);
            if (!child)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                child = go.transform;
            }
            if (addVlg)
            {
                var vlg = child.GetComponent<VerticalLayoutGroup>() ?? child.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 12f;
                vlg.childControlWidth  = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth  = true;
                vlg.childForceExpandHeight = false;
                vlg.padding = new RectOffset(0,0,0,0);
            }
            Stretch(child as RectTransform);
            return child;
        }

        private static Transform EnsureSibling(Transform parent, string name, int index, bool createIfMissing)
        {
            var t = parent.Find(name);
            if (!t && createIfMissing)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                t = go.transform;
            }
            if (t) t.SetSiblingIndex(Mathf.Clamp(index, 0, parent.childCount - 1));
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            if (!rt) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void MarkSceneDirtyAndPing(Object o, string msg)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.SetDirty(o);
            EditorGUIUtility.PingObject(o);
            Debug.Log(msg);
        }
    }
}
#endif
