// Assembly: AQ.Tests.EditMode
// File: Assets/Tests/EditMode/PackageProgressServiceTests.cs
// Purpose: The lead-package spine's crash-boundary suite (feature-lead-packages-v1
//          sections 2 and 3). Coverage: the completion scan with member subsets,
//          pending-beat dedup across repeated scans, beat_seen only via the
//          dismiss path, reward idempotence across a simulated crash (paid but
//          not seen re-fires and does not re-pay), restore re-fire, and catalog
//          validation (ids, member counts, cross-package member claims,
//          database membership).

using System;
using System.Collections.Generic;
using AQ.App.Leads;
using AQ.App.Leads.Packages;
using AQ.SharedKernel.Economy;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    public class PackageProgressServiceTests
    {
        private readonly List<UnityEngine.Object> _made = new List<UnityEngine.Object>();
        private Dictionary<string, bool> _flags;

        [SetUp]
        public void SetUp() => _flags = new Dictionary<string, bool>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) UnityEngine.Object.DestroyImmediate(o);
            _made.Clear();
        }

        // ---- helpers ----

        private PackageData MakePackage(string id, params string[] members)
        {
            var p = ScriptableObject.CreateInstance<PackageData>();
            p.packageId = id;
            p.memberCardIds = members;
            _made.Add(p);
            return p;
        }

        private PackageProgressService MakeService(params PackageData[] packages)
            => new PackageProgressService(
                packages,
                f => _flags.TryGetValue(f, out var v) && v,
                f => _flags[f] = true);

        private sealed class RecordingWallet : IWallet
        {
            public int GrantCalls;
            public readonly List<Reward> Rewards = new List<Reward>();
            public int Get(Currency currency) => 0;
            public void Grant(string reason = null, params Reward[] rewards)
            {
                GrantCalls++;
                if (rewards != null) Rewards.AddRange(rewards);
            }
            public void Grant(params Reward[] rewards) => Grant(null, rewards);
            public bool TrySpend(Currency currency, int amount, string reason = null) => false;
            public event Action<WalletChanged> Changed { add { } remove { } }
            public event Action<RewardsGranted> Granted { add { } remove { } }
        }

        private static ICollection<string> Ids(params string[] ids) => new HashSet<string>(ids);

        // ---- completion scan ----

        [Test]
        public void Scan_IncompleteMembers_DoesNotComplete()
        {
            var p = MakePackage("pkg_a", "card1", "card2", "card3");
            var svc = MakeService(p);

            Assert.IsEmpty(svc.ScanForNewlyComplete(Ids("card1", "card2")));
        }

        [Test]
        public void Scan_AllMembersActivated_CompletesOnce_RegardlessOfOrder()
        {
            var p = MakePackage("pkg_a", "card1", "card2");
            var svc = MakeService(p);

            var first = svc.ScanForNewlyComplete(Ids("card2", "card1", "unrelated"));
            Assert.AreEqual(1, first.Count);
            Assert.AreSame(p, first[0]);

            // Repeated scan while the beat is pending must not re-raise (dedup).
            Assert.IsEmpty(svc.ScanForNewlyComplete(Ids("card1", "card2")));
            Assert.IsTrue(svc.IsBeatPending("pkg_a"));
        }

        [Test]
        public void Scan_SeenBeat_NeverRefires()
        {
            var p = MakePackage("pkg_a", "card1");
            var svc = MakeService(p);
            _flags[p.BeatSeenFlag] = true;

            Assert.IsEmpty(svc.ScanForNewlyComplete(Ids("card1")));
        }

        [Test]
        public void Scan_EmptyMemberList_NeverCompletes()
        {
            var p = MakePackage("pkg_empty");
            var svc = MakeService(p);

            Assert.IsEmpty(svc.ScanForNewlyComplete(Ids("card1")));
        }

        [Test]
        public void Scan_MultiplePackages_CompletesInCatalogOrder()
        {
            var a = MakePackage("pkg_a", "card1");
            var b = MakePackage("pkg_b", "card2");
            var svc = MakeService(a, b);

            var done = svc.ScanForNewlyComplete(Ids("card2", "card1"));
            Assert.AreEqual(2, done.Count);
            Assert.AreSame(a, done[0]);
            Assert.AreSame(b, done[1]);
        }

        // ---- rule 5: seen only via dismissal ----

        [Test]
        public void Scan_DoesNotSetAnyFlag()
        {
            var p = MakePackage("pkg_a", "card1");
            var svc = MakeService(p);

            svc.ScanForNewlyComplete(Ids("card1"));

            Assert.IsEmpty(_flags, "the scan must never write flags; only dismissal does");
        }

        [Test]
        public void MarkBeatSeen_SetsFlag_AndClearsPending()
        {
            var p = MakePackage("pkg_a", "card1");
            var svc = MakeService(p);
            svc.ScanForNewlyComplete(Ids("card1"));

            svc.MarkBeatSeen(p);

            Assert.IsTrue(_flags.ContainsKey(p.BeatSeenFlag));
            Assert.IsFalse(svc.IsBeatPending("pkg_a"));
            Assert.IsEmpty(svc.ScanForNewlyComplete(Ids("card1")));
        }

        // ---- reward idempotence across a simulated crash ----

        [Test]
        public void TryPayRewards_PaysOnce_SetsPaidFlag()
        {
            var p = MakePackage("pkg_a", "card1");
            p.softCurrency = 45;
            p.energyGrant = 20;
            var svc = MakeService(p);
            var wallet = new RecordingWallet();

            Assert.IsTrue(svc.TryPayRewards(p, wallet));
            Assert.IsFalse(svc.TryPayRewards(p, wallet), "second pay must be refused");
            Assert.AreEqual(1, wallet.GrantCalls);
            Assert.AreEqual(2, wallet.Rewards.Count);
            Assert.IsTrue(_flags.ContainsKey(p.BeatPaidFlag));
        }

        [Test]
        public void CrashBetweenPayAndSeen_RefiresBeat_ButNeverRepays()
        {
            var p = MakePackage("pkg_a", "card1");
            p.softCurrency = 10;

            // Session one: complete, pay, CRASH before MarkBeatSeen.
            var svc1 = MakeService(p);
            svc1.ScanForNewlyComplete(Ids("card1"));
            var wallet1 = new RecordingWallet();
            Assert.IsTrue(svc1.TryPayRewards(p, wallet1));

            // Session two: fresh service over the same persisted flags.
            var svc2 = MakeService(p);
            var refire = svc2.ScanForNewlyComplete(Ids("card1"));
            Assert.AreEqual(1, refire.Count, "beat unseen at restore must re-fire (rule 5)");

            var wallet2 = new RecordingWallet();
            Assert.IsFalse(svc2.TryPayRewards(p, wallet2), "beat_paid must block the second pay");
            Assert.AreEqual(0, wallet2.GrantCalls);

            svc2.MarkBeatSeen(p);
            Assert.IsEmpty(svc2.ScanForNewlyComplete(Ids("card1")));
        }

        [Test]
        public void TryPayRewards_ZeroRewards_StillMarksPaid_NoGrantCall()
        {
            var p = MakePackage("pkg_a", "card1");
            var svc = MakeService(p);
            var wallet = new RecordingWallet();

            Assert.IsTrue(svc.TryPayRewards(p, wallet));
            Assert.AreEqual(0, wallet.GrantCalls, "empty reward set must not call Grant");
            Assert.IsTrue(_flags.ContainsKey(p.BeatPaidFlag));
        }

        // ---- catalog validation ----

        [Test]
        public void Validate_CleanCatalog_NoProblems()
        {
            var catalog = ScriptableObject.CreateInstance<PackageCatalog>();
            _made.Add(catalog);
            catalog.packages.Add(MakePackage("fk_p01_01", "fk_p01_01a"));
            catalog.packages.Add(MakePackage("fk_p01_02", "fk_p01_02a", "fk_p01_02b"));

            Assert.IsEmpty(catalog.Validate());
        }

        [Test]
        public void Validate_CatchesDuplicates_Counts_AndCrossClaims()
        {
            var catalog = ScriptableObject.CreateInstance<PackageCatalog>();
            _made.Add(catalog);
            catalog.packages.Add(MakePackage("fk_p01_01", "shared_card"));
            catalog.packages.Add(MakePackage("fk_p01_01", "other_card"));                              // duplicate id
            catalog.packages.Add(MakePackage("fk_p01_03", "shared_card"));                             // cross-claim
            catalog.packages.Add(MakePackage("fk_p01_04", "a", "b", "c", "d", "e", "f"));              // 6 members
            catalog.packages.Add(MakePackage("fk_p01_05"));                                            // 0 members

            var problems = catalog.Validate();
            Assert.IsTrue(problems.Exists(m => m.Contains("duplicate packageId")));
            Assert.IsTrue(problems.Exists(m => m.Contains("claimed by both")));
            Assert.IsTrue(problems.Exists(m => m.Contains("outside 1..5") && m.Contains("fk_p01_04")));
            Assert.IsTrue(problems.Exists(m => m.Contains("outside 1..5") && m.Contains("fk_p01_05")));
        }

        [Test]
        public void Validate_AgainstDatabase_FlagsUnknownMembers()
        {
            var db = ScriptableObject.CreateInstance<LeadsDatabase>();
            _made.Add(db);
            var lead = ScriptableObject.CreateInstance<LeadData>();
            lead.leadId = "known_card";
            _made.Add(lead);
            db.Add(lead);

            var catalog = ScriptableObject.CreateInstance<PackageCatalog>();
            _made.Add(catalog);
            catalog.packages.Add(MakePackage("fk_p01_01", "known_card"));
            catalog.packages.Add(MakePackage("fk_p01_02", "ghost_card"));

            var problems = catalog.Validate(db);
            Assert.AreEqual(1, problems.Count, string.Join("; ", problems));
            Assert.IsTrue(problems[0].Contains("ghost_card"));
        }
    }
}
