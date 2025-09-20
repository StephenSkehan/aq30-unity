using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace AQ.App.Tests
{
    public class BootstrapperSmokeTests
    {
        [UnityTest]
        public IEnumerator Bootstrapper_assigns_service_if_adapter_found()
        {
            var go = new GameObject("Bootstrap");
            var boot = go.AddComponent<GameBootstrapper>();

            var boardGO = new GameObject("Board");
            var adapter = boardGO.AddComponent<MergeInputAdapter>();

            yield return null; // Awake runs

            Assert.IsNotNull(adapter.mergeService, "Bootstrapper should assign a MergeService by default.");
        }
    }
}