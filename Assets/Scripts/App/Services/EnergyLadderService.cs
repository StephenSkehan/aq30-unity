using System;
using UnityEngine;
using AQ.App.Economy;
using AQ.SharedKernel.Economy;

namespace AQ.App.Services
{
    /// <summary>
    /// Daily-resetting ingot cost ladder for energy refills. Each refill grants
    /// +100 energy (allowed past the regen cap) and steps the price up:
    /// 10 → 20 → 40 → 80, then flat 80 for the rest of the day.
    /// Resets at local midnight. There are deliberately no cash energy SKUs —
    /// this ladder is the only ingot→energy path, so it throttles bought progress.
    /// </summary>
    public static class EnergyLadderService
    {
        public const int EnergyPerRefill = 100;
        private static readonly int[] StepCosts = { 10, 20, 40, 80 };

        private const string DayKey   = "aq.energy.ladder.day";
        private const string CountKey = "aq.energy.ladder.count";

        public static int RefillsToday
        {
            get
            {
                RolloverIfNewDay();
                return PlayerPrefs.GetInt(CountKey, 0);
            }
        }

        public static int NextCost
        {
            get
            {
                RolloverIfNewDay();
                int count = PlayerPrefs.GetInt(CountKey, 0);
                return StepCosts[Mathf.Min(count, StepCosts.Length - 1)];
            }
        }

        /// <summary>Spends ingots at the current ladder price and grants energy.</summary>
        public static bool TryBuyRefill()
        {
            var wallet = WalletLocator.Instance;
            if (wallet == null) return false;

            int cost = NextCost;
            if (!wallet.TrySpend(Currency.Premium, cost, "energy.ladder"))
                return false;

            wallet.Grant("energy.ladder.refill", Reward.Energy(EnergyPerRefill));

            PlayerPrefs.SetInt(CountKey, PlayerPrefs.GetInt(CountKey, 0) + 1);
            PlayerPrefs.Save();
            return true;
        }

        private static void RolloverIfNewDay()
        {
            // Local-midnight reset. Stored day in the future = device clock moved
            // backwards; clamp to today rather than freezing the ladder.
            int today  = LocalDayStamp(DateTime.Now);
            int stored = PlayerPrefs.GetInt(DayKey, 0);

            if (stored == today) return;

            PlayerPrefs.SetInt(DayKey, today);
            PlayerPrefs.SetInt(CountKey, 0);
            PlayerPrefs.Save();
        }

        private static int LocalDayStamp(DateTime local)
            => local.Year * 10000 + local.Month * 100 + local.Day;
    }
}
