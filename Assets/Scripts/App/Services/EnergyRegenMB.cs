using System;
using UnityEngine;
using AQ.App.Config;
using AQ.App.Economy;
using AQ.SharedKernel.Economy;

namespace AQ.App.Services
{
    /// <summary>
    /// Drives time-based energy regen into the wallet.
    /// This is the single caller of EnergyRuntime.Manager.TickNow — all other
    /// systems read balance from WalletLocator.Instance.Get(Currency.Energy).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnergyRegenMB : MonoBehaviour
    {
        private void Update()
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return;

            var cfg   = EnergyRuntime.Config;
            var mgr   = EnergyRuntime.Manager;
            var wallet = WalletLocator.Instance;
            if (cfg == null || mgr == null || wallet == null) return;

            int ticks = mgr.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);
            if (ticks <= 0) return;

            int current  = wallet.Get(Currency.Energy);
            int headroom = cfg.Cap - current;
            if (headroom <= 0) return;

            int grant = Math.Min(ticks, headroom);
            wallet.Grant("energy.regen", Reward.Energy(grant));
        }
    }
}
