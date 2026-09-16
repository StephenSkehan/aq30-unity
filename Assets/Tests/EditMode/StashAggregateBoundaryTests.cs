using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using AQ.App.Overflow;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// Stash (overflow bucket) crash-boundary contract, schema 0.8.0: transactions
    /// mutate memory only (persistence is the board aggregate's job, exactly like
    /// the Evidence Locker at 0.7.0), the legacy overflow_state.json migrates once
    /// and is then deleted, and StateHash drives the aggregate's debounced save.
    /// </summary>
    public sealed class StashAggregateBoundaryTests
    {
        private string _legacy;
        private List<OverflowTileData> _preTestState;

        [SetUp]
        public void SetUp()
        {
            _legacy = Path.Combine(Application.persistentDataPath, "overflow_state.json");
            _preTestState = OverflowBucketService.ExportState();
            OverflowBucketService.ImportState(null);
            if (File.Exists(_legacy)) File.Delete(_legacy);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_legacy)) File.Delete(_legacy);
            OverflowBucketService.ImportState(_preTestState);
        }

        private static OverflowTileData Tile(string family, int tier, OverflowKind kind = OverflowKind.Item)
            => new OverflowTileData { kind = kind, family = family, tier = tier };

        [Test]
        public void Transactions_MutateMemoryOnly_NoStashFileAppears()
        {
            OverflowBucketService.Push(Tile("garage", 2));
            Assert.AreEqual(1, OverflowBucketService.Count);

            var popped = OverflowBucketService.Pop();
            Assert.IsTrue(popped.HasValue);
            Assert.AreEqual("garage", popped.Value.family);
            Assert.AreEqual(0, OverflowBucketService.Count);

            Assert.IsFalse(File.Exists(_legacy),
                "stash transactions must not write their own file — persistence is the board aggregate's job");
        }

        [Test]
        public void ExportImport_RoundTripsOrderAndContent()
        {
            OverflowBucketService.Push(Tile("press", 1));
            OverflowBucketService.Push(Tile("gen_junk", 0, OverflowKind.Generator));

            var exported = OverflowBucketService.ExportState();
            OverflowBucketService.ImportState(null);
            Assert.AreEqual(0, OverflowBucketService.Count);

            OverflowBucketService.ImportState(exported);
            Assert.AreEqual(2, OverflowBucketService.Count);

            // FILO: the generator went on last, so it comes off first.
            var top = OverflowBucketService.Pop();
            Assert.AreEqual(OverflowKind.Generator, top.Value.kind);
            Assert.AreEqual("gen_junk", top.Value.family);
        }

        [Test]
        public void StateHash_ChangesOnPush_ReturnsOnPop()
        {
            int empty = OverflowBucketService.StateHash();

            OverflowBucketService.Push(Tile("press", 1));
            Assert.AreNotEqual(empty, OverflowBucketService.StateHash(),
                "push must change the hash so the debounced aggregate save fires");

            OverflowBucketService.Pop();
            Assert.AreEqual(empty, OverflowBucketService.StateHash(), "back to empty state, same hash");
        }

        [Test]
        public void LegacyFile_LoadMigrates_ThenDeleteRemovesIt()
        {
            // Shape must match the service's private DTO ({"items":[...]}).
            File.WriteAllText(_legacy,
                "{\"items\":[{\"kind\":1,\"family\":\"corner_diner\",\"tier\":0}]}",
                Encoding.UTF8);

            OverflowBucketService.Load(); // pre-0.8.0 boot path
            Assert.AreEqual(1, OverflowBucketService.Count);
            var top = OverflowBucketService.Peek();
            Assert.AreEqual(OverflowKind.Generator, top.Value.kind);
            Assert.AreEqual("corner_diner", top.Value.family);

            OverflowBucketService.DeleteLegacyFile(); // what BoardSaveSystem does post-save
            Assert.IsFalse(File.Exists(_legacy));
        }

        [Test]
        public void Clear_EmptiesStack_AndRemovesLegacyFile()
        {
            OverflowBucketService.Push(Tile("garage", 0));
            File.WriteAllText(_legacy, "{\"items\":[]}", Encoding.UTF8);

            OverflowBucketService.Clear(); // QA reset path
            Assert.AreEqual(0, OverflowBucketService.Count);
            Assert.IsFalse(File.Exists(_legacy), "QA reset must not leave a stale legacy file to resurrect");
        }

        [Test]
        public void ImportNull_ResetsToEmpty()
        {
            OverflowBucketService.Push(Tile("press", 3));
            OverflowBucketService.ImportState(null);
            Assert.AreEqual(0, OverflowBucketService.Count);
            Assert.IsFalse(OverflowBucketService.Peek().HasValue);
        }
    }
}
