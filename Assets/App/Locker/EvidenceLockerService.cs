using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AQ.App.Overflow;
using AQ.SharedKernel.Economy;
using UnityEngine;

namespace AQ.App.Locker
{
    /// <summary>
    /// Off-board item storage ("Evidence Locker") — the CaseCash sink.
    /// 8 free slots; slots 9-12 purchased with soft currency (200/400/800/1600).
    /// Generators cannot be stored. Persists via locker_state.json.
    /// Stored items keep counting toward lead satisfaction (LeadRequirementChecker
    /// merges locker counts); consumption pulls board first, then locker.
    /// </summary>
    public static class EvidenceLockerService
    {
        public const int FreeSlots = 8;
        public static readonly int[] SlotPrices = { 200, 400, 800, 1600 };
        public const int MaxSlots = 12; // FreeSlots + SlotPrices.Length

        [Serializable]
        private struct Entry
        {
            public string family;
            public int tier;
            public string itemId; // ItemDefinitionSO.itemId — drives lead satisfaction; may be empty
        }

        private static readonly List<Entry> _entries = new();
        private static int _purchasedSlots;
        private static bool _loaded;

        public static event Action LockerChanged;

        public static int Capacity { get { EnsureLoaded(); return FreeSlots + _purchasedSlots; } }
        public static int Count { get { EnsureLoaded(); return _entries.Count; } }
        public static int PurchasedSlots { get { EnsureLoaded(); return _purchasedSlots; } }
        public static bool CanStore => Count < Capacity;

        /// <summary>Soft-currency price of the next slot, or -1 when all slots are owned.</summary>
        public static int NextSlotPrice
        {
            get
            {
                EnsureLoaded();
                return _purchasedSlots < SlotPrices.Length ? SlotPrices[_purchasedSlots] : -1;
            }
        }

        public static OverflowTileData GetAt(int index)
        {
            EnsureLoaded();
            var e = _entries[index];
            return new OverflowTileData { kind = OverflowKind.Item, family = e.family, tier = e.tier };
        }

        /// <summary>Stores an item. Returns false when full or when handed a generator.</summary>
        public static bool TryStore(OverflowTileData data, string itemId)
        {
            EnsureLoaded();
            if (data.kind == OverflowKind.Generator) return false;
            if (!CanStore) return false;
            _entries.Add(new Entry { family = data.family, tier = data.tier, itemId = itemId ?? string.Empty });
            Save();
            LockerChanged?.Invoke();
            return true;
        }

        /// <summary>Removes the item at index. Call only after it was successfully placed back on the board.</summary>
        public static void RemoveAt(int index)
        {
            EnsureLoaded();
            if (index < 0 || index >= _entries.Count) return;
            _entries.RemoveAt(index);
            Save();
            LockerChanged?.Invoke();
        }

        /// <summary>
        /// Removes the first stored item matching (family, tier) — the lead-consumption path
        /// when the board alone can't cover a requirement. Returns false when absent.
        /// </summary>
        public static bool TryTakeItem(string family, int tier)
        {
            EnsureLoaded();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].tier != tier || _entries[i].family != family) continue;
                _entries.RemoveAt(i);
                Save();
                LockerChanged?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>Fills <paramref name="into"/> with itemId → stored count (entries with empty itemIds are skipped).</summary>
        public static void CopyCounts(Dictionary<string, int> into)
        {
            EnsureLoaded();
            into.Clear();
            foreach (var e in _entries)
            {
                if (string.IsNullOrEmpty(e.itemId)) continue;
                into[e.itemId] = (into.TryGetValue(e.itemId, out int c) ? c : 0) + 1;
            }
        }

        /// <summary>Spends soft currency to unlock the next slot. Returns false when maxed or unaffordable.</summary>
        public static bool TryBuySlot()
        {
            EnsureLoaded();
            int price = NextSlotPrice;
            if (price < 0) return false;

            var wallet = Economy.WalletLocator.Instance;
            if (wallet == null || !wallet.TrySpend(Currency.Soft, price, "locker_slot")) return false;

            _purchasedSlots++;
            Save();
            LockerChanged?.Invoke();
            Analytics.GameAnalytics.LogLockerSlotPurchased(FreeSlots + _purchasedSlots, price);
            return true;
        }

        public static void Clear()
        {
            _entries.Clear();
            _purchasedSlots = 0;
            _loaded = true;
            Save();
            LockerChanged?.Invoke();
        }

        /// <summary>
        /// Drops in-memory state and re-reads locker_state.json. Called on scene boot
        /// (mirrors OverflowBucketService.Load) so statics can't go stale when domain
        /// reload is disabled or QA reset deleted the file from edit mode.
        /// </summary>
        public static void ReloadFromDisk()
        {
            _entries.Clear();
            _purchasedSlots = 0;
            _loaded = false;
            EnsureLoaded();
            LockerChanged?.Invoke();
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            var p = FilePath;
            if (!File.Exists(p)) return;
            try
            {
                var dto = JsonUtility.FromJson<DTO>(File.ReadAllText(p, Encoding.UTF8));
                if (dto != null)
                {
                    if (dto.entries != null) _entries.AddRange(dto.entries);
                    _purchasedSlots = Mathf.Clamp(dto.purchasedSlots, 0, SlotPrices.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvidenceLocker] load failed: {ex.Message}");
            }
        }

        private static string FilePath
            => Path.Combine(Application.persistentDataPath, "locker_state.json");

        private static void Save()
        {
            try
            {
                var dto = new DTO { entries = new List<Entry>(_entries), purchasedSlots = _purchasedSlots };
                File.WriteAllText(FilePath, JsonUtility.ToJson(dto), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvidenceLocker] save failed: {ex.Message}");
            }
        }

        [Serializable]
        private class DTO
        {
            public List<Entry> entries;
            public int purchasedSlots;
        }
    }
}
