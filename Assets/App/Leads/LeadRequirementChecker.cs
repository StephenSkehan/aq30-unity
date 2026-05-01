using System;
using System.Collections.Generic;
using UnityEngine;
using AQ.App.Presentation;
using AQ.SharedKernel.Events;

namespace AQ.App.Leads
{
    /// <summary>
    /// Tracks live board state to drive lead requirement satisfaction.
    /// An item must currently exist on the board at the exact itemId (family+tier)
    /// for its requirement to count. Merging an item away decrements the count and
    /// may un-satisfy requirements. T2 does NOT satisfy a T1 requirement.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public sealed class LeadRequirementChecker : MonoBehaviour
    {
        [SerializeField] private LeadsRepository _repository;

        // itemId → count of that item currently on the board
        private readonly Dictionary<string, int> _liveCounts = new Dictionary<string, int>();

        private IDisposable _subCreated;
        private IDisposable _subRemoved;

        private void Awake()
        {
            if (!_repository) _repository = FindAnyObjectByType<LeadsRepository>();
        }

        private void OnEnable()
        {
            _liveCounts.Clear();
            _subCreated = GlobalBus.Bus.Subscribe<ItemCreatedOnBoard>(OnItemCreated);
            _subRemoved = GlobalBus.Bus.Subscribe<ItemRemovedFromBoard>(OnItemRemoved);
        }

        private void OnDisable()
        {
            _subCreated?.Dispose(); _subCreated = null;
            _subRemoved?.Dispose(); _subRemoved = null;
        }

        private void OnItemCreated(ItemCreatedOnBoard e)
        {
            if (string.IsNullOrEmpty(e.ItemId)) return;
            _liveCounts[e.ItemId] = (_liveCounts.TryGetValue(e.ItemId, out int c) ? c : 0) + 1;
            RecomputeAllLeads();
        }

        private void OnItemRemoved(ItemRemovedFromBoard e)
        {
            if (string.IsNullOrEmpty(e.ItemId)) return;
            if (_liveCounts.TryGetValue(e.ItemId, out int c) && c > 0)
                _liveCounts[e.ItemId] = c - 1;
            RecomputeAllLeads();
        }

        private void RecomputeAllLeads()
        {
            if (_repository == null) return;

            bool anyChanged = false;

            foreach (var lead in _repository.CurrentLeads)
            {
                if (lead == null || lead.RuntimeState == LeadState.Blocked) continue;

                var reqs = lead.requirements;
                if (reqs == null || reqs.Length == 0) continue;

                // Greedy allocation: copy live counts and "spend" one per requirement.
                // Correctly handles multiple requirements pointing to the same itemId.
                var tempCounts  = new Dictionary<string, int>(_liveCounts);
                bool allSat     = true;
                bool anySat     = false;

                for (int i = 0; i < reqs.Length; i++)
                {
                    var def = reqs[i].itemDefinition;
                    bool sat = false;

                    if (def != null && !string.IsNullOrEmpty(def.itemId))
                    {
                        int needed = reqs[i].quantity < 1 ? 1 : reqs[i].quantity;
                        if (tempCounts.TryGetValue(def.itemId, out int cnt) && cnt >= needed)
                        {
                            sat = true;
                            tempCounts[def.itemId] = cnt - needed;
                            anySat = true;
                        }
                    }

                    if (!sat) allSat = false;

                    if (sat != reqs[i].IsSatisfied)
                    {
                        lead.SetRequirementSatisfied(i, sat);
                        anyChanged = true;
                    }
                }

                LeadState newState = allSat  ? LeadState.Ready
                                   : anySat  ? LeadState.InProgress
                                   : LeadState.Available;

                if (lead.RuntimeState != newState)
                {
                    lead.RuntimeState = newState;
                    if (newState == LeadState.Ready)
                        LeadsRuntimeBus.BroadcastState(lead);
                    anyChanged = true;
                }
            }

            if (anyChanged)
                _repository.NotifyChanged();
        }
    }
}
