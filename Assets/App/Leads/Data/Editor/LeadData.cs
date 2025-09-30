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
    public sealed class LeadData : ScriptableObject
    {
        [Header("Display")]
        public string leadId = System.Guid.NewGuid().ToString("N");
        public string title = "Demo Lead";
        [TextArea(1, 3)] public string subtitle = "Collect deli CCTV";
        public Sprite actorPortrait;

        [Header("State")]
        public LeadState state = LeadState.Available;

        [Header("Requirements (max 3 recommended)")]
        public LeadRequirement[] requirements = System.Array.Empty<LeadRequirement>();

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

            // struct-in-array: copy → mutate via public setter → assign back
            var r = requirements[index];
            r.Satisfied = value;          // <-- use property, not private field
            requirements[index] = r;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
