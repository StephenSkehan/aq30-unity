using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Ensures a MergeService is available, and assigns it to any MergeInputAdapter at boot.
    /// Safe, minimal, and test-friendly. No domain wiring here.
    /// </summary>
    public sealed class BootstrapperAutoAdd : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EnsureBootstrapper()
        {
            var holder = new GameObject("~BootstrapperAutoAdd");
            holder.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            holder.AddComponent<BootstrapperAutoAdd>();
            Object.DontDestroyOnLoad(holder);
        }

        void Awake()
        {
            // If a MergeService exists, use it; else create one.
            var svc = AQ.App.MergeService.Instance;
            if (svc == null)
            {
                var host = new GameObject("~MergeServiceStubHost");
                host.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                Object.DontDestroyOnLoad(host);
                svc = host.AddComponent<AQ.App.MergeService>();
            }

            // Assign to any adapter that’s missing a service.
            var adapters = Object.FindObjectsByType<AQ.App.MergeInputAdapter>(FindObjectsSortMode.None);
            foreach (var a in adapters)
            {
                if (a != null && a.mergeService == null)
                    a.mergeService = svc;
            }
        }
    }
}
