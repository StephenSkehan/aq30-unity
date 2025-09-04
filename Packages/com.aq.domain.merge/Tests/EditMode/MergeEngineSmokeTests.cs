using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    [TestFixture]
    public sealed class MergeEngineSmokeTests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Smoke_ItemOverload_merges_and_publishes()
        {
            var bus = new TestBus();
            var engine = new MergeEngine(new Grid(), new RecipeBookPair(Id("twig"), Id("branch")), bus, new FixedRandom(0.5), new FixedTime());

            var res = engine.TryMerge(Id("twig"), Id("twig"));

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(1, bus.Count);
            Assert.AreEqual(Id("branch"), res.Value);
        }

        [Test]
        public void Smoke_GridIndices_merge_updates_cells()
        {
            var grid = new Grid();
            var bus = new TestBus();
            var recipes = new RecipeBookPair(Id("twig"), Id("branch"));
            var engine = new MergeEngine(grid, recipes, bus, new FixedRandom(0.5), new FixedTime());

            grid.Set(0, Id("twig"));
            grid.Set(1, Id("twig"));

            var res = engine.TryMerge(0, 1);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(default(ItemId), grid.Get(0));
            Assert.AreEqual(Id("branch"), grid.Get(1));
            Assert.AreEqual(1, bus.Count);
        }
    }
}
