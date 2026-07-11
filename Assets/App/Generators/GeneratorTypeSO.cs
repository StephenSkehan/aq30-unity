using System;
using UnityEngine;

namespace AQ.App.Generators
{
    public enum DropType { Item, SubGenerator }

    [Serializable]
    public struct DropEntry
    {
        public DropType type;
        [Tooltip("Item family id (e.g. 'helen'). Ignored for SubGenerator.")]
        public string itemFamily;
        [Tooltip("0-based tier of item to spawn. Ignored for SubGenerator.")]
        public int itemTier;
        [Min(0)] public float weight;
        [Tooltip("Optional: entry only eligible when this DialogueFlag is set. Leave empty for always-eligible.")]
        public string requiresStoryFlag;
    }

    [Serializable]
    public struct GeneratorTierParticleConfig
    {
        public float emissionRate;
        public Color startColor;
        public Color endColor;
        public float startSize;
    }

    [Serializable]
    public class GeneratorTierConfig
    {
        public DropEntry[] dropTable;
        public GeneratorTierParticleConfig particles;
    }

    [CreateAssetMenu(menuName = "AQ/Generator Type", fileName = "GeneratorType_New")]
    public class GeneratorTypeSO : ScriptableObject
    {
        [Tooltip("Must be unique. Used as the tile family key for merge rules — two generators of the same type can merge.")]
        public string generatorTypeId;
        public string displayName;

        [Tooltip("Once any generator of this type reaches this tier, sub-gen drops are suppressed for the whole type.")]
        [Min(1)] public int maxGeneratorTier = 5;

        [Tooltip("One entry per tier (index 0 = T1). Contains drop table + particle config.")]
        public GeneratorTierConfig[] tiers;

        [Tooltip("Sprites per tier (index 0 = T1). Clamps to last entry when tier exceeds length.")]
        public Sprite[] generatorSprites;

        public Sprite SpriteForTier(int tier)
        {
            if (generatorSprites == null || generatorSprites.Length == 0) return null;
            return generatorSprites[Mathf.Clamp(tier, 0, generatorSprites.Length - 1)];
        }

        public GeneratorTierConfig ConfigForTier(int tier)
        {
            if (tiers == null || tiers.Length == 0) return null;
            return tiers[Mathf.Clamp(tier, 0, tiers.Length - 1)];
        }
    }
}
