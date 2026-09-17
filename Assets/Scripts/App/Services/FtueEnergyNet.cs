using UnityEngine;
using AQ.App.Analytics;
using AQ.App.UI.Common;
using AQ.SharedKernel.Economy;

namespace AQ.App.Services
{
    /// <summary>
    /// New-player energy safety net (SAS/feature-ftue-onboarding-v1.md, I5;
    /// Travel Town's documented pattern): the first two times a player EVER
    /// runs dry, grant +100 free instead of hard-stopping them mid-learning.
    /// An energy-out during the first session is the worst possible FTUE event
    /// for someone still working out what the game is. After the two grants,
    /// the normal out-of-energy popup (ladder/ads) takes over for good.
    /// Additive early-game energy only — Schedule B drop tables untouched.
    /// </summary>
    public static class FtueEnergyNet
    {
        public const int GrantAmount = 100;
        public const int MaxGrants   = 2;

        private const string CountKey = "aq.ftue.energy_net_used";

        /// <summary>
        /// Called at the energy-gate rejection. True = a net grant fired (the
        /// caller should retry its spend instead of showing the popup).
        /// </summary>
        public static bool TryGrant(IWallet wallet)
        {
            if (wallet == null) return false;

            int used = PlayerPrefs.GetInt(CountKey, 0);
            if (used >= MaxGrants) return false;

            PlayerPrefs.SetInt(CountKey, used + 1);
            PlayerPrefs.Save();

            wallet.Grant("ftue.energy_net", Reward.Energy(GrantAmount));
            // Gerald copy sheet, Stephen-ruled 2026-08-21: the grandad quietly
            // covering the tab — warmth without an endearment (no "love" in
            // player-facing copy; in-story dialogue to Ally only).
            ToastService.Show("energy_net", "On me. +100 energy.", 3f);
            GameAnalytics.LogFtueEvent(used == 0 ? "energy_net_1" : "energy_net_2");
            return true;
        }
    }
}
