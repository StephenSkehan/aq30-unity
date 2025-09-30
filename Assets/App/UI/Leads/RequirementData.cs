using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.UI.Leads
{
    /// <summary>One required item (any tier) in the group.</summary>
    public sealed class RequirementData
    {
        public string GroupTitle;            // e.g., "Stakeout Fuel"
        public List<Sprite> Tiers;           // icons T1..Tn (can be 1..N)
        public int TierIndex;                // selected tier to highlight
        public bool Met;                     // tick state

        public RequirementData(string groupTitle, List<Sprite> tiers, int tierIndex, bool met = false)
        {
            GroupTitle = groupTitle;
            Tiers = tiers;
            TierIndex = tierIndex;
            Met = met;
        }
    }
}
