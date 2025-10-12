using System;
using System.Collections.Generic;

namespace AQ.App.Leads
{
    /// <summary>
    /// Editor ↔ Runtime bridge. The Playground broadcasts here;
    /// the in-scene repository subscribes.
    /// </summary>
    public static class LeadsRuntimeBus
    {
        public static event Action<IReadOnlyList<LeadData>> OnPushAll;
        public static event Action<LeadData> OnPushOne;

        public static void BroadcastAll(IReadOnlyList<LeadData> leads) => OnPushAll?.Invoke(leads);
        public static void BroadcastState(LeadData lead) => OnPushOne?.Invoke(lead);
    }
}
