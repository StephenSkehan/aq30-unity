using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class ItemId_Tests
    {
        [Test]
        public void Equal_when_same_value()
        {
            var a = new ItemId("twig");
            var b = new ItemId("twig");
            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Not_equal_when_different_value()
        {
            var a = new ItemId("twig");
            var b = new ItemId("leaf");
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Default_not_equal_to_named()
        {
            var a = default(ItemId);
            var b = new ItemId("twig");
            Assert.IsFalse(a.Equals(b));
        }
    }
}
