#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    /// <summary>
    /// Binds actor portraits to each lead card so they scroll with Content_Leads.
    /// Creates a per-card ActorOverlay sub-canvas (higher sorting order) to render above the card body.
    /// Reuses existing badges (from FloatLayer_Actors / overlay) or creates Image objects if missing.
    /// </summary>
    public static class LeadsActors_BindToCards
    {
        // Scene paths (current HUD layout)
        private const string HUD_BOARD           = "HUD_Board";
        private const string LEADSBAR            = "HUD_Board/LeadsBar";
        private const string VIEWPORT            = "HUD_Board/LeadsBar/Viewport";
        private const string CONTENT_LEADS       = "HUD_Board/LeadsBar/Viewport/Content_Leads";
        private const string LEGACY_FLOAT        = "FloatLayer_Actors";            // under LeadsBar (old)
        private const string LEGACY_OVERLAY_NAME = "FloatLayer_ActorsOverlay";     // under HUD_Board (old new)

        // New per-card overlay name
        private const string CARD_OVERLAY_NAME   = "ActorOverlay";

        // Visual tuneables
        private const float BadgSize   = 156f;  // portrait square
        private const float XOffset    = 32f;   // push right from card's left edge
        private const float YDrop      = 18f;   // drop down from card's top edge (positive is down)
        private const int   OverlayOrder = 10;  // sub-canvas order inside card to win draw

        [MenuItem("AQ/Leads/Actors • Bind to Cards & Layer Above")]
        public static void Run()
        {
            var hud     = GameObject.Find(HUD_BOARD )?.transform as RectTransform;
            var leads   = GameObject.Find(LEADSBAR   )?.transform as RectTransform;
            var viewport= GameObject.Find(VIEWPORT   )?.transform as RectTransform;
            var content = GameObject.Find(CONTENT_LEADS)?.transform as RectTransform;

            if (!hud || !leads || !viewport || !content)
            {
                Debug.LogError("❌ Expected HUD_Board/LeadsBar/Viewport/Content_Leads not found. Verify wiring first.");
                return;
            }

            // Collect any legacy badges to reuse their sprites
            var legacyFromLeads   = leads.Find(LEGACY_FLOAT) as RectTransform;
            var legacyFromOverlay = hud  .Find(LEGACY_OVERLAY_NAME) as RectTransform;

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            int madeOverlays = 0, bound = 0, moved = 0;

            int cardCount = content.childCount;
            for (int i = 0; i < cardCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // Ensure per-card overlay (sub-canvas with higher sorting order)
                var overlay = card.Find(CARD_OVERLAY_NAME) as RectTransform;
                if (!overlay)
                {
                    overlay = new GameObject(CARD_OVERLAY_NAME, typeof(RectTransform), typeof(Canvas)).GetComponent<RectTransform>();
                    Undo.RegisterCreatedObjectUndo(overlay.gameObject, "Create ActorOverlay");
                    overlay.SetParent(card, false);
                    overlay.anchorMin = new Vector2(0, 1);   // top-left
                    overlay.anchorMax = new Vector2(0, 1);
                    overlay.pivot     = new Vector2(0, 1);
                    overlay.sizeDelta = Vector2.zero;
                    overlay.anchoredPosition = Vector2.zero;

                    var c = overlay.GetComponent<Canvas>();
                    c.overrideSorting = true;
                    c.sortingOrder    = OverlayOrder;

                    madeOverlays++;
                }
                else
                {
                    // Make sure canvas sorting is set correctly
                    var c = overlay.GetComponent<Canvas>() ?? overlay.gameObject.AddComponent<Canvas>();
                    c.overrideSorting = true;
                    c.sortingOrder    = OverlayOrder;
                }

                // Try to get/create a badge Image under this overlay
                Image badgeImg = null;
                if (overlay.childCount > 0)
                    badgeImg = overlay.GetChild(0).GetComponent<Image>();

                if (!badgeImg)
                {
                    var go = new GameObject("ActorBadge", typeof(RectTransform), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(go, "Create ActorBadge");
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(overlay, false);
                    badgeImg = go.GetComponent<Image>();
                }

                // Normalize geometry: place left-top, drop down slightly
                var brt = badgeImg.rectTransform;
                brt.anchorMin = new Vector2(0, 1); // top-left in overlay (which is top-left of card)
                brt.anchorMax = new Vector2(0, 1);
                brt.pivot     = new Vector2(0.5f, 0f); // bottom center of the portrait feels good when it overhangs
                brt.sizeDelta = new Vector2(BadgSize, BadgSize);
                brt.anchoredPosition = new Vector2(XOffset + BadgSize * 0.5f, -YDrop); // half width because pivot is center on X

                // Pick a sprite to show:
                // 1) prefer any legacy badge sprite, then consume it
                Sprite s = TryPopLegacySprite(legacyFromLeads) ?? TryPopLegacySprite(legacyFromOverlay);
                if (s == null)
                {
                    // fallback: try a demo sprite in your art folder (optional)
                    s = badgeImg.sprite; // leave as-is if you already assigned via Attach step
                }

                if (s != null) { badgeImg.sprite = s;  badgeImg.preserveAspect = true; }

                // Set alpha to 1 in case it was low
                badgeImg.color = new Color(1, 1, 1, 1);

                overlay.SetAsLastSibling(); // ensure overlay is drawn after other card children

                bound++;
            }

            // Clean up empty legacy containers
            if (legacyFromLeads && legacyFromLeads.childCount == 0)
                Undo.DestroyObjectImmediate(legacyFromLeads.gameObject);
            if (legacyFromOverlay && legacyFromOverlay.childCount == 0)
                Undo.DestroyObjectImmediate(legacyFromOverlay.gameObject);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"✅ Actor binding complete. Card overlays created: {madeOverlays}, badges bound: {bound}, legacy badges moved: {moved}. Portraits now scroll with cards and render above card content.");
        }

        /// <summary>
        /// Grabs the first child Image sprite from a legacy container, removes that child, and returns the sprite.
        /// Returns null if none available.
        /// </summary>
        private static Sprite TryPopLegacySprite(RectTransform legacyContainer)
        {
            if (!legacyContainer || legacyContainer.childCount == 0) return null;
            for (int i = legacyContainer.childCount - 1; i >= 0; i--)
            {
                var rt = legacyContainer.GetChild(i) as RectTransform;
                var img = rt ? rt.GetComponent<Image>() : null;
                if (img && img.sprite != null)
                {
                    var sprite = img.sprite;
                    Object.DestroyImmediate(rt.gameObject);
                    return sprite;
                }
                if (rt) Object.DestroyImmediate(rt.gameObject);
            }
            return null;
        }
    }
}
#endif
