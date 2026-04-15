using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Single app bootstrapper. Runs automatically after every scene load.
    ///
    /// Responsibilities:
    ///   1. Ensure exactly one MergeService exists (creates a DontDestroyOnLoad host if none).
    ///   2. Assign that service to any MergeInputAdapter whose mergeService field is null.
    ///
    /// AfterSceneLoad timing is intentional: adapters are guaranteed to be present
    /// before this method runs, so FindObjectsByType returns the full set.
    /// </summary>
    public static class BootstrapperAutoAssign
    {
        private static MergeService _shared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMergeService()
        {
            if (_shared == null)
            {
                var host = new GameObject("~MergeService");
                Object.DontDestroyOnLoad(host);
                _shared = host.AddComponent<MergeService>();
            }

            var adapters = Object.FindObjectsByType<MergeInputAdapter>(FindObjectsSortMode.None);
            foreach (var a in adapters)
            {
                if (a != null && a.mergeService == null)
                    a.mergeService = _shared;
            }
        }
    }
}
