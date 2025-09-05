using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_PathDrive_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Sequence_of_merges_and_invalids_across_multiple_indices()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.42);

            var twig = Id("twig");
            var branch = Id("branch");
            var tree = Id("tree");
            recipes.Add(twig, twig, branch);
            recipes.Add(branch, branch, tree);

            var eng = new MergeEngine(grid, recipes, bus, rng, time);

            // V: 0->1 (twig+twig -> branch)
            grid.Set(0, twig); grid.Set(1, twig);
            Assert.IsTrue(eng.TryMerge(0,1).IsSuccess);
            Assert.AreEqual(branch, grid.Get(1));
            Assert.AreEqual(default(ItemId), grid.Get(0));

            // I: dissimilar 1->2 (branch vs rock)
            grid.Set(2, Id("rock"));
            Assert.IsFalse(eng.TryMerge(1,2).IsSuccess);
            Assert.AreEqual(branch, grid.Get(1));
            Assert.AreEqual(Id("rock"), grid.Get(2));

            // I: cooldown immediate 1->3
            grid.Set(3, twig);
            Assert.IsFalse(eng.TryMerge(1,3).IsSuccess);

            // After cooldown, prepare branch+branch then merge to tree
            time.AdvanceSeconds(2);
            grid.Set(3, branch);
            Assert.IsTrue(eng.TryMerge(1,3).IsSuccess);
            Assert.AreEqual(tree, grid.Get(3));
            Assert.AreEqual(default(ItemId), grid.Get(1));

            Assert.GreaterOrEqual(bus.Published.Count, 2);
        }
    }
}
