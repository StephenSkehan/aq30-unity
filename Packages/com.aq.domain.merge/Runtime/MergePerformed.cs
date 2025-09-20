using AQ.SharedKernel.Events;

namespace AQ.Domain.Merge
{
    // Event payload published when a merge succeeds.
    // Kept minimal to stay compatible with EventHelpers.TryCreateMergePerformed(...)
    public readonly struct MergePerformed : IGameEvent
    {
        public ItemId A { get; }
        public ItemId B { get; }
        public ItemId Result { get; }

        public MergePerformed(ItemId a, ItemId b, ItemId result)
        {
            A = a;
            B = b;
            Result = result;
        }

        public override string ToString() => $"MergePerformed {A}+{B}→{Result}";
    }
}