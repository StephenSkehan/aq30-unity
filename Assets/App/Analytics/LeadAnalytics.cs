using UnityEngine;

namespace AQ.App.Leads
{
    public static class LeadAnalytics
    {
        public static void LogLeadViewed(LeadCardSO lead)
        {
            Debug.Log($"[analytics] lead_viewed id={lead.LeadCardId} action={lead.ActionType}");
        }

        public static void LogReqsMet(LeadCardSO lead, int metCount)
        {
            Debug.Log($"[analytics] lead_requirements_met id={lead.LeadCardId} met={metCount}");
        }

        public static void LogLeadProceed(LeadCardSO lead)
        {
            Debug.Log($"[analytics] lead_proceed id={lead.LeadCardId} energy_cost={lead.EnergyCost}");
        }

        public static void LogOutcome(LeadCardSO lead, int evidence, int spawned)
        {
            Debug.Log($"[analytics] lead_outcome id={lead.LeadCardId} evidence={evidence} new_leads={spawned}");
        }

        public static void LogPercentSolved(float oldFrac, float newFrac)
        {
            var delta = newFrac - oldFrac;
            Debug.Log($"[analytics] percent_solved_changed old={oldFrac:0.00} new={newFrac:0.00} delta={delta:0.00}");
        }
    }
}
