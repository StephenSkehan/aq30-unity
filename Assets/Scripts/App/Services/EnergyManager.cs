using System;

namespace AQ.App.Services
{
    /// <summary>
    /// Simple energy model with time-based regeneration.
    /// - Current value increases by TickAmount every SecondsPerTick.
    /// - Dev helpers allow adding without clamping (for hotkeys).
    /// </summary>
    public sealed class EnergyManager
    {
        // State
        public int Current { get; private set; }
        public int Cap { get; private set; }
        public float SecondsPerTick { get; private set; }
        public int TickAmount { get; private set; }
        public DateTime LastTickUtc { get; private set; }

        // ---- Constructors ----

        /// <summary>
        /// Minimal ctor used by save/load paths. SecondsPerTick defaults to 60s; TickAmount defaults to 1.
        /// Call TickNow(...) with your config seconds to align regen after construction.
        /// </summary>
        public EnergyManager(int start, int cap, DateTime lastTickUtc)
        {
            Current = Math.Max(0, start);
            Cap = Math.Max(0, cap);
            SecondsPerTick = 60f;
            TickAmount = 1;
            LastTickUtc = lastTickUtc.ToUniversalTime();
        }

        /// <summary>
        /// Full ctor when all tuning knobs are known.
        /// </summary>
        public EnergyManager(int start, int cap, float secondsPerTick, int tickAmount, DateTime lastTickUtc)
        {
            Current = Math.Max(0, start);
            Cap = Math.Max(0, cap);
            SecondsPerTick = Math.Max(0.001f, secondsPerTick);
            TickAmount = Math.Max(1, tickAmount);
            LastTickUtc = lastTickUtc.ToUniversalTime();
        }

        // ---- Core ----

        /// <summary>
        /// Applies time-based regeneration from LastTickUtc to 'now'.
        /// Uses the provided secondsPerPoint (which also updates SecondsPerTick) and returns ticks applied.
        /// </summary>
        public int TickNow(float secondsPerPoint, DateTime now)
        {
            SecondsPerTick = Math.Max(0.001f, secondsPerPoint);
            var nowUtc = now.ToUniversalTime();

            // Device clock moved backwards (timezone/DST/manual change): without
            // this clamp LastTickUtc sits in the future and regen stalls.
            if (LastTickUtc > nowUtc)
                LastTickUtc = nowUtc;

            double elapsed = (nowUtc - LastTickUtc).TotalSeconds;
            if (elapsed < SecondsPerTick)
                return 0;

            int ticks = (int)Math.Floor(elapsed / SecondsPerTick);
            if (ticks > 0)
            {
                long delta = (long)ticks * Math.Max(1, TickAmount);
                // Regeneration clamps to cap
                long newValue = (long)Current + delta;
                Current = (int)Math.Min(newValue, Cap);

                // Advance LastTickUtc to the most recent tick boundary
                double consumed = ticks * SecondsPerTick;
                LastTickUtc = LastTickUtc.AddSeconds(consumed);
            }

            return ticks;
        }

        /// <summary>
        /// Tries to consume 'amount'. Returns true if successful; false if insufficient.
        /// Does not modify LastTickUtc.
        /// </summary>
        public bool TryConsume(int amount)
        {
            if (amount <= 0) return true;
            if (Current < amount) return false;
            Current -= amount;
            return true;
        }

        /// <summary>
        /// Convenience: tick first using current SecondsPerTick, then consume.
        /// </summary>
        public bool TryConsume(int amount, DateTime now)
        {
            TickNow(SecondsPerTick, now);
            return TryConsume(amount);
        }

        /// <summary>
        /// Adds energy WITHOUT clamping to Cap (for dev tools/hotkeys).
        /// </summary>
        public void Add(int amount)
        {
            if (amount == 0) return;
            long v = (long)Current + amount;
            // Allow going negative only if explicitly asked (we don't).
            Current = (int)Math.Max(0, v);
        }

        /// <summary>
        /// Optional helper: adds but clamps to Cap (unused by dev hotkeys).
        /// </summary>
        public void AddClamped(int amount)
        {
            if (amount == 0) return;
            long v = (long)Current + amount;
            Current = (int)Math.Max(0, Math.Min(v, Cap));
        }

        /// <summary>
        /// Force-set the current value (safety-clamped at zero).
        /// </summary>
        public void Set(int value)
        {
            Current = Math.Max(0, value);
        }
    }
}
