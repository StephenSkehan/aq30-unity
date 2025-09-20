// Assembly: AQ.App
// Purpose: Minimal on-screen display of CaseFlow state for dev builds.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    public sealed class CaseFlowDebugOverlayMB : MonoBehaviour
    {
        public bool show = true;
        public int fontSize = 14;
        private GUIStyle _style;

        void OnGUI()
        {
            if (!show) return;
            var svc = CaseFlowLocator.Instance;
            if (svc == null) return;

            _style ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize };
            var s = svc.Current;
            var activeKey = (s.StepIndex < s.Steps.Count) ? s.Steps[s.StepIndex] : "<complete>";
            GUI.Label(new Rect(10, 10, 600, 40),
                $"[CaseFlow] Episode={s.Episode} StepIndex={s.StepIndex}/{s.Steps.Count} ActiveKey={activeKey}",
                _style);
        }
    }
}
