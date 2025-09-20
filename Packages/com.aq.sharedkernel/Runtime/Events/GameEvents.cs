namespace AQ.SharedKernel.Events
{
    // Generic "state changed" and feature beacons for WK3 skeleton

    public readonly struct CaseFlowStarted : IGameEvent { }

    public enum CaseFlowState
    {
        ColdOpen,
        Briefing,
        Board,
        EvidenceGate
    }

    public readonly struct CaseFlowStateChanged : IGameEvent
    {
        public readonly CaseFlowState Previous;
        public readonly CaseFlowState Next;
        public CaseFlowStateChanged(CaseFlowState previous, CaseFlowState next)
        {
            Previous = previous; Next = next;
        }
        public override string ToString() => $"CaseFlowStateChanged {Previous} → {Next}";
    }

    public readonly struct FirstMergeObserved : IGameEvent { }
}