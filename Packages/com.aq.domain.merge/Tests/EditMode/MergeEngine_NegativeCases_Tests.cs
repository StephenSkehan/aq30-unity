using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_NegativeCases_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Default_cells_no_recipe_results_in_failure_and_no_publish()
        {
            var grid = new Grid();              // both source/target default
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());

            var res = engine.TryMerge(10, 11);  // arbitrary indices reading default values
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(0, bus.Published.Count);
            Assert.AreEqual(default(ItemId), grid.Get(10));
            Assert.AreEqual(default(ItemId), grid.Get(11));
        }

        [Test]
        public void Dissimilar_items_without_recipe_fail_and_do_not_change_grid()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());

            var a = Id("stone");
            var b = Id("twig");
            grid.Set(0, a);
            grid.Set(1, b);

            var res = engine.TryMerge(0, 1);
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(a, grid.Get(0));
            Assert.AreEqual(b, grid.Get(1));
            Assert.AreEqual(0, bus.Published.Count);
        }
    }
}
