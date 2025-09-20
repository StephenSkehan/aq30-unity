using NUnit.Framework;
using UnityEngine;
using AQ.App;

public class Sanity_EditMode_Tests
{
    [Test]
    public void MergeBoard_compiles_and_spawns()
    {
        var go = new GameObject("Board");
        var board = go.AddComponent<MergeBoardView>();
        var item = board.SpawnItem(new Vector2Int(0,0), "A");
        Assert.IsNotNull(item);
    }
}
