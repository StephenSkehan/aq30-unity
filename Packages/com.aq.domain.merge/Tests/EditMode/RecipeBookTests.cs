using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public class RecipeBookTests
    {
        [Test]
        public void Resolves_Symmetric_Pairs()
        {
            var rb = new RecipeBook();
            rb.AddRecipe(new ItemId("A"), new ItemId("B"), new ItemId("C"));

            ItemId result;
            Assert.IsTrue(rb.TryGetResult(new ItemId("A"), new ItemId("B"), out result));
            Assert.AreEqual("C", result.Value);

            Assert.IsTrue(rb.TryGetResult(new ItemId("B"), new ItemId("A"), out result));
            Assert.AreEqual("C", result.Value);
        }

        [Test]
        public void Unknown_Pair_ReturnsFalse()
        {
            var rb = new RecipeBook();
            ItemId result;
            Assert.IsFalse(rb.TryGetResult(new ItemId("X"), new ItemId("Y"), out result));
        }
    }
}
