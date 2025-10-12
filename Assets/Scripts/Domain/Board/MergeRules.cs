using System;
using System.Runtime.CompilerServices;
using AQ.App.UI.Board; // for TileKind

namespace AQ.Domain.Board
{
    /// <summary>Pure merge rules: values in, decision out. No Unity deps.</summary>
    public static class MergeRules
    {
        public enum Outcome { Move, Swap, Merge, CeilingSwap }

        /// <summary>Minimal value object used by the rules engine.</summary>
        public readonly struct Tile
        {
            public readonly TileKind Kind;
            public readonly int Tier;
            public readonly string Family;

            public Tile(TileKind kind, int tier, string family)
            {
                Kind = kind;
                Tier = tier;
                Family = family ?? string.Empty;
            }
        }

        /// <summary>
        /// Family-aware: if BOTH families are specified and differ → Swap.
        /// Same kind + same tier merges; max-tier vs max-tier → CeilingSwap.
        /// Moving into Empty → Move.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Outcome Decide(in Tile a, in Tile b, int maxTier)
        {
            if (b.Kind == TileKind.Empty) return Outcome.Move;        // move

            if (a.Kind != b.Kind) return Outcome.Swap;                // kind mismatch
            if (a.Tier != b.Tier) return Outcome.Swap;                // tier mismatch

            if (a.Tier >= maxTier && b.Tier >= maxTier)               // ceiling rule
                return Outcome.CeilingSwap;

            bool aKnown = !string.IsNullOrEmpty(a.Family);
            bool bKnown = !string.IsNullOrEmpty(b.Family);
            if (aKnown && bKnown && !a.Family.Equals(b.Family, StringComparison.Ordinal))
                return Outcome.Swap;

            return Outcome.Merge;                                      // merge
        }
    }
}
