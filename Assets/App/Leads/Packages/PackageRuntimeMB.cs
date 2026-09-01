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

        private void Awake()
        {
            if (repository == null) repository = FindFirstObjectByType<LeadsRepository>();
            var packages = catalog != null ? (IReadOnlyList<PackageData>)catalog.packages : Array.Empty<PackageData>();
            _service = new PackageProgressService(packages, GameFlags.Has, GameFlags.Set);
        }

        private void OnEnable()
        {
            if (repository != null) repository.LeadsChanged += OnLeadsChanged;
        }

        private void OnDisable()
        {
            if (repository != null) repository.LeadsChanged -= OnLeadsChanged;
        }

        private void Start()
        {
            // Restore-time scan (rule 6): a package completed before a crash but
            // never dismissed re-fires here from persisted lead + flag state.
            OnLeadsChanged();
        }

        private void OnLeadsChanged()
        {
            if (_service == null || repository == null) return;
            var activated = repository.ActivatedLeadIds as ICollection<string>
                            ?? repository.ActivatedLeadIds.ToList();
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
            if (firstPay && !string.IsNullOrEmpty(p.specialRewardId))
                AQ.App.UI.Specials.SpecialItemsService.Grant(p.specialRewardId, Mathf.Max(1, p.specialRewardCount));

            _service.MarkBeatSeen(p);
            _pendingBeats.Remove(p);
        }
    }
}
