using System;

namespace AQ.App.Services
{
    /// <summary>
    /// Global energy with offline regen. No Unity deps.
    /// Slice policy: regen pauses at cap; overflow only via rewards/purchases.
    /// </summary>
    public sealed class EnergyManager
    {
        public int Current { get; private set; }
        public int Cap { get; private set; }
        public DateTime LastTickUtc { get; private set; }

        public EnergyManager(int start, int cap, DateTime lastTickUtc)
        {
            Current = start;
            Cap = cap;
            LastTickUtc = lastTickUtc;
        }

        /// <summary>Recalculate regen up to cap based on elapsed time.</summary>
        public void TickNow(int secondsPerPoint, DateTime nowUtc)
        {
            if (secondsPerPoint <= 0) return;
            if (Current >= Cap) { LastTickUtc = nowUtc; return; }

            var elapsed = (int)(nowUtc - LastTickUtc).TotalSeconds;
            var gain = elapsed / secondsPerPoint;
            if (gain > 0)
            {
                Current = Math.Min(Cap, Current + gain);
                LastTickUtc = LastTickUtc.AddSeconds(gain * secondsPerPoint);
            }
        }

        /// <summary>Try to consume energy. Returns false if insufficient.</summary>
        public bool TryConsume(int amount)
        {
            if (amount <= 0) return true;
            if (Current < amount) return false;
            Current -= amount;
            return true;
        }

        /// <summary>Adds energy beyond cap (used for rewards/purchases overflow).</summary>
        public void AddOverflow(int amount)
        {
            if (amount > 0) Current += amount;
        }
    }
}
