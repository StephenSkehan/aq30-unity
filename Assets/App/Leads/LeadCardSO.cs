using UnityEngine;

namespace AQ.App.Leads
{
    [CreateAssetMenu(fileName = "LeadCard", menuName = "AQ/Leads/Lead Card", order = 10)]
    public class LeadCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string LeadCardId = "Lead_Unique_Id";
        public Sprite PortraitOrPlace;
        public string Title;   // e.g., "Mo — Lab Request" or "Riverside Diner"
        [TextArea] public string OneLiner;

        [Header("Action")]
        public LeadActionType ActionType;
        [Range(0, 10)] public int EnergyCost = 0;
        public LeadRequirement[] Requirements = new LeadRequirement[0];

        [Header("Outcomes")]
        public string[] EvidenceIds;     // new evidence to pin
        public string[] SpawnLeadIds;    // new leads to add
        public string[] NarrativeFlags;  // case flags
        public int SoftCurrency;
        public int EnergyGrant;

        [Header("UI")]
        public LeadOutcomeHint OutcomeHints;
    }
}
