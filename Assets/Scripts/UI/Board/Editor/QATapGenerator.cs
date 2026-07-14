#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using AQ.App.UI.Board;
using AQ.App.Economy;
using AQ.App.Leads;
using AQ.App.Overflow;
using AQ.SharedKernel.Economy;

namespace AQ.EditorTools
{
    /// <summary>
    /// Headless verification aid: simulates a real tap on the first generator tile
    /// found in the scene by calling BoardTileView.OnPointerClick directly (the same
    /// entrypoint the input system calls), so it exercises the full
    /// MergeBoardController.SpawnFromGenerator -> OnItemCreated -> LeadRequirementChecker
    /// pipeline exactly as a player tap would. Play Mode only.
    /// </summary>
    public static class QATapGenerator
    {
        [MenuItem("AQ/Dev/QA Tap Generator")]
        public static void TapFirstGenerator()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            var tiles = Object.FindObjectsByType<BoardTileView>(FindObjectsSortMode.None);
            foreach (var tile in tiles)
            {
                if (tile.Kind != TileKind.Generator) continue;
                tile.OnPointerClick(new PointerEventData(EventSystem.current));
                Debug.Log($"[QATapGenerator] Tapped generator tile '{tile.name}'.");
                return;
            }

            Debug.LogWarning("[QATapGenerator] No generator tile found on the board.");
        }

        [MenuItem("AQ/Dev/QA Show Settings")]
        public static void ShowSettings()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }
            AQ.App.UI.Settings.GameControlPanelMB.Show();
        }

        [MenuItem("AQ/Dev/QA Grant 50 Energy")]
        public static void GrantEnergy()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            var wallet = WalletLocator.Instance;
            if (wallet == null)
            {
                Debug.LogWarning("[QATapGenerator] No wallet instance found.");
                return;
            }

            wallet.Grant("qa.grant", Reward.Energy(50));
            Debug.Log($"[QATapGenerator] Granted 50 energy. Balance now {wallet.Get(Currency.Energy)}.");
        }

        /// <summary>
        /// Places the exact item required by the first active lead with an unsatisfied
        /// requirement, via MergeBoardController.PlaceFromOverflow (the same public,
        /// production entrypoint the overflow-bucket delivery flow uses — it fires
        /// OnItemCreated for real). Verifies the requirement-match checkmark end-to-end
        /// without depending on generator RNG to roll the right family/tier.
        /// </summary>
        [MenuItem("AQ/Dev/QA Place Required Item")]
        public static void PlaceRequiredItem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            var repo = Object.FindAnyObjectByType<LeadsRepository>();
            if (repo == null)
            {
                Debug.LogWarning("[QATapGenerator] No LeadsRepository found.");
                return;
            }

            foreach (var lead in repo.CurrentLeads)
            {
                if (lead == null || lead.RuntimeState == LeadState.Blocked) continue;
                if (lead.requirements == null) continue;

                foreach (var req in lead.requirements)
                {
                    if (req.IsSatisfied || req.itemDefinition == null) continue;

                    var controller = Object.FindAnyObjectByType<MergeBoardController>();
                    if (controller == null)
                    {
                        Debug.LogWarning("[QATapGenerator] No MergeBoardController found.");
                        return;
                    }

                    var def = req.itemDefinition;
                    bool placed = controller.PlaceFromOverflow(new OverflowTileData
                    {
                        kind   = OverflowKind.Item,
                        family = def.family,
                        tier   = def.tier
                    });

                    Debug.Log(placed
                        ? $"[QATapGenerator] Placed required item '{def.itemId}' (family={def.family}, tier={def.tier}) for lead '{lead.leadId}'."
                        : "[QATapGenerator] Board full — could not place item.");
                    return;
                }
            }

            Debug.LogWarning("[QATapGenerator] No active lead has an unsatisfied item requirement.");
        }
    }
}
#endif
