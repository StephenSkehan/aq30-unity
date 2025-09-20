// Assembly: AQ.App
// Purpose: App-level locator for the domain caseflow service.

using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    public static class CaseFlowLocator
    {
        public static ICaseFlowService Instance { get; private set; }
        public static void Set(ICaseFlowService svc) => Instance = svc;
    }
}
