// Assembly: AQ.SharedKernel
// File: Runtime/Economy/WalletService.cs
// Purpose: Deterministic in-memory wallet. Pure C#. Emits local events.

using System;
using System.Collections.Generic;

namespace AQ.SharedKernel.Economy
{
    public sealed class WalletService : IWallet
    {
        private readonly Dictionary<Currency, int> _balances = new()
        {
            { Currency.Soft, 0 },
            { Currency.Premium, 0 },
            { Currency.Energy, 0 },
        };

        public event Action<WalletChanged> Changed;
        public event Action<RewardsGranted> Granted;

        public int Get(Currency currency) => _balances.TryGetValue(currency, out var v) ? v : 0;

        // Convenience overload to match common call sites/tests
        public void Grant(params Reward[] rewards) => Grant(null, rewards);

        public void Grant(string reason = null, params Reward[] rewards)
        {
            if (rewards == null || rewards.Length == 0)
            {
                Granted?.Invoke(new RewardsGranted(Array.Empty<Reward>(), reason));
                return;
            }

            foreach (var r in rewards)
            {
                var oldVal = Get(r.Currency);
                var newVal = checked(oldVal + r.Amount); // throw on overflow
                _balances[r.Currency] = newVal;
                Changed?.Invoke(new WalletChanged(r.Currency, oldVal, newVal, reason ?? "grant"));
            }

            Granted?.Invoke(new RewardsGranted(rewards, reason ?? "grant"));
        }

        public bool TrySpend(Currency currency, int amount, string reason = null)
        {
            if (amount <= 0) return true; // no-op "spend"
            var oldVal = Get(currency);
            if (oldVal < amount) return false;

            var newVal = oldVal - amount;
            _balances[currency] = newVal;
            Changed?.Invoke(new WalletChanged(currency, oldVal, newVal, reason ?? "spend"));
            return true;
        }
    }
}
