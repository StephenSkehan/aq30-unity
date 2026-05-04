using UnityEngine;

namespace AQ.App.Leads
{
    public enum LeadActionType
    {
        Evidence   = 0,
        Interview  = 1,
        Data       = 2,
        Location   = 3,
        MoneyTrail = 4,
        Podcast    = 5,
        Discuss    = 6,
        // Post-MVP (reserved for future episodes):
        Timeline   = 7,
        Alibi      = 8,
        Stakeout   = 9,
        Lab        = 10,
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
