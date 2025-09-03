using NUnit.Framework;
using AQ.App;
using AQ.Domain.Merge;
using AQ.SharedKernel;

public class CompositionSmokeTests
{
    private sealed class StubRecipes : IRecipeBook
    {
        public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
        { result = new ItemId("merged"); return true; }
    }

    [Test]
    public void Composer_Wires_Engine_And_Bus()
    {
        var composer = GameComposer.ForTests(new StubRecipes());

        MergePerformed seen = default;
        composer.EventBus.Subscribe<MergePerformed>(e => seen = e);

        ItemId r;
        var ok = composer.MergeEngine.TryMerge(new ItemId("a"), new ItemId("b"), out r);

        Assert.IsTrue(ok);
        Assert.AreEqual("merged", r.Value);
        Assert.AreEqual("a", seen.A.Value);
        Assert.AreEqual("b", seen.B.Value);
        Assert.AreEqual("merged", seen.Result.Value);
    }
}
