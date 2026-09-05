using AQ.App.CaseFlow;
using AQ.App.Events;
using AQ.App.Leads;
using AQ.App.Presentation;
using AQ.SharedKernel.CaseFlow;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// Episode identity at resolution time (2026-08-27). CaseResolutionService
    /// carried a hardcoded const "e1_the_listener" — an episode demoted on
    /// 2026-08-23 — while the save aggregate recorded the orchestrator's id from
    /// the running caseflow ("Ep01"). Two id namespaces disagreeing: analytics
    /// reported a retired episode for every completion, and would have kept doing
    /// so for episodes two through four.
    ///
    /// The id now comes from CaseFlowLocator's running service. Coverage: the
    /// resolved id matches whatever episode the caseflow began, the event and the
    /// resolver agree, a missing caseflow service degrades to the "unknown"
    /// sentinel instead of a stale literal (and never throws), resolution still
    /// fires exactly once, and leads without the completion flag don't fire it.
    /// </summary>
    public class CaseResolutionEpisodeIdTests
    {
        private ICaseFlowService _priorService;
        private GameObject _serviceGo;
        private readonly System.Collections.Generic.List<Object> _made =
            new System.Collections.Generic.List<Object>();

        [SetUp]
        public void SetUp()
        {
            _priorService = CaseFlowLocator.Instance;
            CaseFlowLocator.Set(null);
            // No catalog: completion-flag resolution takes the fallback path,
            // which is what these tests drive with the e1 literal.
            AQ.App.Episodes.EpisodeRuntime.OverrideForTests(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceGo != null) Object.DestroyImmediate(_serviceGo);
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
            CaseFlowLocator.Set(_priorService);
            AQ.App.Episodes.EpisodeRuntime.ResetForTests();
        }

        // OnEnable (and thus the LeadsRuntimeBus subscription) does not run in
        // edit mode, so tests call service.OnLeadActivated directly — same
        // pattern as LeadsBarCounterTests driving Bind() rather than lifecycle.
        private CaseResolutionService InstallResolutionService()
        {
            _serviceGo = new GameObject("CaseResolutionUnderTest");
            return _serviceGo.AddComponent<CaseResolutionService>();
        }

        private LeadData MakeLead(params string[] narrativeFlags)
        {
            var lead = ScriptableObject.CreateInstance<LeadData>();
            lead.leadId = "test_close";
            lead.NarrativeFlags = narrativeFlags;
            _made.Add(lead);
            return lead;
        }

        private static void BeginEpisode(string episodeId)
        {
            var svc = new InMemoryCaseFlowService();
            svc.Begin(new EpisodeId(episodeId), "Step_A", "Step_B");
            CaseFlowLocator.Set(svc);
        }

        [Test]
        public void ResolvedId_ComesFromTheRunningCaseflow_NotALiteral()
        {
            BeginEpisode("ep02_test_case");
            Assert.AreEqual("ep02_test_case", CaseResolutionService.ResolveEpisodeId(),
                "The id must track whatever episode Begin() started; a literal is stale by construction.");
        }

        [Test]
        public void ResolvedId_IsNever_TheRetiredHardcode()
        {
            BeginEpisode("Ep01");
            Assert.AreNotEqual("e1_the_listener", CaseResolutionService.ResolveEpisodeId(),
                "This is the shipped defect: completions reported an episode demoted on 2026-08-23.");
        }

        [Test]
        public void NoCaseflowService_DegradesToUnknown_AndDoesNotThrow()
        {
            Assert.AreEqual(CaseResolutionService.UnknownEpisodeId,
                CaseResolutionService.ResolveEpisodeId(),
                "No running caseflow must yield the sentinel, not a stale episode name.");
        }

        [Test]
        public void CompletionFlagLead_PublishesEvent_WithTheRunningEpisodeId()
        {
            BeginEpisode("ep_test_running");
            var service = InstallResolutionService();

            string published = null;
            using (GlobalBus.Bus.Subscribe<CaseResolvedEvent>(e => published = e.EpisodeId))
            {
                service.OnLeadActivated(MakeLead("e1.ep01.complete"));
            }

            Assert.AreEqual("ep_test_running", published,
                "CaseResolvedEvent must carry the caseflow's id — the save and analytics namespaces were disagreeing.");
        }

        [Test]
        public void Resolution_FiresExactlyOnce()
        {
            BeginEpisode("ep_once");
            var service = InstallResolutionService();

            int fired = 0;
            using (GlobalBus.Bus.Subscribe<CaseResolvedEvent>(_ => fired++))
            {
                var lead = MakeLead("e1.ep01.complete");
                service.OnLeadActivated(lead);
                service.OnLeadActivated(lead);
            }

            Assert.AreEqual(1, fired, "A second completion-flag activation must not re-publish resolution.");
        }

        [Test]
        public void LeadWithoutTheCompletionFlag_DoesNotResolve()
        {
            BeginEpisode("ep_quiet");
            var service = InstallResolutionService();

            int fired = 0;
            using (GlobalBus.Bus.Subscribe<CaseResolvedEvent>(_ => fired++))
            {
                service.OnLeadActivated(MakeLead("some.other.flag"));
                service.OnLeadActivated(MakeLead());
            }

            Assert.AreEqual(0, fired);
        }
    }
}
