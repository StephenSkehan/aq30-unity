#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Requirements
{
    public static class RequirementsHUDWire
    {
        private const string CanvasName = "Canvas_Board";
        private const string HudBoard = "HUD_Board";
        private const string TopBar = "TopBar";
        private const string StatusRow = "StatusRow";
        private const string RequirementsHUD = "RequirementsHUD";
        private const string LeadsBar = "LeadsBar";

        [MenuItem("AQ/Requirements/Wire RequirementsHUD (3 tokens)")]
        public static void WireRequirementsHUD()
        {
            var root = GameObject.Find(CanvasName);
            if (!root)
            {
                Debug.LogError("❌ Canvas_Board not found. Open the Merge scene first.");
                return;
            }

            var hud = root.transform.Find(HudBoard);
            if (!hud)
            {
                Debug.LogError("❌ HUD_Board not found.");
                return;
            }

            // Ensure sibling order: TopBar (0), StatusRow (1), RequirementsHUD (2), LeadsBar (3)
            var tb  = hud.Find(TopBar);
            var sr  = hud.Find(StatusRow);
            var req = hud.Find(RequirementsHUD);
            var lb  = hud.Find(LeadsBar);

            if (tb) tb.SetSiblingIndex(0);
            if (sr) sr.SetSiblingIndex(1);

            if (!req)
            {
                var go = new GameObject(RequirementsHUD, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(hud, false);
                req = go.transform;
                var img = go.GetComponent<Image>(); var c = img.color; c.a = 0f; img.color = c; // transparent host
            }
            req.SetSiblingIndex(2);

            if (lb) lb.SetSiblingIndex(3);

            var reqRt = (RectTransform)req;
            reqRt.anchorMin = new Vector2(0, 1);
            reqRt.anchorMax = new Vector2(0, 1);
            reqRt.pivot     = new Vector2(0.5f, 1f);
            reqRt.sizeDelta = new Vector2(1080, 120);

            var le = req.GetComponent<LayoutElement>() ?? req.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 120f;
            le.preferredHeight = 120f;
            le.flexibleHeight = 0f;

            // Inner HLG host
            var host = req.Find("Content") as RectTransform;
            if (!host)
            {
                var go = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                go.transform.SetParent(req, false);
                host = go.GetComponent<RectTransform>();
            }
            var hlg = host.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(24,24,12,12);

            // Ensure 3 ReqItem tokens (160x96)
            for (int i = 0; i < 3; i++)
            {
                var name = $"ReqItem_{i}";
                var t = host.Find(name) as RectTransform;
                if (!t)
                {
                    var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
                    go.transform.SetParent(host, false);
                    t = go.GetComponent<RectTransform>();
                }

                var leItem = t.GetComponent<LayoutElement>();
                leItem.minWidth = 160f;
                leItem.preferredWidth = 160f;
                leItem.minHeight = 96f;
                leItem.preferredHeight = 96f;
                leItem.flexibleWidth = 0f;
                leItem.flexibleHeight = 0f;

                // Build token: Icon(56) | Label | Tick(24)
                EnsureTokenChildren(t);
            }

            MarkSceneDirtyAndPing(req.gameObject, "✅ RequirementsHUD wired: 3 tokens at 120px row height.");
        }

        private static void EnsureTokenChildren(RectTransform token)
        {
            // Icon
            var icon = token.Find("Icon") as RectTransform;
            if (!icon)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(token, false);
                icon = go.GetComponent<RectTransform>();
            }

            // Label
            var label = token.Find("Label") as RectTransform;
            if (!label)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
                go.transform.SetParent(token, false);
                label = go.GetComponent<RectTransform>();
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Requirement";
                tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
                tmp.fontSize = 28f; // legible in 96-high token
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.color = Color.white;
            }

            // Tick
            var tick = token.Find("Tick") as RectTransform;
            if (!tick)
            {
                var go = new GameObject("Tick", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(token, false);
                tick = go.GetComponent<RectTransform>();
            }

            // Layout: use HLG on token to arrange children
            var hlg = token.GetComponent<HorizontalLayoutGroup>() ?? token.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(12,12,12,12);

            // Sizes via LayoutElements
            var leIcon = icon.GetComponent<LayoutElement>() ?? icon.gameObject.AddComponent<LayoutElement>();
            leIcon.minWidth = leIcon.preferredWidth = 56f;
            leIcon.minHeight = leIcon.preferredHeight = 56f;

            var leLabel = label.GetComponent<LayoutElement>() ?? label.gameObject.AddComponent<LayoutElement>();
            leLabel.flexibleWidth = 1f;

            var leTick = tick.GetComponent<LayoutElement>() ?? tick.gameObject.AddComponent<LayoutElement>();
            leTick.minWidth = leTick.preferredWidth = 24f;
            leTick.minHeight = leTick.preferredHeight = 24f;
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
