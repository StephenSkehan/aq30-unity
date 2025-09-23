#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Leads
{
    public static class LeadsBarCleanup
    {
        private const string LeadsPath = "Canvas_Board/HUD_Board/LeadsBar/ScrollLeads/Viewport/Content_Leads";

        [MenuItem("AQ/Leads/Audit LeadsBar Placeholders")]
        public static void Audit()
        {
            var content = GameObject.Find(LeadsPath)?.transform;
            if (!content) { Debug.LogWarning("ℹ️ Content_Leads not found."); return; }

            int total = content.childCount;
            var stray = content.Cast<Transform>()
                .Where(t =>
                {
                    // Consider "stray" = has TMP with literal "• Item" (or "Item"), and NOT a lead card script
                    var tmp = t.GetComponentInChildren<TMP_Text>();
                    if (tmp == null) return false;
                    string s = tmp.text?.Trim() ?? "";
                    bool looksItem = s == "• Item" || s == "Item";
                    bool isLeadCard = t.GetComponent("LeadCardView") != null; // duck-typed, safe if script absent
                    return looksItem && !isLeadCard;
                })
                .ToList();

            Debug.Log($"🔎 LeadsBar audit: {total} children, {stray.Count} placeholder(s) detected: " +
                      string.Join(", ", stray.Select(t => t.name)));
        }

        [MenuItem("AQ/Leads/Purge LeadsBar Placeholders (undoable)")]
        public static void Purge()
        {
            var content = GameObject.Find(LeadsPath)?.transform;
            if (!content) { Debug.LogWarning("ℹ️ Content_Leads not found."); return; }

            var targets = content.Cast<Transform>()
                .Where(t =>
                {
                    var tmp = t.GetComponentInChildren<TMP_Text>();
                    if (tmp == null) return false;
                    string s = tmp.text?.Trim() ?? "";
                    bool looksItem = s == "• Item" || s == "Item";
                    bool isLeadCard = t.GetComponent("LeadCardView") != null;
                    return looksItem && !isLeadCard;
                })
                .ToArray();

            if (targets.Length == 0)
            {
                Debug.Log("✅ No placeholders to purge.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(content.gameObject, "Purge LeadsBar Placeholders");
            foreach (var t in targets) Undo.DestroyObjectImmediate(t.gameObject);
            Debug.Log($"🧹 Purged {targets.Length} placeholder(s) from LeadsBar.");
        }
    }
}
#endif
