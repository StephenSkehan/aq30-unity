using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    /// <summary>Horizontal bar of action cards (replaces old "Requests strip").</summary>
    public class LeadsBarView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private LeadCardView cardPrefab;
        [SerializeField] private Transform contentRoot;

        [Header("Data")]
        [SerializeField] private List<LeadCardSO> initialLeads = new List<LeadCardSO>();

        // Runtime state for save
        private readonly Dictionary<string, ActiveLeadState> _active = new();
        public System.Action<LeadCardSO> ProceedRequested;

        private void Awake()
        {
            if (scrollRect) scrollRect.horizontal = true;
        }

        private void Start()
        {
            // Seed runtime states (in a real save, this would load from SaveLoadDriver)
            foreach (var so in initialLeads)
            {
                if (!_active.ContainsKey(so.LeadCardId))
                    _active[so.LeadCardId] = new ActiveLeadState { LeadCardId = so.LeadCardId, RequirementMask = 0, Locked = false };
            }
            Rebuild();
        }

        public void Rebuild()
        {
            foreach (Transform c in contentRoot) Destroy(c.gameObject);
            foreach (var so in initialLeads)
            {
                var state = _active[so.LeadCardId];
                var card = Instantiate(cardPrefab, contentRoot);
                card.Bind(so, state);
                card.OnProceedClicked.AddListener(HandleProceedClicked);
            }
        }

        private void HandleProceedClicked(LeadCardSO data)
        {
            LeadAnalytics.LogLeadProceed(data);
            ProceedRequested?.Invoke(data);
        }

        /// <summary>Notify from merge domain when an item satisfies a requirement.</summary>
        public void NotifyRequirementSatisfied(string leadCardId, int requirementIndex, bool met)
        {
            if (!_active.TryGetValue(leadCardId, out var state)) return;
            state.SetRequirementMet(requirementIndex, met);
            _active[leadCardId] = state;

            // Find card to repaint
            foreach (Transform c in contentRoot)
            {
                var view = c.GetComponent<LeadCardView>();
                if (view == null) continue;
                // crude check by comparing title; robust would store a map
                if (view.name.Contains(leadCardId)) { view.UpdateRequirementTick(requirementIndex, met); break; }
            }
        }

        /// <summary>Apply outcomes after Proceed (spawns new leads, pins evidence, etc.).</summary>
        public void ApplyOutcome(LeadCardSO completed)
        {
            // Lock completed
            var st = _active[completed.LeadCardId];
            st.Locked = true;
            _active[completed.LeadCardId] = st;

            // Spawn new leads
            if (completed.SpawnLeadIds != null)
            {
                foreach (var id in completed.SpawnLeadIds)
                {
                    var so = initialLeads.Find(x => x.LeadCardId == id);
                    if (so != null && !_active.ContainsKey(id))
                    {
                        _active[id] = new ActiveLeadState { LeadCardId = id, RequirementMask = 0, Locked = false };
                    }
                }
            }

            Rebuild();
        }

        public IReadOnlyDictionary<string, ActiveLeadState> GetRuntimeState() => _active;
    }
}
