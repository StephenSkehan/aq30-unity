using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.UI.Leads
{
    /// <summary>
    /// Simple on-demand binder for a single card in the scene.
    /// Finds a component named "LeadCardPresenter" on the same GameObject and calls Bind(data, popup) via reflection.
    /// </summary>
    public class LeadCardDemoBinder : MonoBehaviour
    {
        public MonoBehaviour tierSetPopup;
        public Sprite actorSprite;
        public Sprite[] stakeoutFuelTierSprites;

        [ContextMenu("Bind Demo Now")]
        public void BindNow()
        {
            var presenter = FindPresenter(gameObject);
            if (presenter == null) { Debug.LogWarning("[LeadCardDemoBinder] LeadCardPresenter not found."); return; }

            var data = new DemoLead
            {
                title = "Demo Lead",
                objective = "Collect deli CCTV",
                leadId = "L-999",
                actorSprite = actorSprite,
                visualState = "InProgress",
                requirements = new List<object>
                {
                    new DemoRequirement { GroupTitle="Stakeout Fuel", Tiers=ToTierList(stakeoutFuelTierSprites), HighlightTierIndex=0, Achieved=true },
                    new DemoRequirement { GroupTitle="Stakeout Fuel", Tiers=ToTierList(stakeoutFuelTierSprites), HighlightTierIndex=1, Achieved=false }
                }
            };

            var m = presenter.GetType().GetMethod("Bind", new[] { typeof(object), typeof(MonoBehaviour) });
            if (m != null) m.Invoke(presenter, new object[] { data, tierSetPopup });
        }

        static Component FindPresenter(GameObject go)
        {
            var all = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in all)
                if (c && c.GetType().Name == "LeadCardPresenter")
                    return c;
            return null;
        }

        static List<Sprite> ToTierList(Sprite[] arr)
        {
            var list = new List<Sprite>();
            if (arr != null) list.AddRange(arr);
            return list;
        }

        // private demo models
        class DemoRequirement
        {
            public string GroupTitle;
            public List<Sprite> Tiers;
            public int HighlightTierIndex;
            public bool Achieved;
        }

        class DemoLead
        {
            public string title;
            public string objective;
            public string leadId;
            public Sprite actorSprite;
            public List<object> requirements;
            public string visualState;

            public bool HasAnyRequirementMet() => requirements != null && requirements.Exists(r => ((DemoRequirement)r).Achieved);
            public bool AllRequirementsMet() => requirements != null && requirements.TrueForAll(r => ((DemoRequirement)r).Achieved);
        }
    }
}
