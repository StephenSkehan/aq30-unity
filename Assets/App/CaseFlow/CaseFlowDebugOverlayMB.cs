// Assembly: AQ.App
// Purpose: Minimal on-screen display of CaseFlow + leads state for dev builds.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    /// <summary>
    /// One combined debug status line (CaseFlow step + leads progress), top of
    /// screen. Hidden unless Settings > Debug > Debug Info is ON
    /// (2026-07-18 ruling: single message, toggleable, dev builds only).
    /// </summary>
    public sealed class CaseFlowDebugOverlayMB : MonoBehaviour
    {
        public bool show = true;
        public int fontSize = 14;
        private GUIStyle _style;
        private Leads.LeadsBarView _bar;

        void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!show || !Dev.DebugInfoToggle.Show) return;
            var svc = CaseFlowLocator.Instance;
            if (svc == null) return;

            _style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize };
            var s = svc.Current;
            var activeKey = (s.StepIndex < s.Steps.Count) ? s.Steps[s.StepIndex] : "<complete>";

            if (_bar == null) _bar = FindAnyObjectByType<Leads.LeadsBarView>();
            string leads = _bar != null ? $" · Leads {_bar.ActivatedCount}/12" : string.Empty;

            GUI.Label(new Rect(10, 10, 900, 40),
                $"[Debug] Ep={s.Episode} Step={s.StepIndex}/{s.Steps.Count} {activeKey}{leads}",
                _style);
#endif
        }
    }
}
