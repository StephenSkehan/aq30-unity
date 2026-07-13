using System;
using System.Collections.Generic;
using UnityEngine;
using AQ.App.Analytics;
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

        // itemId set of every item required by a currently non-Blocked lead.
        // Drives the board tile requirement-match checkmark.
        private readonly HashSet<string> _neededItemIds = new HashSet<string>();
        public IReadOnlyCollection<string> NeededItemIds => _neededItemIds;
        public bool IsItemNeeded(string itemId) => !string.IsNullOrEmpty(itemId) && _neededItemIds.Contains(itemId);

        /// <summary>Raised whenever the set of item types needed by active leads changes.</summary>
        public event Action NeededItemsChanged;

        /// <summary>Scene-level singleton so board tiles can query without a wired reference.</summary>
        public static LeadRequirementChecker Instance { get; private set; }

        private IDisposable _subCreated;
        private IDisposable _subRemoved;

        private void Awake()
        {
            if (!_repository) _repository = FindAnyObjectByType<LeadsRepository>();
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            _liveCounts.Clear();
            _subCreated = GlobalBus.Bus.Subscribe<ItemCreatedOnBoard>(OnItemCreated);
            _subRemoved = GlobalBus.Bus.Subscribe<ItemRemovedFromBoard>(OnItemRemoved);

            if (_repository != null) _repository.LeadsChanged += RecomputeNeededItemIds;
            RecomputeNeededItemIds();
        }

        private void OnDisable()
        {
            _subCreated?.Dispose(); _subCreated = null;
            _subRemoved?.Dispose(); _subRemoved = null;

            if (_repository != null) _repository.LeadsChanged -= RecomputeNeededItemIds;
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
                    GameAnalytics.LogCardStateChange(lead.leadId, lead.RuntimeState.ToString(), newState.ToString());
                    lead.RuntimeState = newState;
                    if (newState == LeadState.Ready)
                        LeadsRuntimeBus.BroadcastState(lead);
                    anyChanged = true;
                }
            }

            if (anyChanged)
                _repository.NotifyChanged();

            RecomputeNeededItemIds();
        }

        /// <summary>
        /// Rebuilds the set of itemIds required by any currently non-Blocked lead
        /// (regardless of that specific requirement's satisfaction) and fires
        /// <see cref="NeededItemsChanged"/> if the set actually changed. Called after
        /// board item counts change and whenever the active lead list changes
        /// (spawn/activate/save-restore) so tiles already on the board pick up or
        /// drop their checkmark without needing to be touched themselves.
        /// </summary>
        private void RecomputeNeededItemIds()
        {
            var newSet = new HashSet<string>();
            if (_repository != null)
            {
                foreach (var lead in _repository.CurrentLeads)
                {
                    if (lead == null || lead.RuntimeState == LeadState.Blocked) continue;

                    var reqs = lead.requirements;
                    if (reqs == null) continue;

                    for (int i = 0; i < reqs.Length; i++)
                    {
                        var def = reqs[i].itemDefinition;
                        if (def != null && !string.IsNullOrEmpty(def.itemId))
                            newSet.Add(def.itemId);
                    }
                }
            }

            if (newSet.SetEquals(_neededItemIds)) return;

            _neededItemIds.Clear();
            foreach (var id in newSet) _neededItemIds.Add(id);
            NeededItemsChanged?.Invoke();
        }
    }
}
