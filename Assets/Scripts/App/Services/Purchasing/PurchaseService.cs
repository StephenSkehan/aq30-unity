using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using AQ.App.Analytics;
using AQ.App.Economy;
using AQ.App.UI.Board;
using AQ.SharedKernel.Economy;

namespace AQ.App.Services.Purchasing
{
    /// <summary>
    /// Unity IAP bridge for the v1 catalog (4 consumables). Auto-installs.
    /// Crediting is gated on BoardSaveSystem.WalletRestored: save restore is a
    /// destructive set-to-saved, so purchases that arrive earlier (queued StoreKit
    /// transactions at boot) are held as Pending and confirmed after restore.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PurchaseService : MonoBehaviour, IDetailedStoreListener
    {
        public const string IngotsSmall  = "com.skehan.allyquinn.ingots_small";
        public const string IngotsMedium = "com.skehan.allyquinn.ingots_medium";
        public const string IngotsLarge  = "com.skehan.allyquinn.ingots_large";
        public const string StarterPack  = "com.skehan.allyquinn.starter_pack";

        public static PurchaseService Instance { get; private set; }
        public static event Action Initialized;
        public static event Action<string> PurchaseSucceeded; // productId
        public static event Action<string, string> PurchaseFailedEvent; // productId, reason

        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private readonly List<Product> _heldUntilRestore = new();

        public bool IsInitialized => _controller != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("__PurchaseService");
            DontDestroyOnLoad(go);
            go.AddComponent<PurchaseService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(IngotsSmall,  ProductType.Consumable);
            builder.AddProduct(IngotsMedium, ProductType.Consumable);
            builder.AddProduct(IngotsLarge,  ProductType.Consumable);
            builder.AddProduct(StarterPack,  ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);

            BoardSaveSystem.WalletRestoreCompleted += OnWalletRestoreCompleted;
        }

        private void OnDestroy()
        {
            BoardSaveSystem.WalletRestoreCompleted -= OnWalletRestoreCompleted;
            if (Instance == this) Instance = null;
        }

        // ---- Public API ----

        public void Buy(string productId)
        {
            if (_controller == null)
            {
                Debug.LogWarning("[IAP] Buy before store initialized: " + productId);
                PurchaseFailedEvent?.Invoke(productId, "store_not_ready");
                return;
            }
            AnalyticsLocator.Instance?.LogEvent("iap_attempt",
                new Dictionary<string, object> { ["product"] = productId });
            _controller.InitiatePurchase(productId);
        }

        /// <summary>Localized price string ("A$1.99") or fallback while offline.</summary>
        public string PriceString(string productId, string fallback)
        {
            var p = _controller?.products.WithID(productId);
            var s = p?.metadata?.localizedPriceString;
            return string.IsNullOrEmpty(s) ? fallback : s;
        }

        /// <summary>All products are consumable, so Apple does not require this,
        /// but it recovers interrupted transaction queues on demand.</summary>
        public void RestorePurchases()
        {
            var apple = _extensions?.GetExtension<IAppleExtensions>();
            apple?.RestoreTransactions((success, error) =>
                Debug.Log($"[IAP] RestoreTransactions: success={success} {error}"));
        }

        // ---- IDetailedStoreListener ----

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("[IAP] Initialized.");
            Initialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
            => OnInitializeFailed(error, null);

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning($"[IAP] Init failed: {error} {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (!BoardSaveSystem.WalletRestored)
            {
                // Credit after restore; store re-delivers nothing, we confirm manually.
                if (!_heldUntilRestore.Contains(args.purchasedProduct))
                    _heldUntilRestore.Add(args.purchasedProduct);
                return PurchaseProcessingResult.Pending;
            }

            Credit(args.purchasedProduct);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
            => OnPurchaseFailedInternal(product, reason.ToString());

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
            => OnPurchaseFailedInternal(product, description.reason.ToString());

        // ---- Internals ----

        private void OnWalletRestoreCompleted()
        {
            if (_heldUntilRestore.Count == 0) return;
            foreach (var product in _heldUntilRestore)
            {
                Credit(product);
                _controller?.ConfirmPendingPurchase(product);
            }
            _heldUntilRestore.Clear();
        }

        private void Credit(Product product)
        {
            var wallet = WalletLocator.Instance;
            if (wallet == null)
            {
                Debug.LogError("[IAP] No wallet at credit time; holding product " + product.definition.id);
                if (!_heldUntilRestore.Contains(product)) _heldUntilRestore.Add(product);
                return;
            }

            string id = product.definition.id;
            string reason = "iap." + id;
            switch (id)
            {
                case IngotsSmall:  wallet.Grant(reason, Reward.Premium(20));  break;
                case IngotsMedium: wallet.Grant(reason, Reward.Premium(60));  break;
                case IngotsLarge:  wallet.Grant(reason, Reward.Premium(150)); break;
                case StarterPack:  wallet.Grant(reason, Reward.Premium(50), Reward.Energy(300)); break;
                default:
                    Debug.LogWarning("[IAP] Unknown product credited: " + id);
                    return;
            }

            AnalyticsLocator.Instance?.LogEvent("iap_purchase", new Dictionary<string, object>
            {
                ["product"]  = id,
                ["price"]    = product.metadata?.localizedPrice.ToString() ?? "",
                ["currency"] = product.metadata?.isoCurrencyCode ?? ""
            });
            PurchaseSucceeded?.Invoke(id);
        }

        private void OnPurchaseFailedInternal(Product product, string reason)
        {
            string id = product != null ? product.definition.id : "<null>";
            Debug.LogWarning($"[IAP] Purchase failed: {id} ({reason})");
            AnalyticsLocator.Instance?.LogEvent("iap_failed", new Dictionary<string, object>
            {
                ["product"] = id,
                ["reason"]  = reason
            });
            PurchaseFailedEvent?.Invoke(id, reason);
        }
    }
}
