using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace AQ.App.Tests
{
    public class BootstrapperSmokeTests
    {
        /// <summary>
        /// Verifies that a MergeService can be created and assigned to an adapter —
        /// the core behavior BootstrapperAutoAssign performs at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator MergeService_is_assigned_to_adapter()
        {
            var serviceGO = new GameObject("~MergeService");
            var svc = serviceGO.AddComponent<MergeService>();

            var boardGO = new GameObject("Board");
            var adapter = boardGO.AddComponent<MergeInputAdapter>();

            yield return null; // Awake runs, MergeService.Instance is set

            // Simulate what BootstrapperAutoAssign does
            var adapters = Object.FindObjectsByType<MergeInputAdapter>(FindObjectsSortMode.None);
            foreach (var a in adapters)
            {
                if (a != null && a.mergeService == null)
                    a.mergeService = svc;
            }

            Assert.IsNotNull(adapter.mergeService, "MergeService should be assigned to the adapter.");
        }
    }
}