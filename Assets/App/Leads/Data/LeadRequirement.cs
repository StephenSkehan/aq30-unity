using System;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>Requirement chip on a Lead (label + optional icon + satisfied flag).</summary>
    [Serializable]
    public struct LeadRequirement
    {
        [SerializeField] private string label;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool satisfied;

        public string Label
        {
            get => label;
            set => label = value;
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
    }
}
