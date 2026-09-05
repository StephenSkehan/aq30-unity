using System;
using System.Collections.Generic;
using UnityEngine;
using AQ.App.Leads;

namespace AQ.App.Episodes
{
    /// <summary>
    /// One episode slot in the season. Slot ids are story-neutral (ep01..ep04,
    /// Stephen-ruled 2026-08-27 R5) so a story re-rule never touches saves,
    /// analytics, or code; titles are display data. A slot with no database is
    /// reserved (announced in the selector, not playable).
    /// </summary>
    [Serializable]
    public sealed class EpisodeEntry
    {
        [Tooltip("Slot id (ep01..ep04). Recorded in saves and analytics; never rename.")]
        public string episodeId;

        [Tooltip("Old ids this slot answers to (e.g. e1_the_listener, Ep01). Saves and content written against an alias resolve to this slot.")]
        public string[] legacyIdAliases = Array.Empty<string>();

        [Tooltip("Player-facing title. Display data only; free to change.")]
        public string title;

        public LeadsDatabase database;

        [Tooltip("Ordered step keys for the episode's golden path.")]
        public string[] steps = Array.Empty<string>();

        [Tooltip("Narrative flag whose arrival marks this episode complete (authored on the closing lead).")]
        public string completionFlag;

        [Tooltip("The episode's entry lead. Systems that wait for 'the player is past the first lead' key off its .seen flag.")]
        public string entryLeadId;

        [Tooltip("Flag that unlocks Mo's Back Room during this episode.")]
        public string shopUnlockFlag;

        [Tooltip("Flag that unseals episode-gated dossier facts.")]
        public string dossierGateFlag;

        [Tooltip("Closing summary shown on the resolution screen.")]
        [TextArea(2, 4)] public string closingSummary;

        [Tooltip("Content flag namespaces owned by this episode (e.g. 'e1.', 'aq.lead.e1_'). Used for per-episode flag reset.")]
        public string[] flagPrefixes = Array.Empty<string>();

        [Tooltip("Lead packages for this episode (Four Keys onward). Null = the episode has no packages; cards resolve with their own dialogue (The Listener).")]
        public AQ.App.Leads.Packages.PackageCatalog packages;

        [Tooltip("First-card FTUE choreography for this episode. Null = the built-in Listener choreography (e1_tip, seeded Audio T1 pair).")]
        public AQ.App.FTUE.FtueChoreographyConfig ftue;

        /// <summary>A reserved slot has no database and cannot be played yet.</summary>
        public bool HasContent => database != null;
    }

    /// <summary>
    /// THE registry of episodes. Replaces episode identity serialized in the
    /// scene (CaseFlowOrchestratorMB.episodeId/steps) and the Episode-1 literals
    /// that were hardwired into cross-cutting systems (resolution trigger, shop
    /// unlock, dossier gate, resolution-screen title). One asset, at
    /// Resources/App/Episodes/EpisodeCatalog.
    /// </summary>
    [CreateAssetMenu(fileName = "EpisodeCatalog", menuName = "AQ/Episodes/Catalog", order = 12)]
    public sealed class EpisodeCatalog : ScriptableObject
    {
        [SerializeField] private List<EpisodeEntry> episodes = new List<EpisodeEntry>();

        public IReadOnlyList<EpisodeEntry> Episodes => episodes;

        public EpisodeEntry First => episodes.Count > 0 ? episodes[0] : null;

        /// <summary>Test/editor seam (mirrors LeadsDatabase.Add).</summary>
        public void Add(EpisodeEntry entry)
        {
            if (entry == null || episodes.Contains(entry)) return;
            episodes.Add(entry);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>Resolve a slot id OR a legacy alias to its entry. Null when unknown.</summary>
        public EpisodeEntry FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < episodes.Count; i++)
            {
                var e = episodes[i];
                if (e == null) continue;
                if (e.episodeId == id) return e;
                if (e.legacyIdAliases != null)
                    for (int a = 0; a < e.legacyIdAliases.Length; a++)
                        if (e.legacyIdAliases[a] == id) return e;
            }
            return null;
        }

        /// <summary>
        /// Map any id (slot or alias) to its slot id. Unknown ids pass through
        /// unchanged: a save recording an id this catalog has never heard of must
        /// keep it rather than silently re-homing the player's progress.
        /// </summary>
        public string CanonicalId(string id)
        {
            var entry = FindById(id);
            return entry != null ? entry.episodeId : id;
        }

        public int IndexOf(string id)
        {
            var entry = FindById(id);
            return entry != null ? episodes.IndexOf(entry) : -1;
        }

        /// <summary>The entry after the given episode in season order. Null at the end or for unknown ids.</summary>
        public EpisodeEntry Next(string id)
        {
            int i = IndexOf(id);
            return i >= 0 && i + 1 < episodes.Count ? episodes[i + 1] : null;
        }
    }
}
