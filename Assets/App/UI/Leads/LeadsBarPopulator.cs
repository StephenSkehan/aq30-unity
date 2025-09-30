using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Leads
{
    /// <summary>
    /// Minimal demo populator used by Editor auto-wire. Public fields match editor expectations.
    /// </summary>
    public class LeadsBarPopulator : MonoBehaviour
    {
        [Header("Targets")]
        public ScrollRect leadsBar;           // ScrollRect owning Viewport/Content_Leads
        public GameObject leadCardPrefab;     // Prefab with LeadCardPresenter on root
        public TierSetPopup tierSetPopup;     // Optional scene popup

        [Header("Stakeout Fuel tiers (icons + names)")]
        public Sprite[] stakeoutFuelTierIcons = new Sprite[6];
        public string[] stakeoutFuelTierNames = new string[6]; // kept for editor tools; not used by RequirementData

        public void PopulateNow()
        {
            if (leadsBar == null || leadCardPrefab == null) return;
            var content = leadsBar.content;
            if (content == null) return;

            // Clear existing
            for (int i = content.childCount - 1; i >= 0; i--)
                DestroyImmediate(content.GetChild(i).gameObject);

            // Build three demo cards
            CreateCard(content, DemoNew());
            CreateCard(content, DemoInProgress());
            CreateCard(content, DemoComplete());
        }

        private void CreateCard(Transform parent, LeadCardData data)
        {
            var go = Instantiate(leadCardPrefab, parent, false);
            go.name = $"LeadCard_{data.LeadId}";
            var p = go.GetComponent<LeadCardPresenter>();
            if (p != null) p.Bind(data, tierSetPopup);
        }

        // ---- Demo data ----

        private LeadCardData DemoNew()
        {
            return new LeadCardData
            {
                LeadId = "201",
                Title = "Stakeout Snacks",
                Objective = "Grab a basic coffee",
                ActorBadge = null,
                VisualState = CardState.New,
                Requirements = new List<RequirementData>
                {
                    MakeReq(0, met:false), // paper cup
                }
            };
        }

        private LeadCardData DemoInProgress()
        {
            return new LeadCardData
            {
                LeadId = "202",
                Title = "Night Shift Comforts",
                Objective = "Burger for morale",
                ActorBadge = null,
                VisualState = CardState.InProgress,
                Requirements = new List<RequirementData>
                {
                    MakeReq(3, met:true),  // burger (met)
                    MakeReq(1, met:false), // hot coffee (pending)
                }
            };
        }

        private LeadCardData DemoComplete()
        {
            return new LeadCardData
            {
                LeadId = "203",
                Title = "Stakeout Feast",
                Objective = "All set for a long watch",
                ActorBadge = null,
                VisualState = CardState.Complete,
                Requirements = new List<RequirementData>
                {
                    MakeReq(5, met:true),  // feast caddy (met)
                }
            };
        }

        /// <summary>
        /// Create a RequirementData using your constructor:
        /// RequirementData(string groupTitle, List&lt;Sprite&gt; tiers, int tierIndex, bool met)
        /// </summary>
        private RequirementData MakeReq(int tierIndex, bool met)
        {
            var tiers = new List<Sprite>(stakeoutFuelTierIcons ?? new Sprite[0]);
            var clampedIndex = Mathf.Clamp(tierIndex, 0, Mathf.Max(0, tiers.Count - 1));
            const string groupTitle = "Stakeout fuel";
            return new RequirementData(groupTitle, tiers, clampedIndex, met);
        }
    }
}
