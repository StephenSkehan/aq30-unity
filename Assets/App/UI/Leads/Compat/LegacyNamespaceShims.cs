// Purpose: bridge old Editor utilities that still reference AQ.UI.Leads.*
// to the new runtime types under AQ.App.UI.Leads.* without touching the Editor code.

namespace AQ.UI.Leads
{
    // We inherit (no extra behavior). This requires the App types to be non-sealed.
    public class LeadCardPresenter : AQ.App.UI.Leads.LeadCardPresenter {}
    public class RequirementSlotView : AQ.App.UI.Leads.RequirementSlotView {}
    public class TierSetPopup       : AQ.App.UI.Leads.TierSetPopup {}

    // If your project has a runtime LeadsBarPopulator under AQ.App.UI.Leads,
    // expose it here too so Editor tools can see it under the legacy namespace.
    public class LeadsBarPopulator  : AQ.App.UI.Leads.LeadsBarPopulator {}
}
