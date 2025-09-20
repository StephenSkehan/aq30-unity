using UnityEngine;

namespace AQ.App.Leads
{
    public enum LeadActionType
    {
        Interview,
        Surveillance,
        LabRequest,
        Favor,
        RecordsPull
    }

    /// <summary>Outcome hints for small UI badges on a card.</summary>
    [System.Flags]
    public enum LeadOutcomeHint
    {
        None       = 0,
        Evidence   = 1 << 0,
        NewLeads   = 1 << 1,
        Rewards    = 1 << 2
    }
}
