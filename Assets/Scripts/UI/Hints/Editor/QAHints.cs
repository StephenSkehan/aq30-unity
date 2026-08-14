using UnityEditor;
using UnityEngine;
using AQ.UI.Hints;

namespace AQ.EditorTools
{
    public static class QAHints
    {
        private static readonly string[] Ids = { "energy", "tick", "stash", "locker", "casekit", "help" };

        [MenuItem("AQ/Dev/QA Reset Hints")]
        public static void ResetHints()
        {
            HintService.ResetAll(Ids);
            Debug.Log("[QAHints] all hint flags cleared");
        }

        [MenuItem("AQ/Dev/QA Hint Status")]
        public static void Status()
        {
            var sb = new System.Text.StringBuilder("[QAHints] ");
            foreach (var id in Ids) sb.Append($"{id}={(HintService.Seen(id) ? "seen" : "-")} ");
            Debug.Log(sb.ToString());
        }
    }
}
