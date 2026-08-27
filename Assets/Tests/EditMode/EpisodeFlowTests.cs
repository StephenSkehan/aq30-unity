using System.Collections.Generic;
using AQ.App.Episodes;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// The episode transition seam and the linear unlock rule (multi-episode
    /// M6/M7, rulings R6: linear season, no replay of completed episodes).
    /// Unlock is DERIVED state — previous playable episode's complete bit —
    /// so there is nothing extra to persist and nothing to migrate; these tests
    /// pin the derivation. The seam itself (handlers registered by the save
    /// system) must degrade to safe no-ops when nothing has registered: a
    /// switch that cannot persist must report false so no caller reloads the
    /// scene on top of an unsaved episode.
    /// </summary>
    public class EpisodeFlowTests
    {
        private readonly List<Object> _made = new List<Object>();
        private readonly Dictionary<string, EpisodeProgress> _progress = new Dictionary<string, EpisodeProgress>();

        [SetUp]
        public void SetUp()
        {
            _progress.Clear();
            EpisodeRuntime.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
            EpisodeRuntime.ResetForTests();
            // The seam's statics are reset by RuntimeInitializeOnLoad in play;
            // tests must not leak registrations into each other.
            EpisodeFlow.SwitchHandler = null;
            EpisodeFlow.ProgressProvider = null;
            EpisodeFlow.SelectorOpener = null;
        }

        private EpisodeProgress ProgressOf(string id) =>
            _progress.TryGetValue(id, out var p) ? p : default;

        private void MarkComplete(string id) =>
            _progress[id] = new EpisodeProgress { Started = true, Complete = true };

        /// <summary>ep01..ep03 playable, ep04 a reserved slot (no database).</summary>
        private EpisodeCatalog MakeSeason()
        {
            var catalog = ScriptableObject.CreateInstance<EpisodeCatalog>();
            _made.Add(catalog);
            foreach (var id in new[] { "ep01", "ep02", "ep03" })
            {
                var db = ScriptableObject.CreateInstance<AQ.App.Leads.LeadsDatabase>();
                _made.Add(db);
                catalog.Add(new EpisodeEntry { episodeId = id, database = db, steps = new[] { "S" } });
            }
            catalog.Add(new EpisodeEntry { episodeId = "ep04" }); // reserved
            return catalog;
        }

        // ---- The unlock rule ----

        [Test]
        public void FirstPlayableEpisode_IsUnlocked_FromTheStart()
        {
            var catalog = MakeSeason();
            Assert.IsTrue(EpisodeFlow.IsUnlocked(catalog, "ep01", ProgressOf));
        }

        [Test]
        public void SecondEpisode_LocksUntilTheFirstIsComplete()
        {
            var catalog = MakeSeason();
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, "ep02", ProgressOf),
                "linear season: ep02 must not open before ep01's durable complete bit is set");

            MarkComplete("ep01");
            Assert.IsTrue(EpisodeFlow.IsUnlocked(catalog, "ep02", ProgressOf));
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, "ep03", ProgressOf),
                "completing ep01 opens ep02 only, not the whole season");
        }

        [Test]
        public void ReservedSlot_IsNeverUnlocked()
        {
            var catalog = MakeSeason();
            MarkComplete("ep01");
            MarkComplete("ep02");
            MarkComplete("ep03");
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, "ep04", ProgressOf),
                "a slot with no database has nothing to play regardless of progress");
        }

        [Test]
        public void IsUnlocked_IsNullSafe()
        {
            var catalog = MakeSeason();
            Assert.IsFalse(EpisodeFlow.IsUnlocked(null, "ep01", ProgressOf));
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, "ep99", ProgressOf));
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, null, ProgressOf));
            Assert.IsFalse(EpisodeFlow.IsUnlocked(catalog, "ep01", null));
        }

        // ---- NextPlayable (the resolution screen's "Next Episode" source) ----

        [Test]
        public void NextPlayable_ReturnsTheUnlockedSuccessor()
        {
            var catalog = MakeSeason();
            Assert.IsNull(EpisodeFlow.NextPlayable(catalog, "ep01", ProgressOf),
                "ep01 not complete yet: its successor is still locked, so there is no next to offer");

            MarkComplete("ep01");
            Assert.AreEqual("ep02", EpisodeFlow.NextPlayable(catalog, "ep01", ProgressOf).episodeId);
        }

        [Test]
        public void NextPlayable_SkipsReservedSlots_AndEndsTheSeasonWithNull()
        {
            var catalog = MakeSeason();
            MarkComplete("ep01");
            MarkComplete("ep02");
            MarkComplete("ep03");
            Assert.IsNull(EpisodeFlow.NextPlayable(catalog, "ep03", ProgressOf),
                "ep04 is a reserved slot: after ep03 the season ends until its content ships");
        }

        // ---- The seam degrades safely ----

        [Test]
        public void TrySwitch_WithNoRegisteredHandler_ReportsFalse()
        {
            Assert.IsFalse(EpisodeFlow.TrySwitch("ep02"),
                "false means 'nothing durable changed' — a caller must never reload the scene on it");
        }

        [Test]
        public void ProgressOf_WithNoProvider_ReportsNotStarted()
        {
            var p = EpisodeFlow.ProgressOf("ep01");
            Assert.IsFalse(p.Started);
            Assert.IsFalse(p.Complete);
        }

        [Test]
        public void TrySwitch_RoutesThroughTheRegisteredHandler()
        {
            string asked = null;
            EpisodeFlow.SwitchHandler = id => { asked = id; return true; };
            Assert.IsTrue(EpisodeFlow.TrySwitch("ep02"));
            Assert.AreEqual("ep02", asked);
        }
    }
}
