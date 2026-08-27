using UnityEngine;

namespace AQ.App.Episodes
{
    /// <summary>
    /// Runtime access to the episode catalog and the entry the caseflow is
    /// currently running. CaseFlowOrchestratorMB sets Current when it begins an
    /// episode; systems that were hardwired to Episode-1 literals read their
    /// per-episode values from here, keeping their old literal as the fallback
    /// for scenes with no catalog (dev/demo scenes, tests).
    /// </summary>
    public static class EpisodeRuntime
    {
        private const string ResourcePath = "App/Episodes/EpisodeCatalog";

        private static EpisodeCatalog _catalog;
        private static bool _loadAttempted;

        public static EpisodeCatalog Catalog
        {
            get
            {
                if (_catalog == null && !_loadAttempted)
                {
                    _loadAttempted = true;
                    _catalog = Resources.Load<EpisodeCatalog>(ResourcePath);
                }
                return _catalog;
            }
        }

        /// <summary>The entry the caseflow began. Null before Begin or in catalog-less scenes.</summary>
        public static EpisodeEntry Current { get; private set; }

        public static void SetCurrent(EpisodeEntry entry) => Current = entry;

        /// <summary>Test seam: inject a catalog and bypass the Resources load.</summary>
        public static void OverrideForTests(EpisodeCatalog catalog, EpisodeEntry current)
        {
            _catalog = catalog;
            _loadAttempted = true;
            Current = current;
        }

        /// <summary>Test seam: back to the unloaded state (Resources load re-enabled).</summary>
        public static void ResetForTests() => ResetStatics();

        // Statics survive play sessions when domain reload is off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _catalog = null;
            _loadAttempted = false;
            Current = null;
        }

        // ---- Per-episode values with owner-supplied fallbacks ----

        public static string ShopUnlockFlagOr(string fallback) =>
            NonEmptyOr(Current?.shopUnlockFlag, fallback);

        public static string DossierGateFlagOr(string fallback) =>
            NonEmptyOr(Current?.dossierGateFlag, fallback);

        public static string CompletionFlagOr(string fallback) =>
            NonEmptyOr(Current?.completionFlag, fallback);

        public static string TitleOr(string fallback) =>
            NonEmptyOr(Current?.title, fallback);

        public static string ClosingSummaryOr(string fallback) =>
            NonEmptyOr(Current?.closingSummary, fallback);

        /// <summary>The .seen flag of the running episode's entry lead ("player is past the first lead").</summary>
        public static string EntryLeadSeenFlagOr(string fallback) =>
            string.IsNullOrEmpty(Current?.entryLeadId) ? fallback : "aq.lead." + Current.entryLeadId + ".seen";

        private static string NonEmptyOr(string value, string fallback) =>
            string.IsNullOrEmpty(value) ? fallback : value;
    }

    /// <summary>
    /// Boot handoff from the save system to the orchestrator. BoardSaveSystem
    /// (Assembly-CSharp) pre-reads the save's currentEpisodeId in Awake and
    /// parks it here; CaseFlowOrchestratorMB (AQ.App, which cannot reference
    /// Assembly-CSharp) reads it in Start. Null means "no save / no opinion",
    /// and the orchestrator falls back to the catalog's first episode.
    /// </summary>
    public static class EpisodeBootPointer
    {
        public static string PendingEpisodeId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => PendingEpisodeId = null;
    }
}
