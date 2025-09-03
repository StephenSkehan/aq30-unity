using AQ.Domain.Merge;
using AQ.SharedKernel;

namespace AQ.App
{
    // Engine-agnostic composition root.
    public sealed class GameComposer
    {
        public IEventBus EventBus { get; }
        public MergeEngine MergeEngine { get; }

        public GameComposer(IRecipeBook recipes, IEventBus bus = null)
        {
            var b = bus ?? new InMemoryEventBus();
            EventBus   = b;
            MergeEngine = new MergeEngine(recipes, b);
        }

        // Handy factory for tests.
        public static GameComposer ForTests(IRecipeBook recipes)
            => new GameComposer(recipes, new InMemoryEventBus());
    }
}
