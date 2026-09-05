using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.UI.Specials
{
    public enum SpecialId
    {
        SkeletonKey,   // UP: merges with anything -> target +1 tier
        BoxKnife,      // DOWN: split into 2x T-1 (value-neutral)
        CarbonCopy,    // COPY: duplicate an item tile (<= T4)
        BoltCutters,   // REMOVE: clear any tile (generators keep-one guarded)
        SearchWarrant, // REVEAL: next undiscovered tier of a targeted family
        EvidenceTag    // FULFIL: grants one needed requirement item (<= T3) to the locker
    }

    /// <summary>
    /// Serialized Case Kit state — lives inside BoardSaveSystem's aggregate
    /// (schema 0.9.0) so a special grant/consume persists in the SAME atomic
    /// write as the CaseCash spend or lead activation it belongs to.
    /// </summary>
    [Serializable]
    public class SpecialsStateDTO
    {
        public List<string> ids       = new();
        public List<int>    counts    = new();
        public List<string> cassettes = new();
    }

    /// <summary>
    /// The Case Kit — special item inventory (Stephen-ruled 2026-08-07:
    /// SkeletonKey/BoxKnife/CarbonCopy/BoltCutters/SearchWarrant/EvidenceTag +
    /// Tip-Line Cassettes as replayable keepsakes). Mechanic re-ruled
    /// 2026-08-12: board specials are PLACED from the kit as real board tiles
    /// (TileKind.Special; the SpecialId travels in the board's family key) —
    /// they move, swap, and store like anything else, and the effect fires
    /// when the tile is dragged onto its target (confirm-first). This service
    /// holds only the UNPLACED kit inventory; placing consumes a count here.
    /// EvidenceTag has no board target and keeps the direct USE path.
    /// Persistence: folded into BoardSaveSystem's atomic aggregate at schema
    /// 0.9.0 (the instant-PlayerPrefs store let a crash in the debounce window
    /// separate a grant from its lead/spend and duplicate premium-value
    /// specials). Legacy PlayerPrefs keys migrate once and are then deleted.
    /// </summary>
    public static class SpecialItemsService
    {
        private const string PrefsKey         = "aq.specials.state";     // legacy (pre-0.9.0)
        private const string CassettePrefsKey = "aq.specials.cassettes"; // legacy (pre-0.9.0)

        [Serializable] private class LegacyDTO { public List<string> ids = new(); public List<int> counts = new(); }

        private static Dictionary<SpecialId, int> _counts;
        private static List<string> _cassettes;

        public static event Action Changed;

        public static readonly IReadOnlyDictionary<SpecialId, (string name, string desc, int price)> Catalog =
            new Dictionary<SpecialId, (string, string, int)>
            {
                [SpecialId.BoltCutters]   = ("Bolt Cutters",   "Remove any one tile from the board.",                    60),
                [SpecialId.BoxKnife]      = ("Box Knife",      "Cut one item into two of the tier below.",               80),
                [SpecialId.SearchWarrant] = ("Search Warrant", "Reveal the next undiscovered item in a family.",        100),
                [SpecialId.CarbonCopy]    = ("Carbon Copy",    "Duplicate one item (up to Tier 4).",                    150),
                [SpecialId.SkeletonKey]   = ("Skeleton Key",   "Merge with anything: raise one item a tier.",           200),
                [SpecialId.EvidenceTag]   = ("Evidence Tag",   "File one needed lead item (up to Tier 3) in the locker.", 250),
            };

        /// <summary>Resources icon for a special (null pre-art-delivery).</summary>
        public static Sprite SpriteFor(SpecialId id) =>
            Resources.Load<Sprite>("App/UI/Specials/special_" + id.ToString().ToLowerInvariant());

        public static int CountOf(SpecialId id)
        {
            EnsureContainers();
            return _counts.TryGetValue(id, out var n) ? n : 0;
        }

        public static int TotalCount
        {
            get
            {
                EnsureContainers();
                int n = 0;
                foreach (var kv in _counts) n += kv.Value;
                return n;
            }
        }

        public static void Grant(SpecialId id, int amount = 1)
        {
            EnsureContainers();
            _counts[id] = CountOf(id) + amount;
            Changed?.Invoke();
        }

        /// <summary>Consume one — call ONLY after the effect actually applied.</summary>
        public static bool Consume(SpecialId id)
        {
            EnsureContainers();
            if (CountOf(id) <= 0) return false;
            _counts[id] = _counts[id] - 1;
            Changed?.Invoke();
            return true;
        }

        // ---- cassettes (keepsakes: granted, never consumed) ----

        public static IReadOnlyList<string> Cassettes { get { EnsureContainers(); return _cassettes; } }

        public static void GrantCassette(string clipResourcePath)
        {
            EnsureContainers();
            if (_cassettes.Contains(clipResourcePath)) return; // dedup vs the Mo-shop copy
            _cassettes.Add(clipResourcePath);
            Changed?.Invoke();
        }

        // ---- aggregate hooks (BoardSaveSystem) ----

        /// <summary>Snapshot for the save aggregate.</summary>
        public static SpecialsStateDTO ExportState()
        {
            EnsureContainers();
            var dto = new SpecialsStateDTO();
            foreach (var kv in _counts)
            {
                dto.ids.Add(kv.Key.ToString());
                dto.counts.Add(kv.Value);
            }
            dto.cassettes.AddRange(_cassettes);
            return dto;
        }

        /// <summary>
        /// Replaces state from the aggregate. Null = pre-0.9.0 save (or none):
        /// resets the statics (they survive play sessions when domain reload is
        /// off) and migrates the legacy PlayerPrefs keys if present.
        /// </summary>
        public static void ImportState(SpecialsStateDTO dto)
        {
            _counts    = new Dictionary<SpecialId, int>();
            _cassettes = new List<string>();

            if (dto != null)
            {
                if (dto.ids != null && dto.counts != null)
                    for (int i = 0; i < dto.ids.Count && i < dto.counts.Count; i++)
                        if (Enum.TryParse<SpecialId>(dto.ids[i], out var id))
                            _counts[id] = dto.counts[i];
                if (dto.cassettes != null)
                    foreach (var c in dto.cassettes)
                        if (!string.IsNullOrEmpty(c) && !_cassettes.Contains(c)) _cassettes.Add(c);
            }
            else
            {
                LoadLegacyPrefs();
            }

            Changed?.Invoke();
        }

        /// <summary>Order-insensitive content hash for the aggregate's change detection.</summary>
        public static int StateHash()
        {
            EnsureContainers();
            unchecked
            {
                int h = 29;
                foreach (var kv in _counts)
                    h += ((int)kv.Key + 1) * 31 * kv.Value; // commutative: dictionary order is unstable
                foreach (var c in _cassettes)
                    h = h * 31 + (c != null ? c.GetHashCode() : 0);
                return h;
            }
        }

        /// <summary>Removes the pre-0.9.0 PlayerPrefs keys once the aggregate owns the state.</summary>
        public static void DeleteLegacyKeys()
        {
            if (!PlayerPrefs.HasKey(PrefsKey) && !PlayerPrefs.HasKey(CassettePrefsKey)) return;
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.DeleteKey(CassettePrefsKey);
            PlayerPrefs.Save();
        }

        /// <summary>QA reset: empty state and remove legacy keys.</summary>
        public static void Clear()
        {
            _counts    = new Dictionary<SpecialId, int>();
            _cassettes = new List<string>();
            DeleteLegacyKeys();
            Changed?.Invoke();
        }

        // ---- internals ----

        private static void EnsureContainers()
        {
            if (_counts != null) return;
            _counts    = new Dictionary<SpecialId, int>();
            _cassettes = new List<string>();
        }

        private static void LoadLegacyPrefs()
        {
            try
            {
                var dto = JsonUtility.FromJson<LegacyDTO>(PlayerPrefs.GetString(PrefsKey, string.Empty));
                if (dto?.ids != null)
                    for (int i = 0; i < dto.ids.Count && i < dto.counts.Count; i++)
                        if (Enum.TryParse<SpecialId>(dto.ids[i], out var id))
                            _counts[id] = dto.counts[i];
            }
            catch { /* fresh state */ }

            var cas = PlayerPrefs.GetString(CassettePrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(cas))
                foreach (var c in cas.Split('|'))
                    if (!string.IsNullOrEmpty(c) && !_cassettes.Contains(c)) _cassettes.Add(c);
        }
    }
}
