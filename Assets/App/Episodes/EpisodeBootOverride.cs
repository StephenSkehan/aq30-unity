// Assembly: AQ.App
// File: Assets/App/Episodes/EpisodeBootOverride.cs
// Purpose: Editor-only override of the episode the scene boots into. Replaces
//          the Four Keys slice's database swap: instead of patching the running
//          Listener, the dev picks a catalog entry (AQ > Dev Boot Episode) and
//          the normal boot path plays it: BoardSaveSystem parks the id as the
//          boot pointer, the orchestrator begins that entry, the save keeps a
//          per-episode section for it, and every episode-keyed system (leads
//          database, package catalog, FTUE config, flags) follows the entry.
//
//          EditorPrefs, not PlayerPrefs: QA reset wipes PlayerPrefs, and a dev
//          toggle is editor state, not player state. Builds ignore it.

namespace AQ.App.Episodes
{
    public static class EpisodeBootOverride
    {
        public const string PrefKey = "aq.dev.boot_episode";

        /// <summary>The raw override id, or null when unset (or in a build).</summary>
        public static string EpisodeId
        {
            get
            {
#if UNITY_EDITOR
                var v = UnityEditor.EditorPrefs.GetString(PrefKey, "");
                return string.IsNullOrEmpty(v) ? null : v;
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// The override only applies when the catalog knows the id and the entry
        /// is playable; anything else falls through to <paramref name="fallback"/>
        /// (normally the save's pointer), so a stale pref can never strand the boot.
        /// </summary>
        public static string Resolve(EpisodeCatalog catalog, string fallback)
        {
            var id = EpisodeId;
            if (string.IsNullOrEmpty(id) || catalog == null) return fallback;
            var entry = catalog.FindById(id);
            return entry != null && entry.HasContent ? entry.episodeId : fallback;
        }
    }
}
