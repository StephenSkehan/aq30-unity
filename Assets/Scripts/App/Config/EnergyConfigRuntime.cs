using UnityEngine;

namespace AQ.App.Config
{
    /// <summary>
    /// Simple runtime holder for the active EnergyConfig ScriptableObject.
    /// Mirrors FeatureFlagsRuntime pattern.
    /// </summary>
    public static class EnergyConfigRuntime
    {
        public static EnergyConfig Current { get; set; }
    }
}
