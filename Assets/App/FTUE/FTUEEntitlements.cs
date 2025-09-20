// Assembly: AQ.App
// File: Assets/App/FTUE/FTUEEntitlements.cs
// Purpose: One-time FTUE grant in EditMode/PlayMode. No Firebase dependency.

using UnityEngine;
using AQ.SharedKernel.Economy;
using AQ.App.Economy;

namespace AQ.App.FTUE
{
    [DisallowMultipleComponent]
    public sealed class FTUEEntitlements : MonoBehaviour
    {
        [Header("One-time entitlements")]
        [Min(0)] public int soft    = 500;
        [Min(0)] public int premium = 0;
        [Min(0)] public int energy  = 10;

        [Header("PlayerPrefs key (toggle to reapply in dev)")]
        public string playerPrefsKey = "aq.ftue.entitlements.v1";

        void Awake()
        {
            if (PlayerPrefs.GetInt(playerPrefsKey, 0) == 1) return;

            var wallet = WalletLocator.Instance ?? new WalletService();
            WalletLocator.Set(wallet); // ensure a shared instance for other components

            wallet.Grant("ftue",
                Reward.Soft(soft),
                Reward.Premium(premium),
                Reward.Energy(energy)
            );

            PlayerPrefs.SetInt(playerPrefsKey, 1);
            PlayerPrefs.Save();
        }
    }
}
