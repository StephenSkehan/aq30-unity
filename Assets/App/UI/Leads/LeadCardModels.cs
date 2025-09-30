using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.UI.Leads
{
    [Serializable]
    public class LeadCardData
    {
        public string Title;
        public string ObjectiveOneLiner;
        public string LeadIdForQA; // optional, leave empty to hide
        public Sprite ActorBadge;  // optional

        // Between 1 and 3 requirements
        public List<RequirementData> Requirements = new List<RequirementData>(3);

        // Optional small rewards row (coins / energy / gems / items).
        public List<RewardData> Rewards = new List<RewardData>(0);
    }

    [Serializable]
    public class RequirementData
    {
        // Displayed in the slot:
        public Sprite Icon;         // required (any tier icon)
        public string ShortLabel;   // optional (e.g., "T4")
        public bool Achieved;       // tick overlay

        // For the popup:
        public string GroupTitle;   // e.g., "Stakeout Fuel"
        public List<TierEntry> Tiers = new List<TierEntry>(); // full tier set
        public int HighlightTierIndex = -1; // 0-based index to emphasize (e.g., 3 = T4)
    }

    [Serializable]
    public class TierEntry
    {
        public string Name;  // e.g., "Burger"
        public Sprite Icon;
    }

    [Serializable]
    public class RewardData
    {
        public enum RewardType { Coins, Energy, Gems, Item }
        public RewardType Type;
        public int Amount;
        public Sprite ItemIcon; // if Type == Item
    }
}
