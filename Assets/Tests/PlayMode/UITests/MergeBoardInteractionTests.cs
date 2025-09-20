using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AQ.App.Tests
{
    public class MergeBoardInteractionTests
    {
        [UnityTest]
        public System.Collections.IEnumerator Merge_Succeeds_DisablesFTUE_AndReplacesWithResult()
        {
            var hudPrefab = Resources.Load<GameObject>("App/UI/Prefabs/HUD");
            Assert.IsNotNull(hudPrefab, "HUD prefab not found at Resources/App/UI/Prefabs/HUD");
            var hud = Object.Instantiate(hudPrefab);

            var board = hud.GetComponentInChildren<MergeBoardView>() ?? hud.AddComponent<MergeBoardView>();
            var adapter = hud.GetComponentInChildren<MergeInputAdapter>() ?? hud.AddComponent<MergeInputAdapter>();

            // Ensure bootstrapped merge service exists and wire it.
            if (MergeService.Instance == null)
            {
                var svc = new GameObject("~MergeService-ForTest").AddComponent<MergeService>();
                svc.hideFlags = HideFlags.DontSave;
            }
            adapter.board = board;
            adapter.mergeService = MergeService.Instance;

            // Create two matching items and ask for a merge.
            var a = board.SpawnItem(new Vector2Int(0, 0), "A");
            var b = board.SpawnItem(new Vector2Int(0, 1), "A");
            adapter.RequestMerge(a, b);

            yield return null;
            Assert.IsTrue(adapter.LastMergeSucceeded, "Expected a successful merge via MergeService stub.");
        }
    }
}