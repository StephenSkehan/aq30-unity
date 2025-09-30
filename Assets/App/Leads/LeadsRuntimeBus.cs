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

        public static void BroadcastAll(IReadOnlyList<LeadData> list) => OnLeadsRefreshed?.Invoke(list);
        public static void BroadcastState(LeadData lead) => OnLeadStateChanged?.Invoke(lead);
    }
}
