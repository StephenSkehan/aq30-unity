using UnityEngine;
using AQ.App.Config;

namespace AQ.App.Bootstrap
{
    /// <summary>
    /// Scene-level installer. Drag this onto any boot object in the canonical scene
    /// and assign the FeatureFlags asset. Does not modify existing controllers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ConfigInstaller : MonoBehaviour
    {
        public FeatureFlags FeatureFlags;

        void Awake()
        {
            FeatureFlagsRuntime.Current = FeatureFlags;
        }
    }
}
