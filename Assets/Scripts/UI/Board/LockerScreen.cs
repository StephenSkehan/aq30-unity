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

        private const int Columns = 4;
        private const float SlotSize = 200f;
        private const float SlotGap = 16f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            EvidenceLockerService.ReloadFromDisk();
            BuildHudButton();
            SceneManager.sceneLoaded += (_, _) => { if (_isOpen) Close(); };
        }

        // ---- HUD button: bottom-left, above the overflow bucket (160px + margins) ----

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
            rt.sizeDelta        = new Vector2(140f, 80f);
            rt.anchoredPosition = new Vector2(24f, 224f); // overflow bucket sits at y 32..192

            AQTheme.Round(btnGo.GetComponent<Image>(), AQTheme.Steel);
            var btn = btnGo.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(Toggle);

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp           = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text          = "LOCKER";
            tmp.fontSize      = 30f;
            tmp.color         = AQTheme.Paper;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: true);
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
            panel.sizeDelta        = new Vector2(940f, 1000f);
            panel.anchoredPosition = Vector2.zero;
            // Block dim-close clicks under the panel body.
            var panelBtn = panel.gameObject.AddComponent<Button>();
            panelBtn.transition = Selectable.Transition.None;

            AddLabel(panel, "EVIDENCE LOCKER", 56f, AQTheme.Paper, new Vector2(0f, 430f), new Vector2(860f, 80f), display: true);
            AddLabel(panel, "Stash items off the board. Tap a stored item to bring it back.",
                     26f, AQTheme.PaperDim, new Vector2(0f, 368f), new Vector2(860f, 44f));

            _grid = MakeRect("Grid", panel);
            _grid.anchorMin        = new Vector2(0.5f, 0.5f);
            _grid.anchorMax        = new Vector2(0.5f, 0.5f);
            _grid.pivot            = new Vector2(0.5f, 1f);
            _grid.sizeDelta        = new Vector2((SlotSize + SlotGap) * Columns, 700f);
            _grid.anchoredPosition = new Vector2(0f, 330f);

            var close = MakeButton(panel, "CLOSE", AQTheme.AlertRed, new Vector2(0f, -430f), new Vector2(280f, 90f));
            close.onClick.AddListener(Close);
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

                if (slot < count)
                {
                    // Occupied: item icon, tap to retrieve.
                    AQTheme.Round(img, AQTheme.Card);
                    int index = slot;
                    var data  = EvidenceLockerService.GetAt(index);

                    var iconRt = MakeRect("Icon", cell);
                    iconRt.anchorMin = Vector2.zero;
                    iconRt.anchorMax = Vector2.one;
                    iconRt.offsetMin = new Vector2(16f, 16f);
                    iconRt.offsetMax = new Vector2(-16f, -16f);
                    var icon            = iconRt.gameObject.AddComponent<Image>();
                    icon.sprite         = board != null ? board.SpriteForItem(data.family, data.tier) : null;
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
                    AQTheme.Round(img, AQTheme.SteelDim);
                }
                else if (slot == capacity && EvidenceLockerService.NextSlotPrice > 0)
                {
                    // Next purchasable slot.
                    AQTheme.Round(img, AQTheme.Teal);
                    int price = EvidenceLockerService.NextSlotPrice;
                    AddLabel(cell, $"+ SLOT\n{price} CC", 30f, AQTheme.Paper, Vector2.zero, new Vector2(SlotSize, SlotSize), display: true);
                    var b = cell.gameObject.AddComponent<Button>();
                    b.transition = Selectable.Transition.None;
                    b.onClick.AddListener(BuySlot);
                }
                else
                {
                    // Locked (future slot).
                    AQTheme.Round(img, AQTheme.PanelLine);
                    AddLabel(cell, "LOCKED", 24f, AQTheme.PaperDim, Vector2.zero, new Vector2(SlotSize, 40f));
                }
            }
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
                ToastService.Show("board_full", "Board full — free a slot first.", 2f);
            }
        }

        private static void BuySlot()
        {
            int price = EvidenceLockerService.NextSlotPrice;
            if (EvidenceLockerService.TryBuySlot())
                ToastService.Show("locker_slot", $"Locker slot unlocked (-{price} CC).", 2f);
            else
                ToastService.Show("locker_slot_no", "Not enough CaseCash.", 2f);
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
            AQTheme.Round(go.GetComponent<Image>(), color);

            AddLabel(rt, label, 40f, AQTheme.Paper, Vector2.zero, dims, display: true);
            return go.GetComponent<Button>();
        }
    }
}
