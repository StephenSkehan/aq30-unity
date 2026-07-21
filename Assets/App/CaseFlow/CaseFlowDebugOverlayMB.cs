// Assembly: AQ.App
// Purpose: Builds the one-line CaseFlow + leads status string for the
//          Settings > Debug tab.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    /// <summary>
    /// Status-line provider for Settings > Debug (2026-07-21 ruling: the line
    /// lives INSIDE the settings panel — the old always-on-screen OnGUI overlay
    /// and its DEBUG INFO toggle are gone). The Main Merge scene still carries
    /// this component from the overlay era; it is inert now and removing it
    /// would need scene surgery for no gain.
    /// </summary>
    public sealed class CaseFlowDebugOverlayMB : MonoBehaviour
    {
        public static string BuildStatusLine()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var svc = CaseFlowLocator.Instance;
            if (svc == null) return "CaseFlow service not running.";

            var s = svc.Current;
            var activeKey = (s.StepIndex < s.Steps.Count) ? s.Steps[s.StepIndex] : "<complete>";

            var bar = FindAnyObjectByType<Leads.LeadsBarView>();
            string leads = bar != null ? $" · Leads {bar.ActivatedCount}/12" : string.Empty;

            return $"Ep={s.Episode} Step={s.StepIndex}/{s.Steps.Count} {activeKey}{leads}";
#else
            return string.Empty;
#endif
        }
    }
}
