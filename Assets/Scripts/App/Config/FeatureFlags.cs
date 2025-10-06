using UnityEngine;

namespace AQ.App.Config
{
    public enum SpawnRingMode { ManhattanClockwiseFromNorth = 0 }

    [CreateAssetMenu(fileName = "FeatureFlags", menuName = "AQ/Config/Feature Flags")]
    public sealed class FeatureFlags : ScriptableObject
    {
        [Header("Gameplay Systems")]
        public bool EnergySystem = true;                           // ON by default
        public bool FamilyAwareMerges = true;                      // ON by default
        public bool InventoryPlacement_RowMajorFirstEmpty = true;  // ON by default

        [Header("Spawn Placement")]
        public SpawnRingMode SpawnRingTraversal = SpawnRingMode.ManhattanClockwiseFromNorth;

        [Header("QA Determinism")]
        public bool QA_DeterministicSeeds = false;  // player builds = OFF
        public bool QA_PersistSeedInSave = false;   // optional repro
    }

    /// <summary>Runtime access set by ConfigInstaller at boot.</summary>
    public static class FeatureFlagsRuntime
    {
        public static FeatureFlags Current;
    }
}
