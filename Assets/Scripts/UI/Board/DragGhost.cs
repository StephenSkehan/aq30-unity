using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Lightweight UI sprite that follows the pointer during a drag.
    /// IMPORTANT: It never blocks raycasts so the board beneath can be targeted.
    /// </summary>
    [DisallowMultipleComponent]
    public class DragGhost : MonoBehaviour
    {
        RectTransform rect;
        RectTransform parentLayer;
        Image image;
        CanvasGroup group;
        Canvas parentCanvas;

        public RectTransform Rect => rect;
        public RectTransform ParentLayer => parentLayer;
        public Canvas ParentCanvas => parentCanvas;

        public static DragGhost Spawn(Sprite sprite, RectTransform sourceIcon, RectTransform parentForGhost)
        {
            var go = new GameObject("DragGhost", typeof(RectTransform));
            var ghost = go.AddComponent<DragGhost>();

            ghost.parentLayer = parentForGhost;
            ghost.parentCanvas = parentForGhost.GetComponentInParent<Canvas>();

            ghost.rect = go.GetComponent<RectTransform>();
            ghost.rect.SetParent(parentForGhost, false);
            ghost.rect.anchorMin = new Vector2(0.5f, 0.5f);
            ghost.rect.anchorMax = new Vector2(0.5f, 0.5f);
            ghost.rect.pivot    = new Vector2(0.5f, 0.5f);

            ghost.image = go.AddComponent<Image>();
            ghost.image.sprite = sprite;
            ghost.image.preserveAspect = true;
            ghost.image.raycastTarget  = false; // do not block raycasts

            ghost.group = go.AddComponent<CanvasGroup>();
            ghost.group.blocksRaycasts = false; // do not block raycasts
            ghost.group.interactable   = false;
            ghost.group.ignoreParentGroups = true;

            // The board canvas sorts at 0, below the locker/evidence/overflow
            // button canvases (5/5/200) — without this the dragged item slides
            // BEHIND the locker button on drag-to-store. Override sorting lifts
            // the ghost above every HUD strip canvas (modals sit at 9999).
            var sortCanvas = go.AddComponent<Canvas>();
            sortCanvas.overrideSorting = true;
            sortCanvas.sortingOrder    = 5000;

            // Match source icon size if supplied
            if (sourceIcon != null)
                ghost.rect.sizeDelta = sourceIcon.rect.size;

            return ghost;
        }

        public void Despawn()
        {
            if (this != null && gameObject != null)
                Destroy(gameObject);
        }
    }
}
