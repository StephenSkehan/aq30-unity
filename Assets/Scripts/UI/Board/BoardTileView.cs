using System;
using System.Collections;
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
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private const float LongPressDuration = 0.5f;
        private Coroutine _longPressRoutine;

        public static event Action<BoardTileView> LongHeld;
        MergeBoardController controller;
        int row, col;

        Image bgImage;          // child "Bg"
        public Image itemImage; // child "Item"
        Image energyBadge;      // child "EnergyBadge", created on demand, generators only

        static Sprite _energyBadgeSprite;
        static bool _energyBadgeSpriteLoaded;

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
            GetComponent<AQ.App.UI.Board.FX.GeneratorTileAnimator>()?.Teardown();
            payload.kind = TileKind.Item;
            payload.tier = Mathf.Max(0, tier);
            payload.sprite = sprite;
            Refresh();
        }

        public void Clear()
        {
            GetComponent<AQ.App.UI.Board.FX.GeneratorTileAnimator>()?.Teardown();
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

            RefreshEnergyBadge();
        }

        void RefreshEnergyBadge()
        {
            bool show = payload.kind == TileKind.Generator && itemImage != null && itemImage.enabled;

            if (!show)
            {
                if (energyBadge) energyBadge.enabled = false;
                return;
            }

            if (!energyBadge)
            {
                if (!_energyBadgeSpriteLoaded)
                {
                    _energyBadgeSpriteLoaded = true;
                    _energyBadgeSprite = Resources.Load<Sprite>("App/UI/MergeBoard/energy_badge");
                }
                if (_energyBadgeSprite == null) return;

                var go = new GameObject("EnergyBadge", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(transform, false);
                rt.SetAsLastSibling();
                rt.anchorMin = new Vector2(0.03f, 0.03f);
                rt.anchorMax = new Vector2(0.38f, 0.38f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                energyBadge = go.GetComponent<Image>();
                energyBadge.sprite = _energyBadgeSprite;
                energyBadge.preserveAspect = true;
                energyBadge.raycastTarget = false;
            }

            energyBadge.enabled = true;
        }

        public void SnapToGrid()
        {
            ((RectTransform)transform).anchoredPosition = Vector2.zero;
        }

        // ---------------- input ----------------

        private bool _longPressFired;

        public void OnPointerClick(PointerEventData eventData)
        {
            // A completed long-press must not also count as a tap — on
            // generators that tap would spawn an item.
            if (_longPressFired) { _longPressFired = false; return; }
            controller?.OnTileClicked(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _longPressFired = false;
            if (IsEmpty) return;
            _longPressRoutine = StartCoroutine(LongPressRoutine());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelLongPress();
        }

        private void CancelLongPress()
        {
            if (_longPressRoutine == null) return;
            StopCoroutine(_longPressRoutine);
            _longPressRoutine = null;
        }

        private IEnumerator LongPressRoutine()
        {
            yield return new WaitForSecondsRealtime(LongPressDuration);
            _longPressRoutine = null;
            _longPressFired = true;
            LongHeld?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CancelLongPress();
            if (IsEmpty || !itemImage || !itemImage.enabled) return;

            dragStartRC = controller?.GetIndex(this);

            // Create a non-raycastable ghost and hide source icon
            ghost = DragGhost.Spawn(
                itemImage.sprite,
                (RectTransform)itemImage.transform,
                (RectTransform)controller.boardRoot);

            itemImage.enabled = false;
            if (energyBadge) energyBadge.enabled = false;
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
