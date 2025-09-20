using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// After scene load, ensure every MergeInputAdapter has a MergeService.
    /// Creates one shared MergeService (DontDestroyOnLoad) and assigns it to any null adapters.
    /// </summary>
    public static class BootstrapperAutoAssign
    {
        private static MergeService _shared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMergeService()
        {
            // Lazily create a single shared service
            if (_shared == null)
            {
                var host = new GameObject("~DefaultMergeService");
                Object.DontDestroyOnLoad(host);
                _shared = host.AddComponent<MergeService>();
            }

            // New API to avoid obsolete warnings
            var adapters = Object.FindObjectsByType<MergeInputAdapter>(FindObjectsSortMode.None);
            foreach (var a in adapters)
            {
                if (a != null && a.mergeService == null)
                    a.mergeService = _shared;
            }
        }
    }
}
