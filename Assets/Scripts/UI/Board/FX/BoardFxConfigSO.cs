// Assets/Scripts/UI/Board/FX/BoardFxConfigSO.cs
using UnityEngine;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Tunables for merge-board feedback (animations + audio + optional sparkle).
    /// Create via: Assets > Create > AQ > BoardFxConfig
    /// </summary>
    [CreateAssetMenu(fileName = "BoardFxConfig", menuName = "AQ/BoardFxConfig", order = 10)]
    public sealed class BoardFxConfigSO : ScriptableObject
    {
        [Header("Timings (seconds)")]
        [Min(0.01f)] public float spawnPopDuration = 0.12f;
        [Min(0.01f)] public float mergePopDuration = 0.12f;
        [Min(0.01f)] public float swapSlideDuration = 0.12f;

        [Header("Scales")]
        [Min(0.1f)] public float spawnStartScale = 0.85f;
        [Min(1.0f)] public float popPeakScale = 1.15f;

        [Header("Shake (optional, not used yet)")]
        [Min(0.01f)] public float invalidShakeDuration = 0.20f;
        [Min(0f)]    public float invalidShakeMagnitude = 10f;

        [Header("VFX (optional)")]
        public ParticleSystem sparklePrefab;
        [Tooltip("If true, the sparkle prefab is UI-space (RectTransform) and will be parented under the overlay. If false, it's a world-space ParticleSystem under the board's Canvas.")]
        public bool sparkleIsUI = true;
        public float sparkleLifetime = 0.7f;
    }
}
