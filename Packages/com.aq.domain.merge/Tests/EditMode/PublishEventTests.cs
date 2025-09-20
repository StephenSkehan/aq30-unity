using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    [TestFixture]
    public sealed class PublishEventTests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Valid_merge_publishes_a_MergePerformed_event()
        {
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var engine = new MergeEngine(new Grid(), recipes, bus, new FixedRandom(0.5), new FixedTime());

            var twig = Id("twig");
            var branch = Id("branch");
            recipes.Add(twig, twig, branch);

            var res = engine.TryMerge(twig, twig);

            Assert.IsTrue(res.IsSuccess, res.Error);
            Assert.AreEqual(1, bus.Published.Count, "Expected exactly one event to be published.");
        }
    }
}
