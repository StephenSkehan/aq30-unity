using System.Collections;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Translates UI drag/drop or test hooks into merge actions.
    /// Restores legacy surface expected by your bootstrap scripts:
    ///   • public object mergeService;   // kept loose to avoid extra deps
    ///   • public void HandleDrop(ItemView source, ItemView target)
    /// </summary>
    public class MergeInputAdapter : MonoBehaviour
    {
        [Header("Wiring")]
        public MergeBoardView board;

        [Header("SFX (optional)")]
        public AudioSource audioSource;
        public AudioClip successHum;

        // Kept for compatibility with existing bootstrapper code.
        public object mergeService;

        public bool LastMergeSucceeded { get; private set; }

        /// <summary>Called by your drag/drop hook.</summary>
        public void HandleDrop(ItemView source, ItemView target)
        {
            RequestMerge(source, target);
        }

        /// <summary>
        /// For the vertical slice: treat any pair as valid, execute a "successful merge" flow.
        /// </summary>
        public void RequestMerge(ItemView a, ItemView b)
        {
            if (a == null || b == null || board == null)
            {
                LastMergeSucceeded = false;
                return;
            }

            StopAllCoroutines();
            StartCoroutine(SuccessRoutine(a, b));
        }

        private IEnumerator SuccessRoutine(ItemView a, ItemView b)
        {
            if (audioSource != null && successHum != null)
                audioSource.PlayOneShot(successHum);

            var targetPos = board.GetGridPos(b);

            // tiny delay keeps UGUI happy and tests deterministic
            yield return null;

            board.RemoveItem(a);
            board.RemoveItem(b);
            board.SpawnItem(targetPos, TryGetLabelText(b));

            // Mark success; notify board the *first* time only.
            LastMergeSucceeded = true;
            board.RaiseFirstMergeIfNeeded();
        }

        public IEnumerator SimulateMergeForTests(string aLabel, string bLabel)
        {
            // Optional test hook – no-op for now
            yield return null;
        }

        private static string TryGetLabelText(ItemView v)
        {
            if (v == null) return string.Empty;
            var prop = v.GetType().GetProperty("LabelText",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                var val = prop.GetValue(v) as string;
                return val ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
