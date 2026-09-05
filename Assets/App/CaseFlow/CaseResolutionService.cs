using AQ.App.Analytics;
using AQ.App.Episodes;
using AQ.App.Events;
using AQ.App.Leads;
using AQ.App.Presentation;
using UnityEngine;

namespace AQ.App.CaseFlow
{
    /// Watches for the episode completion flag in activated lead flags; publishes CaseResolvedEvent once.
    public sealed class CaseResolutionService : MonoBehaviour
    {
        // Fallback for catalog-less scenes only; the running episode's
        // completionFlag (EpisodeCatalog) is the real trigger.
        const string FallbackCompletionFlag = "e1.ep01.complete";

        // Reported when no caseflow service is running at resolution time. A literal
        // episode name here would go stale the moment an episode is renamed or added
        // (a hardcoded "e1_the_listener" kept reporting a retired episode after the
        // save was already recording the orchestrator's id) — so the id comes from
        // the running caseflow service, and this sentinel marks the pathological case.
        public const string UnknownEpisodeId = "unknown";

        /// <summary>The episode id of the running caseflow, never a literal.</summary>
        public static string ResolveEpisodeId()
        {
            var svc = CaseFlowLocator.Instance;
            var id  = svc != null ? svc.Current?.Episode.Value : null;
            return string.IsNullOrEmpty(id) ? UnknownEpisodeId : id;
        }

        bool _fired;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInstall()
        {
            if (FindObjectOfType<CaseResolutionService>() != null) return;
            var go = new GameObject("[CaseResolutionService]");
            DontDestroyOnLoad(go);
            go.AddComponent<CaseResolutionService>();
        }

        void OnEnable()  { LeadsRuntimeBus.OnLeadActivated += OnLeadActivated; }
        void OnDisable() { LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated; }

        // Public so EditMode tests can drive it directly: Unity does not run
        // OnEnable on a plain MonoBehaviour outside play mode, so the bus
        // subscription above never exists under the edit-mode runner.
        public void OnLeadActivated(LeadData lead)
        {
            if (_fired || lead == null) return;

            // Check the LeadData.NarrativeFlags array directly — LeadOutcomeMB.ApplyNarrativeFlags
            // and this handler both subscribe to OnLeadActivated with undefined ordering,
            // so NarrativeFlags.Has() may not be set yet.
            var flags = lead.NarrativeFlags;
            if (flags == null) return;
            var completionFlag = EpisodeRuntime.CompletionFlagOr(FallbackCompletionFlag);
            foreach (var f in flags)
            {
                if (f == completionFlag)
                {
                    _fired = true;
                    var episodeId = ResolveEpisodeId();
                    GameAnalytics.LogEpisodeComplete(episodeId);
                    // The closing card's payoff (its resolution dialogue, or the
                    // package beat) opens in this same activation. The Episode
                    // Closed screen must follow the last line's dismissing tap,
                    // never cover it (Stephen-ruled 2026-09-03). Edit-mode tests
                    // drive this handler directly with no runner and no
                    // coroutines: publish synchronously there.
                    if (!Application.isPlaying)
                        GlobalBus.Bus.Publish(new CaseResolvedEvent(episodeId));
                    else
                        StartCoroutine(PublishWhenDialogueClosed(episodeId));
                    return;
                }
            }
        }

        System.Collections.IEnumerator PublishWhenDialogueClosed(string episodeId)
        {
            // One frame so a dialogue booted by this activation is active.
            yield return null;
            var runner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            while (runner != null && runner.gameObject.activeInHierarchy)
                yield return null;
            GlobalBus.Bus.Publish(new CaseResolvedEvent(episodeId));
        }
    }
}
