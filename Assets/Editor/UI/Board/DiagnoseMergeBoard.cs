// SPDX-License-Identifier: MIT
// File: Assets/Editor/UI/Board/DiagnoseMergeBoard.cs
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.UI.Board
{
    public static class DiagnoseMergeBoard
    {
        [MenuItem("AQ/UI/Board/Diagnose/Report Scene Grids")]
        public static void ReportSceneGrids()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[AQ] MergeBoard grid audit:");

#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            var grids = UnityEngine.Object.FindObjectsByType<GridLayoutGroup>(FindObjectsSortMode.None);
#else
            var grids = UnityEngine.Object.FindObjectsOfType<GridLayoutGroup>(true);
#endif
            if (grids == null || grids.Length == 0)
            {
                Debug.LogWarning("[AQ] No GridLayoutGroup components found in the open scenes.");
                return;
            }

            foreach (var g in grids.OrderByDescending(x => x.transform.childCount))
            {
                var path = GetHierarchyPath(g.transform);
                sb.AppendLine($"  • {g.name}  (active:{g.gameObject.activeInHierarchy}, children:{g.transform.childCount})  path: {path}");

                // show up to 6 child names
                var childNames = Enumerable.Range(0, Math.Min(6, g.transform.childCount))
                                           .Select(i => g.transform.GetChild(i).name)
                                           .ToArray();
                if (childNames.Length > 0)
                    sb.AppendLine("     children: " + string.Join(", ", childNames));
            }

            // Preferential pick that the populator will use
            var preferred = PickPreferredGrid(grids);
            if (preferred != null)
            {
                sb.AppendLine($"[AQ] Preferred grid (the one the populator will target): {GetHierarchyPath(preferred.transform)}  (children:{preferred.transform.childCount})");
                Selection.activeObject = preferred.gameObject; // help user jump to it
                EditorGUIUtility.PingObject(preferred.gameObject);
            }

            Debug.Log(sb.ToString());
        }

        [MenuItem("AQ/UI/Board/Diagnose/Select BoardRoot (if any)")]
        public static void SelectBoardRoot()
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            var grids = UnityEngine.Object.FindObjectsByType<GridLayoutGroup>(FindObjectsSortMode.None);
#else
            var grids = UnityEngine.Object.FindObjectsOfType<GridLayoutGroup>(true);
#endif
            var board = grids?.FirstOrDefault(g => string.Equals(g.name, "BoardRoot", StringComparison.OrdinalIgnoreCase));
            if (board == null)
            {
                Debug.LogWarning("[AQ] No GridLayoutGroup named 'BoardRoot' found in the open scenes.");
                return;
            }

            Selection.activeObject = board.gameObject;
            EditorGUIUtility.PingObject(board.gameObject);
            Debug.Log("[AQ] Selected BoardRoot.");
        }

        internal static GridLayoutGroup PickPreferredGrid(GridLayoutGroup[] grids)
        {
            if (grids == null || grids.Length == 0) return null;

            // 1) Prefer a GameObject literally named "BoardRoot"
            var byName = grids.FirstOrDefault(g => string.Equals(g.name, "BoardRoot", StringComparison.OrdinalIgnoreCase));
            if (byName != null) return byName;

            // 2) Else pick the active grid with the highest child count
            var activeWithKids = grids.Where(g => g.gameObject.activeInHierarchy)
                                      .OrderByDescending(g => g.transform.childCount)
                                      .FirstOrDefault();
            if (activeWithKids != null) return activeWithKids;

            // 3) Fallback: any grid with the highest child count
            return grids.OrderByDescending(g => g.transform.childCount).First();
        }

        private static string GetHierarchyPath(Transform t)
        {
            var parts = new System.Collections.Generic.List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif
