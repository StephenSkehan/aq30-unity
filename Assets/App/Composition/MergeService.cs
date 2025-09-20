using AQ.App.Composition;
using System;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// App-level merge service that supports both cell-based and label-based seams.
    /// - Cell seam (Vector2Int/int) matches the composition IMergeService.
    /// - Label seam (string,string,out string) lets UI call without domain wiring.
    /// Tests can inject either resolver.
    /// </summary>
    public class MergeService : MonoBehaviour, IMergeService
    {
        public static MergeService Instance { get; private set; }

        // Cell-based resolver: returns true if merge succeeds.
        private Func<Vector2Int, Vector2Int, bool> _cellResolver;

        // Label-based resolver: returns true if merge succeeds, and produces result label.
        private Func<string, string, (bool ok, string result)> _labelResolver;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>Inject a domain merge function (Vector2Int signature).</summary>
        public void InjectDomainResolver(Func<Vector2Int, Vector2Int, bool> resolver)
        {
            _cellResolver = resolver;
        }

        /// <summary>Inject a domain merge function (int signature).</summary>
        public void InjectDomainResolver(Func<int, int, int, int, bool> resolver)
        {
            _cellResolver = (from, to) => resolver(from.x, from.y, to.x, to.y);
        }

        /// <summary>
        /// Inject a label-based resolver. Return (ok,resultLabel).
        /// </summary>
        public void InjectDomainResolver(Func<string, string, (bool ok, string result)> resolver)
        {
            _labelResolver = resolver;
        }

        // ===== Cell-based IMergeService (composition contract) =====
        public bool TryMerge(Vector2Int from, Vector2Int to)
        {
            if (_cellResolver != null) return _cellResolver(from, to);

            // Default stub: allow merge so UI can progress when domain isn’t wired yet.
            return true;
        }

        public bool TryMerge(int fromX, int fromY, int toX, int toY)
            => TryMerge(new Vector2Int(fromX, fromY), new Vector2Int(toX, toY));

        // ===== Label-based UI seam (for MergeInputAdapter) =====
        public bool TryMerge(string aLabel, string bLabel, out string resultLabel)
        {
            // Use injected label resolver if present
            if (_labelResolver != null)
            {
                var r = _labelResolver(aLabel, bLabel);
                resultLabel = r.result;
                return r.ok;
            }

            // Fallback behavior: if labels match, "merge" into the next tier-ish label.
            // Extremely simple placeholder so PlayMode tests can proceed deterministically.
            if (!string.IsNullOrEmpty(aLabel) && aLabel == bLabel)
            {
                resultLabel = aLabel + "+"; // e.g., "A" -> "A+"
                return true;
            }

            resultLabel = bLabel ?? aLabel ?? string.Empty;
            return false;
        }
    }
}
