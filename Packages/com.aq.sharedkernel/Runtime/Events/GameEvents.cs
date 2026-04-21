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

    /// <summary>
    /// Fired by MergeEventsBridge whenever a new item is placed on the board
    /// (from a generator spawn or a merge result). Used by LeadRequirementChecker
    /// to auto-satisfy lead requirements.
    /// </summary>
    public readonly struct ItemCreatedOnBoard : IGameEvent
    {
        /// <summary>Matches ItemDefinitionSO.itemId (e.g. "forensic_laptop").</summary>
        public readonly string ItemId;
        /// <summary>Matches MergeBoardController family key (e.g. "forensic_tools").</summary>
        public readonly string Family;
        /// <summary>0-based tier (T1 item = 0).</summary>
        public readonly int Tier;

        public ItemCreatedOnBoard(string itemId, string family, int tier)
        {
            ItemId = itemId;
            Family = family;
            Tier   = tier;
        }
    }
}