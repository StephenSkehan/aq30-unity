using System.Collections.Generic;
using AQ.App.Episodes;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// The episode registry (multi-episode phase 2, M1). Episode identity used
    /// to live in a scene file and a scatter of Episode-1 literals; the catalog
    /// is now the single source of truth, so this suite is the contract that
    /// keeps it trustworthy: slot ids unique across ids AND aliases, the alias
    /// table covers every id ever written into a save ("e1_the_listener" from
    /// the shipped scene, "Ep01" from tests/Addressables), unknown ids pass
    /// through CanonicalId unchanged (a save must never be silently re-homed),
    /// and every playable entry carries the fields the de-hardcoded systems
    /// now read from it.
    /// </summary>
    public class EpisodeCatalogValidationTests
    {
        private readonly List<Object> _made = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
            EpisodeRuntime.ResetForTests();
        }

        private EpisodeCatalog MakeCatalog(params EpisodeEntry[] entries)
        {
            var catalog = ScriptableObject.CreateInstance<EpisodeCatalog>();
            _made.Add(catalog);
            foreach (var e in entries) catalog.Add(e);
            return catalog;
        }

        private static EpisodeEntry Entry(string id, params string[] aliases) =>
            new EpisodeEntry { episodeId = id, legacyIdAliases = aliases };

        // ---- Behaviour (runtime-built catalogs) ----

        [Test]
        public void FindById_ResolvesSlotIds_AndAliases()
        {
            var catalog = MakeCatalog(Entry("ep01", "e1_the_listener", "Ep01"), Entry("ep02"));

            Assert.AreSame(catalog.Episodes[0], catalog.FindById("ep01"));
            Assert.AreSame(catalog.Episodes[0], catalog.FindById("e1_the_listener"),
                "saves in the wild recorded the scene-serialized id; the alias table is what keeps their progress owned");
            Assert.AreSame(catalog.Episodes[0], catalog.FindById("Ep01"));
            Assert.AreSame(catalog.Episodes[1], catalog.FindById("ep02"));
        }

        [Test]
        public void FindById_UnknownNullOrEmpty_ReturnsNull()
        {
            var catalog = MakeCatalog(Entry("ep01"));
            Assert.IsNull(catalog.FindById("ep99"));
            Assert.IsNull(catalog.FindById(null));
            Assert.IsNull(catalog.FindById(string.Empty));
        }

        [Test]
        public void CanonicalId_MapsAliases_AndPassesUnknownThrough()
        {
            var catalog = MakeCatalog(Entry("ep01", "e1_the_listener"));

            Assert.AreEqual("ep01", catalog.CanonicalId("e1_the_listener"));
            Assert.AreEqual("ep01", catalog.CanonicalId("ep01"));
            Assert.AreEqual("some_future_id", catalog.CanonicalId("some_future_id"),
                "an id the catalog has never heard of must survive round-trip, never be re-homed");
        }

        [Test]
        public void Next_WalksSeasonOrder_AndEndsWithNull()
        {
            var catalog = MakeCatalog(Entry("ep01", "e1_the_listener"), Entry("ep02"), Entry("ep03"));

            Assert.AreEqual("ep02", catalog.Next("ep01").episodeId);
            Assert.AreEqual("ep02", catalog.Next("e1_the_listener").episodeId, "Next must resolve aliases too");
            Assert.IsNull(catalog.Next("ep03"));
            Assert.IsNull(catalog.Next("ep99"));
        }

        // ---- The shipped asset ----

        private static EpisodeCatalog LoadShipped()
        {
            var catalog = Resources.Load<EpisodeCatalog>("App/Episodes/EpisodeCatalog");
            Assert.IsNotNull(catalog, "Resources/App/Episodes/EpisodeCatalog.asset must exist — every de-hardcoded system boots from it");
            return catalog;
        }

        [Test]
        public void ShippedCatalog_IdsAndAliases_AreUniqueAcrossTheSeason()
        {
            var catalog = LoadShipped();
            var seen = new HashSet<string>();
            foreach (var e in catalog.Episodes)
            {
                Assert.IsNotNull(e);
                Assert.IsFalse(string.IsNullOrEmpty(e.episodeId), "an entry with no id is unaddressable");
                Assert.IsTrue(seen.Add(e.episodeId), $"duplicate id '{e.episodeId}'");
                if (e.legacyIdAliases == null) continue;
                foreach (var alias in e.legacyIdAliases)
                    Assert.IsTrue(seen.Add(alias), $"alias '{alias}' collides with another entry's id or alias");
            }
        }

        [Test]
        public void ShippedCatalog_Ep01_CoversBothHistoricIdNamespaces()
        {
            var catalog = LoadShipped();
            Assert.AreEqual("ep01", catalog.CanonicalId("e1_the_listener"),
                "the shipped scene serialized this id into every real save");
            Assert.AreEqual("ep01", catalog.CanonicalId("Ep01"),
                "tests and the Addressables label use this spelling");
        }

        [Test]
        public void ShippedCatalog_PlayableEntries_CarryEverythingTheSystemsRead()
        {
            var catalog = LoadShipped();
            int playable = 0;
            foreach (var e in catalog.Episodes)
            {
                if (!e.HasContent) continue;
                playable++;
                Assert.IsTrue(e.steps != null && e.steps.Length > 0, $"{e.episodeId}: playable entry needs a golden path");
                Assert.IsFalse(string.IsNullOrEmpty(e.completionFlag), $"{e.episodeId}: no completionFlag means the episode can never be detected complete");
                Assert.IsFalse(string.IsNullOrEmpty(e.title), $"{e.episodeId}: title feeds the resolution screen");
                Assert.IsFalse(string.IsNullOrEmpty(e.entryLeadId), $"{e.episodeId}: entryLeadId feeds the hint gate");
                Assert.IsNotNull(e.database.FindById(e.entryLeadId),
                    $"{e.episodeId}: entryLeadId '{e.entryLeadId}' is not in the entry's own database");
            }
            Assert.GreaterOrEqual(playable, 1, "the season must have at least one playable episode");
        }

        [Test]
        public void ShippedCatalog_CompletionFlags_AreUniquePerPlayableEpisode()
        {
            var catalog = LoadShipped();
            var seen = new HashSet<string>();
            foreach (var e in catalog.Episodes)
            {
                if (!e.HasContent) continue;
                Assert.IsTrue(seen.Add(e.completionFlag),
                    $"completion flag '{e.completionFlag}' is shared by two episodes — completing one would complete both");
            }
        }

        [Test]
        public void ShippedCatalog_FirstPlayableEpisode_IsEp01()
        {
            var catalog = LoadShipped();
            Assert.IsNotNull(catalog.First);
            Assert.AreEqual("ep01", catalog.First.episodeId);
            Assert.IsTrue(catalog.First.HasContent, "the season opener must be playable");
        }
    }
}
