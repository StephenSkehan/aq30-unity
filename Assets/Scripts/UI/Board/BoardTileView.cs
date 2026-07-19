using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AQ.App.Leads;

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
        Image requirementTick;  // child "RequirementTick", created on demand, items only
        Image mergeHint;        // child "MergeHint", created on demand — a pair exists on the board
        Image maxTierGlow;      // child "MaxTierGlow", created on demand — item at family ceiling
        Image dropHighlight;    // child "Highlight" from the slot prefab (cyan, ships disabled)

        // Only one drag exists at a time; the source tile owns the hover state.
        static BoardTileView _hovered;

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

        // ---------------- lifecycle ----------------

        private void OnEnable()
        {
            if (LeadRequirementChecker.Instance != null)
                LeadRequirementChecker.Instance.NeededItemsChanged += RefreshRequirementTick;
            MergeBoardController.BoardCompositionChanged += RefreshMergeHint;
        }

        private void OnDisable()
        {
            if (LeadRequirementChecker.Instance != null)
                LeadRequirementChecker.Instance.NeededItemsChanged -= RefreshRequirementTick;
            MergeBoardController.BoardCompositionChanged -= RefreshMergeHint;
        }

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
            RefreshRequirementTick();
            RefreshMergeHint();
            RefreshMaxTierGlow();
        }

        /// <summary>
        /// Soft amber halo behind items sitting at their family's top tier —
        /// the "you finished this ladder" trophy read.
        /// </summary>
        void RefreshMaxTierGlow()
        {
            bool maxed = !IsEmpty
                      && itemImage != null && itemImage.enabled
                      && controller != null
                      && controller.IsAtFamilyCeiling(this);

            if (!maxed)
            {
                if (maxTierGlow) maxTierGlow.gameObject.SetActive(false);
                return;
            }

            if (!maxTierGlow) maxTierGlow = CreateMaxTierGlow();
            maxTierGlow.gameObject.SetActive(true);
            // Render just beneath the item icon.
            maxTierGlow.transform.SetSiblingIndex(itemImage.transform.GetSiblingIndex());
        }

        Image CreateMaxTierGlow()
        {
            var go = new GameObject("MaxTierGlow", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.01f, 0.01f);
            rt.anchorMax = new Vector2(0.99f, 0.99f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.9f;
            var c = AQTheme.Amber; c.a = 0.32f;
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// Amber spark badge (top-left) when this tile can merge with another tile
        /// currently on the board — the "always a move available" signal. Driven by
        /// MergeBoardController.BoardCompositionChanged.
        /// </summary>
        void RefreshMergeHint()
        {
            // Amber merge-pair badge DISABLED (Stephen-ruled 2026-07-19 — a
            // different mergeability signal will be designed later; the
            // candidate logic stays live for whatever replaces it).
            bool mergeable = false;

            if (!mergeable)
            {
                if (mergeHint) mergeHint.gameObject.SetActive(false);
                return;
            }

            if (!mergeHint) mergeHint = CreateMergeHint();
            mergeHint.gameObject.SetActive(true);
            mergeHint.transform.SetAsLastSibling();
        }

        Image CreateMergeHint()
        {
            var go = new GameObject("MergeHint", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            // Top-left corner: top-right is the requirement tick, bottom-left the
            // energy badge — every badge keeps its own corner.
            rt.anchorMin = new Vector2(0.03f, 0.66f);
            rt.anchorMax = new Vector2(0.34f, 0.97f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var badge = go.GetComponent<Image>();
            badge.sprite = AQTheme.Rounded;
            badge.type   = Image.Type.Sliced;
            badge.pixelsPerUnitMultiplier = 0.35f; // corners overrun -> circular badge
            badge.color  = AQTheme.Amber;
            badge.raycastTarget = false;

            var spark = new GameObject("Spark", typeof(RectTransform), typeof(Image));
            var srt = (RectTransform)spark.transform;
            srt.SetParent(rt, false);
            srt.sizeDelta = new Vector2(13f, 13f);
            srt.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var simg = spark.GetComponent<Image>();
            simg.color = Color.white;
            simg.raycastTarget = false;

            return badge;
        }

        /// <summary>
        /// Shows a green tick badge when this tile's item is currently needed by an
        /// active lead (Gossip-Harbor-style board↔story connection). Driven off
        /// LeadRequirementChecker's live itemId set — generators never qualify.
        /// </summary>
        void RefreshRequirementTick()
        {
            bool needed = payload.kind == TileKind.Item
                       && itemImage != null && itemImage.enabled
                       && controller != null
                       && LeadRequirementChecker.Instance != null
                       && LeadRequirementChecker.Instance.IsItemNeeded(controller.GetItemId(this));

            if (!needed)
            {
                // SetActive, not Image.enabled — the badge has child stroke images
                // that would otherwise linger on their own.
                if (requirementTick) requirementTick.gameObject.SetActive(false);
                return;
            }

            if (!requirementTick) requirementTick = CreateRequirementTick();
            requirementTick.gameObject.SetActive(true);
            requirementTick.transform.SetAsLastSibling();
        }

        Image CreateRequirementTick()
        {
            var go = new GameObject("RequirementTick", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.SetAsLastSibling();
            // Top-right corner, GH-stamp bold: ~46% of the cell, slight overhang
            // past the tile edge.
            rt.anchorMin = new Vector2(0.58f, 0.58f);
            rt.anchorMax = new Vector2(1.04f, 1.04f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var badge = go.GetComponent<Image>();
            badge.sprite = AQTheme.Rounded;
            badge.type   = Image.Type.Sliced;
            badge.pixelsPerUnitMultiplier = 0.35f; // corners overrun -> circular badge
            badge.color  = AQTheme.Success;
            badge.raycastTarget = false;

            AddTickStroke(rt, new Vector2(5f, 12f),  45f, new Vector2(-9f, -2.5f));
            AddTickStroke(rt, new Vector2(5f, 20f), -45f, new Vector2(2f, 1.5f));

            return badge;
        }

        static void AddTickStroke(RectTransform parent, Vector2 size, float zRot, Vector2 pos)
        {
            var go = new GameObject("Stroke", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta        = size;
            rt.anchoredPosition = pos;
            rt.localRotation    = Quaternion.Euler(0f, 0f, zRot);

            var img = go.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = false;
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
            if (requirementTick) requirementTick.gameObject.SetActive(false);
            if (mergeHint) mergeHint.gameObject.SetActive(false);
            if (maxTierGlow) maxTierGlow.gameObject.SetActive(false);
            Follow(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghost == null) return;
            Follow(eventData);

            // Drop-target hover: light the cell under the pointer (visual only —
            // drop resolution stays in OnEndDrag, untouched).
            var over = TileUnderPointer(eventData);
            if (over == this) over = null;
            if (over != _hovered)
            {
                SetHover(_hovered, false);
                SetHover(over, true);
                _hovered = over;
            }
        }

        static void SetHover(BoardTileView tile, bool on)
        {
            if (tile == null) return;
            if (!tile.dropHighlight)
            {
                var t = tile.transform.Find("Highlight");
                if (t) tile.dropHighlight = t.GetComponent<Image>();
                if (!tile.dropHighlight) return;
            }
            tile.dropHighlight.enabled = on;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetHover(_hovered, false);
            _hovered = null;

            if (ghost != null)
            {
                ghost.Despawn();
                ghost = null;
            }

            // Raycast through ALL GraphicRaycasters via EventSystem
            (int r, int c)? dst = null;
            var target = TileUnderPointer(eventData);
            if (target != null) dst = controller.GetIndex(target);

            // Dropped on the locker HUD button? Note it, but let the normal drag
            // lifecycle complete first (state machinery untouched — the store
            // runs after, exactly like the long-press STORE path).
            bool toLocker = dst == null && Kind == TileKind.Item &&
                            AQ.App.UI.Board.LockerScreen.IsOverHudButton(eventData.position);

            controller?.EndDrag(dragStartRC, dst);
            dragStartRC = null;

            // Ensure icon shows at its final slot
            Refresh();

            if (toLocker) controller?.StoreTileToLocker(this);
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
