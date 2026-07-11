using System.Collections.Generic;
using AQ.App.Analytics;

namespace AQ.App.Leads
{
    public static class LeadAnalytics
    {
        public static void LogLeadViewed(LeadData lead)
        {
            AnalyticsLocator.Instance?.LogEvent("lead_viewed", new Dictionary<string, object>
            {
                ["lead_id"]     = lead?.leadId     ?? string.Empty,
                ["action_type"] = lead?.ActionType.ToString() ?? string.Empty
            });
        }

        public static void LogReqsMet(LeadData lead, int metCount)
        {
            AnalyticsLocator.Instance?.LogEvent("lead_requirements_met", new Dictionary<string, object>
            {
                ["lead_id"]   = lead?.leadId ?? string.Empty,
                ["met_count"] = metCount
            });
        }

        public static void LogLeadProceed(LeadData lead)
        {
            AnalyticsLocator.Instance?.LogEvent("lead_proceed", new Dictionary<string, object>
            {
                ["lead_id"]     = lead?.leadId     ?? string.Empty,
                ["energy_cost"] = lead?.EnergyCost ?? 0
            });
        }

        public static void LogOutcome(LeadData lead, int evidence, int spawned)
        {
            AnalyticsLocator.Instance?.LogEvent("lead_outcome", new Dictionary<string, object>
            {
                ["lead_id"]   = lead?.leadId ?? string.Empty,
                ["evidence"]  = evidence,
                ["new_leads"] = spawned
            });
        }

        public static void LogPercentSolved(float oldFrac, float newFrac)
        {
            AnalyticsLocator.Instance?.LogEvent("percent_solved_changed", new Dictionary<string, object>
            {
                ["old"]   = System.Math.Round(oldFrac, 2),
                ["@new"]  = System.Math.Round(newFrac, 2),
                ["delta"] = System.Math.Round(newFrac - oldFrac, 2)
            });
        }
    }
}
