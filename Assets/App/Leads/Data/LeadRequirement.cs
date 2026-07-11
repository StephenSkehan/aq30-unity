using System;
using UnityEngine;
using AQ.App.Items;

namespace AQ.App.Leads
{
    /// <summary>
    /// Requirement chip on a Lead card.
    /// Assign <see cref="itemDefinition"/> in the Inspector to wire automatic
    /// board-matching. The label and icon fall back to the legacy serialized
    /// fields when itemDefinition is null, preserving existing asset data.
    /// </summary>
    [Serializable]
    public struct LeadRequirement
    {
        // Legacy display fields — kept for backward compat with existing assets.
        [SerializeField] private string label;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool satisfied;

        [Tooltip("Link to an ItemDefinitionSO to enable automatic board matching. " +
                 "When set, Label and Icon are sourced from the definition.")]
        public ItemDefinitionSO itemDefinition;

        [Tooltip("How many items of this type must exist on the board simultaneously.")]
        [Range(1, 3)] public int quantity;

        public string Label
        {
            get => itemDefinition != null ? itemDefinition.displayName : label;
            set => label = value;
        }

        public Sprite Icon
        {
            get => itemDefinition != null ? itemDefinition.icon : icon;
            set => icon = value;
        }

        public bool IsSatisfied => satisfied;

        public bool Satisfied
        {
            get => satisfied;
            set => satisfied = value;
        }
    }
}
