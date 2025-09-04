using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class Grid_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Unset_cell_returns_default()
        {
            var g = new Grid();
            Assert.AreEqual(default(ItemId), g.Get(42));
        }

        [Test]
        public void Set_then_get_roundtrips_value()
        {
            var g = new Grid();
            var twig = Id("twig");
            g.Set(3, twig);
            Assert.AreEqual(twig, g.Get(3));
        }

        [Test]
        public void Overwrite_replaces_previous_value()
        {
            var g = new Grid();
            g.Set(1, Id("twig"));
            g.Set(1, Id("branch"));
            Assert.AreEqual(Id("branch"), g.Get(1));
        }

        [Test]
        public void Contains_true_when_any_cell_matches()
        {
            var g = new Grid();
            var leaf = Id("leaf");
            g.Set(0, Id("twig"));
            g.Set(5, leaf);
            Assert.IsTrue(g.Contains(leaf));
        }

        [Test]
        public void Contains_false_when_no_cell_matches()
        {
            var g = new Grid();
            g.Set(0, Id("twig"));
            Assert.IsFalse(g.Contains(Id("rock")));
        }
    }
}
