using AQ.App.Locker;
using AQ.App.Overflow;
using AQ.App.UI.Board;
using AQ.SharedKernel.Economy;
using UnityEditor;
using UnityEngine;

/// <summary>Headless drivers for the Evidence Locker (play mode only).</summary>
public static class QALockerMenu
{
    [MenuItem("AQ/Dev/QA Locker - Store First Board Item")]
    private static void StoreFirstItem()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[QA Locker] Play mode only."); return; }
        var board = Object.FindFirstObjectByType<MergeBoardController>();
        if (board == null) { Debug.LogWarning("[QA Locker] No MergeBoardController."); return; }

        foreach (var def in board.ItemDefinitions)
        {
            if (def == null) continue;
            if (!EvidenceLockerService.CanStore) { Debug.Log("[QA Locker] Locker full."); return; }
            if (board.TryClearItem(def.family, def.tier, out _, out _))
            {
                bool ok = EvidenceLockerService.TryStore(new OverflowTileData
                {
                    kind = OverflowKind.Item, family = def.family, tier = def.tier
                });
                Debug.Log($"[QA Locker] Stored {def.family} T{def.tier + 1}: {ok}. Count={EvidenceLockerService.Count}/{EvidenceLockerService.Capacity}");
                return;
            }
        }
        Debug.Log("[QA Locker] No item on board to store.");
    }

    [MenuItem("AQ/Dev/QA Locker - Retrieve First Item")]
    private static void RetrieveFirst()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[QA Locker] Play mode only."); return; }
        var board = Object.FindFirstObjectByType<MergeBoardController>();
        if (board == null || EvidenceLockerService.Count == 0)
        {
            Debug.Log($"[QA Locker] Nothing to retrieve (count={EvidenceLockerService.Count}).");
            return;
        }
        var data = EvidenceLockerService.GetAt(0);
        bool placed = board.PlaceFromOverflow(data);
        if (placed) EvidenceLockerService.RemoveAt(0);
        Debug.Log($"[QA Locker] Retrieve {data.family} T{data.tier + 1}: placed={placed}. Count={EvidenceLockerService.Count}/{EvidenceLockerService.Capacity}");
    }

    [MenuItem("AQ/Dev/QA Locker - Buy Next Slot")]
    private static void BuySlot()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[QA Locker] Play mode only."); return; }
        int price = EvidenceLockerService.NextSlotPrice;
        bool ok = EvidenceLockerService.TryBuySlot();
        Debug.Log($"[QA Locker] Buy slot (price {price}): {ok}. Capacity={EvidenceLockerService.Capacity}, purchased={EvidenceLockerService.PurchasedSlots}");
    }

    [MenuItem("AQ/Dev/QA Locker - Grant 500 CC")]
    private static void GrantCash()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[QA Locker] Play mode only."); return; }
        AQ.App.Economy.WalletLocator.Instance?.Grant("qa_locker", Reward.Soft(500));
        Debug.Log("[QA Locker] Granted 500 CC.");
    }

    [MenuItem("AQ/Dev/QA Locker - Open Panel")]
    private static void OpenPanel()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[QA Locker] Play mode only."); return; }
        LockerScreen.Open();
        Debug.Log($"[QA Locker] Panel opened. Count={EvidenceLockerService.Count}/{EvidenceLockerService.Capacity}");
    }
}
