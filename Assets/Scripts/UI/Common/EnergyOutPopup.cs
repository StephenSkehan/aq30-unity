using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.Economy;
using AQ.App.Services;
using AQ.App.Services.Ads;
using AQ.App.Services.Purchasing;
using AQ.SharedKernel.Economy;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Shown when a spawn fails for lack of energy. One funnel:
    /// (ad — Sprint 6) → ladder refill for ingots → ingot packs (IAP).
    /// </summary>
    public static class EnergyOutPopup
    {
        private static GameObject _root;
        private static TextMeshProUGUI _balanceLbl;
        private static TextMeshProUGUI _refillLbl;
        private static Button _refillBtn;
        private static TextMeshProUGUI _adLbl;
        private static Button _adBtn;

        public static void Show()
        {
            if (_root != null) return;

            _root = new GameObject("__EnergyOutPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000; // below TileInfoPopup/modals at 9999

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

            var panel = MakeRect("Panel", _root.transform);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(660f, 980f);
            panel.anchoredPosition = Vector2.zero;

            AddLabel("OUT OF ENERGY", panel, 52f, AQTheme.Paper, new Vector2(0f, 430f), new Vector2(600f, 70f), bold: true);
            AddLabel("Energy refills over time (+1 every 90s)", panel, 28f,
                     AQTheme.PaperDim, new Vector2(0f, 370f), new Vector2(600f, 40f));

            _balanceLbl = AddLabel("", panel, 34f, AQTheme.Paper,
                                   new Vector2(0f, 305f), new Vector2(600f, 50f));

            // Ladder refill
            _refillBtn = MakeButton("", panel, AQTheme.Teal, new Vector2(0f, 215f), new Vector2(520f, 100f));
            _refillLbl = _refillBtn.GetComponentInChildren<TextMeshProUGUI>();
            _refillBtn.onClick.AddListener(OnRefillClicked);

            _adBtn = MakeButton("", panel, AQTheme.Steel, new Vector2(0f, 100f), new Vector2(520f, 90f));
            _adLbl = _adBtn.GetComponentInChildren<TextMeshProUGUI>();
            _adBtn.onClick.AddListener(OnWatchAdClicked);
            AdService.AvailabilityChanged += RefreshDynamic;

            AddLabel("GET PLATINUM INGOTS", panel, 34f, AQTheme.Amber,
                     new Vector2(0f, 25f), new Vector2(600f, 50f), bold: true);

            MakeProductButton(panel, PurchaseService.IngotsSmall,  "20 Ingots",              "A$1.99", new Vector2(-140f, -75f));
            MakeProductButton(panel, PurchaseService.IngotsMedium, "60 Ingots",              "A$4.99", new Vector2( 140f, -75f));
            MakeProductButton(panel, PurchaseService.IngotsLarge,  "150 Ingots",             "A$9.99", new Vector2(-140f, -215f));
            MakeProductButton(panel, PurchaseService.StarterPack,  "Starter Pack\n300 Energy + 50 Ingots", "A$3.99", new Vector2(140f, -215f));

            var close = MakeButton("Close", panel, AQTheme.AlertRed, new Vector2(0f, -390f), new Vector2(280f, 90f));
            close.onClick.AddListener(Close);

            PurchaseService.PurchaseSucceeded += OnPurchaseSucceeded;
            RefreshDynamic();
        }

        private static void Close()
        {
            if (_root == null) return;
            PurchaseService.PurchaseSucceeded -= OnPurchaseSucceeded;
            AdService.AvailabilityChanged -= RefreshDynamic;
            Object.Destroy(_root);
            _root = null;
            _balanceLbl = null;
            _refillLbl  = null;
            _refillBtn  = null;
            _adLbl      = null;
            _adBtn      = null;
        }

        private static void OnWatchAdClicked()
        {
            var ads = AdService.Instance;
            if (ads == null || !ads.AdReady) return;

            ads.ShowRewardedEnergy(rewarded =>
            {
                if (rewarded)
                {
                    ToastService.Show("ad_energy", $"+{AdService.EnergyPerAd} energy", 1.5f);
                    Close();
                }
                else
                {
                    RefreshDynamic();
                }
            });
        }

        private static void OnRefillClicked()
        {
            if (EnergyLadderService.TryBuyRefill())
            {
                ToastService.Show("energy_refilled", $"+{EnergyLadderService.EnergyPerRefill} energy", 1.5f);
                Close();
            }
            else
            {
                ToastService.Show("need_ingots", "Not enough Platinum Ingots.", 1.8f);
                RefreshDynamic();
            }
        }

        private static void OnPurchaseSucceeded(string productId)
        {
            AnalyticsShim(productId);
            RefreshDynamic();
        }

        private static void AnalyticsShim(string productId)
        {
            // Purchase analytics live in PurchaseService; popup only refreshes UI.
        }

        private static void RefreshDynamic()
        {
            if (_root == null) return;

            var wallet  = WalletLocator.Instance;
            int ingots  = wallet?.Get(Currency.Premium) ?? 0;
            int cost    = EnergyLadderService.NextCost;

            if (_balanceLbl != null)
                _balanceLbl.text = $"You have {ingots} Platinum Ingots";

            if (_refillLbl != null)
                _refillLbl.text = $"Refill +{EnergyLadderService.EnergyPerRefill} Energy  —  {cost} Ingots";

            if (_refillBtn != null)
            {
                bool affordable = ingots >= cost;
                _refillBtn.interactable = affordable;
                _refillBtn.GetComponent<Image>().color = affordable ? AQTheme.Teal : AQTheme.TealDim;
            }

            var ads = AdService.Instance;
            if (_adBtn != null && _adLbl != null)
            {
                int left = ads != null ? ads.ViewsLeftToday : 0;
                bool ready = ads != null && ads.AdReady;
                _adLbl.text = left > 0
                    ? $"Watch Ad  +{AdService.EnergyPerAd} Energy  ({left} left today)"
                    : "Watch Ad  —  come back tomorrow";
                _adBtn.interactable = ready;
                _adBtn.GetComponent<Image>().color = ready ? AQTheme.Steel : AQTheme.SteelDim;
            }
        }

        private static void MakeProductButton(RectTransform parent, string productId,
                                              string contents, string fallbackPrice, Vector2 pos)
        {
            string price = PurchaseService.Instance != null
                ? PurchaseService.Instance.PriceString(productId, fallbackPrice)
                : fallbackPrice;

            var btn = MakeButton($"{contents}\n{price}", parent,
                AQTheme.Card, pos, new Vector2(250f, 120f), fontSize: 28f);
            btn.onClick.AddListener(() => PurchaseService.Instance?.Buy(productId));
        }

        // ---- UI helpers (TileInfoPopup style) ----

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI AddLabel(string text, RectTransform parent, float fontSize, Color color,
                                                Vector2 anchoredPosition, Vector2 sizeDelta, bool bold = false)
        {
            var rt              = MakeRect("Lbl_" + text, parent);
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
                                         Vector2 anchoredPosition, Vector2 size, float fontSize = 36f)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = anchoredPosition;
            AQTheme.Round(go.GetComponent<Image>(), color);

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
