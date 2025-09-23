#if false
using UnityEditor;
using UnityEngine;
using AQ.SharedKernel.Events;
using AQ.Domain.CaseFlow;

namespace AQ.Tools
{
    public static class CaseFlowProbe
    {
        [MenuItem("AQ/Tools/CaseFlow Probe/Run Happy Path", priority = 2200)]
        public static void RunHappyPath()
        {
            var bus = new EventBusInMemory();
            using var sub1 = bus.Subscribe<CaseFlowStarted>(_ => Debug.Log("[CaseFlow] FTUE started"));
            using var sub2 = bus.Subscribe<CaseFlowStateChanged>(e => Debug.Log($"[CaseFlow] {e}"));
            using var sub3 = bus.Subscribe<FirstMergeObserved>(_ => Debug.Log("[CaseFlow] First merge observed"));

            var flow = new CaseFlowController(bus);
            flow.StartFtue();            // ColdOpen -> Briefing
            flow.OnBriefingComplete();   // Briefing -> Board
            flow.OnFirstMergeObserved(); // Board -> EvidenceGate

            Debug.Log($"[CaseFlow] Final state = {flow.State}");
        }
    }
}
#endif
