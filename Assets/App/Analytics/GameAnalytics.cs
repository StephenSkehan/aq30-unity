using System.Collections.Generic;

namespace AQ.App.Analytics
{
    /// <summary>
    /// Named-event helpers for all SAS-defined analytics events.
    /// All methods are null-safe — callers don't need to check AnalyticsLocator.
    /// </summary>
    public static class GameAnalytics
    {
        public static void LogFtueStep(string stepKey, int stepIndex)
        {
            AnalyticsLocator.Instance?.LogEvent("ftue_step", new Dictionary<string, object>
            {
                ["step_key"]   = stepKey   ?? string.Empty,
                ["step_index"] = stepIndex
            });
        }

        /// <summary>
        /// Onboarding funnel milestone (SAS/feature-ftue-onboarding-v1.md, I8).
        /// One event name, one step param — funnel tools sequence them by
        /// timestamp. Review rule: any step dropping &gt;20% of the previous
        /// step's players is a defect, not a stat.
        /// </summary>
        public static void LogFtueEvent(string step)
        {
            AnalyticsLocator.Instance?.LogEvent("ftue_funnel", new Dictionary<string, object>
            {
                ["step"] = step ?? string.Empty
            });
        }

        public static void LogCardStateChange(string leadId, string fromState, string toState)
        {
            AnalyticsLocator.Instance?.LogEvent("card_state_change", new Dictionary<string, object>
            {
                ["lead_id"]    = leadId    ?? string.Empty,
                ["from_state"] = fromState ?? string.Empty,
                ["to_state"]   = toState   ?? string.Empty
            });
        }

        public static void LogCardSubmit(string leadId)
        {
            AnalyticsLocator.Instance?.LogEvent("card_submit", new Dictionary<string, object>
            {
                ["lead_id"] = leadId ?? string.Empty
            });
        }

        public static void LogSpawnRoll(string family, int tier)
        {
            AnalyticsLocator.Instance?.LogEvent("spawn_roll", new Dictionary<string, object>
            {
                ["family"] = family ?? string.Empty,
                ["tier"]   = tier + 1
            });
        }

        public static void LogMerge(string family, int fromTier, int resultTier)
        {
            AnalyticsLocator.Instance?.LogEvent("merge", new Dictionary<string, object>
            {
                ["family"]      = family ?? string.Empty,
                ["from_tier"]   = fromTier + 1,
                ["result_tier"] = resultTier + 1
            });
        }

        public static void LogEpisodeComplete(string episodeId)
        {
            AnalyticsLocator.Instance?.LogEvent("episode_complete", new Dictionary<string, object>
            {
                ["episode_id"] = episodeId ?? string.Empty
            });
        }

        public static void LogEnergyGain(int amount, string reason)
        {
            AnalyticsLocator.Instance?.LogEvent("energy_gain", new Dictionary<string, object>
            {
                ["amount"] = amount,
                ["reason"] = reason ?? string.Empty
            });
        }

        public static void LogEnergySpend(int amount, string reason)
        {
            AnalyticsLocator.Instance?.LogEvent("energy_spend", new Dictionary<string, object>
            {
                ["amount"] = amount,
                ["reason"] = reason ?? string.Empty
            });
        }

        public static void LogLockerSlotPurchased(int slotIndex, int price)
        {
            AnalyticsLocator.Instance?.LogEvent("locker_slot_purchased", new Dictionary<string, object>
            {
                ["slot_index"] = slotIndex,
                ["price"] = price
            });
        }

        public static void LogShopPurchase(string sku, int price, int balanceAfter)
        {
            AnalyticsLocator.Instance?.LogEvent("mo_shop_purchase", new Dictionary<string, object>
            {
                ["sku"] = sku,
                ["price"] = price,
                ["balance_after"] = balanceAfter
            });
        }

        public static void LogDossierFact(string character, int factIndex, int price, int balanceAfter)
        {
            AnalyticsLocator.Instance?.LogEvent("dossier_fact_unlocked", new Dictionary<string, object>
            {
                ["character"] = character,
                ["fact_index"] = factIndex,
                ["price"] = price,
                ["balance_after"] = balanceAfter
            });
        }
    }
}
