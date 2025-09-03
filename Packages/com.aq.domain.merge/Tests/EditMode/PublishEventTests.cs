using NUnit.Framework;
using AQ.Domain.Merge;
using AQ.SharedKernel;

public class PublishEventTests
{
    private sealed class StubRecipes : IRecipeBook
    {
        public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
        {
            result = new ItemId("merged");
            return true;
        }
    }

    [Test]
    public void TryMerge_Publishes_MergePerformed()
    {
        var bus = new InMemoryEventBus();
        MergePerformed received = default;
        bus.Subscribe<MergePerformed>(e => received = e);

        var engine = new MergeEngine(new StubRecipes(), bus);

        ItemId outId;
        var ok = engine.TryMerge(new ItemId("a"), new ItemId("b"), out outId);

        Assert.IsTrue(ok);
        Assert.AreEqual("merged", outId.Value);
        Assert.AreEqual("a", received.A.Value);
        Assert.AreEqual("b", received.B.Value);
        Assert.AreEqual("merged", received.Result.Value);
    }
}
