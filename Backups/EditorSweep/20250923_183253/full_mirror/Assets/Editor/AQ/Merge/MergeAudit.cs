// SPDX-License-Identifier: MIT
// AQ Merge Audit — maps merge-related MonoBehaviours to prefabs/scenes by GUID reference.
// Menu: AQ → Merge → Audit (Types & Usage Report)
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AQ.EditorTools.Merge
{
    public static class MergeAudit
    {
        [MenuItem("AQ/Merge/Audit (Types & Usage Report)")]
        public static void Run()
        {
            var outDir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "_audit", "merge");
            Directory.CreateDirectory(outDir);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var outPath = Path.Combine(outDir, $"merge_editor_audit_{stamp}.txt");

            var candidates = GetCandidateTypes();
            var guidMap = BuildScriptGuidMap(candidates);

            var allAssets = AssetDatabase.FindAssets("t:Prefab t:Scene", new[] { "Assets" })
                                         .Select(AssetDatabase.GUIDToAssetPath)
                                         .ToArray();

            var report = new List<string>();
            report.Add($"AQ Merge Editor Audit  {DateTime.UtcNow:O}");
            report.Add($"Assets scanned: {allAssets.Length}");
            report.Add("");

            foreach (var t in candidates)
            {
                var typeName = t.FullName ?? t.Name;
                var guid = guidMap.TryGetValue(t, out var g) ? g : "<no-guid>";
                var uses = new List<string>();

                if (guid != "<no-guid>")
                {
                    foreach (var path in allAssets)
                    {
                        // Cheap YAML hit test by GUID reference
                        var txt = File.ReadAllText(path);
                        if (txt.Contains($"guid: {guid}"))
                            uses.Add(path);
                    }
                }

                report.Add($"TYPE: {typeName}   (guid: {guid})   uses={uses.Count}");
                if (uses.Count > 0)
                    report.AddRange(uses.Select(p => $"  - {p}"));
                report.Add("");
            }

            // Quick scene presence snapshot for "MergeBoard" objects
            var open = EditorSceneManager.GetActiveScene();
            if (open.IsValid())
            {
                var roots = open.GetRootGameObjects();
                var boards = new List<string>();
                foreach (var r in roots)
                {
                    foreach (var t in r.GetComponentsInChildren<Transform>(true))
                        if (t.name.IndexOf("MergeBoard", StringComparison.OrdinalIgnoreCase) >= 0)
                            boards.Add(t.name);
                }
                report.Add($"Active Scene MergeBoard objects: {boards.Count}");
            }

            File.WriteAllLines(outPath, report);
            Debug.Log($"[AQ MergeAudit] Wrote report → {outPath}");
        }

        private static List<Type> GetCandidateTypes()
        {
            // Grab all MonoBehaviours, filter by name/namespace hints.
            var list = new List<Type>();
            foreach (var t in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            {
                if (t == null || t.IsAbstract) continue;
                var n = t.Name;
                var ns = t.Namespace ?? string.Empty;

                bool looksMerge =
                    n.IndexOf("Merge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ( (n.IndexOf("Board", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       n.IndexOf("Cell",  StringComparison.OrdinalIgnoreCase) >= 0 ||
                       n.IndexOf("Slot",  StringComparison.OrdinalIgnoreCase) >= 0 ||
                       n.IndexOf("Tile",  StringComparison.OrdinalIgnoreCase) >= 0 )
                      && ns.IndexOf("Merge", StringComparison.OrdinalIgnoreCase) >= 0 );

                if (looksMerge) list.Add(t);
            }
            return list.OrderBy(t => t.FullName).ToList();
        }

        private static Dictionary<Type,string> BuildScriptGuidMap(IEnumerable<Type> types)
        {
            var want = new HashSet<Type>(types);
            var guids = new Dictionary<Type, string>();

            var scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });
            foreach (var g in scriptGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (!ms) continue;
                var cls = ms.GetClass();
                if (cls != null && want.Contains(cls))
                {
                    guids[cls] = g;
                }
            }
            return guids;
        }
    }
}
#endif
