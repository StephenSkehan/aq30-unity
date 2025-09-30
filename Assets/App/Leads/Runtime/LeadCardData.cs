using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads
{
    [Serializable]
    public enum LeadCardVisualState
    {
        New,        // no requirements met yet
        InProgress, // at least one met
        Ready       // all met
    }

    [Serializable]
    public enum RewardType { Coins, Energy, Gems, Item }

    [Serializable]
    public class LeadRequirementData
    {
        // e.g. "stakeout_fuel"
        public string groupKey;
        // 1-based tier index in that group
        public int tierIndex = 1;
        // Optional: direct icon reference (otherwise presenter can resolve however you like)
        public Sprite icon;
        // Display label (e.g. "Burger", "Paper Cup")
        public string displayName;
        // Gameplay state
        public bool achieved;
    }

    [Serializable]
    public class LeadRewardData
    {
        public RewardType type;
        public int amount = 0;
        public Sprite icon;
    }

    [Serializable]
    public class LeadCardData
    {
        public string leadId;          // testing/QA convenience
        public string title;           // "Demo Lead"
        public string objective;       // "Collect deli CCTV"
        public List<LeadRequirementData> requirements = new();
        public List<LeadRewardData> rewards = new();
        public LeadCardVisualState visualState = LeadCardVisualState.New;

        public bool HasAnyRequirementMet()
        {
            if (requirements == null || requirements.Count == 0) return false;
            for (int i = 0; i < requirements.Count; i++)
                if (requirements[i].achieved) return true;
            return false;
        }

        public bool AllRequirementsMet()
        {
            if (requirements == null || requirements.Count == 0) return false;
            for (int i = 0; i < requirements.Count; i++)
                if (!requirements[i].achieved) return false;
            return true;
        }
    }
}
