#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    /// <summary>
    /// Renders actor badges above the LeadsBar by moving them to a HUD overlay layer.
    /// Solves "only a thin sliver visible" caused by sibling draw order / Viewport image.
    /// </summary>
    public static class LeadsActorsFloat_Overlay
    {
        private const string HudBoardPath   = "HUD_Board";
        private const string LeadsBarPath   = "HUD_Board/LeadsBar";
        private const string ContentPath    = "HUD_Board/LeadsBar/Viewport/Content_Leads";
        private const string OldFloatLayer  = "FloatLayer_Actors";          // child under LeadsBar (old location)
        private const string NewOverlayName = "FloatLayer_ActorsOverlay";   // child under HUD_Board (on top)

        // Visual tuning
        private const float BadgeSize       = 156f;
        private const float DropFromTopPx   = 40f; // how far below the card's top edge the portrait "sits"

        [MenuItem("AQ/Leads/Actors • Move to HUD Overlay + Reposition")]
        public static void MoveAndReposition()
        {
            var hud   = GameObject.Find(HudBoardPath)?.transform as RectTransform;
            var leads = GameObject.Find(LeadsBarPath )?.transform as RectTransform;
            var cards = GameObject.Find(ContentPath  )?.transform as RectTransform;

            if (!hud || !leads || !cards)
            {
                Debug.LogError("❌ Expected paths not found. Make sure HUD_Board/LeadsBar/Viewport/Content_Leads exist.");
                return;
            }

            // Ensure overlay container exists under HUD_Board and is drawn last.
            var overlay = hud.Find(NewOverlayName) as RectTransform;
            if (!overlay)
            {
                overlay = new GameObject(NewOverlayName, typeof(RectTransform)).GetComponent<RectTransform>();
                overlay.SetParent(hud, false);
                overlay.anchorMin = overlay.anchorMax = new Vector2(0.5f, 0.5f);
                overlay.pivot     = new Vector2(0.5f, 0.5f);
                overlay.sizeDelta = Vector2.zero;
            }
            overlay.SetAsLastSibling(); // render on top of other HUD children

            // Gather any existing badges (either already in overlay or still under the old layer).
            var legacy = leads.Find(OldFloatLayer) as RectTransform;
            int moved = 0, positioned = 0;

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            // Move legacy children into overlay
            if (legacy)
            {
                for (int i = legacy.childCount - 1; i >= 0; i--)
                {
                    var child = legacy.GetChild(i) as RectTransform;
                    if (!child) continue;
                    Undo.SetTransformParent(child, overlay, "Move actor badge to overlay");
                    moved++;
                }
            }

            // Also include any badges already in overlay
            int badgeCount = overlay.childCount;
            int cardCount  = cards.childCount;
            int count      = Mathf.Min(badgeCount, cardCount);

            // Reposition badges to the top-center of each visible card
            for (int i = 0; i < count; i++)
            {
                var badge = overlay.GetChild(i) as RectTransform;
                var card  = cards.GetChild(i)    as RectTransform;
                if (!badge || !card) continue;

                Undo.RecordObject(badge, "Reposition actor badge");

                // Normalize geometry on the badge
                badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 1f); // top-center anchors on overlay
                badge.pivot     = new Vector2(0.5f, 0f);                   // bottom of the portrait
                badge.sizeDelta = new Vector2(BadgeSize, BadgeSize);

                // Compute the card's world-space top-center
                Vector3 worldTopCenter = card.TransformPoint(new Vector3(card.rect.width * 0.5f, card.rect.height, 0f));
                // Convert to overlay local space
                Vector3 local = overlay.InverseTransformPoint(worldTopCenter);

                // For anchor (0.5,1), anchoredPosition is from overlay's top edge (y = +h/2).
                float topEdgeLocalY = overlay.rect.height * 0.5f;
                float anchoredY     = (local.y - topEdgeLocalY) - DropFromTopPx; // negative goes DOWN from the top
                float anchoredX     = local.x;

                badge.anchoredPosition = new Vector2(anchoredX, anchoredY);
                badge.SetAsLastSibling(); // each badge above previous
                positioned++;
            }

            // Clean up empty legacy container (optional)
            if (legacy && legacy.childCount == 0)
                Undo.DestroyObjectImmediate(legacy.gameObject);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"✅ Actors moved to overlay and positioned. Moved: {moved}, Positioned: {positioned}. Overlay='{NewOverlayName}' under HUD_Board (drawn last).");
        }
    }
}
#endif
