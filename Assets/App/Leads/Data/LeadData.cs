using System;
using UnityEngine;

namespace AQ.App.Leads
{
    [Serializable]
    public struct LeadBranchOutcome
    {
        public string label;
        public string[] SpawnLeadIds;
    }

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
        [Tooltip("Design-time initial state. Never read this at runtime — use RuntimeState.")]
        public LeadState state = LeadState.Available;

        // Runtime-only shadow. Never serialised, so SO mutations can't persist across sessions.
        // Initialised from state by LeadsRepository. All runtime reads/writes go through here.
        [NonSerialized] public LeadState RuntimeState;

        [Header("Action")]
        public LeadActionType ActionType;
        [Range(0, 10)] public int EnergyCost = 0;

        [Header("Requirements (max 3 recommended)")]
        public LeadRequirement[] requirements = System.Array.Empty<LeadRequirement>();

        [Header("Outcomes")]
        public string[] RequiredLeadIds;
        public string[] EvidenceIds;
        public string[] SpawnLeadIds;
        public LeadBranchOutcome[] BranchOutcomes;
        public string[] NarrativeFlags;
        public int SoftCurrency;
        public int EnergyGrant;

        [Header("Generator Reward")]
        [Tooltip("Push a generator of this type to the overflow bucket on lead activation. Empty = no generator reward.")]
        public string generatorRewardTypeId;
        public int generatorRewardTier;

        [Header("Resolution Dialogue")]
        public CaseGraph resolutionDialogue;

        [Header("UI")]
        public LeadOutcomeHint OutcomeHints;

        [Header("Evidence Board")]
        [Tooltip("LeadIds this lead connects to on the evidence board. Draw a string when both ends are resolved.")]
        public string[] boardConnections = System.Array.Empty<string>();

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
                return RuntimeState == LeadState.Ready;

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
            // Note: intentionally no EditorUtility.SetDirty — satisfaction is runtime
            // state only and must not be baked into the SO asset on disk.
        }
    }
}
