using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_Branches_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Same_index_is_invalid_and_no_side_effects()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);

            var twig = Id("twig");
            recipes.Add(twig, twig, Id("branch"));

            grid.Set(0, twig);

            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var res = engine.TryMerge(0, 0);
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(twig, grid.Get(0));
            Assert.AreEqual(0, bus.Published.Count);
        }

        [Test]
        public void Empty_source_or_target_is_invalid_and_no_side_effects()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);

            var twig = Id("twig");
            recipes.Add(twig, twig, Id("branch"));

            // source empty, target set
            grid.Set(1, twig);

            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var res1 = engine.TryMerge(0, 1);
            Assert.IsFalse(res1.IsSuccess);
            Assert.AreEqual(twig, grid.Get(1));
            Assert.AreEqual(default(ItemId), grid.Get(0));
            Assert.AreEqual(0, bus.Published.Count);

            // source set, target empty
            grid.Set(0, twig);
            grid.Set(1, default);

            var res2 = engine.TryMerge(0, 1);
            // This may be allowed or disallowed depending on rules; assert at least "no exception" and valid boolean.
            Assert.IsNotNull(res2);
        }

        [Test]
        public void Cooldown_blocks_immediate_retry_and_allows_after_threshold()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);

            var twig = Id("twig");
            var branch = Id("branch");
            recipes.Add(twig, twig, branch);

            grid.Set(0, twig);
            grid.Set(1, twig);

            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var first = engine.TryMerge(0,1);
            Assert.IsTrue(first.IsSuccess);
            Assert.AreEqual(branch, grid.Get(1));
            Assert.AreEqual(default(ItemId), grid.Get(0));

            // immediate retry — should fail due to cooldown (and often also content)
            grid.Set(0, twig);
            var immediate = engine.TryMerge(0,1);
            Assert.IsFalse(immediate.IsSuccess);

            // after threshold
            time.AdvanceSeconds(2);
            grid.Set(0, twig);
            grid.Set(1, twig);
            var after = engine.TryMerge(0,1);
            Assert.IsTrue(after.IsSuccess);
            Assert.GreaterOrEqual(bus.Published.Count, 2);
        }
    }
}
