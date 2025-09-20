using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_RepeatedMerges_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Two_valid_merges_then_an_invalid_one_exercises_paths()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);

            var twig = Id("twig");
            var branch = Id("branch");
            recipes.Add(twig, twig, branch);
            recipes.Add(branch, branch, Id("tree"));

            // First merge: twig+twig -> branch
            grid.Set(0, twig);
            grid.Set(1, twig);
            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            var r1 = engine.TryMerge(0,1);
            Assert.IsTrue(r1.IsSuccess);
            Assert.AreEqual(branch, grid.Get(1));
            Assert.AreEqual(default(ItemId), grid.Get(0));

            // Cooldown still active; try immediately (should fail)
            grid.Set(0, branch);
            var rFailCooldown = engine.TryMerge(0,1);
            Assert.IsFalse(rFailCooldown.IsSuccess);

            // Advance time past cooldown and merge branch+branch -> tree
            time.AdvanceSeconds(2);
            var r2 = engine.TryMerge(0,1);
            Assert.IsTrue(r2.IsSuccess);
            Assert.AreEqual(Id("tree"), grid.Get(1));
            Assert.AreEqual(default(ItemId), grid.Get(0));

            // Now put dissimilar items -> invalid merge
            grid.Set(0, twig);
            grid.Set(1, Id("rock"));
            var r3 = engine.TryMerge(0,1);
            Assert.IsFalse(r3.IsSuccess);

            // We published only on the two successes
            Assert.GreaterOrEqual(bus.Published.Count, 2);
        }
    }
}
