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
    }
}
