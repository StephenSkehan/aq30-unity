using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AQ.App.Overflow;
using AQ.SharedKernel.Economy;
using UnityEngine;

namespace AQ.App.Locker
{
    [Serializable]
    public sealed class LockerEntryDTO
    {
        public string family;
        public int tier;
        public string itemId;
    }

    /// <summary>
    /// Persisted locker state. Field names match the retired locker_state.json layout,
    /// so that legacy file parses directly as this DTO during migration.
    /// </summary>
    [Serializable]
    public sealed class LockerStateDTO
    {
        public List<LockerEntryDTO> entries = new();
        public int purchasedSlots;
    }

    /// <summary>
    /// Off-board item storage ("Evidence Locker") — the CaseCash sink.
    /// 8 free slots; slots 9-12 purchased with soft currency (200/400/800/1600).
    /// Generators cannot be stored. Stored items keep counting toward lead satisfaction
    /// (LeadRequirementChecker merges locker counts); consumption pulls board first, then locker.
    ///
    /// Persistence: this service holds RUNTIME STATE ONLY. Since schema 0.7.0 the state
    /// is folded into BoardSaveSystem's atomic save aggregate (ExportState/ImportState),
    /// so a crash can never separate a locker transaction from its board-side half —
    /// both roll back together to the last consistent save. locker_state.json is the
    /// legacy pre-aggregate file: read once for migration, deleted after the first
    /// successful aggregate save.
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

        /// <summary>Test seam: overrides where the legacy locker_state.json is read from.</summary>
        public static string LegacyPathOverride;

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
            LockerChanged?.Invoke();
            return true;
        }

        /// <summary>Removes the item at index. Call only after it was successfully placed back on the board.</summary>
        public static void RemoveAt(int index)
        {
            EnsureLoaded();
            if (index < 0 || index >= _entries.Count) return;
            _entries.RemoveAt(index);
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
            LockerChanged?.Invoke();
            Analytics.GameAnalytics.LogLockerSlotPurchased(FreeSlots + _purchasedSlots, price);
            return true;
        }

        public static void Clear()
        {
            _entries.Clear();
            _purchasedSlots = 0;
            _loaded = true;
            DeleteLegacyFile();
            LockerChanged?.Invoke();
        }

        /// <summary>Deep-copies current state for the board save aggregate.</summary>
        public static LockerStateDTO ExportState()
        {
            EnsureLoaded();
            var dto = new LockerStateDTO { purchasedSlots = _purchasedSlots };
            foreach (var e in _entries)
                dto.entries.Add(new LockerEntryDTO { family = e.family, tier = e.tier, itemId = e.itemId });
            return dto;
        }

        /// <summary>
        /// Replaces runtime state from the board save aggregate on restore. A null state
        /// means the save predates the aggregate (or no save exists): state resets and
        /// the legacy locker_state.json, if present, is migrated in.
        /// </summary>
        public static void ImportState(LockerStateDTO state)
        {
            _loaded = true;
            _entries.Clear();
            _purchasedSlots = 0;
            if (state != null) ApplyDto(state);
            else LoadLegacy();
            LockerChanged?.Invoke();
        }

        /// <summary>Hash of persisted state — lets BoardSaveSystem's snapshot poll catch locker changes.</summary>
        public static int StateHash()
        {
            EnsureLoaded();
            unchecked
            {
                int h = 17;
                h = h * 31 + _purchasedSlots;
                foreach (var e in _entries)
                {
                    h = h * 31 + (e.family?.GetHashCode() ?? 0);
                    h = h * 31 + e.tier;
                    h = h * 31 + (e.itemId?.GetHashCode() ?? 0);
                }
                return h;
            }
        }

        /// <summary>
        /// Deletes the pre-aggregate legacy file. BoardSaveSystem calls this after every
        /// successful aggregate write (the contents are folded in and a stale file must
        /// not resurrect on a future boot).
        /// </summary>
        public static void DeleteLegacyFile()
        {
            try
            {
                var p = LegacyPath;
                if (File.Exists(p)) File.Delete(p);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvidenceLocker] legacy file delete failed: {ex.Message}");
            }
        }

        private static string LegacyPath
            => LegacyPathOverride ?? Path.Combine(Application.persistentDataPath, "locker_state.json");

        // Lazy legacy fallback for callers that touch the service before
        // BoardSaveSystem.Start restores the aggregate (which then replaces
        // this via ImportState either way).
        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            LoadLegacy();
        }

        private static void LoadLegacy()
        {
            var p = LegacyPath;
            if (!File.Exists(p)) return;
            try
            {
                ApplyDto(JsonUtility.FromJson<LockerStateDTO>(File.ReadAllText(p, Encoding.UTF8)));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvidenceLocker] legacy load failed: {ex.Message}");
            }
        }

        private static void ApplyDto(LockerStateDTO dto)
        {
            if (dto == null) return;
            if (dto.entries != null)
                foreach (var e in dto.entries)
                    _entries.Add(new Entry { family = e.family, tier = e.tier, itemId = e.itemId ?? string.Empty });
            _purchasedSlots = Mathf.Clamp(dto.purchasedSlots, 0, SlotPrices.Length);
        }
    }
}
