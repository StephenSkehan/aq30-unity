using System.Collections.Generic;
using AQ.App.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// Schema 1.0.0's episode partition (multi-episode phase 2, M2/M3). The DTO
    /// shapes and the 0.9.0 migration live in AQ.App.Persistence.SaveModel so
    /// they can be driven here without the scene-bound BoardSaveSystem
    /// (Assembly-CSharp is unreferencable from test assemblies). Coverage per
    /// robustness rule 2: the 0.9.0 flat file wraps into a section keyed by the
    /// id the save recorded with globals untouched and nothing dropped,
    /// migration is idempotent, a 1.0.0 file is left alone, sections round-trip
    /// through JsonUtility intact, and the §1.4 regression — one episode's save
    /// dropping another's section — is pinned at the serialization layer.
    /// </summary>
    public class EpisodeSaveSchemaTests
    {
        // ---- Version parsing ----

        [Test]
        public void AtLeast_ParsesRealVersions()
        {
            Assert.IsTrue(SaveSchema.AtLeast("1.0.0", 0, 9), "1.0.0 must pass every 0.x gate — the locker/stash/specials imports depend on it");
            Assert.IsTrue(SaveSchema.AtLeast("1.0.0", 1, 0));
            Assert.IsTrue(SaveSchema.AtLeast("0.9.0", 0, 7));
            Assert.IsFalse(SaveSchema.AtLeast("0.9.0", 1, 0));
            Assert.IsFalse(SaveSchema.AtLeast(null, 0, 7));
            Assert.IsFalse(SaveSchema.AtLeast("garbage", 0, 7));
        }

        // ---- 0.9.0 → 1.0.0 migration ----

        private static SaveDTO MakeLegacy090()
        {
            return new SaveDTO
            {
                schemaVersion = "0.9.0",
                rows = 7,
                cols = 6,
                cells = new List<CellDTO> { new CellDTO { r = 1, c = 2, kind = "Item", tier = 3, family = "corner_diner" } },
                caseFlow = new CaseFlowDTO { episodeId = "e1_the_listener", stepIndex = 2 },
                leads = new List<LeadStateDTO> { new LeadStateDTO { leadId = "e1_tip", runtimeState = 3 } },
                wallet = new WalletDTO { soft = 410, premium = 12 },
                energy = new EnergyDTO { current = 55, lastTickUtc = "2026-08-27T00:00:00.0000000Z" },
            };
        }

        [Test]
        public void Migration_WrapsFlatFields_KeyedByTheIdTheSaveRecorded()
        {
            var dto = MakeLegacy090();
            var section = SaveSchema.MigrateFlatToSection(dto, "ep01");

            Assert.IsNotNull(section);
            Assert.AreEqual("e1_the_listener", section.episodeId,
                "the save's own recorded id keys the section — canonicalization is the catalog's job at load, not the migration's");
            Assert.AreEqual("e1_the_listener", dto.currentEpisodeId);
            Assert.AreEqual(7, section.rows);
            Assert.AreEqual(6, section.cols);
            Assert.AreEqual(1, section.cells.Count);
            Assert.AreEqual(1, section.leads.Count);
            Assert.AreEqual(2, section.caseFlow.stepIndex);
            Assert.IsFalse(section.complete, "no pre-1.0.0 player finished an episode: completion did not exist");
        }

        [Test]
        public void Migration_LeavesGlobals_Untouched()
        {
            var dto = MakeLegacy090();
            SaveSchema.MigrateFlatToSection(dto, "ep01");

            Assert.AreEqual(410, dto.wallet.soft);
            Assert.AreEqual(12, dto.wallet.premium);
            Assert.AreEqual(55, dto.energy.current);
        }

        [Test]
        public void Migration_WithNoCaseFlow_UsesTheFallbackId()
        {
            var dto = MakeLegacy090();
            dto.caseFlow = null;
            var section = SaveSchema.MigrateFlatToSection(dto, "ep01");

            Assert.AreEqual("ep01", section.episodeId);
            Assert.AreEqual("ep01", dto.currentEpisodeId);
        }

        [Test]
        public void Migration_IsIdempotent()
        {
            var dto = MakeLegacy090();
            Assert.IsNotNull(SaveSchema.MigrateFlatToSection(dto, "ep01"));
            Assert.AreEqual(SaveSchema.Current, dto.schemaVersion, "migration must stamp the version so it cannot run twice");
            Assert.IsNull(SaveSchema.MigrateFlatToSection(dto, "ep01"));
            Assert.AreEqual(1, dto.episodes.Count, "a second migration pass must not duplicate the section");
        }

        [Test]
        public void Migration_LeavesA100File_Alone()
        {
            var dto = new SaveDTO
            {
                schemaVersion = "1.0.0",
                currentEpisodeId = "ep02",
                episodes = new List<EpisodeSectionDTO> { new EpisodeSectionDTO { episodeId = "ep01", complete = true } }
            };

            Assert.IsNull(SaveSchema.MigrateFlatToSection(dto, "ep01"));
            Assert.AreEqual("ep02", dto.currentEpisodeId);
            Assert.AreEqual(1, dto.episodes.Count);
        }

        // ---- Sections ----

        [Test]
        public void FindSection_ByExactId_NullSafe()
        {
            var dto = new SaveDTO();
            dto.episodes.Add(new EpisodeSectionDTO { episodeId = "ep01" });
            dto.episodes.Add(null);
            dto.episodes.Add(new EpisodeSectionDTO { episodeId = "ep02" });

            Assert.AreEqual("ep02", SaveSchema.FindSection(dto, "ep02").episodeId);
            Assert.IsNull(SaveSchema.FindSection(dto, "ep99"));
            Assert.IsNull(SaveSchema.FindSection(dto, null));
            Assert.IsNull(SaveSchema.FindSection(null, "ep01"));
        }

        // ---- Round-trip through JsonUtility (the real serializer) ----

        [Test]
        public void MultiEpisodeSave_RoundTrips_WithNothingDropped()
        {
            var dto = new SaveDTO { timestampUtc = "2026-08-27T00:00:00.0000000Z", currentEpisodeId = "ep02" };
            dto.wallet = new WalletDTO { soft = 999, premium = 3 };
            dto.episodes.Add(new EpisodeSectionDTO
            {
                episodeId = "ep01",
                complete  = true,
                rows = 7, cols = 6,
                cells = new List<CellDTO> { new CellDTO { r = 0, c = 0, kind = "Generator", tier = 4, family = "field_kit" } },
                caseFlow = new CaseFlowDTO { episodeId = "ep01", stepIndex = 4 },
                leads = new List<LeadStateDTO> { new LeadStateDTO { leadId = "e1_close", activated = true } }
            });
            dto.episodes.Add(new EpisodeSectionDTO
            {
                episodeId = "ep02",
                rows = 7, cols = 6,
                cells = new List<CellDTO> { new CellDTO { r = 2, c = 3, kind = "Item", tier = 1, family = "bar" } },
                caseFlow = new CaseFlowDTO { episodeId = "ep02", stepIndex = 1 },
            });

            var back = JsonUtility.FromJson<SaveDTO>(JsonUtility.ToJson(dto));

            // This is the §1.4 regression pinned at the serializer: saving while
            // playing ep02 must never drop ep01's section.
            Assert.AreEqual(2, back.episodes.Count);
            Assert.AreEqual("ep02", back.currentEpisodeId);

            var ep01 = SaveSchema.FindSection(back, "ep01");
            Assert.IsTrue(ep01.complete, "the durable completion record must survive the round-trip");
            Assert.AreEqual(1, ep01.cells.Count);
            Assert.AreEqual("field_kit", ep01.cells[0].family);
            Assert.AreEqual(4, ep01.caseFlow.stepIndex);
            Assert.AreEqual(1, ep01.leads.Count);
            Assert.IsTrue(ep01.leads[0].activated);

            var ep02 = SaveSchema.FindSection(back, "ep02");
            Assert.IsFalse(ep02.complete);
            Assert.AreEqual(1, ep02.cells.Count);
            Assert.AreEqual(999, back.wallet.soft);
        }

        [Test]
        public void LegacyJson_ParsesIntoTheNewDTO_ThenMigrates()
        {
            // A 0.9.0 file as BoardSaveSystem actually wrote it (flat fields).
            const string legacyJson =
                "{\"schemaVersion\":\"0.9.0\",\"timestampUtc\":\"2026-08-20T10:00:00.0000000Z\"," +
                "\"rows\":7,\"cols\":6," +
                "\"cells\":[{\"r\":3,\"c\":1,\"kind\":\"Item\",\"tier\":2,\"family\":\"forensic\"}]," +
                "\"energy\":{\"current\":40,\"lastTickUtc\":\"2026-08-20T09:59:00.0000000Z\"}," +
                "\"wallet\":{\"soft\":120,\"premium\":5}," +
                "\"caseFlow\":{\"episodeId\":\"e1_the_listener\",\"stepIndex\":3}," +
                "\"leads\":[{\"leadId\":\"e1_tip\",\"runtimeState\":3,\"satisfied\":[true],\"activated\":false}]}";

            var dto = JsonUtility.FromJson<SaveDTO>(legacyJson);
            Assert.IsNotNull(dto);
            Assert.AreEqual("0.9.0", dto.schemaVersion);

            var section = SaveSchema.MigrateFlatToSection(dto, "ep01");
            Assert.AreEqual("e1_the_listener", section.episodeId);
            Assert.AreEqual(1, section.cells.Count);
            Assert.AreEqual("forensic", section.cells[0].family);
            Assert.AreEqual(3, section.caseFlow.stepIndex);
            Assert.AreEqual(1, section.leads.Count);
            Assert.AreEqual(120, dto.wallet.soft, "globals must pass through the parse+migrate path untouched");
        }
    }
}
