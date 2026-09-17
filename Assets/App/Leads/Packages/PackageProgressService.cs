// Assembly: AQ.App
// File: Assets/App/Leads/Packages/PackageProgressService.cs
// Purpose: Pure package-completion logic, MonoBehaviour-free so EditMode tests
//          drive it directly (the "completing card is not knowable at author
//          time" mechanism from feature-lead-packages-v1 section 2).
//
// Robustness rules honoured (CLAUDE.md):
//  - Rule 6: completion detection is a STATE-SCAN over activated ids, never an
//    edge event chain; restore-time and mid-play converge on the same scan.
//  - Rule 5: beat_seen is set only after the presentation is dismissed
//    (MarkBeatSeen is called by the dismiss handler, never by the scan).
//  - Idempotent pay: beat_paid is checked before granting, so a crash between
//    pay and seen re-shows the beat but never double-pays.
//  - Flags ride GameFlags, which is folded into the save aggregate (rule 1).

using System;
using System.Collections.Generic;
using AQ.SharedKernel.Economy;

namespace AQ.App.Leads.Packages
{
    public sealed class PackageProgressService
    {
        private readonly IReadOnlyList<PackageData> _packages;
        private readonly Func<string, bool> _hasFlag;
        private readonly Action<string> _setFlag;

        // Packages surfaced by a scan whose beat is not yet dismissed. Keeps a
        // repeated scan from re-raising a beat that is already on screen.
        private readonly HashSet<string> _pendingBeatIds = new HashSet<string>(StringComparer.Ordinal);

        public PackageProgressService(
            IReadOnlyList<PackageData> packages,
            Func<string, bool> hasFlag,
            Action<string> setFlag)
        {
            _packages = packages ?? throw new ArgumentNullException(nameof(packages));
            _hasFlag = hasFlag ?? throw new ArgumentNullException(nameof(hasFlag));
            _setFlag = setFlag ?? throw new ArgumentNullException(nameof(setFlag));
        }

        /// <summary>
        /// The state-scan. Returns packages that are newly complete: every member
        /// activated, beat not yet seen, not already pending presentation.
        /// Call on every LeadsChanged and once at restore.
        /// </summary>
        public List<PackageData> ScanForNewlyComplete(ICollection<string> activatedLeadIds)
        {
            var result = new List<PackageData>();
            if (activatedLeadIds == null) return result;

            foreach (var p in _packages)
            {
                if (p == null || string.IsNullOrEmpty(p.packageId)) continue;
                if (_pendingBeatIds.Contains(p.packageId)) continue;
                if (_hasFlag(p.BeatSeenFlag)) continue;
                if (!AllMembersActivated(p, activatedLeadIds)) continue;

                _pendingBeatIds.Add(p.packageId);
                result.Add(p);
            }
            return result;
        }

        private static bool AllMembersActivated(PackageData p, ICollection<string> activatedLeadIds)
        {
            var members = p.memberCardIds;
            if (members == null || members.Length == 0) return false;
            for (int i = 0; i < members.Length; i++)
            {
                if (string.IsNullOrEmpty(members[i])) return false;
                if (!activatedLeadIds.Contains(members[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Idempotent package payout. Grants wallet rewards only if beat_paid is
        /// unset, then sets beat_paid. Returns true when a grant happened.
        /// Specials are the caller's job (they live in a UI-layer service).
        /// </summary>
        public bool TryPayRewards(PackageData p, IWallet wallet, string reason = "package.beat")
        {
            if (p == null || wallet == null) return false;
            if (_hasFlag(p.BeatPaidFlag)) return false;

            var rewards = new List<Reward>(3);
            if (p.softCurrency > 0) rewards.Add(Reward.Soft(p.softCurrency));
            if (p.energyGrant > 0) rewards.Add(Reward.Energy(p.energyGrant));
            if (p.premiumGrant > 0) rewards.Add(Reward.Premium(p.premiumGrant));
            if (rewards.Count > 0)
                wallet.Grant(reason, rewards.ToArray());

            _setFlag(p.BeatPaidFlag);
            return true;
        }

        /// <summary>
        /// Rule 5: called by the beat presentation's dismiss handler, never by
        /// the scan. After this the package can never re-fire.
        /// </summary>
        public void MarkBeatSeen(PackageData p)
        {
            if (p == null) return;
            _setFlag(p.BeatSeenFlag);
            _pendingBeatIds.Remove(p.packageId);
        }

        /// <summary>True while a package's beat has been surfaced but not dismissed.</summary>
        public bool IsBeatPending(string packageId) => _pendingBeatIds.Contains(packageId);
    }
}
