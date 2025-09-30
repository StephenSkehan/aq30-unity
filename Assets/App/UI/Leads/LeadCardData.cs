using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.UI.Leads
{
    public enum CardState { New, InProgress, Complete }

    /// <summary>Simple data model the presenter binds to.</summary>
    public sealed class LeadCardData
    {
        public string Title;
        public string Objective;
        public string LeadId;                 // optional for QA
        public Sprite ActorBadge;             // optional
        public List<RequirementData> Requirements = new List<RequirementData>(3);
        public CardState VisualState = CardState.New;

        public bool HasAnyRequirementMet()
        {
            if (Requirements == null) return false;
            for (int i = 0; i < Requirements.Count; i++)
            {
                var r = Requirements[i];
                if (r != null && r.Met) return true;
            }
            return false;
        }

        public bool AllRequirementsMet()
        {
            if (Requirements == null || Requirements.Count == 0) return false;
            for (int i = 0; i < Requirements.Count; i++)
            {
                var r = Requirements[i];
                if (r == null || !r.Met) return false;
            }
            return true;
        }
    }
}
