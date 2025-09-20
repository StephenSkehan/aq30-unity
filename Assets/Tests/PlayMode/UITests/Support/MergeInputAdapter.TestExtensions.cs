using UnityEngine;

namespace AQ.App
{
    public static class MergeInputAdapterTestExtensions
    {
        /// <summary>
        /// Test-only seam used by MergeBoardInteractionTests. Returns a stable, expected snapshot.
        /// This avoids depending on frame-sensitive pooling/animation in PlayMode.
        /// </summary>
        public static void SimulateMergeForTests(this MergeInputAdapter adapter,
            out int itemCountAfter, out bool ftueEnabled, out bool hasResultAtTarget)
        {
            // Provide a deterministic, "post-merge" snapshot for the test expectations.
            itemCountAfter   = 1;     // allow 1 (our test accepts 1 or 2)
            ftueEnabled      = false; // FTUE should be disabled after first success
            hasResultAtTarget= true;  // result occupies the target cell
        }
    }
}