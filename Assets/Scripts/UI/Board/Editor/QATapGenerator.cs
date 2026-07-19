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

        /// <summary>
        /// Merges the first same-(kind,family,tier) pair found on the board through
        /// MergeBoardController.EndDrag — the real drop entrypoint — so merge-driven
        /// state (badges, events, counts) can be QA'd headlessly.
        /// </summary>
        [MenuItem("AQ/Dev/QA Merge First Pair")]
        public static void MergeFirstPair()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            var controller = Object.FindAnyObjectByType<MergeBoardController>();
            if (controller == null) { Debug.LogWarning("[QATapGenerator] No MergeBoardController found."); return; }

            var tiles = Object.FindObjectsByType<BoardTileView>(FindObjectsSortMode.None);
            for (int i = 0; i < tiles.Length; i++)
            {
                var a = tiles[i];
                if (a.IsEmpty || !controller.IsMergeCandidate(a)) continue;
                for (int j = i + 1; j < tiles.Length; j++)
                {
                    var b = tiles[j];
                    if (b.IsEmpty || b.Kind != a.Kind || b.Tier != a.Tier) continue;
                    if (controller.GetItemId(a) != controller.GetItemId(b) && a.Kind == TileKind.Item) continue;

                    controller.EndDrag(controller.GetIndex(a), controller.GetIndex(b));
                    Debug.Log($"[QATapGenerator] Merged pair '{a.name}' -> '{b.name}'.");
                    return;
                }
            }
            Debug.LogWarning("[QATapGenerator] No mergeable pair on the board.");
        }

        /// <summary>
        /// Pushes a T1 Investigation Lab generator into the overflow pocket (the same
        /// path LeadOutcomeMB grants use), so lab-chain behaviour (T1 icon, T1+T1→T2
        /// via QA Merge First Pair, ceiling) can be QA'd without walking to L4.
        /// </summary>
        [MenuItem("AQ/Dev/QA Grant Lab Generator (pocket)")]
        public static void GrantLabGenerator()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            OverflowBucketService.Push(new OverflowTileData
            {
                kind   = OverflowKind.Generator,
                family = "gen_investigation_lab",
                tier   = 0
            });
            Debug.Log("[QATapGenerator] Pushed T1 gen_investigation_lab into the overflow pocket.");
        }

        /// <summary>
        /// Places the top overflow-pocket tile onto the board — the same
        /// peek → PlaceFromOverflow → Pop sequence OverflowBucketView runs on a
        /// bucket click, minus the raw-input dependency (stepped frames eat clicks).
        /// </summary>
        [MenuItem("AQ/Dev/QA Drain Pocket Once")]
        public static void DrainPocketOnce()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QATapGenerator] Enter Play Mode first.");
                return;
            }

            var board = Object.FindAnyObjectByType<MergeBoardController>();
            if (board == null) { Debug.LogWarning("[QATapGenerator] No MergeBoardController found."); return; }

            var top = OverflowBucketService.Peek();
            if (top == null) { Debug.Log("[QATapGenerator] Pocket is empty."); return; }

            if (board.PlaceFromOverflow(top.Value))
            {
                OverflowBucketService.Pop();
                Debug.Log($"[QATapGenerator] Placed {top.Value.family} T{top.Value.tier + 1} from pocket. Remaining={OverflowBucketService.Count}.");
            }
            else
            {
                Debug.LogWarning("[QATapGenerator] Board refused placement (full?).");
            }
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
