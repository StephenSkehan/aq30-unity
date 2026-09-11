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
            RequirementSlotView.OnRequirementTapped -= ShowFamilyTrail;
            RequirementSlotView.OnRequirementTapped += ShowFamilyTrail;
        }

        /// <summary>
        /// SHOW ME trail (SAS FTUE I3, Merge Mansion's task-"Show" pattern): a
        /// TAP on a requirement chip opens the item's family ladder — current
        /// tier marked, undiscovered tiers silhouetted — and pulses the board
        /// generator whose drop table starts that family, so one tap answers
        /// both "what is this" and "where does it come from".
        /// </summary>
        private static void ShowFamilyTrail(string itemId)
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            AQ.App.Items.ItemDefinitionSO def = null;
            foreach (var d in board.ItemDefinitions)
                if (d != null && d.itemId == itemId) { def = d; break; }
            if (def == null) return;

            ItemFamilyPopup.Show(def.family, def.tier);
            SourceGeneratorPulse.PulseFor(board, def.family);
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
