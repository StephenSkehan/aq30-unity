using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_Step2_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Merge_clears_source_and_updates_target()
        {
            var grid = new Grid();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);
            var bus = new TestBus();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));

            grid.Set(0, Id("twig"));
            grid.Set(1, Id("twig"));

            var engine = new MergeEngine(grid, recipes, bus, rng, time);
            var res = engine.TryMerge(0,1);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(default(ItemId), grid.Get(0));
            Assert.AreEqual(Id("branch"), grid.Get(1));
            Assert.AreEqual(1, bus.Count);
        }

        [Test]
        public void Merge_blocked_if_on_cooldown_then_succeeds_after_time()
        {
            var grid = new Grid();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);
            var bus = new TestBus();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));

            grid.Set(0, Id("twig"));
            grid.Set(1, Id("twig"));
            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var first = engine.TryMerge(0,1);
            Assert.IsTrue(first.IsSuccess);

            grid.Set(0, Id("twig"));
            grid.Set(1, Id("twig"));
            var second = engine.TryMerge(0,1);
            Assert.IsFalse(second.IsSuccess);

            time.AdvanceSeconds(2);
            grid.Set(0, Id("twig"));
            grid.Set(1, Id("twig"));
            var third = engine.TryMerge(0,1);
            Assert.IsTrue(third.IsSuccess);
        }

        [Test]
        public void Same_seed_and_time_produces_identical_results()
        {
            var g1 = new Grid(); var g2 = new Grid();
            var t1 = new FixedTime(); var t2 = new FixedTime();
            var r1 = new FixedRandom(0.42); var r2 = new FixedRandom(0.42);
            var b1 = new TestBus(); var b2 = new TestBus();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));

            g1.Set(0, Id("twig")); g1.Set(1, Id("twig"));
            g2.Set(0, Id("twig")); g2.Set(1, Id("twig"));

            var e1 = new MergeEngine(g1, recipes, b1, r1, t1);
            var e2 = new MergeEngine(g2, recipes, b2, r2, t2);

            var x1 = e1.TryMerge(0,1);
            var x2 = e2.TryMerge(0,1);

            Assert.AreEqual(x1.Value, x2.Value);
        }
    }
}
