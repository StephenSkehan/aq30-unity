using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Minimal bootstrap that the tests expect:
    /// - Ensure a MergeService exists
    /// - After one frame, assign that service to any MergeInputAdapter with a null mergeService
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private MergeService _svc;

        void Awake()
        {
            // Prefer an existing service if one already registered an Instance
            _svc = MergeService.Instance;

            // If none yet, create our own and hold the component reference directly
            if (_svc == null)
            {
                var host = new GameObject("~MergeService (Bootstrap)");
                DontDestroyOnLoad(host);
                _svc = host.AddComponent<MergeService>();
            }
        }

        void Start()
        {
            // One frame later: adapters should exist and MergeService.Awake has run.
            AssignAdapters();
        }

        private void AssignAdapters()
        {
            if (_svc == null) _svc = MergeService.Instance;

            var adapters = Object.FindObjectsByType<MergeInputAdapter>(FindObjectsSortMode.None);
            foreach (var a in adapters)
            {
                if (a != null && a.mergeService == null)
                    a.mergeService = _svc;
            }
        }
    }
}
