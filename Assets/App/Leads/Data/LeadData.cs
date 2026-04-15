using UnityEngine;

namespace AQ.App.Leads
{
    public enum LeadState
    {
        Blocked = 0,
        Available = 1,
        InProgress = 2,
        Ready = 3
    }

    [CreateAssetMenu(fileName = "Lead", menuName = "AQ/Leads/Lead", order = 10)]
    public sealed class LeadData : ScriptableObject, ILeadCardModel
    {
        [Header("Identity")]
        public string leadId = System.Guid.NewGuid().ToString("N");

        [Header("Display")]
        public string title = "Demo Lead";
        [TextArea(1, 3)] public string subtitle = "";
        public Sprite actorPortrait;

        [Header("State")]
        public LeadState state = LeadState.Available;

        [Header("Action")]
        public LeadActionType ActionType;
        [Range(0, 10)] public int EnergyCost = 0;

        [Header("Requirements (max 3 recommended)")]
        public LeadRequirement[] requirements = System.Array.Empty<LeadRequirement>();

        [Header("Outcomes")]
        public string[] EvidenceIds;
        public string[] SpawnLeadIds;
        public string[] NarrativeFlags;
        public int SoftCurrency;
        public int EnergyGrant;

        [Header("UI")]
        public LeadOutcomeHint OutcomeHints;

        // ---- ILeadCardModel ----
        string ILeadCardModel.Title        => title;
        string ILeadCardModel.Subtitle     => subtitle;
        string ILeadCardModel.ActionTag    => ActionType.ToString();
        Sprite ILeadCardModel.ActorPortrait => actorPortrait;
        LeadRequirement[] ILeadCardModel.Requirements => requirements;
        bool   ILeadCardModel.CanProceed   => IsReady();

        // ---- Logic ----

        public bool IsReady()
        {
            if (requirements == null || requirements.Length == 0)
                return state == LeadState.Ready;

            for (int i = 0; i < requirements.Length; i++)
            {
                if (!requirements[i].IsSatisfied)
                    return false;
            }
            return true;
        }

        public void SetRequirementSatisfied(int index, bool value)
        {
            if (requirements == null) return;
            if ((uint)index >= (uint)requirements.Length) return;

            // struct-in-array: copy → mutate → assign back
            var r = requirements[index];
            r.Satisfied = value;
            requirements[index] = r;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
