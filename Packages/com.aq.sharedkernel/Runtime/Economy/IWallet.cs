// Assembly: AQ.SharedKernel (Packages/com.aq.sharedkernel)
// File: Runtime/Economy/IWallet.cs
// Purpose: Minimal wallet interface for sprint-lite economy (no Unity refs)

using System;

namespace AQ.SharedKernel.Economy
{
    public enum Currency
    {
        Soft = 0,
        Premium = 1,
        Energy = 2
    }

    public interface IWallet
    {
        /// <summary>Get current balance for the given currency.</summary>
        int Get(Currency currency);

        /// <summary>
        /// Grant a set of rewards with an optional reason (helps telemetry/debug).
        /// </summary>
        void Grant(string reason = null, params Reward[] rewards);

        /// <summary>
        /// Grant a set of rewards (no reason). Convenience overload to simplify call sites/tests.
        /// </summary>
        void Grant(params Reward[] rewards);

        /// <summary>Try to spend. Returns true if success and applies the debit.</summary>
        bool TrySpend(Currency currency, int amount, string reason = null);

        /// <summary>Raised whenever a single-currency balance changes.</summary>
        event Action<WalletChanged> Changed;

        /// <summary>Raised after a Grant call completes (even if rewards array is empty).</summary>
        event Action<RewardsGranted> Granted;
    }
}
