using System.Collections.Generic;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class ItemId_MoreTests
    {
        [Test]
        public void ToString_returns_original_value()
        {
            var id = new ItemId("twig");
            Assert.AreEqual("twig", id.ToString());
        }

        [Test]
        public void Equals_object_works_with_same_value()
        {
            var a = new ItemId("twig");
            object b = new ItemId("twig");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Works_as_dictionary_key_with_value_equality()
        {
            var a = new ItemId("twig");
            var b = new ItemId("twig");
            var map = new Dictionary<ItemId, int>();
            map[a] = 7;
            Assert.AreEqual(7, map[b], "Different instances with same value should hash/compare equal");
        }

        [Test]
        public void Default_ToString_is_safe()
        {
            var d = default(ItemId);
            Assert.DoesNotThrow(() => { var _ = d.ToString(); });
        }
    }
}
