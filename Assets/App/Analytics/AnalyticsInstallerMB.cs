using UnityEngine;

namespace AQ.App.Analytics
{
    // Drop this on a boot/root object in your startup scene.
    public sealed class AnalyticsInstallerMB : MonoBehaviour
    {
        [SerializeField] bool installDebugBackend = true;

        void Awake()
        {
            if (!AnalyticsLocator.TryGet(out _))
            {
                if (installDebugBackend) AnalyticsLocator.Set(new DebugLogAnalytics());
            }
        }
    }
}
