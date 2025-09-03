using NUnit.Framework;
using AQ.Domain.Merge;

public class MergeEngineSmokeTests
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
    public void Construct_And_TryMerge_DoesNotThrow()
    {
        var engine = new MergeEngine(new StubRecipes());
        Assert.DoesNotThrow(() => engine.TryMerge(new ItemId("a"), new ItemId("b"), out _));
    }
}
