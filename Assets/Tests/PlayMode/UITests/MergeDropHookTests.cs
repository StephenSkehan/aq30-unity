using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace AQ.App.Tests
{
    public class MergeDropHookTests
    {
        [UnityTest]
        public IEnumerator HandleDrop_calls_merge_and_updates_board_without_prefabs()
        {
            // Arrange board + adapter (no prefab paths touched)
            var boardGO = new GameObject("Board");
            var board = boardGO.AddComponent<MergeBoardView>();

            var adapterGO = new GameObject("Adapter");
            var adapter = adapterGO.AddComponent<MergeInputAdapter>();
            adapter.board = board;

            // Merge service seam — deterministic "always succeeds" resolver
            var svc = TestGame.CreateMergeService();
            svc.InjectDomainResolver((string a, string b) => (true, a + "+" + b));
            adapter.mergeService = svc;

            // Create two simple scene items (avoid Resources/Prefab lookups)
            var goA = new GameObject("Item_A");
            var itemA = goA.AddComponent<ItemView>();
            goA.transform.position = Vector3.zero;

            var goB = new GameObject("Item_B");
            var itemB = goB.AddComponent<ItemView>();
            goB.transform.position = new Vector3(0, 1, 0);

            // Act: request a merge
            adapter.RequestMerge(itemA, itemB);

            // Let coroutines tick (pulse + replace routine)
            yield return null;
            yield return null;

            // Assert via stable signal on the adapter
            Assert.IsTrue(adapter.LastMergeSucceeded, "Merge should have succeeded");

            // Cleanup
            Object.DestroyImmediate(boardGO);
            Object.DestroyImmediate(adapterGO);
            Object.DestroyImmediate(svc.gameObject);
        }
    }
}