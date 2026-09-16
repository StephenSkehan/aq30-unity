using NUnit.Framework;
using UnityEngine;
using AQ.App.UI.Specials;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// Case Kit crash-boundary contract, schema 0.9.0: grants/consumes mutate
    /// memory only (persistence is the board aggregate's job, like the locker
    /// at 0.7.0 and the Stash at 0.8.0), legacy PlayerPrefs keys migrate once
    /// and are then deleted, and StateHash drives the debounced aggregate save.
    /// </summary>
    public sealed class SpecialsAggregateBoundaryTests
    {
        private const string LegacyStateKey    = "aq.specials.state";
        private const string LegacyCassetteKey = "aq.specials.cassettes";

        private SpecialsStateDTO _preTestState;

        [SetUp]
        public void SetUp()
        {
            _preTestState = SpecialItemsService.ExportState();
            PlayerPrefs.DeleteKey(LegacyStateKey);
            PlayerPrefs.DeleteKey(LegacyCassetteKey);
            SpecialItemsService.ImportState(new SpecialsStateDTO());
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(LegacyStateKey);
            PlayerPrefs.DeleteKey(LegacyCassetteKey);
            SpecialItemsService.ImportState(_preTestState);
        }

        [Test]
        public void Transactions_MutateMemoryOnly_NoPrefsKeysAppear()
        {
            SpecialItemsService.Grant(SpecialId.BoxKnife);
            Assert.AreEqual(1, SpecialItemsService.CountOf(SpecialId.BoxKnife));

            Assert.IsTrue(SpecialItemsService.Consume(SpecialId.BoxKnife));
            Assert.AreEqual(0, SpecialItemsService.CountOf(SpecialId.BoxKnife));

            SpecialItemsService.GrantCassette("App/Audio/Cassettes/test_tape");

            Assert.IsFalse(PlayerPrefs.HasKey(LegacyStateKey),
                "kit transactions must not write PlayerPrefs — persistence is the board aggregate's job");
            Assert.IsFalse(PlayerPrefs.HasKey(LegacyCassetteKey),
                "cassette grants must not write PlayerPrefs — persistence is the board aggregate's job");
        }

        [Test]
        public void Consume_AtZero_RefusesAndStaysZero()
        {
            Assert.IsFalse(SpecialItemsService.Consume(SpecialId.SkeletonKey));
            Assert.AreEqual(0, SpecialItemsService.CountOf(SpecialId.SkeletonKey));
        }

        [Test]
        public void ExportImport_RoundTripsCountsAndCassettes()
        {
            SpecialItemsService.Grant(SpecialId.SearchWarrant, 2);
            SpecialItemsService.Grant(SpecialId.EvidenceTag);
            SpecialItemsService.GrantCassette("App/Audio/Cassettes/cassette_dot_goodnight");

            var exported = SpecialItemsService.ExportState();
            SpecialItemsService.ImportState(new SpecialsStateDTO());
            Assert.AreEqual(0, SpecialItemsService.TotalCount);
            Assert.AreEqual(0, SpecialItemsService.Cassettes.Count);

            SpecialItemsService.ImportState(exported);
            Assert.AreEqual(2, SpecialItemsService.CountOf(SpecialId.SearchWarrant));
            Assert.AreEqual(1, SpecialItemsService.CountOf(SpecialId.EvidenceTag));
            Assert.AreEqual(3, SpecialItemsService.TotalCount);
            Assert.AreEqual(1, SpecialItemsService.Cassettes.Count);
            Assert.AreEqual("App/Audio/Cassettes/cassette_dot_goodnight", SpecialItemsService.Cassettes[0]);
        }

        [Test]
        public void StateHash_ChangesOnGrant_ReturnsOnConsume()
        {
            int empty = SpecialItemsService.StateHash();

            SpecialItemsService.Grant(SpecialId.BoltCutters);
            Assert.AreNotEqual(empty, SpecialItemsService.StateHash(),
                "grant must change the hash so the debounced aggregate save fires");

            SpecialItemsService.Consume(SpecialId.BoltCutters);
            Assert.AreEqual(empty, SpecialItemsService.StateHash(), "back to empty state, same hash");
        }

        [Test]
        public void CassetteGrant_Dedupes_AndChangesHash()
        {
            int empty = SpecialItemsService.StateHash();
            SpecialItemsService.GrantCassette("App/Audio/Cassettes/tape_a");
            SpecialItemsService.GrantCassette("App/Audio/Cassettes/tape_a"); // Mo-shop dupe path
            Assert.AreEqual(1, SpecialItemsService.Cassettes.Count);
            Assert.AreNotEqual(empty, SpecialItemsService.StateHash());
        }

        [Test]
        public void LegacyKeys_MigrateOnNullImport_ThenDeleteRemovesThem()
        {
            PlayerPrefs.SetString(LegacyStateKey, "{\"ids\":[\"CarbonCopy\"],\"counts\":[3]}");
            PlayerPrefs.SetString(LegacyCassetteKey, "App/Audio/Cassettes/tape_a|App/Audio/Cassettes/tape_b");

            SpecialItemsService.ImportState(null); // pre-0.9.0 save (or none): legacy path
            Assert.AreEqual(3, SpecialItemsService.CountOf(SpecialId.CarbonCopy));
            Assert.AreEqual(2, SpecialItemsService.Cassettes.Count);

            SpecialItemsService.DeleteLegacyKeys(); // what BoardSaveSystem does post-save
            Assert.IsFalse(PlayerPrefs.HasKey(LegacyStateKey));
            Assert.IsFalse(PlayerPrefs.HasKey(LegacyCassetteKey));
        }

        [Test]
        public void ImportNull_NoLegacyKeys_ResetsToEmpty()
        {
            SpecialItemsService.Grant(SpecialId.SkeletonKey, 5);
            SpecialItemsService.ImportState(null); // QA-reset boot: stale statics must clear
            Assert.AreEqual(0, SpecialItemsService.TotalCount);
            Assert.AreEqual(0, SpecialItemsService.Cassettes.Count);
        }

        [Test]
        public void Clear_EmptiesState_AndRemovesLegacyKeys()
        {
            SpecialItemsService.Grant(SpecialId.BoxKnife);
            PlayerPrefs.SetString(LegacyStateKey, "{\"ids\":[],\"counts\":[]}");

            SpecialItemsService.Clear(); // QA reset path
            Assert.AreEqual(0, SpecialItemsService.TotalCount);
            Assert.IsFalse(PlayerPrefs.HasKey(LegacyStateKey),
                "QA reset must not leave stale legacy keys to resurrect");
        }
    }
}
