using AQ.App;   // NarrativeFlags
using UnityEngine;

namespace AQ.App.Generators
{
    public static class DropRoller
    {
        /// <summary>
        /// Weighted roll against the drop table for the given generator type at the given tier.
        /// Returns null only if no eligible entries exist (all story-gated and flags unset, or all
        /// sub-gen entries and sub-gen is locked).
        /// </summary>
        public static DropEntry? Roll(GeneratorTypeSO so, int tier)
        {
            var cfg = so.ConfigForTier(tier);
            if (cfg?.dropTable == null || cfg.dropTable.Length == 0) return null;

            bool subGenLocked = GeneratorFamilyRegistry.IsSubGenLocked(so.generatorTypeId);

            float total = 0f;
            foreach (var e in cfg.dropTable)
                if (IsEligible(e, subGenLocked)) total += e.weight;

            if (total <= 0f) return null;

            float roll = Random.Range(0f, total);
            float acc = 0f;
            foreach (var e in cfg.dropTable)
            {
                if (!IsEligible(e, subGenLocked)) continue;
                acc += e.weight;
                if (roll < acc) return e;
            }

            return null;
        }

        public static bool IsEligible(in DropEntry e, bool subGenLocked)
        {
            if (e.type == DropType.SubGenerator && subGenLocked) return false;
            if (!string.IsNullOrEmpty(e.requiresStoryFlag) && !NarrativeFlags.Has(e.requiresStoryFlag)) return false;
            return true;
        }
    }
}
