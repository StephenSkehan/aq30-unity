namespace AQ.Domain.Merge
{
    public struct MergePerformed
    {
        public ItemId A { get; }
        public ItemId B { get; }
        public ItemId Result { get; }

        public MergePerformed(ItemId a, ItemId b, ItemId result)
        {
            A = a; B = b; Result = result;
        }
    }
}
