// Assembly: com.aq.sharedkernel.tests (EditMode)
// File: Tests/EditMode/CaseFlow/CaseFlowServiceTests.cs

using NUnit.Framework;
using AQ.SharedKernel.CaseFlow;

namespace AQ.SharedKernel.Tests.CaseFlow
{
    public class CaseFlowServiceTests
    {
        [Test]
        public void Begin_SetsEpisode_AndStartsAtFirstStep()
        {
            var svc = new InMemoryCaseFlowService();
            svc.Begin(new EpisodeId("Ep01"), "Intro", "Minigame", "Resolution");
            Assert.AreEqual("Ep01", svc.Current.Episode.Value);
            Assert.AreEqual(0, svc.Current.StepIndex);
            Assert.IsFalse(svc.Current.IsComplete);
        }

        [Test]
        public void Complete_Advances_AndCompletesAtEnd()
        {
            var svc = new InMemoryCaseFlowService();
            svc.Begin(new EpisodeId("Ep01"), "A","B");
            Assert.IsTrue(svc.CompleteCurrentStep()); // -> step 1
            Assert.AreEqual(1, svc.Current.StepIndex);
            Assert.IsTrue(svc.CompleteCurrentStep()); // -> step 2 == complete
            Assert.IsTrue(svc.Current.IsComplete);
            Assert.IsFalse(svc.CompleteCurrentStep()); // no further advance
        }

        [Test]
        public void Reset_ReturnsToStart()
        {
            var svc = new InMemoryCaseFlowService();
            svc.Begin(new EpisodeId("Ep01"), "A","B");
            svc.CompleteCurrentStep();
            svc.Reset();
            Assert.AreEqual(0, svc.Current.StepIndex);
            Assert.IsFalse(svc.Current.IsComplete);
        }
    }
}
