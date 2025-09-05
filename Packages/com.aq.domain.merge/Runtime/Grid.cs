using System.Collections.Generic;

namespace AQ.Domain.Merge
{
    public sealed class Grid : IGrid
    {
        private readonly Dictionary<int, ItemId> _cells = new Dictionary<int, ItemId>();

        // Convenience indexer (whether or not IGrid declares one).
        public ItemId this[int index]
        {
            get { return _cells.TryGetValue(index, out var value) ? value : default; }
            set { _cells[index] = value; }
        }

        // Convenience helpers (not necessarily on IGrid)
        public ItemId Get(int index) => this[index];
        public void   Set(int index, ItemId value) => this[index] = value;

        // Required by your IGrid (based on earlier compiler error)
        public bool Contains(ItemId value)
        {
            foreach (var kv in _cells)
            {
                if (kv.Value.Equals(value)) return true;
            }
            return false;
        }
    }
}
