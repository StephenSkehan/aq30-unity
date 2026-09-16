// Assembly: AQ.Tests (EditMode)
// Purpose: The first-card FTUE choreography became per-episode data on
//          2026-09-02. These pin the two contracts that keep The Listener
//          playing exactly as shipped: the built-in defaults reproduce the old
//          constants, and a catalog entry without a config resolves to them.

using AQ.App.Episodes;
using AQ.App.FTUE;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    public class FtueChoreographyConfigTests
    {
        [Test]
        public void ListenerDefaults_ReproduceTheShippedConstants()
        {
            var c = FtueChoreographyConfig.ListenerDefaults();
            try
            {
                Assert.AreEqual("e1_tip", c.leadId);
                Assert.IsNull(c.introGraph, "Listener intro is a span of the lead's own resolution dialogue");
                Assert.AreEqual("E1_L1_N1", c.introStartNodeId);
                Assert.AreEqual("E1_L1_N3", c.introEndAfterNodeId);
                Assert.AreEqual("E1_L1_N4", c.payoffStartNodeId);
                Assert.AreEqual("audio_investigation", c.seedFamily);
                Assert.AreEqual(0, c.seedTier);
                Assert.AreEqual(2, c.seedCount);
                Assert.AreEqual("audio_investigation_t1", c.seedItemId);
                Assert.AreEqual("audio_investigation_t2", c.targetItemId);
                Assert.IsFalse(c.GuidesGeneratorTap, "two seeds = guided merge, not a guided tap");
                Assert.IsTrue(string.IsNullOrEmpty(c.prePlayedPackageId));
            }
            finally { Object.DestroyImmediate(c); }
        }

        [Test]
        public void SeedCountZero_MeansGuidedGeneratorTap()
        {
            var c = ScriptableObject.CreateInstance<FtueChoreographyConfig>();
            try
            {
                c.seedCount = 0;
                Assert.IsTrue(c.GuidesGeneratorTap);
                c.seedCount = 2;
                Assert.IsFalse(c.GuidesGeneratorTap);
            }
            finally { Object.DestroyImmediate(c); }
        }

        [Test]
        public void EpisodeEntry_WithoutConfig_ResolvesToNull_SoTheChoreographyUsesDefaults()
        {
            var entry = new EpisodeEntry { episodeId = "ep01" };
            Assert.IsNull(entry.ftue);
            Assert.IsNull(entry.packages);
        }

        [Test]
        public void BootOverride_Resolve_FallsBackWhenCatalogIsNull()
        {
            // With no catalog there is nothing to validate the pref against, so the
            // save's pointer must win regardless of editor state.
            Assert.AreEqual("saved", EpisodeBootOverride.Resolve(null, "saved"));
        }
    }
}
