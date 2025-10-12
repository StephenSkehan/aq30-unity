#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Strict pass to normalize LeadCard prefab/layout in the scene.
    /// Applies deterministic sizes, anchors, ordering, and common Image/Text settings.
    /// </summary>
    public static class ApplyLeadCardStyle_STRICT
    {
        [MenuItem("AQ/UI/Leads/Style/Apply LeadCard Style (STRICT)")]
        public static void StyleSceneCards()
        {
            var cards = FindAllLeadCardsInScene();
            int changed = 0;

            foreach (var cardRoot in cards)
            {
                if (ApplyStyleToCard(cardRoot))
                    changed++;
            }

            Debug.Log($"[AQ LeadCard Style STRICT] Cards found={cards.Count}, changed={changed}.");
        }

        private static List<RectTransform> FindAllLeadCardsInScene()
        {
            // Unity 6000 API
            var all = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var list = new List<RectTransform>(64);

            foreach (var rt in all)
            {
                if (!rt) continue;
                var go = rt.gameObject;
                if (!go) continue;

                // Heuristic: treat anything with name containing "LeadCard" as a card root
                if (go.name.Contains("LeadCard"))
                {
                    // Ensure it's likely a root (either has CanvasGroup or an Image w/ Raycast)
                    if (go.GetComponent<CanvasGroup>() || (go.GetComponent<Image>() && go.GetComponent<Image>().raycastTarget))
                        list.Add(rt);
                }
            }
            return list.Distinct().ToList();
        }

        private static void EnsureOpaque(Image img)
        {
            if (!img) return;
            var c = img.color;
            if (c.a < 1f) { c.a = 1f; img.color = c; }
            img.enabled = true;
        }

        private static RectTransform EnsureChild(RectTransform parent, string childName)
        {
            var t = parent.Find(childName) as RectTransform;
            if (!t)
            {
                var go = new GameObject(childName, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                t = go.transform as RectTransform;
            }
            return t;
        }

        // Returns true if any change was applied.
        private static bool ApplyStyleToCard(RectTransform cardRoot)
        {
            bool changed = false;
            if (!cardRoot) return false;

            // Normalize card rect
            var rt = cardRoot;
            var size = new Vector2(420, 260);
            if (rt.sizeDelta != size) { rt.sizeDelta = size; changed = true; }
            if (rt.anchorMin != new Vector2(0.5f, 0.5f)) { rt.anchorMin = new Vector2(0.5f, 0.5f); changed = true; }
            if (rt.anchorMax != new Vector2(0.5f, 0.5f)) { rt.anchorMax = new Vector2(0.5f, 0.5f); changed = true; }
            if (rt.pivot != new Vector2(0.5f, 0.5f)) { rt.pivot = new Vector2(0.5f, 0.5f); changed = true; }

            // Ensure common layers exist
            var overlay = EnsureChild(rt, "Overlay");
            var header  = EnsureChild(rt, "Header");
            var body    = EnsureChild(rt, "Body");
            var actors  = EnsureChild(rt, "Actors");
            var reqRow  = EnsureChild(rt, "Requirements");

            // Layout within card (deterministically)
            // Header top strip
            var headerRT = (RectTransform)header;
            if (headerRT.anchorMin != new Vector2(0, 1)) { headerRT.anchorMin = new Vector2(0, 1); changed = true; }
            if (headerRT.anchorMax != new Vector2(1, 1)) { headerRT.anchorMax = new Vector2(1, 1); changed = true; }
            if (headerRT.pivot != new Vector2(0.5f, 1))   { headerRT.pivot   = new Vector2(0.5f, 1); changed = true; }
            var hSize = new Vector2(0, 64);
            if (headerRT.sizeDelta != hSize) { headerRT.sizeDelta = hSize; changed = true; }
            if (headerRT.anchoredPosition != new Vector2(0, 0)) { headerRT.anchoredPosition = Vector2.zero; changed = true; }

            // Body fills under header
            var bodyRT = (RectTransform)body;
            if (bodyRT.anchorMin != new Vector2(0, 0)) { bodyRT.anchorMin = new Vector2(0, 0); changed = true; }
            if (bodyRT.anchorMax != new Vector2(1, 1)) { bodyRT.anchorMax = new Vector2(1, 1); changed = true; }
            if (bodyRT.pivot != new Vector2(0.5f, 0.5f)) { bodyRT.pivot = new Vector2(0.5f, 0.5f); changed = true; }
            if (bodyRT.offsetMin != new Vector2(16, 16)) { bodyRT.offsetMin = new Vector2(16, 16); changed = true; }
            if (bodyRT.offsetMax != new Vector2( -16, -72)) { bodyRT.offsetMax = new Vector2(-16, -72); changed = true; }

            // Requirements row at bottom of body
            var reqRT = (RectTransform)reqRow;
            if (reqRT.parent != bodyRT) { reqRT.SetParent(bodyRT, false); changed = true; }
            if (reqRT.anchorMin != new Vector2(0, 0)) { reqRT.anchorMin = new Vector2(0, 0); changed = true; }
            if (reqRT.anchorMax != new Vector2(1, 0)) { reqRT.anchorMax = new Vector2(1, 0); changed = true; }
            if (reqRT.pivot != new Vector2(0.5f, 0))   { reqRT.pivot   = new Vector2(0.5f, 0); changed = true; }
            var rSize = new Vector2(0, 56);
            if (reqRT.sizeDelta != rSize) { reqRT.sizeDelta = rSize; changed = true; }
            if (reqRT.anchoredPosition != new Vector2(0, 0)) { reqRT.anchoredPosition = Vector2.zero; changed = true; }

            // Actors float overlay (nudged down a bit to sit inside mask)
            var actorsRT = (RectTransform)actors;
            if (actorsRT.parent != bodyRT) { actorsRT.SetParent(bodyRT, false); changed = true; }
            var ap = actorsRT.anchoredPosition;
            if (ap.y != -18f) { ap.y = -18f; actorsRT.anchoredPosition = ap; changed = true; }
            actorsRT.SetAsLastSibling();

            // Overlay always top-most
            var overlayRT = (RectTransform)overlay;
            if (overlayRT.parent != rt) { overlayRT.SetParent(rt, false); changed = true; }
            overlayRT.SetAsLastSibling();

            // Ensure visuals are not accidentally invisible
            EnsureOpaque(rt.GetComponent<Image>());
            EnsureOpaque(headerRT.GetComponent<Image>());
            EnsureOpaque(bodyRT.GetComponent<Image>());
            EnsureOpaque(overlayRT.GetComponent<Image>());

            return changed; // ✅ fixes CS0219 (changed is now meaningful)
        }
    }
}
#endif
