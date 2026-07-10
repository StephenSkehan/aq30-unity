using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Runtime store for the active leads list in the scene.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public sealed class LeadsRepository : MonoBehaviour
    {
        [SerializeField] public LeadsDatabase database;

        private readonly List<LeadData> _current = new List<LeadData>();
        private readonly HashSet<string> _activatedLeadIds = new HashSet<string>();
        public IReadOnlyList<LeadData> CurrentLeads => _current;
        public IEnumerable<string> ActivatedLeadIds => _activatedLeadIds;

        /// <summary>Raised whenever the in-memory list changes.</summary>
        public event Action LeadsChanged;

        private void OnEnable()  => LeadsRuntimeBus.OnLeadActivated += OnLeadActivated;
        private void OnDisable() => LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated;

        private void OnLeadActivated(LeadData lead)
        {
            if (lead == null || string.IsNullOrEmpty(lead.leadId)) return;
            _activatedLeadIds.Add(lead.leadId);
            CheckAndUnlockBlockedLeads();
        }

        private void CheckAndUnlockBlockedLeads()
        {
            bool anyUnlocked = false;
            foreach (var lead in _current)
            {
                if (lead == null || lead.RuntimeState != LeadState.Blocked) continue;
                if (lead.RequiredLeadIds == null || lead.RequiredLeadIds.Length == 0) continue;

                bool allSatisfied = true;
                foreach (var requiredId in lead.RequiredLeadIds)
                {
                    if (!_activatedLeadIds.Contains(requiredId)) { allSatisfied = false; break; }
                }

                if (allSatisfied)
                {
                    lead.RuntimeState = LeadState.Available;
                    anyUnlocked = true;
                }
            }

            if (anyUnlocked) Broadcast();
        }

        private void Start()
        {
            if (database != null)
                ReplaceFromDatabase(database);
            else
                Debug.LogWarning("[LeadsRepository] No database assigned.", this);
        }

        public void SetDatabase(LeadsDatabase db) => database = db;

        /// <summary>Replace the in-memory list with the content of the given database.</summary>
        public void ReplaceFromDatabase(LeadsDatabase db)
        {
            database = db;
            _current.Clear();
            _activatedLeadIds.Clear();

            if (db != null)
            {
                foreach (var lead in db.Leads)
                {
                    if (lead == null) continue;
                    lead.RuntimeState = lead.state;
                    if (lead.requirements != null)
                        for (int i = 0; i < lead.requirements.Length; i++)
                            lead.SetRequirementSatisfied(i, false);
                    _current.Add(lead);
                }
            }

            Broadcast();
        }

        /// <summary>Replace with a supplied set of leads.</summary>
        public void ReplaceWith(IEnumerable<LeadData> leads)
        {
            _current.Clear();
            _activatedLeadIds.Clear();
            if (leads != null) _current.AddRange(leads);
            Broadcast();
        }

        /// <summary>
        /// Removes a single lead from the active list and notifies subscribers.
        /// Called on lead activation (player taps Proceed on a Ready card).
        /// </summary>
        public void RemoveLead(LeadData lead)
        {
            if (lead == null || !_current.Contains(lead)) return;
            _current.Remove(lead);
            Broadcast();
        }

        /// <summary>
        /// Makes a lead available to the player.
        /// If it is already in the list as Blocked, transitions it to Available.
        /// If it is not yet in the list, adds it and sets RuntimeState to Available.
        /// </summary>
        public void SpawnLead(LeadData lead)
        {
            if (lead == null) return;

            if (_current.Contains(lead))
            {
                if (lead.RuntimeState == LeadState.Blocked)
                {
                    lead.RuntimeState = LeadState.Available;
                    Broadcast();
                }
                return;
            }

            lead.RuntimeState = LeadState.Available;
            if (lead.requirements != null)
                for (int i = 0; i < lead.requirements.Length; i++)
                    lead.SetRequirementSatisfied(i, false);
            _current.Add(lead);
            Broadcast();
        }

        /// <summary>
        /// Call after mutating lead state outside the repository (e.g. from
        /// LeadRequirementChecker) to push the updated list to all UI subscribers.
        /// </summary>
        public void NotifyChanged()
        {
            LeadsChanged?.Invoke();
            LeadsRuntimeBus.BroadcastAll(CurrentLeads);
        }

        private void Broadcast() => NotifyChanged();

        // ---- Persistence ----

        public struct LeadSaveState
        {
            public string LeadId;
            public int    RuntimeState;
            public bool[] SatisfiedRequirements;
            public bool   Activated;
        }

        /// <summary>
        /// Restores lead states from a save. Call after ReplaceFromDatabase() has already run.
        /// Activated leads are removed from the active list but tracked for dependency resolution.
        /// </summary>
        public void ApplySavedStates(IReadOnlyList<LeadSaveState> states)
        {
            if (states == null || states.Count == 0) return;

            // Pass 1: register history and remove activated leads. Must run before
            // state entries so a repeatable lead that was activated and later
            // re-armed (present as BOTH an activated entry and a state entry)
            // is not removed after its live state was applied.
            foreach (var saved in states)
            {
                if (string.IsNullOrEmpty(saved.LeadId) || !saved.Activated) continue;
                _activatedLeadIds.Add(saved.LeadId);
                var resolved = _current.Find(l => l != null && l.leadId == saved.LeadId);
                if (resolved != null) _current.Remove(resolved);
            }

            // Pass 2: apply live states, re-adding any lead pass 1 removed.
            foreach (var saved in states)
            {
                if (string.IsNullOrEmpty(saved.LeadId) || saved.Activated) continue;

                var lead = _current.Find(l => l != null && l.leadId == saved.LeadId);
                if (lead == null)
                {
                    lead = database != null ? database.FindById(saved.LeadId) : null;
                    if (lead == null) continue;
                    _current.Add(lead);
                }

                lead.RuntimeState = (LeadState)saved.RuntimeState;

                if (saved.SatisfiedRequirements != null && lead.requirements != null)
                {
                    int count = Math.Min(saved.SatisfiedRequirements.Length, lead.requirements.Length);
                    for (int i = 0; i < count; i++)
                        lead.SetRequirementSatisfied(i, saved.SatisfiedRequirements[i]);
                }
            }

            Broadcast();
        }
    }
}
