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
        public IReadOnlyList<LeadData> CurrentLeads => _current;

        /// <summary>Raised whenever the in-memory list changes.</summary>
        public event Action LeadsChanged;

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
        /// Adds a lead to the active list (e.g. spawned by a lead outcome).
        /// Resets RuntimeState and requirement satisfaction to design-time defaults.
        /// No-ops if the lead is already present.
        /// </summary>
        public void SpawnLead(LeadData lead)
        {
            if (lead == null || _current.Contains(lead)) return;
            lead.RuntimeState = lead.state;
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
    }
}
