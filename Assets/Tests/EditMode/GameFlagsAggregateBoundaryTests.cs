using System.Collections.Generic;
using AQ.App;
using AQ.App.Episodes;
using AQ.App.Leads;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// GameFlags folded into the save aggregate (schema 1.0.0, multi-episode
    /// phase 2 M4/M5). The pre-fold split was rule 1's exact bug class: a lead
    /// activation wrote its state into the aggregate but its NarrativeFlags into
    /// PlayerPrefs — two stores, one transaction. Coverage per rule 2: imported
    /// mode mutates memory only (no stray prefs write), export/import
    /// round-trips, StateHash changes-and-returns, the probe migration finds
    /// legacy values for content-declared names AND the frozen system list,
    /// legacy keys are deleted only when the caller says the aggregate landed,
    /// null-import resets, QA reset empties without falling back to prefs, and
    /// a cleared flag cannot resurrect from a lingering legacy key.
    /// (Passthrough mode — no import — is pinned by GameFlagsUnificationTests.)
    /// </summary>
    public class GameFlagsAggregateBoundaryTests
    {
        private const string ContentFlag = "aq.test.fold.choice";
        private const string SeenLeadId  = "aq_test_fold_lead";
        private const string GraphFlag   = "aq.test.fold.gs_spoke";
        private const string SystemFlag  = "aq.char.arthur.active"; // on the frozen probe list

        private readonly List<Object> _made = new List<Object>();

        private static readonly string[] AllTestNames =
        {
            ContentFlag, GraphFlag, SystemFlag,
            "aq.lead." + SeenLeadId + ".seen",
            "aq.test.fold.transient",
        };

        // SystemFlag is a REAL QA toggle that may be set on a dev machine —
        // save and restore it so running the suite never clears live QA state.
        private int _priorSystemFlagValue;

        [SetUp]
        public void SetUp()
        {
            _priorSystemFlagValue = PlayerPrefs.GetInt("flag_" + SystemFlag, 0);
            DeleteAllPrefsVariants();
            GameFlags.ResetForTests();
            EpisodeRuntime.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteAllPrefsVariants();
            if (_priorSystemFlagValue != 0)
                PlayerPrefs.SetInt("flag_" + SystemFlag, _priorSystemFlagValue);
            PlayerPrefs.Save();
            GameFlags.ResetForTests();
            EpisodeRuntime.ResetForTests();
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
        }

        private static void DeleteAllPrefsVariants()
        {
            foreach (var name in AllTestNames)
            {
                PlayerPrefs.DeleteKey("flag_" + name);
                PlayerPrefs.DeleteKey("nar_flag_" + name);
                PlayerPrefs.DeleteKey("dlg_flag_" + name);
            }
        }

        /// <summary>A catalog whose ep01 database declares the content flags above.</summary>
        private void InstallContentCatalog()
        {
            var graph = ScriptableObject.CreateInstance<CaseGraph>();
            graph.nodes = new[] { new CaseGraph.Node { id = "N1", setsFlag = GraphFlag } };
            _made.Add(graph);

            var lead = ScriptableObject.CreateInstance<LeadData>();
            lead.leadId = SeenLeadId;
            lead.NarrativeFlags = new[] { ContentFlag };
            lead.resolutionDialogue = graph;
            _made.Add(lead);

            var db = ScriptableObject.CreateInstance<LeadsDatabase>();
            db.Add(lead);
            _made.Add(db);

            var catalog = ScriptableObject.CreateInstance<EpisodeCatalog>();
            catalog.Add(new EpisodeEntry { episodeId = "ep01", database = db });
            _made.Add(catalog);

            EpisodeRuntime.OverrideForTests(catalog, null);
        }

        // ---- Imported mode mutates memory only ----

        [Test]
        public void ImportedMode_Set_WritesNoPlayerPrefsKey()
        {
            GameFlags.ImportState(new List<string>());
            GameFlags.Set("aq.test.fold.transient");

            Assert.IsTrue(GameFlags.Has("aq.test.fold.transient"));
            Assert.AreEqual(0, PlayerPrefs.GetInt("flag_aq.test.fold.transient", 0),
                "the fold's whole point: a flag write must land in the aggregate snapshot, never in a second store");
        }

        [Test]
        public void ExportImport_RoundTrips()
        {
            GameFlags.ImportState(new List<string>());
            GameFlags.Set(ContentFlag);
            GameFlags.Set(GraphFlag);

            var exported = GameFlags.ExportState();
            GameFlags.ImportState(exported);

            Assert.IsTrue(GameFlags.Has(ContentFlag));
            Assert.IsTrue(GameFlags.Has(GraphFlag));
            Assert.AreEqual(2, GameFlags.ExportState().Count);
        }

        [Test]
        public void StateHash_Changes_AndReturns()
        {
            GameFlags.ImportState(new List<string>());
            int empty = GameFlags.StateHash();

            GameFlags.Set(ContentFlag);
            int withFlag = GameFlags.StateHash();
            Assert.AreNotEqual(empty, withFlag, "a flag write must move the hash or the debounced save never fires");

            GameFlags.Clear(ContentFlag);
            Assert.AreEqual(empty, GameFlags.StateHash());
        }

        // ---- Migration (the null-import probe) ----

        [Test]
        public void NullImport_ProbesContentDeclaredNames_FromAllThreeLegacyPrefixes()
        {
            InstallContentCatalog();
            PlayerPrefs.SetInt("flag_" + ContentFlag, 1);                       // unified key
            PlayerPrefs.SetInt("nar_flag_aq.lead." + SeenLeadId + ".seen", 1);  // pre-unification store A
            PlayerPrefs.SetInt("dlg_flag_" + GraphFlag, 1);                     // pre-unification store B

            GameFlags.ImportState(null);

            Assert.IsTrue(GameFlags.Has(ContentFlag), "lead NarrativeFlags are content-declared and must be probed");
            Assert.IsTrue(GameFlags.Has("aq.lead." + SeenLeadId + ".seen"), "the .seen convention is derived from every lead id");
            Assert.IsTrue(GameFlags.Has(GraphFlag), "dialogue setsFlag names come from walking the lead's CaseGraph");
        }

        [Test]
        public void NullImport_ProbesTheFrozenSystemList()
        {
            EpisodeRuntime.OverrideForTests(null, null); // no catalog at all
            PlayerPrefs.SetInt("flag_" + SystemFlag, 1);

            GameFlags.ImportState(null);

            Assert.IsTrue(GameFlags.Has(SystemFlag));
        }

        [Test]
        public void NullImport_WithNothingInPrefs_YieldsAnEmptyImportedStore()
        {
            InstallContentCatalog();
            GameFlags.ImportState(new List<string> { ContentFlag });
            Assert.IsTrue(GameFlags.Has(ContentFlag));

            GameFlags.ImportState(null); // fresh-boot path must RESET, not merge
            Assert.IsFalse(GameFlags.Has(ContentFlag),
                "no prefs key for it and the prior in-memory value must not survive a null import");
        }

        [Test]
        public void LegacyKeys_SurviveUntilTheAggregateWriteIsConfirmed()
        {
            InstallContentCatalog();
            PlayerPrefs.SetInt("flag_" + ContentFlag, 1);

            GameFlags.ImportState(null);
            Assert.AreEqual(1, PlayerPrefs.GetInt("flag_" + ContentFlag, 0),
                "probe must not delete: a crash before the first save must be able to re-probe the same values");

            GameFlags.DeleteLegacyKeys(); // BoardSaveSystem calls this only AFTER a successful write
            Assert.AreEqual(0, PlayerPrefs.GetInt("flag_" + ContentFlag, 0));
            Assert.IsTrue(GameFlags.Has(ContentFlag), "deleting the legacy key must not touch the migrated value");
        }

        // ---- Reset and resurrection ----

        [Test]
        public void ResetForNewSave_Empties_ButStaysMemoryBacked()
        {
            GameFlags.ImportState(new List<string> { ContentFlag });
            PlayerPrefs.SetInt("flag_" + ContentFlag, 1); // a stale legacy key lying around

            GameFlags.ResetForNewSave();

            Assert.IsFalse(GameFlags.Has(ContentFlag),
                "after QA reset the store must be empty AND must not fall back to reading prefs");
        }

        [Test]
        public void ClearedFlag_CannotResurrect_FromALingeringLegacyKey()
        {
            InstallContentCatalog();
            PlayerPrefs.SetInt("nar_flag_" + ContentFlag, 1);
            GameFlags.ImportState(null);
            Assert.IsTrue(GameFlags.Has(ContentFlag));

            GameFlags.Clear(ContentFlag);
            GameFlags.ImportState(null); // a future boot probing again

            Assert.IsFalse(GameFlags.Has(ContentFlag),
                "Clear must delete the prefs variants too, or the next probe resurrects the flag");
        }
    }
}
