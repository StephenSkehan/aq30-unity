// Assembly: AQ.App
// Purpose: Subscribe to Wallet events and forward them to the current analytics backend.
// No Firebase dependency; can be attached to a bootstrap GameObject.

using System.Collections.Generic;
using UnityEngine;
using AQ.App.Economy;
using AQ.App.Analytics;
using AQ.SharedKernel.Economy;

namespace AQ.App.Analytics
{
    [DisallowMultipleComponent]
    public sealed class WalletAnalyticsBridge : MonoBehaviour
    {
        [Tooltip("Optionally create a wallet if none exists (useful in standalone test scenes).")]
        public bool createWalletIfMissing = false;

        private IWallet _wallet;

        void OnEnable()
        {
            TryAttach();
        }

        void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnWalletChanged;
                _wallet.Granted -= OnWalletGranted;
            }
        }

        private void TryAttach()
        {
            _wallet = WalletLocator.Instance;
            if (_wallet == null && createWalletIfMissing)
            {
                _wallet = new WalletService();
                WalletLocator.Set(_wallet);
                Debug.Log("[WalletAnalyticsBridge] Created WalletService (dev-only).");
            }

            if (_wallet == null)
            {
                Debug.LogWarning("[WalletAnalyticsBridge] No wallet instance available; analytics not attached.");
                return;
            }

            _wallet.Changed += OnWalletChanged;
            _wallet.Granted += OnWalletGranted;
        }

        private void OnWalletChanged(WalletChanged e)
        {
            int delta = e.NewValue - e.OldValue;
            var evt = new Dictionary<string, object>
            {
                ["currency"] = e.Currency.ToString(),
                ["old"]      = e.OldValue,
                ["@new"]     = e.NewValue,       // '@' to avoid reserved word collisions
                ["delta"]    = delta,
                ["reason"]   = e.Reason
            };
            AnalyticsLocator.Instance?.LogEvent("economy_changed", evt);

            if (e.Currency == Currency.Energy)
            {
                if (delta > 0)
                    GameAnalytics.LogEnergyGain(delta, e.Reason);
                else if (delta < 0)
                    GameAnalytics.LogEnergySpend(-delta, e.Reason);
            }
        }

        private void OnWalletGranted(RewardsGranted e)
        {
            var evt = new Dictionary<string, object>
            {
                ["rewards_count"] = e.Rewards.Length,
                ["reason"]        = e.Reason
            };
            // Emit total per-currency too (aggregated)
            int soft=0, premium=0, energy=0;
            foreach (var r in e.Rewards)
            {
                switch (r.Currency)
                {
                    case Currency.Soft:    soft    += r.Amount; break;
                    case Currency.Premium: premium += r.Amount; break;
                    case Currency.Energy:  energy  += r.Amount; break;
                }
            }
            evt["soft"]    = soft;
            evt["premium"] = premium;
            evt["energy"]  = energy;

            AnalyticsLocator.Instance?.LogEvent("economy_granted", evt);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (Object.FindFirstObjectByType<WalletAnalyticsBridge>() != null) return;
            var go = new GameObject("WalletAnalyticsBridge_Auto");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<WalletAnalyticsBridge>();
        }
    }
}
