// Assets/Editor/AQ/UI/Leads/Fix_RequirementsRowOnly.cs
// Safe, minimal fixer: ONLY adjusts RequirementsRow layout on LeadCard objects.
// Changes:
//   • RequirementsRow: Left = 20 (if stretched horizontally)  • PosY = -120
// Nothing else is modified.
//
// Menus:
//   Tools ▸ AQ ▸ Leads ▸ FIX: RequirementsRow layout (Selection + Scene)
//   Tools ▸ AQ ▸ Leads ▸ FIX+APPLY: RequirementsRow layout to prefab(s)

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.UI.Leads
{
    public static class Fix_RequirementsRowOnly
    {
        // Desired settings
        const float kLeft   = 20f;
        const float kPosY   = -120f;

        // ---------- PUBLIC MENU: fix (no prefab apply) ----------
        [MenuItem("Tools/AQ/Leads/FIX: RequirementsRow layout (Selection + Scene)")]
        public static void FixOnly()
        {
            var cards = CollectLeadCardsSelectionOrScene();
            int changed = 0;

            foreach (var card in cards)
            {
                var req = FindRequirementsRow(card);
                if (req == null) continue;

                if (ApplyToReqRow(req))
                    changed++;
            }

            Debug.Log($"[ReqRow FIX] Updated {changed} card(s). (no prefab apply)");
        }

        // ---------- PUBLIC MENU: fix and apply overrides to prefab(s) ----------
        [MenuItem("Tools/AQ/Leads/FIX+APPLY: RequirementsRow layout to prefab(s)")]
        public static void FixAndApplyToPrefabs()
        {
            var cards = CollectLeadCardsSelectionOrScene();
            int changed = 0, applied = 0;

            foreach (var card in cards)
            {
                var req = FindRequirementsRow(card);
                if (req == null) continue;

                if (!ApplyToReqRow(req))
                    continue;

                changed++;

                // Apply ONLY this object's overrides back to the nearest prefab asset, if any.
                if (PrefabUtility.IsPartOfPrefabInstance(req))
                {
                    var source = PrefabUtility.GetCorrespondingObjectFromSource(req);
                    if (source != null)
                    {
                        PrefabUtility.ApplyObjectOverride(
                            req, 
                            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(req), 
                            InteractionMode.UserAction
                        );
                        applied++;
                    }
                }
            }

            Debug.Log($"[ReqRow FIX+APPLY] Updated {changed} card(s), applied to {applied} prefab object(s).");
        }

        // ---------- core: perform the exact minimal changes ----------
        static bool ApplyToReqRow(RectTransform req)
        {
            if (req == null) return false;

            Undo.RecordObject(req, "Fix RequirementsRow");

            bool changed = false;

            // Keep existing anchors/pivot intact. Only adjust Left (if stretched) and Y position.
            // 1) Y position (anchoredPosition.y)
            if (!Mathf.Approximately(req.anchoredPosition.y, kPosY))
            {
                var ap = req.anchoredPosition;
                ap.y = kPosY;
                req.anchoredPosition = ap;
                changed = true;
            }

            // 2) Left inset = 20 if horizontally stretched, else nudge X by +20 (rare fallback).
            bool stretchedHoriz = Mathf.Approximately(req.anchorMin.x, 0f) && Mathf.Approximately(req.anchorMax.x, 1f);
            if (stretchedHoriz)
            {
                if (!Mathf.Approximately(req.offsetMin.x, kLeft))
                {
                    var offMin = req.offsetMin;
                    offMin.x = kLeft; // Left
                    req.offsetMin = offMin;
                    changed = true;
                }
                // Do NOT touch req.offsetMax.x (Right), sizeDelta, or anything else.
            }
            else
            {
                // Fallback: non-stretched—nudge X so the visual left margin behaves similarly.
                float targetX = req.anchoredPosition.x + (kLeft - 0f);
                if (!Mathf.Approximately(req.anchoredPosition.x, targetX))
                {
                    var ap = req.anchoredPosition;
                    ap.x = targetX;
                    req.anchoredPosition = ap;
                    changed = true;
                }
            }

            return changed;
        }

        // ---------- helpers ----------
        static RectTransform FindRequirementsRow(RectTransform cardRoot)
        {
            if (cardRoot == null) return null;
            var child = cardRoot.Find("RequirementsRow");
            return child as RectTransform;
        }

        static HashSet<RectTransform> CollectLeadCardsSelectionOrScene()
        {
            var set = new HashSet<RectTransform>();

            // From selection (explicit + children)
            foreach (var t in Selection.transforms)
            {
                if (t is RectTransform rt && IsLeadCard(rt)) set.Add(rt);
                foreach (var c in t.GetComponentsInChildren<RectTransform>(true))
                    if (IsLeadCard(c)) set.Add(c);
            }

            // Fallback: scan entire scene if nothing selected
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
            // Heuristic: lead cards usually have these children
            bool hasReq = rt.Find("RequirementsRow") != null;
            bool hasTitle = rt.Find("Text_Title") != null;
            return (hasReq && hasTitle) || rt.name.StartsWith("LeadCard");
        }
    }
}
#endif
