// Assets/Editor/AQ/UI/Leads/Fix_IndexCardOverlayOnly.cs
// Minimal, safe fixer: ONLY adjusts IndexCardOverlay's RectTransform on LeadCard objects.
// It does not modify any other component or layout.
//
// Menu: Tools ▸ AQ ▸ Leads ▸ FIX: IndexCardOverlay layout (Selection + Scene)

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class Fix_IndexCardOverlayOnly
    {
        static readonly Vector2 kOverlayPos  = new Vector2(5f, -10f);
        static readonly Vector2 kOverlaySize = new Vector2(580f, 378f);

        [MenuItem("Tools/AQ/Leads/FIX: IndexCardOverlay layout (Selection + Scene)")]
        public static void Run()
        {
            var cards = CollectLeadCardsSelectionOrScene();
            if (cards.Count == 0)
            {
                Debug.Log("[IndexCardOverlay FIX] No LeadCard objects found.");
                return;
            }

            int fixedCount = 0;

            foreach (var card in cards)
            {
                var overlayTr = card.Find("IndexCardOverlay");
                if (overlayTr == null) continue;

                var rt = overlayTr as RectTransform;
                if (rt == null) continue;

                Undo.RecordObject(rt, "Fix IndexCardOverlay");

                // Anchor preset: Center/Middle
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition = kOverlayPos;
                rt.sizeDelta        = kOverlaySize;

                // Do NOT change sibling order, colors, sprites, or any other components.
                fixedCount++;
            }

            Debug.Log($"[IndexCardOverlay FIX] Updated {fixedCount} card(s).");
        }

        // ---- helpers ----

        static HashSet<RectTransform> CollectLeadCardsSelectionOrScene()
        {
            var set = new HashSet<RectTransform>();

            // 1) From selection (explicit + children)
            foreach (var t in Selection.transforms)
            {
                if (t is RectTransform rt && IsLeadCard(rt)) set.Add(rt);

                foreach (var child in t.GetComponentsInChildren<RectTransform>(true))
                    if (IsLeadCard(child)) set.Add(child);
            }

            // 2) Fallback: scan entire scene
            if (set.Count == 0)
            {
#if UNITY_2023_1_OR_NEWER
                var all = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                var all = Object.FindObjectsOfType<RectTransform>(true);
#endif
                foreach (var rt in all)
                    if (IsLeadCard(rt)) set.Add(rt);
            }

            return set;
        }

        static bool IsLeadCard(RectTransform rt)
        {
            if (rt == null) return false;

            // Heuristic: typical lead card structure or name
            bool hasReq = rt.Find("RequirementsRow") != null;
            bool hasTitle = rt.Find("Text_Title") != null;
            bool looksLike = rt.name.StartsWith("LeadCard");
            return (hasReq && hasTitle) || looksLike;
        }
    }
}
#endif
