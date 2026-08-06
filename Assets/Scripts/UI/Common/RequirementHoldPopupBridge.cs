using AQ.App.UI.Board;
using AQ.App.UI.Leads;
using UnityEngine;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Opens TileInfoPopup when a lead card requirement slot is long-pressed
    /// (RequirementSlotView.OnRequirementHeld). Lives in Assembly-CSharp because
    /// the slot view's assembly (AQ.App) cannot reference the popup. Shows the
    /// live owned count (board + locker, via LeadRequirementChecker).
    /// </summary>
    public static class RequirementHoldPopupBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            RequirementSlotView.OnRequirementHeld -= Show; // domain-reload-off safety
            RequirementSlotView.OnRequirementHeld += Show;
        }

        private static void Show(string itemId)
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            AQ.App.Items.ItemDefinitionSO def = null;
            foreach (var d in board.ItemDefinitions)
                if (d != null && d.itemId == itemId) { def = d; break; }
            if (def == null) return;

            int owned = AQ.App.Leads.LeadRequirementChecker.Instance != null
                ? AQ.App.Leads.LeadRequirementChecker.Instance.GetLiveCount(itemId)
                : 0;

            TileInfoPopup.Show(def.displayName, def.icon, def.family, def.tier,
                               onStore: null, ownedCount: owned);
        }
    }
}
