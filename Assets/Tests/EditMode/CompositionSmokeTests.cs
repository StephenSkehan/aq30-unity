using AQ.App;
using AQ.Domain.Merge;
using NUnit.Framework;

namespace AQ.App.Tests
{
    public sealed class CompositionSmokeTests
    {
        [Test]
        public void Build_returns_non_null_components_and_engine_is_operational()
        {
            var c = GameComposer.Build(); // empty recipes by default
            Assert.IsNotNull(c);
            Assert.IsNotNull(c.Engine);
            Assert.IsNotNull(c.Grid);

            // With empty recipes, merges should fail safely without throwing
            c.Grid.Set(0, new ItemId("twig"));
            c.Grid.Set(1, new ItemId("twig"));
            var res = c.Engine.TryMerge(0, 1);
            Assert.IsFalse(res.IsSuccess);
        }
    }
}
