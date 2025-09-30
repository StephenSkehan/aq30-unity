#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Leads
{
    /// <summary>
    /// Idempotent: verifies LeadsBar subtree and repairs ScrollRect/Viewport/Content wiring.
    /// Produces a concise report so audits & humans agree on the same state.
    /// </summary>
    public static class LeadsBarVerifyAndRepair
    {
        private const string RootGOName   = "LeadsBar";
        private const string ScrollName   = "ScrollLeads";
        private const string ViewportName = "Viewport";
        private const string ContentName  = "Content_Leads";

        [MenuItem("AQ/Leads/Verify + Repair LeadsBar")]
        public static void Run()
        {
            var root = GameObject.Find(RootGOName);
            if (!root)
            {
                Debug.LogError($"❌ {RootGOName} not found at scene root.");
                return;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            // Ensure/locate ScrollLeads under LeadsBar
            var scrollTF = root.transform.Find(ScrollName);
            if (!scrollTF)
            {
                scrollTF = new GameObject(ScrollName, typeof(RectTransform)).transform;
                Undo.RegisterCreatedObjectUndo(scrollTF.gameObject, "Create ScrollLeads");
                scrollTF.SetParent(root.transform, false);
                var rt = (RectTransform)scrollTF;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            }

            // Ensure ScrollRect + Image + Mask (or RectMask2D)
            var srGO = scrollTF.gameObject;
            var sr = srGO.GetComponent<ScrollRect>() ?? Undo.AddComponent<ScrollRect>(srGO);
            var img = srGO.GetComponent<Image>() ?? Undo.AddComponent<Image>(srGO);
            img.color = new Color(0,0,0,0); // invisible
            var mask = srGO.GetComponent<Mask>();
            var rmask = srGO.GetComponent<RectMask2D>();
            if (!mask && !rmask) { rmask = Undo.AddComponent<RectMask2D>(srGO); }

            // Ensure/locate Viewport child
            var viewportTF = scrollTF.Find(ViewportName) as RectTransform;
            if (!viewportTF)
            {
                viewportTF = new GameObject(ViewportName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(viewportTF.gameObject, "Create Viewport");
                viewportTF.SetParent(scrollTF, false);
                viewportTF.anchorMin = Vector2.zero; viewportTF.anchorMax = Vector2.one;
                viewportTF.offsetMin = Vector2.zero; viewportTF.offsetMax = Vector2.zero;
            }
            if (!viewportTF.GetComponent<Mask>() && !viewportTF.GetComponent<RectMask2D>())
            {
                Undo.AddComponent<RectMask2D>(viewportTF.gameObject);
            }

            // Ensure/locate Content_Leads inside Viewport
            var contentTF = viewportTF.Find(ContentName) as RectTransform;
            if (!contentTF)
            {
                contentTF = new GameObject(ContentName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(contentTF.gameObject, "Create Content_Leads");
                contentTF.SetParent(viewportTF, false);
            }

            // Layout on Content: HLG + CSF (preferred width; fixed height 220)
            var hlg = contentTF.GetComponent<HorizontalLayoutGroup>() ?? Undo.AddComponent<HorizontalLayoutGroup>(contentTF.gameObject);
            hlg.spacing = 24f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(24, 24, 0, 0);

            var fit = contentTF.GetComponent<ContentSizeFitter>() ?? Undo.AddComponent<ContentSizeFitter>(contentTF.gameObject);
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            contentTF.pivot = new Vector2(0, 0.5f);
            contentTF.anchorMin = new Vector2(0, 0.5f);
            contentTF.anchorMax = new Vector2(0, 0.5f);
            contentTF.anchoredPosition = Vector2.zero;
            contentTF.sizeDelta = new Vector2(0, 220); // fixed card height

            // Wire ScrollRect refs
            sr.viewport = viewportTF;
            sr.content  = contentTF;
            sr.horizontal = true;
            sr.vertical   = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.inertia = true;

            Undo.CollapseUndoOperations(group);

            // Report (so the audit and this agree)
            Debug.Log(
                $"✅ LeadsBar verified:\n" +
                $"- ScrollRect: {(sr ? "OK" : "MISSING")}\n" +
                $"- Viewport:  {(viewportTF ? "OK" : "MISSING")}\n" +
                $"- Content:   {(contentTF ? "OK" : "MISSING")} (HLG + CSF)\n" +
                $"- HLG spacing={hlg.spacing} pad L/R={hlg.padding.left}/{hlg.padding.right} height=220\n" +
                $"- Scroll settings: horizontal={sr.horizontal} vertical={sr.vertical}");
        }

        // Small utility: spawns a single blank card so you can see layout fill
        [MenuItem("AQ/Leads/Spawn Blank LeadCard")]
        private static void SpawnOne()
        {
            var root = GameObject.Find(RootGOName);
            var content = root ? root.transform.Find($"{ScrollName}/{ViewportName}/{ContentName}") as RectTransform : null;
            if (!content) { Debug.LogError("❌ Content_Leads not found. Run 'Verify + Repair LeadsBar' first."); return; }

            Undo.IncrementCurrentGroup();
            var card = new GameObject("LeadCard_Blank", typeof(RectTransform), typeof(LayoutElement), typeof(Image));
            Undo.RegisterCreatedObjectUndo(card, "Create LeadCard_Blank");
            var rt = card.GetComponent<RectTransform>();
            rt.SetParent(content, false);
            rt.sizeDelta = new Vector2(360, 220);
            var le = card.GetComponent<LayoutElement>();
            le.preferredWidth = 360; le.minWidth = 360; le.preferredHeight = 220; le.minHeight = 220;
            card.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
            Debug.Log("🧪 Spawned LeadCard_Blank.");
        }
    }
}
#endif
