using UnityEngine;

namespace AQ.App.Leads
{
    [System.Serializable]
    public struct LeadRequirement
    {
        [Tooltip("The merge item identifier (match your item catalog).")]
        public string ItemId;

        [Min(0)]
        [Tooltip("Minimum tier needed to satisfy this requirement.")]
        public int MinTier;

        [Tooltip("Consume the item when proceeding?")]
        public bool ConsumeOnProceed;

        [Header("UI")]
        public Sprite Icon;
        [Tooltip("Optional friendly label (falls back to ItemId).")]
        public string Label;
    }
}
