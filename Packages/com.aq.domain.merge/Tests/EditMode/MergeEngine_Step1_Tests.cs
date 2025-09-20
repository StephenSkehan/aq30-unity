using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_Step1_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Valid_merge_updates_target_and_publishes_event()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();

            var twig = Id("twig");
            var branch = Id("branch");

            recipes.Add(twig, twig, branch);
            grid.Set(0, twig);
            grid.Set(1, twig);

            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());
            var result = engine.TryMerge(0, 1);

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(branch, grid.Get(1));
            Assert.AreEqual(1, bus.Published.Count);
        }

        [Test]
        public void Invalid_when_different_items_does_not_publish_or_change()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();

            var twig = Id("twig");
            var leaf = Id("leaf");
            recipes.Add(twig, twig, Id("branch"));

            grid.Set(0, twig);
            grid.Set(1, leaf);

            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());
            var result = engine.TryMerge(0, 1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(0, bus.Published.Count);
            Assert.AreEqual(leaf, grid.Get(1));
        }

        [Test]
        public void Invalid_when_no_recipe_does_not_publish_or_change()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var stone = Id("stone");
            grid.Set(0, stone);
            grid.Set(1, stone);

            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());
            var result = engine.TryMerge(0, 1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(0, bus.Published.Count);
            Assert.AreEqual(stone, grid.Get(1));
        }
    }
}
