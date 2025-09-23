#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;   // ← needed for EditorSceneManager
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Scenes
{
    public static class PolishBoardLayout
    {
        [MenuItem("AQ/Scenes/Board → Polish Layout (anchors, sizes, BG, scroll)")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
                return;
            }

            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas)
            {
                EditorUtility.DisplayDialog("Not Found", "Canvas_Board not found in this scene.", "OK");
                return;
            }

            var hud = canvas.transform.Find("HUD_Board");
            if (!hud)
            {
                EditorUtility.DisplayDialog("Not Found", "HUD_Board not found.", "OK");
                return;
            }

            // 0) BG
            var bg = canvas.transform.Find("BG");
            if (!bg)
            {
                var go = new GameObject("BG", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(canvas.transform, false);
                go.transform.SetSiblingIndex(0);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                go.GetComponent<Image>().color = new Color(0.13f, 0.18f, 0.26f, 1f); // dark navy
            }

            // 1) StatusRow
            var statusRow = hud.Find("StatusRow") as RectTransform;
            if (statusRow)
            {
                AnchorTopStretch(statusRow, left: 24, top: 120, right: 24, height: 72);
                SetTMP(statusRow, "Text_Solved", "Solved 0%");
                SetTMP(statusRow, "Text_Evidence", "Evidence 0");
                SetTMP(statusRow, "Text_Leads", "Leads 3");
                SetTMP(statusRow, "Text_LastBreakthrough", "Last OK — —");
            }

            // 2) LeadsBar + ScrollRect internals
            var leadsBar = hud.Find("LeadsBar") as RectTransform ?? CreateChild(hud, "LeadsBar", 1080, 340);
            AnchorTopStretch(leadsBar, left: 24, top: 210, right: 24, height: 340);

            var scroll = leadsBar.Find("ScrollLeads") as RectTransform;
            if (!scroll)
            {
                var go = new GameObject("ScrollLeads", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
                go.transform.SetParent(leadsBar, false);
                scroll = (RectTransform)go.transform;
                scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
                scroll.offsetMin = Vector2.zero; scroll.offsetMax = Vector2.zero;
                go.GetComponent<Image>().color = new Color(1, 1, 1, 0.06f);
                go.GetComponent<Mask>().showMaskGraphic = false;
            }

            var viewport = scroll.Find("Viewport") as RectTransform;
            if (!viewport)
            {
                var go = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                go.transform.SetParent(scroll, false);
                viewport = (RectTransform)go.transform;
                viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
                viewport.offsetMin = Vector2.zero; viewport.offsetMax = Vector2.zero;
                go.GetComponent<Image>().color = new Color(1, 1, 1, 0.04f);
                go.GetComponent<Mask>().showMaskGraphic = false;
            }

            var content = viewport.Find("Content_Leads") as RectTransform;
            if (!content)
            {
                var go = new GameObject("Content_Leads", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
                go.transform.SetParent(viewport, false);
                content = (RectTransform)go.transform;
                content.anchorMin = new Vector2(0, 0.5f);
                content.anchorMax = new Vector2(0, 0.5f);
                content.pivot = new Vector2(0, 0.5f);
                var hlg = go.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 24;
                hlg.childControlWidth = true;  hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
                var csf = go.GetComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit   = ContentSizeFitter.FitMode.MinSize;
            }

            var sr = scroll.GetComponent<ScrollRect>();
            sr.horizontal = true; sr.vertical = false;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.inertia = true;
            sr.viewport = viewport;
            sr.content  = content;

            // 3) TopBar nudge if present
            var topBar = hud.Find("TopBar") as RectTransform;
            if (topBar) AnchorTopStretch(topBar, left: 24, top: 24, right: 24, height: 80);

            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(canvas.scene);

            Debug.Log("[Board Polish] Anchors, sizes, BG, and scroll rect normalized.");
        }

        private static void AnchorTopStretch(RectTransform rt, float left, float top, float right, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(left, -top - height);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static RectTransform CreateChild(Transform parent, string name, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        private static void SetTMP(Transform parent, string childName, string text)
        {
            var t = parent.Find(childName);
            if (!t) return;
            var tmp = t.GetComponent<TextMeshProUGUI>();
            if (!tmp) tmp = t.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 20; tmp.fontSizeMax = 36;
            tmp.color = Color.white;
        }
    }
}
#endif
