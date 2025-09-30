using System;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>Requirement chip on a Lead (label + optional icon + satisfied flag).</summary>
    [Serializable]
    public struct LeadRequirement
    {
        // Serialized fields (keep these names for asset compatibility)
        [SerializeField] private string label;
        [SerializeField] private string itemId;          // optional: an internal id / key (was referenced by runtime)
        [SerializeField] private Sprite icon;
        [SerializeField] private bool satisfied;

        // Backwards/forwards-compatible API (matches callers in LeadCardView & LeadRequirementsHUD)
        public string Label
        {
            get => label;
            set => label = value;
        }

        public string ItemId
        {
            get => itemId;
            set => itemId = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        public bool IsSatisfied => satisfied;

        public bool Satisfied
        {
            get => satisfied;
            set => satisfied = value;
        }

        // Also expose lowercase aliases for any newer code using fields (non-breaking).
        // (Unity serializes private fields above; these aliases are helpers only.)
        public string labelRef => label;
        public Sprite iconRef => icon;
    }
}
