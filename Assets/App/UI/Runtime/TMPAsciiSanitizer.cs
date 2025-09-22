// AQ — TMP ASCII Sanitizer
// Replaces common non-ASCII glyphs with ASCII at runtime (✓→OK) under this GameObject.
using UnityEngine;

namespace AQ.App.UI
{
    [DefaultExecutionOrder(-1000)]
    public sealed class TMPAsciiSanitizer : MonoBehaviour
    {
        [Tooltip("If true, runs every frame for dynamic texts; otherwise runs once on Awake.")]
        public bool continuous = false;

        private static readonly (string from, string to)[] Map = new[]
        {
            ("✓", "OK"),
            ("\\u2713", "OK") // defensive: if a literal backslash-u slips in
        };

        private void Awake() { Sweep(); }
        private void LateUpdate() { if (continuous) Sweep(); }

        private void Sweep()
        {
            var tmps = GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var t in tmps)
            {
                if (string.IsNullOrEmpty(t.text)) continue;
                var s = t.text;
                foreach (var rule in Map) s = s.Replace(rule.from, rule.to);
                if (s != t.text) t.text = s;
            }
        }
    }
}
