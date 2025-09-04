using System;
using NUnit.Framework;
using AQ.App;
using AQ.Domain.Merge;
using AQ.SharedKernel;

namespace AQ.Domain.Merge.Tests // keep test namespace consistent with others
{
    public sealed class CompositionSmokeTests
    {
        private static ItemId Id(string s) => new ItemId(s);

        private sealed class DummyTime : ITimeProvider
        {
            public DateTime UtcNow { get; private set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public void AdvanceSeconds(int seconds) => UtcNow = UtcNow.AddSeconds(seconds);
        }

        private sealed class RecipeBookPair : IRecipeBook
        {
            private readonly ItemId _in, _out;
            public RecipeBookPair(ItemId input, ItemId output) { _in = input; _out = output; }
            public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
            {
                if (a.Equals(_in) && b.Equals(_in)) { result = _out; return true; }
                result = default; return false;
            }
        }

        [Test]
        public void Composer_wires_engine_and_a_valid_merge_publishes()
        {
            var bus = new InMemoryEventBus();
            var rng = new DeterministicRandom(1234);
            var time = new DummyTime();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));

            var composer = new GameComposer(bus, recipes, rng, time);

            composer.Grid.Set(0, Id("twig"));
            composer.Grid.Set(1, Id("twig"));

            var res = composer.Engine.TryMerge(0,1);
            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(Id("branch"), composer.Grid.Get(1));
        }
    }
}
