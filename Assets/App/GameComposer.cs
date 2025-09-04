namespace AQ.App
{
    using AQ.Domain.Merge;
    using AQ.SharedKernel;

    // App-layer composition that wires domain types using provided dependencies.
    public sealed class GameComposer
    {
        public IEventBus Bus { get; }
        public IRecipeBook Recipes { get; }
        public Grid Grid { get; }
        public MergeEngine Engine { get; }

        public GameComposer(IEventBus bus, IRecipeBook recipes, IRandom rng, ITimeProvider time)
        {
            Bus = bus;
            Recipes = recipes;
            Grid = new Grid();
            Engine = new MergeEngine(Grid, Recipes, Bus, rng, time);
        }
    }
}
