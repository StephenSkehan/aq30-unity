using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Static utility for managing dialogue flags (requirements/conditions).
    /// Uses PlayerPrefs with "dlg_flag_" prefix for persistence.
    /// </summary>
    public static class DialogueFlags
    {
        private const string PREFIX = "dlg_flag_";

        /// <summary>
        /// Set a dialogue flag (marks it as true/visited).
        /// </summary>
        public static void Set(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            
            PlayerPrefs.SetInt(PREFIX + flag, 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[DialogueFlags] Set: {flag}");
        }

        /// <summary>
        /// Check if a dialogue flag is set.
        /// </summary>
        public static bool Has(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;
            return PlayerPrefs.GetInt(PREFIX + flag, 0) == 1;
        }

        /// <summary>
        /// Clear a specific dialogue flag.
        /// </summary>
        public static void Clear(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            
            PlayerPrefs.DeleteKey(PREFIX + flag);
            PlayerPrefs.Save();
            
            Debug.Log($"[DialogueFlags] Cleared: {flag}");
        }

        /// <summary>
        /// Clear all dialogue flags (use for testing or new game).
        /// </summary>
        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            Debug.Log("[DialogueFlags] Cleared all flags");
        }

        /// <summary>
        /// Get all flag keys (for debugging).
        /// </summary>
        public static string[] GetAllFlags()
        {
            // Note: PlayerPrefs doesn't have a GetAllKeys method,
            // so this is a simplified implementation for debugging
            var flags = new System.Collections.Generic.List<string>();
            
            // Common flag patterns to check
            string[] commonFlags = new string[]
            {
                "has_evidence_001", "talked_to_ally", "visited_crime_scene",
                "tutorial_done", "first_merge", "case_01_complete"
            };

            foreach (var flag in commonFlags)
            {
                if (Has(flag))
                    flags.Add(flag);
            }

            return flags.ToArray();
        }

        /// <summary>
        /// Debug: Print all active flags to console.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DebugPrintAll()
        {
            var flags = GetAllFlags();
            if (flags.Length == 0)
            {
                Debug.Log("[DialogueFlags] No active flags");
                return;
            }

            Debug.Log($"[DialogueFlags] Active flags ({flags.Length}):");
            foreach (var flag in flags)
            {
                Debug.Log($"  - {flag}");
            }
        }
    }
}