using NUnit.Framework;
using UnityEngine;
using AQ.App;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// The two-store trap is dead: NarrativeFlags and DialogueFlags forward to
    /// GameFlags, so a flag set through either API is visible through both, and
    /// legacy nar_flag_*/dlg_flag_* keys migrate lazily on first read.
    /// </summary>
    public sealed class GameFlagsUnificationTests
    {
        private const string TestFlag = "aq.test.unification_probe";

        [SetUp]
        public void SetUp() => DeleteAllVariants();

        [TearDown]
        public void TearDown() => DeleteAllVariants();

        private static void DeleteAllVariants()
        {
            PlayerPrefs.DeleteKey("flag_" + TestFlag);
            PlayerPrefs.DeleteKey("nar_flag_" + TestFlag);
            PlayerPrefs.DeleteKey("dlg_flag_" + TestFlag);
        }

        [Test]
        public void SetThroughEitherLegacyApi_VisibleThroughBoth()
        {
            NarrativeFlags.Set(TestFlag);
            Assert.IsTrue(DialogueFlags.Has(TestFlag),
                "a flag set via NarrativeFlags must be visible via DialogueFlags — this exact mismatch shipped the silenced-hints bug");
            Assert.IsTrue(GameFlags.Has(TestFlag));

            DeleteAllVariants();

            DialogueFlags.Set(TestFlag);
            Assert.IsTrue(NarrativeFlags.Has(TestFlag));
        }

        [Test]
        public void LegacyNarKey_MigratesOnRead()
        {
            PlayerPrefs.SetInt("nar_flag_" + TestFlag, 1); // key written by a pre-unification build
            Assert.IsTrue(GameFlags.Has(TestFlag));
            Assert.AreEqual(1, PlayerPrefs.GetInt("flag_" + TestFlag, 0), "read must copy the legacy value to the unified key");
        }

        [Test]
        public void LegacyDlgKey_MigratesOnRead()
        {
            PlayerPrefs.SetInt("dlg_flag_" + TestFlag, 1);
            Assert.IsTrue(GameFlags.Has(TestFlag));
            Assert.AreEqual(1, PlayerPrefs.GetInt("flag_" + TestFlag, 0));
        }

        [Test]
        public void Clear_RemovesUnifiedAndBothLegacyKeys()
        {
            GameFlags.Set(TestFlag);
            PlayerPrefs.SetInt("nar_flag_" + TestFlag, 1);
            PlayerPrefs.SetInt("dlg_flag_" + TestFlag, 1);

            GameFlags.Clear(TestFlag);
            Assert.IsFalse(GameFlags.Has(TestFlag),
                "a cleared flag must not resurrect from a lingering legacy key");
        }

        [Test]
        public void UnsetFlag_IsFalse_AndNullOrEmptySafe()
        {
            Assert.IsFalse(GameFlags.Has(TestFlag));
            Assert.IsFalse(GameFlags.Has(null));
            Assert.IsFalse(GameFlags.Has(string.Empty));
        }
    }
}
