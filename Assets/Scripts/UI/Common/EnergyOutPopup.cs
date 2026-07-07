using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.Economy;
using AQ.App.Services;
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
        private const bool AdsEnabled = false; // Sprint 6 flips this

        private static GameObject _root;
        private static TextMeshProUGUI _balanceLbl;
        private static TextMeshProUGUI _refillLbl;
        private static Button _refillBtn;

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
            dimImg.color  = new Color(0f, 0f, 0f, 0.75f);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;

            var panel = MakeRect("Panel", _root.transform);
            panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 1f);
            panel.anchorMin        = new Vector2(0.5f, 0.5f);
            panel.anchorMax        = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(660f, 980f);
            panel.anchoredPosition = Vector2.zero;

            AddLabel("OUT OF ENERGY", panel, 52f, Color.white, new Vector2(0f, 430f), new Vector2(600f, 70f), bold: true);
            AddLabel("Energy refills over time (+1 every 90s)", panel, 28f,
                     new Color(0.65f, 0.65f, 0.65f), new Vector2(0f, 370f), new Vector2(600f, 40f));

            _balanceLbl = AddLabel("", panel, 34f, new Color(0.85f, 0.85f, 0.95f),
                                   new Vector2(0f, 305f), new Vector2(600f, 50f));

            // Ladder refill
            _refillBtn = MakeButton("", panel, new Color(0.18f, 0.52f, 0.35f), new Vector2(0f, 215f), new Vector2(520f, 100f));
            _refillLbl = _refillBtn.GetComponentInChildren<TextMeshProUGUI>();
            _refillBtn.onClick.AddListener(OnRefillClicked);

#pragma warning disable CS0162
            if (AdsEnabled)
            {
                var adBtn = MakeButton("Watch Ad  +20 Energy", panel,
                    new Color(0.25f, 0.40f, 0.60f), new Vector2(0f, 100f), new Vector2(520f, 90f));
                // Sprint 6: adBtn.onClick → rewarded ad flow
            }
#pragma warning restore CS0162

            AddLabel("GET PLATINUM INGOTS", panel, 34f, new Color(0.80f, 0.75f, 0.55f),
                     new Vector2(0f, 25f), new Vector2(600f, 50f), bold: true);

            MakeProductButton(panel, PurchaseService.IngotsSmall,  "20 Ingots",              "A$1.99", new Vector2(-140f, -75f));
            MakeProductButton(panel, PurchaseService.IngotsMedium, "60 Ingots",              "A$4.99", new Vector2( 140f, -75f));
            MakeProductButton(panel, PurchaseService.IngotsLarge,  "150 Ingots",             "A$9.99", new Vector2(-140f, -215f));
            MakeProductButton(panel, PurchaseService.StarterPack,  "Starter\n+300⚡  50 Ingots", "A$3.99", new Vector2(140f, -215f));

            var close = MakeButton("Close", panel, new Color(0.35f, 0.20f, 0.20f), new Vector2(0f, -390f), new Vector2(280f, 90f));
            close.onClick.AddListener(Close);

            PurchaseService.PurchaseSucceeded += OnPurchaseSucceeded;
            RefreshDynamic();
        }

        private static void Close()
        {
            if (_root == null) return;
            PurchaseService.PurchaseSucceeded -= OnPurchaseSucceeded;
            Object.Destroy(_root);
            _root = null;
            _balanceLbl = null;
            _refillLbl  = null;
            _refillBtn  = null;
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
                _refillBtn.GetComponent<Image>().color = affordable
                    ? new Color(0.18f, 0.52f, 0.35f)
                    : new Color(0.20f, 0.28f, 0.23f);
            }
        }

        private static void MakeProductButton(RectTransform parent, string productId,
                                              string contents, string fallbackPrice, Vector2 pos)
        {
            string price = PurchaseService.Instance != null
                ? PurchaseService.Instance.PriceString(productId, fallbackPrice)
                : fallbackPrice;

            var btn = MakeButton($"{contents}\n{price}", parent,
                new Color(0.22f, 0.22f, 0.30f), pos, new Vector2(250f, 120f), fontSize: 28f);
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
            go.GetComponent<Image>().color = color;

            var lbl       = MakeRect("Label", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text           = label;
            tmp.fontSize       = fontSize;
            tmp.color          = Color.white;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;

            return go.GetComponent<Button>();
        }
    }
}
