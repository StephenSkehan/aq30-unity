using System;
using NUnit.Framework;
using AQ.Domain.Merge;
using AQ.SharedKernel;

namespace AQ.Domain.Merge.Tests
{
    public sealed class CompositionSmokeTests
    {
        private static ItemId Id(string s) => new ItemId(s);

        // Minimal time provider used only for this smoke
        private sealed class DummyTime : ITimeProvider
        {
            public DateTime UtcNow { get; private set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public void AdvanceSeconds(int seconds) => UtcNow = UtcNow.AddSeconds(seconds);
        }

        // Minimal recipe book: merges identical input -> specific output
        private sealed class RecipeBookPair : IRecipeBook
        {
            private readonly ItemId _input, _output;
            public RecipeBookPair(ItemId input, ItemId output) { _input = input; _output = output; }
            public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
            {
                if (a.Equals(_input) && b.Equals(_input)) { result = _output; return true; }
                result = default; return false;
            }
        }

        // Local composer used only by this test (no separate assembly needed)
        private sealed class Composer
        {
            public IEventBus Bus { get; }
            public IRecipeBook Recipes { get; }
            public Grid Grid { get; }
            public MergeEngine Engine { get; }

            public Composer(IEventBus bus, IRecipeBook recipes, IRandom rng, ITimeProvider time)
            {
                Bus = bus;
                Recipes = recipes;
                Grid = new Grid();
                Engine = new MergeEngine(Grid, Recipes, Bus, rng, time);
            }
        }

        [Test]
        public void Composer_wires_engine_and_a_valid_merge_publishes()
        {
            var bus = new InMemoryEventBus();
            var rng = new DeterministicRandom(1234);
            var time = new DummyTime();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));

            var composer = new Composer(bus, recipes, rng, time);

            composer.Grid.Set(0, Id("twig"));
            composer.Grid.Set(1, Id("twig"));

            var res = composer.Engine.TryMerge(0, 1);
            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(Id("branch"), composer.Grid.Get(1));
        }
    }
}
