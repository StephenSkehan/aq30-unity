using System.Collections.Generic;
using AQ.App.Leads;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// The lead counter denominator (2026-08-26). LeadsBarView and CaseFlowDebugOverlayMB
    /// both hardcoded twelve, which was correct for The Listener and wrong for every
    /// episode after it: a sixteen-lead case shipped a counter reading "14 / 12".
    ///
    /// The denominator now derives from the bound repository's database, counting leads
    /// with boardPhase greater than zero. boardPhase 0 is repeatables and teasers, which
    /// sit outside the case arc and must not be counted (Episode 1's tail spawns
    /// cold_case_a and ep2_teaser, so this is not hypothetical).
    ///
    /// Coverage: the twelve-lead case still reads twelve, a sixteen-lead case reads
    /// sixteen, teasers are excluded, an unbound view degrades instead of throwing, a
    /// runtime-spawned lead cannot push the numerator past the denominator, and rebinding
    /// a different database recomputes rather than serving a stale cache.
    /// </summary>
    public class LeadsBarCounterTests
    {
        private GameObject _barGo;
        private GameObject _repoGo;
        private LeadsBarView _bar;
        private LeadsRepository _repo;
        private readonly List<Object> _made = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _barGo  = new GameObject("LeadsBarUnderTest");
            _bar    = _barGo.AddComponent<LeadsBarView>();
            _repoGo = new GameObject("LeadsRepoUnderTest");
            _repo   = _repoGo.AddComponent<LeadsRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
            if (_barGo  != null) Object.DestroyImmediate(_barGo);
            if (_repoGo != null) Object.DestroyImmediate(_repoGo);
        }

        private LeadData MakeLead(string id, int boardPhase)
        {
            var lead = ScriptableObject.CreateInstance<LeadData>();
            lead.leadId     = id;
            lead.boardPhase = boardPhase;
            _made.Add(lead);
            return lead;
        }

        private LeadsDatabase MakeDatabase(int arcLeads, int teasers)
        {
            var db = ScriptableObject.CreateInstance<LeadsDatabase>();
            _made.Add(db);
            for (int i = 0; i < arcLeads; i++) db.Add(MakeLead($"arc_{i}", 1));
            for (int i = 0; i < teasers;  i++) db.Add(MakeLead($"teaser_{i}", 0));
            return db;
        }

        private void BindDatabase(LeadsDatabase db)
        {
            _repo.SetDatabase(db);
            _bar.Bind(_repo);
        }

        [Test]
        public void TwelveLeadCase_StillReadsTwelve()
        {
            BindDatabase(MakeDatabase(12, 0));
            Assert.AreEqual(12, _bar.CaseArcTotal,
                "The Listener's arc must be unaffected by making the denominator dynamic.");
        }

        [Test]
        public void SixteenLeadCase_ReadsSixteen_NotTwelve()
        {
            BindDatabase(MakeDatabase(16, 0));
            Assert.AreEqual(16, _bar.CaseArcTotal,
                "This is the shipped bug: a sixteen-lead episode used to render out of twelve.");
        }

        [Test]
        public void TeasersAndRepeatables_AreExcludedFromTheArc()
        {
            // Episode 1's close spawns cold_case_a and ep2_teaser, both boardPhase 0.
            BindDatabase(MakeDatabase(16, 3));
            Assert.AreEqual(16, _bar.CaseArcTotal,
                "boardPhase 0 sits outside the case arc and must not inflate the denominator.");
        }

        [Test]
        public void NullLeadsInTheDatabase_AreSkipped()
        {
            var db = MakeDatabase(4, 0);
            db.Add(null);
            BindDatabase(db);
            Assert.AreEqual(4, _bar.CaseArcTotal,
                "A missing asset reference must not be counted and must not throw.");
        }

        [Test]
        public void UnboundView_DegradesToZero_AndDoesNotThrow()
        {
            Assert.AreEqual(0, _bar.CaseArcTotal,
                "A view with no repository must report zero rather than a stale literal.");
        }

        [Test]
        public void BoundRepositoryWithNoDatabase_DegradesToZero()
        {
            _bar.Bind(_repo);
            Assert.AreEqual(0, _bar.CaseArcTotal);
        }

        [Test]
        public void NumeratorCanNeverExceedDenominator()
        {
            BindDatabase(MakeDatabase(3, 0));

            // A lead spawned at runtime that was never in the authored database.
            for (int i = 0; i < 5; i++)
                LeadsRuntimeBus.BroadcastActivated(MakeLead($"runtime_{i}", 1));

            Assert.GreaterOrEqual(_bar.CaseArcTotal, _bar.ActivatedCount,
                "\"14 / 12\" is the exact defect this guards: the counter must never read past itself.");
        }

        [Test]
        public void RebindingADifferentDatabase_Recomputes()
        {
            BindDatabase(MakeDatabase(12, 0));
            Assert.AreEqual(12, _bar.CaseArcTotal);

            BindDatabase(MakeDatabase(16, 0));
            Assert.AreEqual(16, _bar.CaseArcTotal,
                "Bind must invalidate the cached total or an episode change serves a stale count.");
        }
    }
}
