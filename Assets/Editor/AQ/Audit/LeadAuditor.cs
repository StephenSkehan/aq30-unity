#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AQ.App.Leads;
using UnityEditor;
using UnityEngine;

static class LeadAuditor
{
    [MenuItem("AQ/Leads/Run Lead Audit", false, 50)]
    static void RunAudit()
    {
        // ── Load database ──────────────────────────────────────────────────────
        var dbGuids = AssetDatabase.FindAssets("t:LeadsDatabase");
        if (dbGuids.Length == 0)
        {
            Debug.LogError("[LeadAudit] No LeadsDatabase asset found in project.");
            return;
        }
        var db = AssetDatabase.LoadAssetAtPath<LeadsDatabase>(
            AssetDatabase.GUIDToAssetPath(dbGuids[0]));
        if (db == null || db.Leads == null)
        {
            Debug.LogError("[LeadAudit] LeadsDatabase could not be loaded.");
            return;
        }

        // ── Build known-id set ─────────────────────────────────────────────────
        var knownIds = new HashSet<string>();
        foreach (var lead in db.Leads)
            if (lead != null && !string.IsNullOrEmpty(lead.leadId))
                knownIds.Add(lead.leadId);

        // ── Collect checked NarrativeFlags from source ─────────────────────────
        var checkedFlags = CollectCheckedNarrativeFlags();

        // ── Audit ──────────────────────────────────────────────────────────────
        int errors = 0, warnings = 0;
        var sb = new StringBuilder();
        sb.AppendLine($"=== LEAD AUDIT — {db.Leads.Count} leads in database ===\n");

        foreach (var lead in db.Leads)
        {
            if (lead == null)
            {
                sb.AppendLine("[ERR] Null entry in LeadsDatabase.leads[]");
                errors++;
                continue;
            }

            var id = string.IsNullOrEmpty(lead.leadId) ? "(no id)" : lead.leadId;
            var leadErrors   = new List<string>();
            var leadWarnings = new List<string>();

            // ERROR: missing resolution dialogue
            if (lead.resolutionDialogue == null)
                leadErrors.Add("resolutionDialogue is null — Proceed will show fallback text");

            // ERROR: SpawnLeadIds reference unknown leadIds
            if (lead.SpawnLeadIds != null)
                foreach (var spawnId in lead.SpawnLeadIds)
                    if (!string.IsNullOrEmpty(spawnId) && !knownIds.Contains(spawnId))
                        leadErrors.Add($"SpawnLeadIds['{spawnId}'] not in database");

            // ERROR: RequiredLeadIds reference unknown leadIds
            if (lead.RequiredLeadIds != null)
                foreach (var reqId in lead.RequiredLeadIds)
                    if (!string.IsNullOrEmpty(reqId) && !knownIds.Contains(reqId))
                        leadErrors.Add($"RequiredLeadIds['{reqId}'] not in database");

            // WARNING: null actor portrait
            if (lead.actorPortrait == null)
                leadWarnings.Add("actorPortrait is null — lead card will show no badge");

            // WARNING: NarrativeFlags set but never checked in code
            if (lead.NarrativeFlags != null)
                foreach (var flag in lead.NarrativeFlags)
                    if (!string.IsNullOrEmpty(flag) && !checkedFlags.Contains(flag))
                        leadWarnings.Add($"NarrativeFlag '{flag}' is never read via NarrativeFlags.Has() in source");

            errors   += leadErrors.Count;
            warnings += leadWarnings.Count;

            if (leadErrors.Count > 0 || leadWarnings.Count > 0)
            {
                sb.AppendLine($"── {id} ──");
                foreach (var e in leadErrors)   sb.AppendLine($"  [ERR]  {e}");
                foreach (var w in leadWarnings) sb.AppendLine($"  [WARN] {w}");
            }
        }

        // ── Summary ────────────────────────────────────────────────────────────
        sb.AppendLine();
        sb.AppendLine($"=== RESULT: {errors} error(s), {warnings} warning(s) ===");
        if (errors == 0 && warnings == 0)
            sb.AppendLine("All leads passed. Safe to build.");

        WriteBigToConsole(sb.ToString(), errors > 0);
    }

    // ── Collect all flag strings passed to NarrativeFlags.Has() in .cs files ──
    static HashSet<string> CollectCheckedNarrativeFlags()
    {
        var result = new HashSet<string>();
        var csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        var rx = new Regex(@"NarrativeFlags\.Has\(\s*""([^""]+)""\s*\)", RegexOptions.Compiled);

        foreach (var file in csFiles)
        {
            try
            {
                var text = File.ReadAllText(file);
                foreach (Match m in rx.Matches(text))
                    result.Add(m.Groups[1].Value);
            }
            catch { /* skip unreadable files */ }
        }
        return result;
    }

    static void WriteBigToConsole(string text, bool hasErrors)
    {
        const int chunk = 7000;
        if (text.Length <= chunk)
        {
            if (hasErrors) Debug.LogError(text);
            else           Debug.Log(text);
            return;
        }
        int i = 0; int n = 1 + text.Length / chunk;
        while (i < text.Length)
        {
            var len = Mathf.Min(chunk, text.Length - i);
            var part = $"[audit chunk {i / chunk + 1}/{n}]\n" + text.Substring(i, len);
            if (hasErrors) Debug.LogError(part);
            else           Debug.Log(part);
            i += len;
        }
    }
}
#endif
