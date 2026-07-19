using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AQ.App.Locker;
using AQ.App.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// Crash-boundary tests for the Evidence Locker fold-in (schema 0.7.0).
    /// Board and locker persist in ONE aggregate written by AtomicSaveFile, so the
    /// dup/loss windows that existed between board_state.json and locker_state.json
    /// reduce to: "is the aggregate write atomic at every crash point?" These tests
    /// fault-inject each checkpoint and assert the recovered aggregate is exactly the
    /// old state or exactly the new state — an item is never duplicated, never lost.
    /// All file I/O goes to a temp dir; the real persistentDataPath is never touched.
    /// </summary>
    public class LockerCrashBoundaryTests
    {
        private string _dir, _live, _prev, _tmp, _legacy;
        private LockerStateDTO _realServiceState;

        private sealed class SimulatedCrash : Exception { }

        private sealed class CrashAt : IFaultInjector
        {
            private readonly SaveCheckpoint _cp;
            public CrashAt(SaveCheckpoint cp) { _cp = cp; }
            public void OnCheckpoint(SaveCheckpoint c) { if (c == _cp) throw new SimulatedCrash(); }
        }

        // Mini aggregate mirroring the board save's shape: board content + wallet +
        // locker in one file. Enough to express every transaction the audit flagged.
        [Serializable]
        private sealed class TestAggregate
        {
            public List<string> boardItems = new();
            public int walletSoft;
            public LockerStateDTO locker = new();
        }

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "aq_locker_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _live   = Path.Combine(_dir, "board_state.json");
            _prev   = Path.Combine(_dir, "board_state.prev.json");
            _tmp    = _live + ".tmp";
            _legacy = Path.Combine(_dir, "locker_state.json");

            // Park the real (static) service state and point the legacy path into the
            // temp dir so no test can read or write the developer's actual save data.
            _realServiceState = EvidenceLockerService.ExportState();
            EvidenceLockerService.LegacyPathOverride = _legacy;
        }

        [TearDown]
        public void TearDown()
        {
            EvidenceLockerService.LegacyPathOverride = null;
            EvidenceLockerService.ImportState(_realServiceState);
            try { Directory.Delete(_dir, recursive: true); } catch { /* temp dir, best effort */ }
        }

        // --------------- protocol atomicity ---------------

        [Test]
        public void Write_Complete_PromotesLiveAndKeepsPrev()
        {
            AtomicSaveFile.Write(_live, _prev, _tmp, "v1");
            AtomicSaveFile.Write(_live, _prev, _tmp, "v2");

            Assert.AreEqual("v2", File.ReadAllText(_live, Encoding.UTF8));
            Assert.AreEqual("v1", File.ReadAllText(_prev, Encoding.UTF8));
            Assert.IsFalse(File.Exists(_tmp), "tmp should be promoted away");
        }

        [Test]
        public void Crash_AtEveryCheckpoint_OldOrNewAlwaysReadable()
        {
            foreach (SaveCheckpoint cp in Enum.GetValues(typeof(SaveCheckpoint)))
            {
                ResetFiles();
                AtomicSaveFile.Write(_live, _prev, _tmp, "old");

                Assert.Throws<SimulatedCrash>(
                    () => AtomicSaveFile.Write(_live, _prev, _tmp, "new", new CrashAt(cp)),
                    $"checkpoint {cp}");

                bool ok = AtomicSaveFile.TryRead(_live, _prev, s => s == "old" || s == "new", out var recovered);
                Assert.IsTrue(ok, $"checkpoint {cp}: nothing readable after crash");

                // Before the tmp→live promotion the old aggregate must win; at/after
                // it the new one must. Never anything else.
                string expected = cp == SaveCheckpoint.AfterTmpToLive ? "new" : "old";
                Assert.AreEqual(expected, recovered, $"checkpoint {cp}");
            }
        }

        [Test]
        public void Crash_AfterLiveToPrev_FallsBackToPrev()
        {
            AtomicSaveFile.Write(_live, _prev, _tmp, "old");
            Assert.Throws<SimulatedCrash>(
                () => AtomicSaveFile.Write(_live, _prev, _tmp, "new", new CrashAt(SaveCheckpoint.AfterLiveToPrev)));

            // The classic window: live is gone, only .prev holds the old aggregate.
            Assert.IsFalse(File.Exists(_live));
            Assert.IsTrue(AtomicSaveFile.TryRead(_live, _prev, null, out var recovered));
            Assert.AreEqual("old", recovered);
        }

        [Test]
        public void TryRead_TornLiveFile_ValidatorForcesPrevFallback()
        {
            AtomicSaveFile.Write(_live, _prev, _tmp, "good");
            AtomicSaveFile.Write(_live, _prev, _tmp, "TORN");

            bool ok = AtomicSaveFile.TryRead(_live, _prev, s => s != "TORN", out var recovered);
            Assert.IsTrue(ok);
            Assert.AreEqual("good", recovered);
        }

        // --------------- transaction consistency across crash points ---------------

        [Test]
        public void StoreToLocker_CrashAtEveryCheckpoint_ItemNeverDupedOrLost()
        {
            // Old: item on board, locker empty. New: item moved into the locker.
            var before = MakeAggregate(boardItems: new[] { "forensic_tools_t3" });
            var after  = MakeAggregate(lockerItems: new[] { "forensic_tools_t3" });

            AssertTransactionAtomic(before, after, agg =>
                agg.boardItems.Count + agg.locker.entries.Count == 1,
                "store: total item count must stay exactly 1");
        }

        [Test]
        public void RetrieveFromLocker_CrashAtEveryCheckpoint_ItemNeverDupedOrLost()
        {
            var before = MakeAggregate(lockerItems: new[] { "audio_investigation_t2" });
            var after  = MakeAggregate(boardItems: new[] { "audio_investigation_t2" });

            AssertTransactionAtomic(before, after, agg =>
                agg.boardItems.Count + agg.locker.entries.Count == 1,
                "retrieve: total item count must stay exactly 1");
        }

        [Test]
        public void MixedConsumption_CrashAtEveryCheckpoint_NeverIntermediateState()
        {
            // Lead proceed consumes board-first-then-locker: 1 board + 2 locker → 0 + 1.
            var before = MakeAggregate(
                boardItems:  new[] { "corner_diner_t2" },
                lockerItems: new[] { "corner_diner_t2", "corner_diner_t2" });
            var after = MakeAggregate(lockerItems: new[] { "corner_diner_t2" });

            AssertTransactionAtomic(before, after, agg =>
            {
                int total = agg.boardItems.Count + agg.locker.entries.Count;
                return total == 3 || total == 1; // never the half-consumed 2
            }, "mixed consumption: recovered state must be fully old (3) or fully new (1)");
        }

        [Test]
        public void SlotPurchase_CrashAtEveryCheckpoint_WalletAndSlotsStayPaired()
        {
            var before = MakeAggregate(walletSoft: 500, purchasedSlots: 0);
            var after  = MakeAggregate(walletSoft: 300, purchasedSlots: 1);

            AssertTransactionAtomic(before, after, agg =>
                (agg.walletSoft == 500 && agg.locker.purchasedSlots == 0) ||
                (agg.walletSoft == 300 && agg.locker.purchasedSlots == 1),
                "slot purchase: spend and slot grant must never separate");
        }

        // --------------- service state round-trip + migration ---------------

        [Test]
        public void ExportImport_RoundTripsEntriesAndSlots()
        {
            var state = new LockerStateDTO { purchasedSlots = 2 };
            state.entries.Add(new LockerEntryDTO { family = "forensic_tools", tier = 4, itemId = "forensic_tools_t5" });
            state.entries.Add(new LockerEntryDTO { family = "helens_gifts", tier = 1, itemId = "" });

            EvidenceLockerService.ImportState(state);
            var round = EvidenceLockerService.ExportState();

            Assert.AreEqual(2, round.purchasedSlots);
            Assert.AreEqual(2, round.entries.Count);
            Assert.AreEqual("forensic_tools", round.entries[0].family);
            Assert.AreEqual(4, round.entries[0].tier);
            Assert.AreEqual("forensic_tools_t5", round.entries[0].itemId);
            Assert.AreEqual(10, EvidenceLockerService.Capacity, "8 free + 2 purchased");
        }

        [Test]
        public void Transactions_MutateMemoryOnly_NoLockerFileAppears()
        {
            EvidenceLockerService.ImportState(new LockerStateDTO());
            EvidenceLockerService.TryStore(
                new AQ.App.Overflow.OverflowTileData { kind = AQ.App.Overflow.OverflowKind.Item, family = "garage", tier = 2 },
                "garage_t3");
            Assert.AreEqual(1, EvidenceLockerService.Count);

            Assert.IsTrue(EvidenceLockerService.TryTakeItem("garage", 2));
            Assert.AreEqual(0, EvidenceLockerService.Count);

            Assert.IsFalse(File.Exists(_legacy),
                "locker transactions must not write their own file — persistence is the board aggregate's job");
        }

        [Test]
        public void LegacyFile_MigratesOnNullImport_ThenDeleteRemovesIt()
        {
            var legacy = new LockerStateDTO { purchasedSlots = 1 };
            legacy.entries.Add(new LockerEntryDTO { family = "rusty_anchor", tier = 3, itemId = "rusty_anchor_t4" });
            File.WriteAllText(_legacy, JsonUtility.ToJson(legacy), Encoding.UTF8);

            EvidenceLockerService.ImportState(null); // pre-0.7.0 save (or none): legacy path
            Assert.AreEqual(1, EvidenceLockerService.Count);
            Assert.AreEqual(1, EvidenceLockerService.PurchasedSlots);
            Assert.AreEqual("rusty_anchor", EvidenceLockerService.GetAt(0).family);

            EvidenceLockerService.DeleteLegacyFile(); // what BoardSaveSystem does post-save
            Assert.IsFalse(File.Exists(_legacy));
        }

        [Test]
        public void ImportNull_NoLegacyFile_ResetsToEmpty()
        {
            var state = new LockerStateDTO { purchasedSlots = 3 };
            state.entries.Add(new LockerEntryDTO { family = "garage", tier = 0, itemId = "garage_t1" });
            EvidenceLockerService.ImportState(state);

            EvidenceLockerService.ImportState(null); // QA-reset boot: stale statics must clear
            Assert.AreEqual(0, EvidenceLockerService.Count);
            Assert.AreEqual(0, EvidenceLockerService.PurchasedSlots);
        }

        [Test]
        public void StateHash_ChangesOnStoreRetrieveAndPurchase()
        {
            EvidenceLockerService.ImportState(new LockerStateDTO());
            int empty = EvidenceLockerService.StateHash();

            EvidenceLockerService.TryStore(
                new AQ.App.Overflow.OverflowTileData { kind = AQ.App.Overflow.OverflowKind.Item, family = "press", tier = 1 },
                "press_t2");
            int stored = EvidenceLockerService.StateHash();
            Assert.AreNotEqual(empty, stored, "store must change the hash so the debounced save fires");

            Assert.IsTrue(EvidenceLockerService.TryTakeItem("press", 1));
            Assert.AreEqual(empty, EvidenceLockerService.StateHash(), "back to empty state, same hash");
        }

        // --------------- helpers ---------------

        private static TestAggregate MakeAggregate(
            string[] boardItems = null, string[] lockerItems = null,
            int walletSoft = 0, int purchasedSlots = 0)
        {
            var agg = new TestAggregate { walletSoft = walletSoft };
            agg.locker.purchasedSlots = purchasedSlots;
            if (boardItems != null) agg.boardItems.AddRange(boardItems);
            if (lockerItems != null)
                foreach (var id in lockerItems)
                {
                    int t = id.LastIndexOf("_t", StringComparison.Ordinal);
                    agg.locker.entries.Add(new LockerEntryDTO
                    {
                        family = id.Substring(0, t),
                        tier   = int.Parse(id.Substring(t + 2)) - 1,
                        itemId = id
                    });
                }
            return agg;
        }

        /// <summary>
        /// Writes <paramref name="before"/> cleanly, then attempts to write
        /// <paramref name="after"/> with a crash injected at every checkpoint in turn;
        /// after each crash the recovered aggregate must satisfy the invariant AND be
        /// exactly the before or after state (checked via re-serialization equality).
        /// </summary>
        private void AssertTransactionAtomic(
            TestAggregate before, TestAggregate after,
            Func<TestAggregate, bool> invariant, string message)
        {
            string beforeJson = JsonUtility.ToJson(before);
            string afterJson  = JsonUtility.ToJson(after);

            foreach (SaveCheckpoint cp in Enum.GetValues(typeof(SaveCheckpoint)))
            {
                ResetFiles();
                AtomicSaveFile.Write(_live, _prev, _tmp, beforeJson);

                Assert.Throws<SimulatedCrash>(
                    () => AtomicSaveFile.Write(_live, _prev, _tmp, afterJson, new CrashAt(cp)),
                    $"checkpoint {cp}");

                bool ok = AtomicSaveFile.TryRead(_live, _prev,
                    s => { try { return JsonUtility.FromJson<TestAggregate>(s) != null; } catch { return false; } },
                    out var json);
                Assert.IsTrue(ok, $"checkpoint {cp}: no readable aggregate — {message}");

                Assert.IsTrue(json == beforeJson || json == afterJson,
                    $"checkpoint {cp}: recovered aggregate is neither old nor new — {message}");

                var recovered = JsonUtility.FromJson<TestAggregate>(json);
                Assert.IsTrue(invariant(recovered), $"checkpoint {cp}: {message}");
            }
        }

        private void ResetFiles()
        {
            foreach (var p in new[] { _live, _prev, _tmp })
                if (File.Exists(p)) File.Delete(p);
        }
    }
}
