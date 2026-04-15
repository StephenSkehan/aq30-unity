using UnityEngine;

namespace AQ.App.Leads
{
    public static class LeadAnalytics
    {
        public static void LogLeadViewed(LeadData lead)
        {
            Debug.Log($"[analytics] lead_viewed id={lead.leadId} action={lead.ActionType}");
        }

        public static void LogReqsMet(LeadData lead, int metCount)
        {
            Debug.Log($"[analytics] lead_requirements_met id={lead.leadId} met={metCount}");
        }

        public static void LogLeadProceed(LeadData lead)
        {
            Debug.Log($"[analytics] lead_proceed id={lead.leadId} energy_cost={lead.EnergyCost}");
        }

        public static void LogOutcome(LeadData lead, int evidence, int spawned)
        {
            Debug.Log($"[analytics] lead_outcome id={lead.leadId} evidence={evidence} new_leads={spawned}");
        }

        public static void LogPercentSolved(float oldFrac, float newFrac)
        {
            var delta = newFrac - oldFrac;
            Debug.Log($"[analytics] percent_solved_changed old={oldFrac:0.00} new={newFrac:0.00} delta={delta:0.00}");
        }
    }
}
