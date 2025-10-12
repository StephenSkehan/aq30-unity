// Assets/Editor/UI/Board/BoardSanityAudit.cs
// Legacy audit (Classic). Kept alongside the Strict audit, with a unique menu item.

#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AQ.App.Editor.Board
{
    public static class BoardSanityAudit
    {
        [MenuItem("AQ/Board/Audit (Classic)")]
        public static void AuditCurrentScene()
        {
            var sb = new StringBuilder();
            var scene = SceneManager.GetActiveScene();
            sb.AppendLine($"=== BOARD SANITY AUDIT (CLASSIC): {scene.path} @ {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

            var grid = FindOne<GridLayoutGroup>();
            if (grid == null)
            {
                sb.AppendLine("FAIL: No GridLayoutGroup found.");
                Write(sb.ToString());
                return;
            }

            // Grid stats
            sb.AppendLine($"INFO: Grid object: {grid.gameObject.GetHierarchyPath()}");
            sb.AppendLine($"INFO: cellSize={grid.cellSize} spacing={grid.spacing} constraint={grid.constraint} count={grid.constraintCount}");

            int slotCount = 0;
            foreach (Transform child in grid.transform)
            {
                if (LooksLikeSlot(child)) slotCount++;
            }
            sb.AppendLine($"INFO: Slots found under grid: {slotCount}");

            if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount || grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                if (grid.spacing != new Vector2(2, 2))
                    sb.AppendLine("WARN: Grid spacing is not (2,2).");
            }
            else
            {
                sb.AppendLine("WARN: Grid constraint is Flexible (expected FixedColumnCount or FixedRowCount).");
            }

            Write(sb.ToString());
        }

        private static void Write(string text)
        {
            Debug.Log(text);
        }

        private static bool LooksLikeSlot(Transform t)
        {
            if (t == null) return false;
            if (t.name.StartsWith("slot_", StringComparison.Ordinal)) return true;
            if (t.GetComponent<Button>() != null) return true;
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c.GetType().FullName == "AQ.App.UI.Board.BoardTileView") return true;
            }
            return false;
        }

        // Version-safe finders (no name collisions)
        private static T FindOne<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>();
#else
            return UnityEngine.Object.FindObjectOfType<T>();
#endif
        }

        private static string GetHierarchyPath(this GameObject go)
        {
            if (go == null) return "<null>";
            var path = go.name;
            var p = go.transform.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }
}
#endif
