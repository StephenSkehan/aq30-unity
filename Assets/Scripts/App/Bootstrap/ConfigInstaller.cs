// Assets/Scripts/App/Bootstrap/ConfigInstaller.cs
using System;
using UnityEngine;
using AQ.App.Config;
using AQ.App.Economy;
using AQ.App.Services;
using AQ.SharedKernel.Economy;

namespace AQ.App.Bootstrap
{
    /// <summary>
    /// Scene bootstrapper: assigns FeatureFlagsRuntime.Current and EnergyRuntime.Config/Manager.
    /// Put this on any always-loaded object in your starting scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ConfigInstaller : MonoBehaviour
    {
        [Header("Assign from Assets/App/Config")]
        public FeatureFlags featureFlags;
        public EnergyConfig energyConfig;

        private void Awake()
        {
            // Wallet must exist before any other system reads it — FTUEEntitlements
            // only calls WalletLocator.Set() on first boot, so we ensure it here.
            if (WalletLocator.Instance == null)
                WalletLocator.Set(new WalletService());

            if (featureFlags)
                FeatureFlagsRuntime.Current = featureFlags;

            if (energyConfig)
                EnergyRuntime.Config = energyConfig;

            var flags = FeatureFlagsRuntime.Current;
            if (flags != null && flags.EnergySystem && EnergyRuntime.Config != null && EnergyRuntime.Manager == null)
            {
                var cfg = EnergyRuntime.Config;
                EnergyRuntime.Manager = new EnergyManager(0, cfg.Cap, DateTime.UtcNow);
            }
        }
    }
}
