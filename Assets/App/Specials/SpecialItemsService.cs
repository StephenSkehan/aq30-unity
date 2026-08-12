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
    /// The Case Kit — special item inventory (Stephen-ruled 2026-08-07:
    /// SkeletonKey/BoxKnife/CarbonCopy/BoltCutters/SearchWarrant/EvidenceTag +
    /// Tip-Line Cassettes as replayable keepsakes). Mechanic re-ruled
    /// 2026-08-12: board specials are PLACED from the kit as real board tiles
    /// (TileKind.Special; the SpecialId travels in the board's family key) —
    /// they move, swap, and store like anything else, and the effect fires
    /// when the tile is dragged onto its target (confirm-first). This service
    /// holds only the UNPLACED kit inventory; placing consumes a count here.
    /// EvidenceTag has no board target and keeps the direct USE path.
    /// Persistence: PlayerPrefs (QA Reset's DeleteAll clears it).
    /// </summary>
    public static class SpecialItemsService
    {
        private const string PrefsKey        = "aq.specials.state";
        private const string CassettePrefsKey = "aq.specials.cassettes";

        [Serializable] private class DTO { public List<string> ids = new(); public List<int> counts = new(); }

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
            EnsureLoaded();
            return _counts.TryGetValue(id, out var n) ? n : 0;
        }

        public static int TotalCount
        {
            get
            {
                EnsureLoaded();
                int n = 0;
                foreach (var kv in _counts) n += kv.Value;
                return n;
            }
        }

        public static void Grant(SpecialId id, int amount = 1)
        {
            EnsureLoaded();
            _counts[id] = CountOf(id) + amount;
            Save();
            Changed?.Invoke();
        }

        /// <summary>Consume one — call ONLY after the effect actually applied.</summary>
        public static bool Consume(SpecialId id)
        {
            EnsureLoaded();
            if (CountOf(id) <= 0) return false;
            _counts[id] = _counts[id] - 1;
            Save();
            Changed?.Invoke();
            return true;
        }

        // ---- cassettes (keepsakes: granted, never consumed) ----

        public static IReadOnlyList<string> Cassettes { get { EnsureLoaded(); return _cassettes; } }

        public static void GrantCassette(string clipResourcePath)
        {
            EnsureLoaded();
            if (_cassettes.Contains(clipResourcePath)) return;
            _cassettes.Add(clipResourcePath);
            PlayerPrefs.SetString(CassettePrefsKey, string.Join("|", _cassettes));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        // ---- persistence ----

        private static void EnsureLoaded()
        {
            if (_counts != null) return;
            _counts = new Dictionary<SpecialId, int>();
            _cassettes = new List<string>();
            try
            {
                var dto = JsonUtility.FromJson<DTO>(PlayerPrefs.GetString(PrefsKey, string.Empty));
                if (dto?.ids != null)
                    for (int i = 0; i < dto.ids.Count && i < dto.counts.Count; i++)
                        if (Enum.TryParse<SpecialId>(dto.ids[i], out var id))
                            _counts[id] = dto.counts[i];
            }
            catch { /* fresh state */ }
            var cas = PlayerPrefs.GetString(CassettePrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(cas))
                foreach (var c in cas.Split('|'))
                    if (!string.IsNullOrEmpty(c)) _cassettes.Add(c);
        }

        private static void Save()
        {
            var dto = new DTO();
            foreach (var kv in _counts)
            {
                dto.ids.Add(kv.Key.ToString());
                dto.counts.Add(kv.Value);
            }
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(dto));
            PlayerPrefs.Save();
        }
    }
}
