using UnityEngine;

namespace AQ.App.Gameplay.Merge
{
    [CreateAssetMenu(fileName = "MergeGeneratorConfig", menuName = "AQ/Merge/Generator Config")]
    public class MergeGeneratorConfig : ScriptableObject
    {
        [Min(1)] public int width = 5;
        [Min(1)] public int height = 5;
        [Min(1)] public int maxTier = 6;

        [Tooltip("Power spent per spawn")]
        [Min(0)] public int spawnCost = 1;

        [Tooltip("Weights for T1..Tmax (array size should match maxTier).")]
        public int[] weights = new int[] { 75, 15, 7, 2, 1, 0 };

        public int NextTier(System.Random rng)
        {
            if (weights == null || weights.Length == 0) return 1;
            int len = Mathf.Min(weights.Length, Mathf.Max(1, maxTier));
            int sum = 0; for (int i = 0; i < len; i++) sum += Mathf.Max(0, weights[i]);
            if (sum <= 0) return 1;

            int r = rng.Next(sum); // 0..sum-1
            int acc = 0;
            for (int t = 0; t < len; t++)
            {
                acc += Mathf.Max(0, weights[t]);
                if (r < acc) return Mathf.Clamp(t + 1, 1, maxTier);
            }
            return 1;
        }
    }
}
