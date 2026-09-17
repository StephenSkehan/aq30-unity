using System;
using System.Collections.Generic;
using AQ.App.Locker;
using AQ.App.Overflow;

namespace AQ.App.Persistence
{
    // The save aggregate's shape (BoardSaveSystem writes/reads it). These live
    // in AQ.App rather than nested in BoardSaveSystem so the schema and its
    // migration are testable: test asmdefs cannot reference Assembly-CSharp.

    [Serializable]
    public sealed class CellDTO
    {
        public int r;
        public int c;
        public string kind;   // "Item" | "Generator" | "Special"
        public int tier;      // 0-based
        public string family; // e.g. "corner_diner" — empty in legacy saves
    }

    [Serializable]
    public sealed class EnergyDTO
    {
        public int current;
        public string lastTickUtc; // ISO-8601 string
    }

    [Serializable]
    public sealed class WalletDTO
    {
        public int soft;
        public int premium;
    }

    [Serializable]
    public sealed class CaseFlowDTO
    {
        public string episodeId;
        public int stepIndex;
    }

    [Serializable]
    public sealed class LeadStateDTO
    {
        public string leadId;
        public int    runtimeState;
        public bool[] satisfied;
        public bool   activated;
    }

    /// <summary>
    /// One episode's slice of the aggregate (schema 1.0.0). Everything §1 of the
    /// multi-episode audit identified as per-episode lives here; everything that
    /// exchanges value with the wallet stays top-level in SaveDTO so a crash can
    /// never separate a transaction's halves across files (robustness rule 1).
    /// </summary>
    [Serializable]
    public sealed class EpisodeSectionDTO
    {
        public string episodeId; // canonical slot id (ep01..ep04)
        public bool   complete;  // the durable "episode done" record

        public int rows;
        public int cols;
        public List<CellDTO>      cells = new List<CellDTO>();
        public CaseFlowDTO        caseFlow;
        public List<LeadStateDTO> leads = new List<LeadStateDTO>();
    }

    [Serializable]
    public sealed class SaveDTO
    {
        public string schemaVersion = SaveSchema.Current;
        public string timestampUtc;

        // 0.x flat per-episode fields. Read for migration only — 1.0.0 writes
        // per-episode state into `episodes` and leaves these at defaults.
        public int rows;
        public int cols;
        public List<CellDTO>      cells = new List<CellDTO>();
        public CaseFlowDTO        caseFlow;
        public List<LeadStateDTO> leads = new List<LeadStateDTO>();

        // Global sections (episode-independent by ruling R3: the kit travels).
        public EnergyDTO              energy;
        public WalletDTO              wallet;
        public LockerStateDTO         locker;   // folded in at 0.7.0
        public List<OverflowTileData> overflow = new List<OverflowTileData>(); // folded in at 0.8.0
        public UI.Specials.SpecialsStateDTO specials; // folded in at 0.9.0
        public List<string>           flags = new List<string>(); // folded in at 1.0.0 — see GameFlags

        // 1.0.0: the episode partition.
        public string currentEpisodeId;
        public List<EpisodeSectionDTO> episodes = new List<EpisodeSectionDTO>();
    }

    /// <summary>
    /// Schema versioning plus the 0.9.0 → 1.0.0 episode-partition migration.
    /// Pure functions over DTOs — no IO, no scene, no statics — so the
    /// crash-boundary suite can drive them directly.
    /// </summary>
    public static class SaveSchema
    {
        public const string Current = "1.0.0";

        public static bool AtLeast(string version, int major, int minor)
        {
            if (string.IsNullOrEmpty(version)) return false;
            var parts = version.Split('.');
            if (parts.Length < 2) return false;
            if (!int.TryParse(parts[0], out int maj) || !int.TryParse(parts[1], out int min)) return false;
            return maj > major || (maj == major && min >= minor);
        }

        public static EpisodeSectionDTO FindSection(SaveDTO dto, string episodeId)
        {
            if (dto?.episodes == null || string.IsNullOrEmpty(episodeId)) return null;
            for (int i = 0; i < dto.episodes.Count; i++)
                if (dto.episodes[i] != null && dto.episodes[i].episodeId == episodeId)
                    return dto.episodes[i];
            return null;
        }

        /// <summary>
        /// Wrap a pre-1.0.0 save's flat per-episode fields into one episode
        /// section, keyed by the id the save itself recorded (in every real
        /// save that is "e1_the_listener", the shipped scene's serialized id —
        /// the caller canonicalizes ids, this function must not guess).
        /// Globals are untouched; nothing is dropped. Idempotent: the version
        /// is stamped to Current so a second call is a no-op. Returns the
        /// created section, or null when no migration was needed.
        /// </summary>
        public static EpisodeSectionDTO MigrateFlatToSection(SaveDTO dto, string fallbackEpisodeId)
        {
            if (dto == null || AtLeast(dto.schemaVersion, 1, 0)) return null;

            string id = dto.caseFlow != null && !string.IsNullOrEmpty(dto.caseFlow.episodeId)
                ? dto.caseFlow.episodeId
                : fallbackEpisodeId;

            var section = new EpisodeSectionDTO
            {
                episodeId = id,
                complete  = false, // no pre-1.0.0 player finished an episode: completion did not exist
                rows      = dto.rows,
                cols      = dto.cols,
                cells     = dto.cells ?? new List<CellDTO>(),
                caseFlow  = dto.caseFlow,
                leads     = dto.leads ?? new List<LeadStateDTO>()
            };

            if (dto.episodes == null) dto.episodes = new List<EpisodeSectionDTO>();
            dto.episodes.Add(section);
            dto.currentEpisodeId = id;
            dto.schemaVersion = Current;
            return section;
        }
    }
}
