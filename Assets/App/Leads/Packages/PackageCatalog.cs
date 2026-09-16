// Assembly: AQ.App
// File: Assets/App/Leads/Packages/PackageCatalog.cs
// Purpose: Ordered list of an episode's packages plus authoring-time validation.
//          The catalog is content truth for package membership; the database
//          decides lead membership (standing rule: never the folder).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads.Packages
{
    [CreateAssetMenu(fileName = "PackageCatalog", menuName = "AQ/Leads/Package Catalog", order = 12)]
    public sealed class PackageCatalog : ScriptableObject
    {
        [Tooltip("Episode slot this catalog belongs to (ep01..ep04).")]
        public string episodeId = "ep01";

        [Tooltip("All packages, chapter order then package order.")]
        public List<PackageData> packages = new List<PackageData>();

        /// <summary>
        /// Authoring validation. Returns human-readable problems; empty = valid.
        /// Checks: non-null entries, unique non-empty package ids, member counts
        /// 1..5, no member id used by two packages, and (when a database is
        /// supplied) every member id resolves to a lead in it.
        /// </summary>
        public List<string> Validate(LeadsDatabase database = null)
        {
            var problems = new List<string>();
            var seenPackageIds = new HashSet<string>(StringComparer.Ordinal);
            var seenMemberIds = new Dictionary<string, string>(StringComparer.Ordinal);

            var knownLeadIds = new HashSet<string>(StringComparer.Ordinal);
            if (database != null && database.Leads != null)
            {
                foreach (var lead in database.Leads)
                    if (lead != null && !string.IsNullOrEmpty(lead.leadId))
                        knownLeadIds.Add(lead.leadId);
            }

            for (int i = 0; i < packages.Count; i++)
            {
                var p = packages[i];
                if (p == null) { problems.Add($"[{i}] null package entry"); continue; }

                if (string.IsNullOrEmpty(p.packageId))
                    problems.Add($"[{i}] empty packageId");
                else if (!seenPackageIds.Add(p.packageId))
                    problems.Add($"[{i}] duplicate packageId '{p.packageId}'");

                int members = p.memberCardIds?.Length ?? 0;
                if (members < 1 || members > 5)
                    problems.Add($"'{p.packageId}': member count {members} outside 1..5");

                if (p.memberCardIds == null) continue;
                foreach (var id in p.memberCardIds)
                {
                    if (string.IsNullOrEmpty(id)) { problems.Add($"'{p.packageId}': empty member id"); continue; }
                    if (seenMemberIds.TryGetValue(id, out var owner) && owner != p.packageId)
                        problems.Add($"member '{id}' claimed by both '{owner}' and '{p.packageId}'");
                    else
                        seenMemberIds[id] = p.packageId;

                    if (database != null && !knownLeadIds.Contains(id))
                        problems.Add($"'{p.packageId}': member '{id}' not found in database '{database.name}'");
                }
            }

            return problems;
        }
    }
}
