using AQ.App.Events;
using AQ.App.Leads;
using AQ.App.Presentation;
using UnityEngine;

namespace AQ.App.CaseFlow
{
    /// Watches for gs.ep01.complete in activated lead flags; publishes CaseResolvedEvent once.
    public sealed class CaseResolutionService : MonoBehaviour
    {
        const string EpisodeId    = "gs_ep01";
        const string CompletionFlag = "gs.ep01.complete";

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

        void OnLeadActivated(LeadData lead)
        {
            if (_fired || lead == null) return;

            // Check the LeadData.NarrativeFlags array directly — LeadOutcomeMB.ApplyNarrativeFlags
            // and this handler both subscribe to OnLeadActivated with undefined ordering,
            // so NarrativeFlags.Has() may not be set yet.
            var flags = lead.NarrativeFlags;
            if (flags == null) return;
            foreach (var f in flags)
            {
                if (f == CompletionFlag)
                {
                    _fired = true;
                    GlobalBus.Bus.Publish(new CaseResolvedEvent(EpisodeId));
                    return;
                }
            }
        }
    }
}
