#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class LeadsBarConformToAudit
    {
        private const string RootGOName   = "LeadsBar";
        private const string OldScroll    = "ScrollLeads";
        private const string ViewportName = "Viewport";
        private const string ContentName  = "Content_Leads";

        [MenuItem("AQ/Leads/Conform to Audit (ScrollRect on LeadsBar)")]
        public static void Run()
        {
            var root = GameObject.Find(RootGOName);
            if (!root) { Debug.LogError("❌ LeadsBar not found at scene root."); return; }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            var rootTF = root.transform as RectTransform;
            EnsureFullStretch(rootTF);

            // 1) Ensure ScrollRect lives ON LeadsBar
            var rootSR = root.GetComponent<ScrollRect>() ?? Undo.AddComponent<ScrollRect>(root);
            var rootImg = root.GetComponent<Image>() ?? Undo.AddComponent<Image>(root);
            rootImg.color = new Color(0,0,0,0);
            if (!root.GetComponent<Mask>() && !root.GetComponent<RectMask2D>())
                Undo.AddComponent<RectMask2D>(root);

            // 2) Ensure Viewport child under LeadsBar
            var viewport = rootTF.Find(ViewportName) as RectTransform;
            if (!viewport)
            {
                viewport = new GameObject(ViewportName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(viewport.gameObject, "Create Viewport");
                viewport.SetParent(rootTF, false);
            }
            EnsureFullStretch(viewport);
            if (!viewport.GetComponent<Mask>() && !viewport.GetComponent<RectMask2D>())
                Undo.AddComponent<RectMask2D>(viewport.gameObject);

            // 3) Ensure Content_Leads child under Viewport
            var content = viewport.Find(ContentName) as RectTransform;
            if (!content)
            {
                content = new GameObject(ContentName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(content.gameObject, "Create Content_Leads");
                content.SetParent(viewport, false);
            }
            content.pivot = new Vector2(0, 0.5f);
            content.anchorMin = new Vector2(0, 0.5f);
            content.anchorMax = new Vector2(0, 0.5f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0, 220);

            var hlg = content.GetComponent<HorizontalLayoutGroup>() ?? Undo.AddComponent<HorizontalLayoutGroup>(content.gameObject);
            hlg.spacing = 24;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.padding = new RectOffset(24, 24, 0, 0);

            var fit = content.GetComponent<ContentSizeFitter>() ?? Undo.AddComponent<ContentSizeFitter>(content.gameObject);
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 4) Wire ScrollRect references
            rootSR.viewport = viewport;
            rootSR.content  = content;
            rootSR.horizontal = true;
            rootSR.vertical = false;
            rootSR.movementType = ScrollRect.MovementType.Clamped;
            rootSR.inertia = true;

            // 5) If an old "ScrollLeads" exists, migrate its children, then remove it
            var old = rootTF.Find(OldScroll);
            if (old)
            {
                // Move any grandchildren if they exist
                var oldViewport = old.Find(ViewportName);
                if (oldViewport && oldViewport != viewport)
                {
                    // Move content/cards under new viewport
                    var oldContent = oldViewport.Find(ContentName);
                    if (oldContent && oldContent != content)
                    {
                        // Move children from oldContent to new content
                        var oc = oldContent as RectTransform;
                        while (oc.childCount > 0)
                            oc.GetChild(0).SetParent(content, false);
                        Undo.DestroyObjectImmediate(oldContent.gameObject);
                    }
                    Undo.DestroyObjectImmediate(oldViewport.gameObject);
                }
                Undo.DestroyObjectImmediate(old.gameObject);
            }

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("✅ Conformed LeadsBar: ScrollRect now on LeadsBar; Viewport and Content_Leads are direct children. Audit should report a ScrollRect.");
        }

        private static void EnsureFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (rt == rt.root) return;
        }
    }
}
#endif
