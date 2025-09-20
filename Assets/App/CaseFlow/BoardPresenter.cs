using UnityEngine;
using AQ.App.Leads;
using AQ.App.HUD;

namespace AQ.App.CaseFlow
{
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Wiring")]
        public LeadsBarView leadsBar;
        public TopStatusRow topStatusRow;

        private float _percentSolved;
        private int _evidenceCount;

        private void OnEnable()
        {
            if (leadsBar != null)
                leadsBar.ProceedRequested += HandleProceedRequested;
        }

        private void OnDisable()
        {
            if (leadsBar != null)
                leadsBar.ProceedRequested -= HandleProceedRequested;
        }

        private void Start()
        {
            topStatusRow?.SetSolved(_percentSolved);
            topStatusRow?.SetEvidenceCount(_evidenceCount);
            // Update leads count once your bar knows its state.
        }

        private void HandleProceedRequested(LeadCardSO data)
        {
            // TODO: charge energy & run action scene.
            leadsBar.ApplyOutcome(data);

            // Naive HUD updates for now.
            int spawned = data.SpawnLeadIds != null ? data.SpawnLeadIds.Length : 0;
            int evidenceAdded = data.EvidenceIds != null ? data.EvidenceIds.Length : 1;
            _evidenceCount += evidenceAdded;

            float old = _percentSolved;
            _percentSolved = Mathf.Clamp01(_percentSolved + 0.10f);

            topStatusRow?.SetSolved(_percentSolved);
            topStatusRow?.SetEvidenceCount(_evidenceCount);
            topStatusRow?.SetActiveLeads(spawned > 0 ? 2 + spawned : 2);
            topStatusRow?.TickBreakthroughNow();

            LeadAnalytics.LogOutcome(data, evidenceAdded, spawned);
            LeadAnalytics.LogPercentSolved(old, _percentSolved);

            // TODO: SaveLoadDriver.Save(...)
        }
    }
}
