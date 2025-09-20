using UnityEngine;

namespace AQ.App.Tests
{
    public static class TestGame
    {
        public static MergeService CreateMergeService(string name = "Test.MergeService")
        {
            var go = new GameObject(name);
            // Keep it simple; cleanup is left to the test via Object.DestroyImmediate(go)
            return go.AddComponent<AQ.App.MergeService>();
        }
    }
}