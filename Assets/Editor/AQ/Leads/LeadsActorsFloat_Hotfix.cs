#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    /// <summary>
    /// Fixes actor badges placed under LeadsBar/FloatLayer_Actors:
    /// - Sets anchors to top-center (0.5,1), pivot to (0.5,0)
    /// - Recomputes anchoredPosition so the badge sits just above each card
    /// Works with current scene layout reported by Audit.
    /// </summary>
    public static class LeadsActorsFloat_Hotfix
    {
        private const string LeadsBarPath    = "LeadsBar";
        private const string ContentPath     = "LeadsBar/Viewport/Content_Leads";
        private const string FloatLayerName  = "FloatLayer_Actors";
        private const float  YOffsetDown     = 6f;   // small drop from the top edge of the card
        private const float  BadgeWidth      = 156f; // visual size we expect

        [MenuItem("AQ/Leads/Actors • HOTFIX anchors & positions")]
        public static void RunHotfix()
        {
            var leadsBar  = GameObject.Find(LeadsBarPath)?.transform as RectTransform;
            var content   = GameObject.Find(ContentPath )?.transform as RectTransform;
            if (!leadsBar || !content) { Debug.LogError("❌ LeadsBar/Content_Leads not found. Run Verify/Ensure first."); return; }

            var floatLayer = leadsBar.Find(FloatLayerName) as RectTransform;
            if (!floatLayer) { Debug.LogWarning("⚠️ FloatLayer_Actors not found. Run the Attach step first."); return; }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            int fixedCount = 0;
            int limit = Mathf.Min(floatLayer.childCount, content.childCount);

            for (int i = 0; i < limit; i++)
            {
                var badge = floatLayer.GetChild(i) as RectTransform;
                var card  = content.GetChild(i)    as RectTransform;
                if (!badge || !card) continue;

                Undo.RecordObject(badge, "Fix Actor Badge");

                // Normalize geometry so calculations are stable
                badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 1f); // top-center
                badge.pivot     = new Vector2(0.5f, 0f);                   // bottom center of the portrait
                badge.sizeDelta = new Vector2(BadgeWidth, BadgeWidth);

                // Find world top-center of the card
                Vector3 worldTopCenter = card.TransformPoint(new Vector3(card.rect.width * 0.5f, card.rect.height, 0f));
                // Convert to LeadsBar local space
                Vector3 local = leadsBar.InverseTransformPoint(worldTopCenter);

                // LeadsBar usually has pivot (0.5,0.5). For anchor (0.5,1),
                // anchoredPosition is measured from the TOP edge (y = +height/2) and x = center.
                float topEdgeLocalY = leadsBar.rect.height * 0.5f;
                float anchoredY = (local.y - topEdgeLocalY) - YOffsetDown; // negative goes DOWN from top edge
                float anchoredX = local.x;                                 // centered already

                badge.anchoredPosition = new Vector2(anchoredX, anchoredY);
                fixedCount++;
            }

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"✅ Hotfixed {fixedCount} actor badge(s): anchors set to top-center, positions recomputed (down {YOffsetDown}px).");
        }
    }
}
#endif
