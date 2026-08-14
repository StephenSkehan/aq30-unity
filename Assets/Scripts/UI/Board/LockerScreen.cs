using AQ.App.Locker;
using AQ.App.Overflow;
using AQ.App.UI;
using AQ.App.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Evidence Locker panel + its HUD button. Auto-installs like EvidenceBoardScreen.
    /// Store happens via TileInfoPopup's Store button; this screen retrieves and sells slots.
    /// </summary>
    public static class LockerScreen
    {
        private static GameObject _root;      // panel canvas (built on demand)
        private static RectTransform _grid;   // slot grid parent, rebuilt on refresh
        private static bool _isOpen;
        private static RectTransform _hudBtn; // for drag-drop hit testing

        /// <summary>True when the screen point sits on the locker HUD button (drag-to-store).</summary>
        public static bool IsOverHudButton(Vector2 screenPos)
            => _hudBtn != null &&
               RectTransformUtility.RectangleContainsScreenPoint(_hudBtn, screenPos);

        private const int Columns = 4;
        private const float SlotSize = 200f;
        private const float SlotGap = 16f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            // Locker state is restored by BoardSaveSystem.Start (ImportState fires
            // LockerChanged, so panels built here refresh once the aggregate loads).
            BuildHudButton();
            SceneManager.sceneLoaded += (_, _) => { if (_isOpen) Close(); };
        }

        // ---- HUD button: bottom-left, overflow bucket beside it ----

        private static void BuildHudButton()
        {
            var btnRoot = new GameObject("__LockerBtn",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(btnRoot);

            var c          = btnRoot.GetComponent<Canvas>();
            c.renderMode   = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 5;

            var sc                 = btnRoot.GetComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight  = 0.5f;

            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(btnRoot.transform, false);
            var rt              = btnGo.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(0f, 0f);
            rt.pivot            = new Vector2(0f, 0f);
            // Grid-square parity (Stephen-ruled 2026-07-20): 142 = the board's
            // design cell, which BoardFitMB never exceeds — so the button is
            // always at least a cell. BoardFit reserves above the button top,
            // so the grid shrinks to make room automatically.
            rt.sizeDelta        = new Vector2(142f, 142f);
            rt.anchoredPosition = new Vector2(24f, 219f); // level with the evidence-board button

            var img  = btnGo.GetComponent<Image>();
            var icon = Resources.Load<Sprite>("App/UI/Icons/ui_btn_locker");
            if (icon != null)
            {
                img.sprite         = icon; // proper icon art (delivered 2026-07-18)
                img.preserveAspect = true;
            }
            else
            {
                // Fallback: text pill (pre-icon look) if the sprite ever goes missing.
                AQTheme.Round(img, AQTheme.Steel);
                var lbl       = MakeRect("Label", rt);
                lbl.anchorMin = Vector2.zero;
                lbl.anchorMax = Vector2.one;
                lbl.offsetMin = lbl.offsetMax = Vector2.zero;
                var tmp           = lbl.gameObject.AddComponent<TextMeshProUGUI>();
                tmp.text          = "LOCKER";
                tmp.fontSize      = 24f;
                tmp.color         = AQTheme.Paper;
                tmp.alignment     = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                AQTheme.StyleText(tmp, display: true);
            }

            var btn = btnGo.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(Toggle);
            _hudBtn = rt;
        }

        // ---- Panel ----

        public static void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public static void Open()
        {
            if (_isOpen) return;
            BuildPanel();
            _isOpen = true;
            EvidenceLockerService.LockerChanged += Refresh;
            Refresh();
        }

        public static void Close()
        {
            _isOpen = false;
            EvidenceLockerService.LockerChanged -= Refresh;
            if (_root != null) { Object.Destroy(_root); _root = null; _grid = null; }
        }

        private static void BuildPanel()
        {
            _root = new GameObject("__LockerScreen",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas          = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400; // above evidence board (300), below modals (9999)

            var sc                 = _root.GetComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight  = 0.5f;

            var dim    = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color  = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            var dimBtn = dim.gameObject.AddComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(Close);

            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(940f, 1250f); // 4x4 grid since slots 13-16
            panel.anchoredPosition = Vector2.zero;
            // Block dim-close clicks under the panel body.
            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.transition = Selectable.Transition.None;

            // Safe icon watermark behind the slots — same 40% read as the grid
            // backdrop (Stephen-ruled 2026-08-12).
            var markSprite = Resources.Load<Sprite>("App/UI/Icons/ui_btn_locker");
            if (markSprite != null)
            {
                var mark = MakeRect("Watermark", panel);
                mark.anchorMin        = mark.anchorMax = new Vector2(0.5f, 0.5f);
                mark.pivot            = new Vector2(0.5f, 0.5f);
                mark.sizeDelta        = new Vector2(900f, 900f);
                mark.anchoredPosition = new Vector2(0f, -60f);
                var mImg              = mark.gameObject.AddComponent<Image>();
                mImg.sprite           = markSprite;
                mImg.preserveAspect   = true;
                mImg.raycastTarget    = false;
                mImg.color            = new Color(1f, 1f, 1f, 0.8f); // Stephen-ruled 2026-08-14 (was 0.4)
            }

            _grid = MakeRect("Grid", panel);
            _grid.anchorMin        = new Vector2(0.5f, 0.5f);
            _grid.anchorMax        = new Vector2(0.5f, 0.5f);
            _grid.pivot            = new Vector2(0.5f, 1f);
            _grid.sizeDelta        = new Vector2((SlotSize + SlotGap) * Columns, (SlotSize + SlotGap) * 4f);
            _grid.anchoredPosition = new Vector2(0f, 300f);

            // Title bar restyle (Stephen-ruled 2026-08-12): X close + ? help in
            // the bar; the instruction line moved behind the ? and the bottom
            // CLOSE button retired (dim-tap still closes too).
            AQTheme.TitleBar(panel, "EVIDENCE LOCKER", Close,
                "Stash items and generators off the board. Tap an item to bring it back. Buy extra slots with CaseCash.");
        }

        private static void Refresh()
        {
            if (_grid == null) return;
            for (int i = _grid.childCount - 1; i >= 0; i--)
                Object.Destroy(_grid.GetChild(i).gameObject);

            var board = Object.FindFirstObjectByType<MergeBoardController>();
            int capacity = EvidenceLockerService.Capacity;
            int count    = EvidenceLockerService.Count;

            for (int slot = 0; slot < EvidenceLockerService.MaxSlots; slot++)
            {
                int row = slot / Columns, col = slot % Columns;
                var cell = MakeRect($"Slot{slot}", _grid);
                cell.anchorMin        = new Vector2(0f, 1f);
                cell.anchorMax        = new Vector2(0f, 1f);
                cell.pivot            = new Vector2(0f, 1f);
                cell.sizeDelta        = new Vector2(SlotSize, SlotSize);
                cell.anchoredPosition = new Vector2(col * (SlotSize + SlotGap) + SlotGap * 0.5f,
                                                    -row * (SlotSize + SlotGap));

                var img = cell.gameObject.AddComponent<Image>();
                // Slot squares sit at the board grid's 40% read so the safe
                // watermark shows through (0.85 Stephen-ruled 2026-08-14 after
                // an 0.5 look; 40pct original).
                const float slotAlpha = 0.85f;

                if (slot < count)
                {
                    // Occupied: item icon, tap to retrieve.
                    var c = AQTheme.Card; c.a = slotAlpha;
                    AQTheme.Round(img, c);
                    int index = slot;
                    var data  = EvidenceLockerService.GetAt(index);

                    var iconRt = MakeRect("Icon", cell);
                    iconRt.anchorMin = Vector2.zero;
                    iconRt.anchorMax = Vector2.one;
                    iconRt.offsetMin = new Vector2(16f, 16f);
                    iconRt.offsetMax = new Vector2(-16f, -16f);
                    var icon            = iconRt.gameObject.AddComponent<Image>();
                    icon.sprite         = ResolveSprite(board, data);
                    icon.preserveAspect = true;
                    icon.raycastTarget  = false;
                    if (icon.sprite == null) icon.color = new Color(1f, 1f, 1f, 0.2f);

                    var b = cell.gameObject.AddComponent<Button>();
                    b.transition = Selectable.Transition.None;
                    b.onClick.AddListener(() => Retrieve(index));
                }
                else if (slot < capacity)
                {
                    // Unlocked and empty.
                    var c = AQTheme.SteelDim; c.a = slotAlpha;
                    AQTheme.Round(img, c);
                }
                else if (slot == capacity && EvidenceLockerService.NextSlotPrice > 0)
                {
                    // Next purchasable slot — reads as a real button (2026-08-06),
                    // so it stays opaque on purpose.
                    var b = cell.gameObject.AddComponent<Button>();
                    AQTheme.StyleButton(img, AQTheme.Teal);
                    int price = EvidenceLockerService.NextSlotPrice;
                    AddLabel(cell, $"+ SLOT\n{price} CC", 48f, AQTheme.Paper, Vector2.zero, new Vector2(SlotSize, SlotSize), display: true);
                    b.onClick.AddListener(BuySlot);
                }
                else
                {
                    // Locked (future slot).
                    var c = AQTheme.PanelLine; c.a = slotAlpha;
                    AQTheme.Round(img, c);
                    AddLabel(cell, "LOCKED", 40f, AQTheme.PaperDim, Vector2.zero, new Vector2(SlotSize, 52f));
                }
            }
        }

        private static Sprite ResolveSprite(MergeBoardController board, AQ.App.Overflow.OverflowTileData data)
        {
            if (board == null) return null;
            if (data.kind == AQ.App.Overflow.OverflowKind.Generator)
            {
                var so = board.FindGeneratorType(data.family);
                return so != null ? so.SpriteForTier(data.tier) : board.generatorSprite;
            }
            if (data.kind == AQ.App.Overflow.OverflowKind.Special &&
                System.Enum.TryParse<AQ.App.UI.Specials.SpecialId>(data.family, out var sid))
                return AQ.App.UI.Specials.SpecialItemsService.SpriteFor(sid);
            return board.SpriteForItem(data.family, data.tier);
        }

        private static void Retrieve(int index)
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            var data = EvidenceLockerService.GetAt(index);
            if (board.PlaceFromOverflow(data))
            {
                EvidenceLockerService.RemoveAt(index);
                ToastService.Show("locker_retrieve", "Returned to board.", 1.5f);
            }
            else
            {
                ToastService.Show("board_full", "Board full. Free a slot first.", 2f);
            }
        }

        private static void BuySlot()
        {
            int price = EvidenceLockerService.NextSlotPrice;
            if (price < 0) return;
            int slotNo = EvidenceLockerService.Capacity + 1;
            // Purchase confirmation (Stephen-ruled 2026-07-18).
            ConfirmPopup.Show(
                "BUY LOCKER SLOT?",
                $"Unlock slot {slotNo} for {price} CaseCash?",
                "BUY",
                onConfirm: () =>
                {
                    if (EvidenceLockerService.TryBuySlot())
                        ToastService.Show("locker_slot", $"Locker slot unlocked (-{price} CC).", 2f);
                    else
                        ToastService.Show("locker_slot_no", "Not enough CaseCash.", 2f);
                });
        }

        // ---- helpers ----

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLabel(RectTransform parent, string text, float size, Color color,
                                     Vector2 pos, Vector2 dims, bool display = false)
        {
            var rt              = MakeRect("Lbl", parent);
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = dims;
            var tmp             = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text            = text;
            tmp.fontSize        = size;
            tmp.color           = color;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.raycastTarget   = false;
            AQTheme.StyleText(tmp, display: display);
        }

        private static Button MakeButton(RectTransform parent, string label, Color color, Vector2 pos, Vector2 dims)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = dims;
            rt.anchoredPosition = pos;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            AddLabel(rt, label, 40f, AQTheme.Paper, Vector2.zero, dims, display: true);
            return go.GetComponent<Button>();
        }
    }
}
