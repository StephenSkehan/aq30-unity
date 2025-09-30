#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.UI
{
    public static class BoardGridCleaner
    {
        private const string TilePrefabPath = "Assets/UI/Prefabs/board_tile_slot.prefab";

        [MenuItem("AQ/UI/Board/Clear Grid Tiles")]
        public static void ClearTiles()
        {
            var grid = FindBoardGrid();
            if (!grid)
            {
                Debug.LogError("[BoardGridCleaner] Could not find the main BoardGrid under Canvas_Board/HUD_Board.");
                return;
            }

            int count = grid.transform.childCount;
            for (int i = count - 1; i >= 0; i--)
                Object.DestroyImmediate(grid.transform.GetChild(i).gameObject);
            Debug.Log($"[BoardGridCleaner] Cleared {count} tiles from {FullPath(grid.transform)}.");
        }

        [MenuItem("AQ/UI/Board/Rebuild Grid 7x9 Placeholders")]
        public static void Rebuild()
        {
            var grid = FindBoardGrid();
            if (!grid)
            {
                Debug.LogError("[BoardGridCleaner] Could not find the main BoardGrid under Canvas_Board/HUD_Board.");
                return;
            }

            // Clear first
            for (int i = grid.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(grid.transform.GetChild(i).gameObject);

            // Enforce layout: 7 cols, 9 rows, spacing 2px
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            grid.spacing = new Vector2(2f, 2f);

            // Fit cells to width
            var rt = grid.GetComponent<RectTransform>();
            float width = rt.rect.width;
            float totalSpacing = grid.spacing.x * (7 - 1);
            float cellW = Mathf.Floor((width - totalSpacing) / 7f);
            grid.cellSize = new Vector2(cellW, cellW);

            // Spawn 7x9 = 63 placeholders
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TilePrefabPath);
            if (!tilePrefab)
            {
                Debug.LogError($"[BoardGridCleaner] Missing tile prefab at {TilePrefabPath}");
                return;
            }

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 7; c++)
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, grid.transform);
                    inst.name = $"Tile_{c}_{r}";
                }

            Debug.Log("[BoardGridCleaner] Rebuilt grid to 7x9 with 2px spacing.");
        }

        // ---------- helpers ----------

        private static GridLayoutGroup FindBoardGrid()
        {
            // Prefer canonical path: Canvas_Board/HUD_Board/BoardRoot/BoardFrame/BoardViewport/BoardGrid
            var canvasBoard = GameObject.Find("Canvas_Board");
            if (!canvasBoard) return null;

            var hud = FindChild(canvasBoard.transform, "HUD_Board");
            var root = hud ? FindChild(hud, "BoardRoot") : null;
            var frame = root ? FindChild(root, "BoardFrame") : null;
            var view = frame ? FindChild(frame, "BoardViewport") : null;
            var grid = view ? FindChild(view, "BoardGrid") : null;

            if (grid)
                return grid.GetComponent<GridLayoutGroup>();

            // Fallback: pick a GridLayoutGroup named "BoardGrid" directly under Canvas_Board
            var all = canvasBoard.GetComponentsInChildren<GridLayoutGroup>(true);
            var named = all.FirstOrDefault(g => g.name == "BoardGrid");
            if (named) return named;

            // Final fallback: the grid with the most children under Canvas_Board (but NOT under LeadsBar)
            var filtered = all.Where(g => !IsUnderLeadsBar(g.transform)).ToList();
            return filtered.OrderByDescending(g => g.transform.childCount).FirstOrDefault();
        }

        private static Transform FindChild(Transform parent, string name)
        {
            if (!parent) return null;
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static bool IsUnderLeadsBar(Transform t)
        {
            while (t)
            {
                if (t.name == "LeadsBar") return true;
                t = t.parent;
            }
            return false;
        }

        private static string FullPath(Transform t)
        {
            string path = t.name;
            while (t.parent) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif
