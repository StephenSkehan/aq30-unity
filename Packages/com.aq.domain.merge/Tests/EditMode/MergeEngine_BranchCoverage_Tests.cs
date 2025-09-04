using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_BranchCoverage_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Merge_same_index_is_invalid()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var rng = new FixedRandom(0.5);
            var time = new FixedTime();
            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            grid.Set(1, Id("twig"));
            var res = engine.TryMerge(1, 1);

            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(Id("twig"), grid.Get(1));
            Assert.AreEqual(0, bus.Published.Count);
        }

        [Test]
        public void Merge_when_source_empty_is_invalid_and_no_change()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var rng = new FixedRandom(0.5);
            var time = new FixedTime();
            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var leaf = Id("leaf");
            grid.Set(0, default(ItemId));   // empty source
            grid.Set(1, leaf);              // non-empty target

            var res = engine.TryMerge(0, 1);

            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(leaf, grid.Get(1));
            Assert.AreEqual(0, bus.Published.Count);
        }

        [Test]
        public void Cooldown_edge_requires_full_interval_before_next_merge()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var rng = new FixedRandom(0.5);
            var time = new FixedTime();

            var twig = Id("twig");
            var branch = Id("branch");
            recipes.Add(twig, twig, branch);

            // First successful merge 0 -> 1
            grid.Set(0, twig);
            grid.Set(1, twig);
            var engine = new MergeEngine(grid, recipes, bus, rng, time);
            var first = engine.TryMerge(0, 1);
            Assert.IsTrue(first.IsSuccess);

            // Immediately try again at half cooldown (assume 2s cooldown as used elsewhere)
            grid.Set(0, twig);
            grid.Set(1, twig);
            time.AdvanceSeconds(1);
            var second = engine.TryMerge(0, 1);
            Assert.IsFalse(second.IsSuccess, "Cooldown should still block at 1s if interval is 2s");

            // Advance the remaining time, then it should pass
            time.AdvanceSeconds(1);
            grid.Set(0, twig);
            grid.Set(1, twig);
            var third = engine.TryMerge(0, 1);
            Assert.IsTrue(third.IsSuccess);
        }
    }
}
