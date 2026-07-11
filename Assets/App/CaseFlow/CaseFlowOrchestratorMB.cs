// Assembly: AQ.App
// Purpose: Scene binder that owns/starts CaseFlow and advances on FTUE grant,
//          with an idempotent "catch-up" if the grant happened before we subscribed.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;
using AQ.SharedKernel.Economy;
using AQ.App.Economy;
using AQ.App.Analytics;

namespace AQ.App.CaseFlow
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-2)]
    public sealed class CaseFlowOrchestratorMB : MonoBehaviour
    {
        [Header("Episode config")]
        public string episodeId = "Ep01";
        [Tooltip("Ordered step keys for the episode's 'golden path'.")]
        public string[] steps = new[] { "FTUE_Entitlements", "Minigame_Scrub", "Resolution" };

        [Header("Lifecycle")]
        public bool beginOnStart = true;

        [Header("Auto-advance triggers")]
        [Tooltip("Advance one step when FTUE entitlements are granted (reason='ftue').")]
        public bool advanceOnFtueGrant = true;

        [Tooltip("If FTUE already happened before we subscribed, advance once at Start.")]
        public bool catchUpOnFtueFlag = true;

        [Tooltip("Must match FTUEEntitlements.playerPrefsKey so we can detect prior grant.")]
        public string ftuePlayerPrefsKey = "aq.ftue.entitlements.v1";

        private ICaseFlowService _svc;
        private IWallet _wallet;

        void Awake()
        {
            // Ensure caseflow service exists very early
            _svc = CaseFlowLocator.Instance ?? new InMemoryCaseFlowService();
            CaseFlowLocator.Set(_svc);
        }

        void OnEnable()
        {
            // Early attempt to attach if wallet already exists
            TryAttachWallet();
        }

        void Start()
        {
            if (beginOnStart)
            {
                _svc.Begin(new EpisodeId(episodeId), steps);
                Debug.Log($"[CaseFlow] Began episode '{episodeId}' with {steps.Length} step(s).");
            }

            // Attach again in case wallet was created during other components' OnEnable/Awake
            TryAttachWallet();

            // Catch-up: if FTUE already granted (flag set) but we didn't see the event,
            // advance exactly one step iff we're at step 0 and step key matches.
            if (advanceOnFtueGrant && catchUpOnFtueFlag)
            {
                if (PlayerPrefs.GetInt(ftuePlayerPrefsKey, 0) == 1 &&
                    _svc.Current.StepIndex == 0 &&
                    _svc.Current.Steps.Count > 0 &&
                    _svc.Current.Steps[0] == "FTUE_Entitlements")
                {
                    if (_svc.CompleteCurrentStep())
                    {
                        Debug.Log("[CaseFlow] Catch-up advance on existing FTUE flag → stepIndex=1");
                        var cur = _svc.Current;
                        string key = cur.StepIndex < cur.Steps.Count ? cur.Steps[cur.StepIndex] : "end";
                        GameAnalytics.LogFtueStep(key, cur.StepIndex);
                    }
                }
            }
        }

        void OnDisable()
        {
            if (_wallet != null) _wallet.Granted -= OnGranted;
        }

        private void TryAttachWallet()
        {
            if (!advanceOnFtueGrant) return;

            var current = WalletLocator.Instance;
            if (current == null || current == _wallet) return;

            if (_wallet != null) _wallet.Granted -= OnGranted; // safety reattach
            _wallet = current;
            _wallet.Granted += OnGranted;
        }

        private void OnGranted(RewardsGranted e)
        {
            if (!string.Equals(e.Reason, "ftue", System.StringComparison.OrdinalIgnoreCase)) return;

            var stepped = _svc.CompleteCurrentStep();
            if (stepped)
            {
                Debug.Log($"[CaseFlow] Auto-advanced on FTUE grant → stepIndex={_svc.Current.StepIndex}/{_svc.Current.Steps.Count}");
                var cur = _svc.Current;
                string key = cur.StepIndex < cur.Steps.Count ? cur.Steps[cur.StepIndex] : "end";
                GameAnalytics.LogFtueStep(key, cur.StepIndex);
            }
        }

        // Optional UI hooks:
        public void Advance() { if (_svc.CompleteCurrentStep()) Debug.Log($"[CaseFlow] Advance → { _svc.Current.StepIndex }"); }
        public void ResetProgress() { _svc.Reset(); Debug.Log("[CaseFlow] Reset progress to start"); }
        public void BeginEpisode() { _svc.Begin(new EpisodeId(episodeId), steps); Debug.Log($"[CaseFlow] Begin '{episodeId}'"); }
    }
}
