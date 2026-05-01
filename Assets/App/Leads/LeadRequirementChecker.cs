using System;
using UnityEngine;
using AQ.App.Presentation;
using AQ.SharedKernel.Events;

namespace AQ.App.Leads
{
    /// <summary>
    /// Listens for ItemCreatedOnBoard events and automatically satisfies matching
    /// lead requirements. Satisfaction is additive within a session: once an item
    /// matches a requirement it stays satisfied even if the player merges the item away
    /// (the requirement represents "you acquired X", not "you currently hold X").
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public sealed class LeadRequirementChecker : MonoBehaviour
    {
        [SerializeField] private LeadsRepository _repository;

        private IDisposable _sub;

        private void Awake()
        {
            if (!_repository) _repository = FindAnyObjectByType<LeadsRepository>();
        }

        private void OnEnable()
        {
            _sub = GlobalBus.Bus.Subscribe<ItemCreatedOnBoard>(OnItemCreated);
        }

        private void OnDisable()
        {
            _sub?.Dispose();
            _sub = null;
        }

        private void OnItemCreated(ItemCreatedOnBoard e)
        {
            if (_repository == null || string.IsNullOrEmpty(e.ItemId)) return;

            bool anyChanged = false;

            foreach (var lead in _repository.CurrentLeads)
            {
                if (lead == null) continue;
                if (lead.RuntimeState == LeadState.Blocked || lead.RuntimeState == LeadState.Ready) continue;

                var reqs = lead.requirements;
                if (reqs == null || reqs.Length == 0) continue;

                for (int i = 0; i < reqs.Length; i++)
                {
                    if (reqs[i].IsSatisfied) continue;

                    var def = reqs[i].itemDefinition;
                    if (def == null || def.itemId != e.ItemId) continue;

                    lead.SetRequirementSatisfied(i, true);
                    anyChanged = true;

                    if (lead.IsReady())
                    {
                        lead.RuntimeState = LeadState.Ready;
                        LeadsRuntimeBus.BroadcastState(lead);
                        Debug.Log($"[LeadChecker] Lead '{lead.title}' is now Ready.");
                    }
                }
            }

            if (anyChanged)
                _repository.NotifyChanged();
        }
    }
}
