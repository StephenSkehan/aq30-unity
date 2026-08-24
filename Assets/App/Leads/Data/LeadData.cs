using System;
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
        [Tooltip("Design-time initial state. Never read this at runtime — use RuntimeState.")]
        public LeadState state = LeadState.Available;

        // Runtime-only shadow. Never serialised, so SO mutations can't persist across sessions.
        // Initialised from state by LeadsRepository. All runtime reads/writes go through here.
        [NonSerialized] public LeadState RuntimeState;

        [Header("Action")]
        public LeadActionType ActionType;
        [Range(0, 10)] public int EnergyCost = 0;

        [Header("Flag Gate (agency)")]
        [Tooltip("GameFlags flag that must be SET before this lead can leave Blocked. Empty = no requirement. " +
                 "Pair a sibling lead's forbidsFlag with this one's requiresFlag to make a player choice decide " +
                 "which of two leads exists.")]
        public string requiresFlag;

        [Tooltip("GameFlags flag that must NOT be set for this lead to leave Blocked. Empty = no restriction.")]
        public string forbidsFlag;

        [Header("Requirements (max 3 recommended)")]
        public LeadRequirement[] requirements = System.Array.Empty<LeadRequirement>();

        [Header("Outcomes")]
        public string[] RequiredLeadIds;
        public string[] EvidenceIds;
        public string[] SpawnLeadIds;
        public string[] NarrativeFlags;
        public int SoftCurrency;
        public int EnergyGrant;
        [Tooltip("Platinum Ingots granted on activation (economy sheet: Very Hard band milestones).")]
        public int PremiumGrant;

        [Header("Generator Reward")]
        [Tooltip("Push a generator of this type to the overflow bucket on lead activation. Empty = no generator reward.")]
        public string generatorRewardTypeId;
        public int generatorRewardTier;

        [Header("Special Reward (Case Kit)")]
        [Tooltip("SpecialId name granted on lead activation (SkeletonKey, BoxKnife, CarbonCopy, BoltCutters, SearchWarrant, EvidenceTag). Empty = none.")]
        public string specialRewardId;
        public int specialRewardCount = 1;

        [Header("Resolution Dialogue")]
        public CaseGraph resolutionDialogue;

        [Header("UI")]
        public LeadOutcomeHint OutcomeHints;

        [Header("Evidence Board")]
        [Tooltip("LeadIds this lead connects to on the evidence board. Draw a string when both ends are resolved.")]
        public string[] boardConnections = System.Array.Empty<string>();

        [Tooltip("Phase cluster this lead's card belongs to on the evidence board (1-based). 0 = never shown on the board and excluded from case progress (repeatables, teasers).")]
        public int boardPhase = 1;

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
