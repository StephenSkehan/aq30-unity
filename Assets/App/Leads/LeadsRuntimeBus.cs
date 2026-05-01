using System;
using System.Collections.Generic;

namespace AQ.App.Leads
{
    /// <summary>
    /// Ultra-simple static bus so Editor or gameplay can broadcast changes
    /// without hard dependencies. LeadsRepository subscribes to this.
    /// </summary>
    public static class LeadsRuntimeBus
    {
        public static event Action<IReadOnlyList<LeadData>> OnLeadsRefreshed;
        public static event Action<LeadData> OnLeadStateChanged;
        /// <summary>
        /// Fired when the player taps Proceed on a Ready lead card.
        /// LeadActivationBridgeMB (Assembly-CSharp) responds by consuming board items
        /// and removing the lead from the repository.
        /// </summary>
        public static event Action<LeadData> OnLeadActivated;

        public static void BroadcastAll(IReadOnlyList<LeadData> list)       => OnLeadsRefreshed?.Invoke(list);
        public static void BroadcastState(LeadData lead)                    => OnLeadStateChanged?.Invoke(lead);
        public static void BroadcastActivated(LeadData lead)                => OnLeadActivated?.Invoke(lead);
    }
}
