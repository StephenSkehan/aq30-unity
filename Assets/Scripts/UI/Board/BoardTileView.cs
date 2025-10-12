using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    [DisallowMultipleComponent]
    public class BoardTileView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        MergeBoardController controller;
        int row, col;

        Image bgImage;          // child "Bg"
        public Image itemImage; // child "Item"

        public struct PayloadData
        {
            public TileKind kind;
            public int tier;
            public Sprite sprite;
        }

        PayloadData payload;
        public PayloadData Payload { get => payload; set { payload = value; Refresh(); } }
        public TileKind Kind => payload.kind;
        public int Tier => payload.tier;
        public bool IsEmpty => payload.kind == TileKind.Empty;

        DragGhost ghost;
        (int r, int c)? dragStartRC;

        // ---------------- binding ----------------

        public void Bind(MergeBoardController c, int r, int c0)
        {
            controller = c;
            row = r; col = c0;

            if (!bgImage)
            {
                var t = transform.Find("Bg"); if (t) bgImage = t.GetComponent<Image>();
            }
            if (!itemImage)
            {
                var t = transform.Find("Item"); if (t) itemImage = t.GetComponent<Image>();
            }

            if (bgImage)
            {
                bgImage.enabled = true;
                bgImage.raycastTarget = true;
            }

            if (itemImage)
            {
                itemImage.preserveAspect = true;
                itemImage.raycastTarget  = true;
            }

            Clear();
        }

        // --------------- payload ops ---------------

        public void SetGenerator(Sprite sprite, int tier)
        {
            payload.kind = TileKind.Generator;
            payload.tier = Mathf.Max(0, tier);
            payload.sprite = sprite;
            Refresh();
        }

        public void SetItem(Sprite sprite, int tier)
        {
            payload.kind = TileKind.Item;
            payload.tier = Mathf.Max(0, tier);
            payload.sprite = sprite;
            Refresh();
        }

        public void Clear()
        {
            payload.kind = TileKind.Empty;
            payload.tier = -1;
            payload.sprite = null;
            Refresh();
        }

        public void CopyFrom(BoardTileView other)
        {
            Payload = other.payload;
            other.Clear();
        }

        public void Refresh()
        {
            if (!itemImage) return;

            if (payload.kind == TileKind.Empty || payload.sprite == null)
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
            else
            {
                itemImage.sprite = payload.sprite;
                itemImage.enabled = true;
            }
        }

        public void SnapToGrid()
        {
            ((RectTransform)transform).anchoredPosition = Vector2.zero;
        }

        // ---------------- input ----------------

        public void OnPointerClick(PointerEventData eventData)
        {
            controller?.OnTileClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty || !itemImage || !itemImage.enabled) return;

            dragStartRC = controller?.GetIndex(this);

            // Create a non-raycastable ghost and hide source icon
            ghost = DragGhost.Spawn(
                itemImage.sprite,
                (RectTransform)itemImage.transform,
                (RectTransform)controller.boardRoot);

            itemImage.enabled = false;
            Follow(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghost == null) return;
            Follow(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (ghost != null)
            {
                ghost.Despawn();
                ghost = null;
            }

            // Raycast through ALL GraphicRaycasters via EventSystem
            (int r, int c)? dst = null;
            var target = TileUnderPointer(eventData);
            if (target != null) dst = controller.GetIndex(target);

            controller?.EndDrag(dragStartRC, dst);
            dragStartRC = null;

            // Ensure icon shows at its final slot
            Refresh();
        }

        void Follow(PointerEventData eventData)
        {
            if (ghost == null) return;

            var canvas = ghost.ParentCanvas;
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                ghost.ParentLayer, eventData.position, cam, out var world))
            {
                ghost.Rect.position = world;
            }
        }

        static BoardTileView TileUnderPointer(PointerEventData eventData)
        {
            if (EventSystem.current == null) return null;

            var results = new List<RaycastResult>(16);
            EventSystem.current.RaycastAll(eventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go.name == "DragGhost") continue; // ignore the ghost defensively
                var view = go.GetComponentInParent<BoardTileView>();
                if (view != null) return view;
            }
            return null;
        }
    }
}
