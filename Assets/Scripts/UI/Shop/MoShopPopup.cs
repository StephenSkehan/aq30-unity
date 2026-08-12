using AQ.App.UI.Board;
using AQ.App.UI.Common;
using AQ.SharedKernel.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Shop
{
    /// <summary>
    /// Mo's Back Room storefront — daily offers from MoShopService.
    /// Opened from the cash pill's + button (ShowMoShopMB). Dynamic canvas,
    /// created on demand, so standard Buttons are reliable here (the
    /// GeneratorInfoPopup precedent).
    /// </summary>
    public static class MoShopPopup
    {
        private static GameObject _root;
        private static TMP_Text   _balance;

        private const float RowH = 128f;

        public static void Show()
        {
            if (_root != null) return;
            if (!MoShopService.Unlocked) return;

            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            var offers = MoShopService.Offers;

            _root = new GameObject("__MoShopPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim    = MakeRect("Dim", _root.transform);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color  = AQTheme.Scrim;
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            float panelH = 264f + offers.Count * (RowH + 14f) + (offers.Count == 0 ? 90f : 0f);
            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(680f, panelH);
            panel.anchoredPosition = Vector2.zero;

            // Fiction tagline stays on the surface; the instructions moved
            // behind the title bar's ? (Stephen-ruled 2026-08-12).
            float y = panelH / 2f - 96f - 18f;
            AddLabel("Neutral ground. Fair prices. No questions.", panel, 26f, AQTheme.PaperDim,
                     new Vector2(0f, y - 20f), new Vector2(620f, 40f));
            y -= 64f;

            if (offers.Count == 0)
            {
                AddLabel("Nothing on the shelf tonight. Come back tomorrow.", panel, 28f, AQTheme.PaperDim,
                         new Vector2(0f, y - 40f), new Vector2(600f, 60f));
            }

            foreach (var offer in offers)
            {
                BuildRow(panel, board, offer, new Vector2(0f, y - RowH / 2f));
                y -= RowH + 14f;
            }

            _balance = AddLabel($"CaseCash: {Balance()}", panel, 28f, AQTheme.Amber,
                                new Vector2(0f, -panelH / 2f + 44f), new Vector2(600f, 40f));

            AQTheme.TitleBar(panel, "MO'S BACK ROOM", Close,
                "Mo restocks at midnight. Items and generators go to the Stash; specials go to the Case Kit. Prices are in CaseCash.");
        }

        private static void BuildRow(RectTransform panel, MergeBoardController board, MoShopOffer offer, Vector2 pos)
        {
            var row = MakeRect("Offer", panel);
            row.anchorMin        = new Vector2(0.5f, 0.5f);
            row.anchorMax        = new Vector2(0.5f, 0.5f);
            row.pivot            = new Vector2(0.5f, 0.5f);
            row.sizeDelta        = new Vector2(620f, RowH);
            row.anchoredPosition = pos;
            var rowBg = row.gameObject.AddComponent<Image>();
            AQTheme.Round(rowBg, AQTheme.Card);
            rowBg.raycastTarget = false;

            Sprite sprite; string label;
            if (offer.isSpecial)
            {
                sprite = Specials.SpecialsTrayView.SpecialSprite(offer.family); // null until art lands
                System.Enum.TryParse<Specials.SpecialId>(offer.family, out var sid);
                var (sName, sDesc, _) = Specials.SpecialItemsService.Catalog[sid];
                label = sName + $"\n<size=20><color=#A5A092>{sDesc}</color></size>";
            }
            else if (offer.isGenerator)
            {
                var so = board.FindGeneratorType(offer.family);
                sprite = so != null ? so.SpriteForTier(offer.tier) : null;
                label  = (so != null && !string.IsNullOrEmpty(so.displayName) ? so.displayName : offer.family)
                         + "\n<size=20><color=#A5A092>Generator · goes to the Stash</color></size>";
            }
            else
            {
                sprite = board.SpriteForItem(offer.family, offer.tier);
                string name = offer.family;
                foreach (var d in board.ItemDefinitions)
                    if (d != null && d.family == offer.family && d.tier == offer.tier) { name = d.displayName; break; }
                label = name + $"\n<size=20><color=#A5A092>Tier {offer.tier + 1}</color></size>";
            }

            var iconRt = MakeRect("Icon", row);
            iconRt.anchorMin        = new Vector2(0f, 0.5f);
            iconRt.anchorMax        = new Vector2(0f, 0.5f);
            iconRt.pivot            = new Vector2(0f, 0.5f);
            iconRt.sizeDelta        = new Vector2(96f, 96f);
            iconRt.anchoredPosition = new Vector2(14f, 0f);
            var icon = iconRt.gameObject.AddComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            if (sprite == null) icon.color = new Color(1f, 1f, 1f, 0.15f);

            var nameRt = MakeRect("Name", row);
            nameRt.anchorMin = new Vector2(0f, 0f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.offsetMin = new Vector2(124f, 8f);
            nameRt.offsetMax = new Vector2(-230f, -8f);
            var nameTmp = nameRt.gameObject.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = label;
            nameTmp.fontSize  = 27f;
            nameTmp.color     = AQTheme.Paper;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.raycastTarget = false;
            AQTheme.StyleText(nameTmp);

            var buy = MakeButton(offer.bought ? "SOLD" : $"{offer.price} CC", row,
                                 offer.bought ? AQTheme.SteelDim : AQTheme.Teal,
                                 new Vector2(620f / 2f - 110f, 0f), new Vector2(196f, 78f), 30f);
            buy.interactable = !offer.bought;
            var captured = offer;
            var buyBtn = buy;
            buy.onClick.AddListener(() =>
            {
                if (MoShopService.TryBuy(captured))
                {
                    SetButtonLabel(buyBtn, "SOLD");
                    AQTheme.RetintButton(buyBtn, AQTheme.SteelDim);
                    buyBtn.interactable = false;
                    if (_balance != null) _balance.text = $"CaseCash: {Balance()}";
                }
                else
                {
                    ToastService.Show("mo_shop_poor", "Not enough CaseCash.", 2f);
                }
            });
        }

        private static int Balance()
        {
            var wallet = Economy.WalletLocator.Instance;
            return wallet != null ? wallet.Get(Currency.Soft) : 0;
        }

        private static void SetButtonLabel(Button btn, string text)
        {
            var tmp = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) tmp.text = text;
        }

        private static void Close()
        {
            if (_root == null) return;
            Object.Destroy(_root);
            _root = null;
            _balance = null;
        }

        // ---- helpers (house popup style) ----

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static TMP_Text AddLabel(string text, RectTransform parent, float fontSize, Color color,
                                         Vector2 anchoredPosition, Vector2 sizeDelta, bool bold = false)
        {
            var rt              = MakeRect("Lbl", parent);
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta        = sizeDelta;
            var tmp             = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text            = text;
            tmp.fontSize        = fontSize;
            tmp.color           = color;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.raycastTarget   = false;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            AQTheme.StyleText(tmp, display: bold);
            return tmp;
        }

        private static Button MakeButton(string label, RectTransform parent, Color color,
                                         Vector2 anchoredPosition, Vector2 size, float fontSize)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = anchoredPosition;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text           = label;
            tmp.fontSize       = fontSize;
            tmp.color          = AQTheme.Paper;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;
            AQTheme.StyleText(tmp, display: true);

            return go.GetComponent<Button>();
        }
    }
}
