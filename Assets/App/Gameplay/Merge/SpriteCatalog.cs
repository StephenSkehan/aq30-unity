using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Gameplay.Merge
{
    [CreateAssetMenu(fileName = "SpriteCatalog", menuName = "AQ/Merge/Sprite Catalog")]
    public class SpriteCatalog : ScriptableObject
    {
        [Tooltip("Index 0 unused; tiers start at 1. Size should be >= maxTier + 1.")]
        public List<Sprite> byTier = new List<Sprite>();

        public Sprite ForTier(int tier)
        {
            if (tier <= 0) return null;
            int idx = tier;
            if (byTier == null || byTier.Count == 0) return null;
            if (idx < 0 || idx >= byTier.Count) idx = Mathf.Clamp(idx, 0, byTier.Count - 1);
            return byTier[idx];
        }
    }
}
