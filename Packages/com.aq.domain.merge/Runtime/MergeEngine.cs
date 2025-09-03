using AQ.SharedKernel;

namespace AQ.Domain.Merge
{
    public sealed class MergeEngine
    {
        private readonly IRecipeBook _recipes;
        private readonly IEventBus _bus;

        public MergeEngine(IRecipeBook recipes, IEventBus bus)
        {
            _recipes = recipes ?? throw new System.ArgumentNullException("recipes");
            _bus = bus; // bus may be null in tests if you want; it's okay
        }

        public bool TryMerge(ItemId a, ItemId b, out ItemId result)
        {
            if (_recipes.TryGetResult(a, b, out result))
            {
                if (_bus != null) _bus.Publish(new MergePerformed(a, b, result));
                return true;
            }
            result = default;
            return false;
        }
    }
}
