using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Merge
{
    [CreateAssetMenu(fileName = "MergeIconCatalog", menuName = "AQ/Merge/Icon Catalog", order = 0)]
    public sealed class MergeIconCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;      // e.g., "stakeout_fuel_t1_paper_cup"
            public Sprite sprite;  // atlas sprite or standalone
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        private Dictionary<string, Sprite> _lookup;

        public IReadOnlyList<Entry> Entries => entries;

        public void SetEntries(List<Entry> newEntries)
        {
            entries = newEntries ?? new List<Entry>();
            _lookup = null;
        }

        public bool TryGet(string id, out Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                sprite = null;
                return false;
            }
            _lookup ??= Build();
            return _lookup.TryGetValue(id, out sprite);
        }

        public Sprite GetOrNull(string id)
        {
            return TryGet(id, out var s) ? s : null;
        }

        private Dictionary<string, Sprite> Build()
        {
            var dict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (!string.IsNullOrWhiteSpace(e.id) && e.sprite != null)
                {
                    dict[e.id.Trim()] = e.sprite;
                }
            }
            return dict;
        }
    }
}
