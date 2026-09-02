// Assembly: AQ.App
// File: Assets/App/Leads/Packages/PackageRuntimeMB.cs
// Purpose: Scene adapter for PackageProgressService. Subscribes to the leads
//          repository, runs the completion scan, queues beats for presentation,
//          and completes the pay-and-flag transaction when a beat is dismissed.
//          The presentation surface itself (interstitial art+caption panel) is
//          a separate component that listens to BeatReady and calls
//          NotifyBeatDismissed; until it exists, beats queue and log.

using System;
using System.Collections.Generic;
using System.Linq;
using AQ.App.Economy;
using UnityEngine;

namespace AQ.App.Leads.Packages
{
    [DefaultExecutionOrder(-5)] // after LeadsRepository (-10)
    public sealed class PackageRuntimeMB : MonoBehaviour
    {
        [SerializeField] public PackageCatalog catalog;
        [SerializeField] public LeadsRepository repository;

        /// <summary>Raised once per completed package, in catalog order. UI presents, then calls NotifyBeatDismissed.</summary>
        public static event Action<PackageData> BeatReady;

        private PackageProgressService _service;
        private readonly List<PackageData> _pendingBeats = new List<PackageData>();
        public IReadOnlyList<PackageData> PendingBeats => _pendingBeats;

        // Live runtimes, so the lead bridge can ask whether a lead's payoff is a
        // package beat (and skip its own per-lead resolution dialogue).
        private static readonly List<PackageRuntimeMB> s_live = new List<PackageRuntimeMB>();
        private readonly HashSet<string> _memberIds = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>True when an enabled runtime's catalog lists this lead as a package member.</summary>
        public static bool OwnsPayoff(string leadId)
        {
            if (string.IsNullOrEmpty(leadId)) return false;
            for (int i = 0; i < s_live.Count; i++)
                if (s_live[i] != null && s_live[i]._memberIds.Contains(leadId)) return true;
            return false;
        }

        private void Awake()
        {
            if (repository == null) repository = FindFirstObjectByType<LeadsRepository>();
            var packages = catalog != null ? (IReadOnlyList<PackageData>)catalog.packages : Array.Empty<PackageData>();
            _service = new PackageProgressService(packages, GameFlags.Has, GameFlags.Set);

            _memberIds.Clear();
            foreach (var p in packages)
            {
                if (p == null || p.memberCardIds == null) continue;
                foreach (var id in p.memberCardIds)
                    if (!string.IsNullOrEmpty(id)) _memberIds.Add(id);
            }
        }

        private void OnEnable()
        {
            if (!s_live.Contains(this)) s_live.Add(this);
            if (repository != null) repository.LeadsChanged += OnLeadsChanged;
            LeadsRuntimeBus.OnLeadActivated += OnLeadActivated;
        }

        private void OnDisable()
        {
            s_live.Remove(this);
            if (repository != null) repository.LeadsChanged -= OnLeadsChanged;
            LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated;
        }

        private void Start()
        {
            // Restore-time scan (rule 6): a package completed before a crash but
            // never dismissed re-fires here from persisted lead + flag state.
            OnLeadsChanged();
        }

        private void OnLeadsChanged() => Scan(null);

        // The repository only broadcasts LeadsChanged on activation when the
        // activation unlocks a gated card; the chapter's last card unlocks
        // nothing, so its beat needs this direct hook. The activated id is
        // folded in explicitly so the scan does not depend on handler order.
        private void OnLeadActivated(LeadData lead) => Scan(lead != null ? lead.leadId : null);

        private void Scan(string justActivatedId)
        {
            if (_service == null || repository == null) return;
            var activated = new HashSet<string>(repository.ActivatedLeadIds, StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(justActivatedId)) activated.Add(justActivatedId);
            var newlyComplete = _service.ScanForNewlyComplete(activated);
            foreach (var p in newlyComplete)
            {
                _pendingBeats.Add(p);
                Debug.Log($"[Packages] complete: {p.packageId} ({p.beatType})", this);
                BeatReady?.Invoke(p);
            }
        }

        /// <summary>
        /// Dismiss handler: pay (idempotent via beat_paid), grant specials, then
        /// mark seen. Crash between pay and seen re-shows the beat, never
        /// double-pays (feature-lead-packages-v1 section 3).
        /// </summary>
        public void NotifyBeatDismissed(PackageData p)
        {
            if (p == null || _service == null) return;

            bool firstPay = _service.TryPayRewards(p, WalletLocator.Instance);
            if (firstPay && !string.IsNullOrEmpty(p.specialRewardId) &&
                Enum.TryParse<AQ.App.UI.Specials.SpecialId>(p.specialRewardId, out var specialId))
                AQ.App.UI.Specials.SpecialItemsService.Grant(specialId, Mathf.Max(1, p.specialRewardCount));

            _service.MarkBeatSeen(p);
            _pendingBeats.Remove(p);
        }
    }
}
