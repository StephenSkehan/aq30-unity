using System;

namespace AQ.Domain.Merge
{
    public readonly struct ItemId : IEquatable<ItemId>
    {
        public string Value { get; }

        public ItemId(string value)
        {
            Value = value ?? string.Empty;
        }

        public override string ToString() => Value;

        public static bool TryParse(string s, out ItemId id)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                id = default;
                return false;
            }
            id = new ItemId(s.Trim());
            return true;
        }

        public static ItemId Parse(string s)
        {
            ItemId id;
            if (TryParse(s, out id)) return id;
            throw new ArgumentException("Invalid ItemId", nameof(s));
        }

        public bool Equals(ItemId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ItemId other && Equals(other);

        public override int GetHashCode() => (Value ?? string.Empty).GetHashCode();
    }
}
