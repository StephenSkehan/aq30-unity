// Assembly: AQ.App
// Purpose: Expose public methods for UnityEvents/buttons to advance/reset CaseFlow.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    public sealed class CaseFlowAdvanceOnEventMB : MonoBehaviour
    {
        public void Advance()
        {
            var svc = CaseFlowLocator.Instance;
            if (svc != null && svc.CompleteCurrentStep())
                Debug.Log($"[CaseFlow] Advance (manual/event) → stepIndex={svc.Current.StepIndex}/{svc.Current.Steps.Count}");
        }

        public void ResetProgress()
        {
            var svc = CaseFlowLocator.Instance;
            if (svc != null)
            {
                svc.Reset();
                Debug.Log("[CaseFlow] Reset progress to start (manual/event)");
            }
        }
    }
}
