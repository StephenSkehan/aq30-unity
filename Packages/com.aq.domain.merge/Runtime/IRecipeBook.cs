namespace AQ.Domain.Merge
{
    public interface IRecipeBook
    {
        bool TryGetResult(ItemId a, ItemId b, out ItemId result);
    }
}
