// COMPLETE FILE: Assets/Editor/UI/Board/RebuildMergeBoardDemo.cs
// CREATE THIS NEW FILE (make sure folder path exists) 123

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.UI.Board
{
    public static class RebuildMergeBoardDemo
    {
        [MenuItem("AQ/Board/Rebuild MergeBoard_Demo Scene")]
        public static void Rebuild()
        {
            // Configuration
            const int ROWS = 9;
            const int COLS = 7;
            const float SPACING = 2f;
            
            // Find required objects
            var mergeBoard = GameObject.Find("MergeBoard");
            if (!mergeBoard)
            {
                EditorUtility.DisplayDialog("Error", "MergeBoard GameObject not found in scene", "OK");
                return;
            }

            // Find MergeBoardController component
            MonoBehaviour controller = null;
            foreach (var comp in mergeBoard.GetComponents<MonoBehaviour>())
            {
                if (comp != null && comp.GetType().Name.Contains("MergeBoardController"))
                {
                    controller = comp;
                    break;
                }
            }
            
            if (!controller)
            {
                EditorUtility.DisplayDialog("Error", 
                    "MergeBoardController component not found on MergeBoard GameObject.\n\n" +
                    "Make sure the MergeBoard GameObject has the MergeBoardController script attached.", "OK");
                return;
            }

            // Get the tile prefab from controller
            var so = new SerializedObject(controller);
            var tilePrefabProp = so.FindProperty("tilePrefab");
            var tilePrefab = tilePrefabProp?.objectReferenceValue as GameObject;
            
            if (!tilePrefab)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Tile prefab not assigned on MergeBoardController.\n\n" +
                    "Please assign the board_tile_slot prefab to the TilePrefab field in the Inspector.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Rebuild MergeBoard");
            var group = Undo.GetCurrentGroup();

            // 1. Clear existing children
            var transform = mergeBoard.transform;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("slot_") || child.name.StartsWith("Slot_"))
                    Undo.DestroyObjectImmediate(child.gameObject);
            }

            // 2. Setup GridLayoutGroup
            var grid = mergeBoard.GetComponent<GridLayoutGroup>();
            if (!grid)
                grid = Undo.AddComponent<GridLayoutGroup>(mergeBoard);

            Undo.RecordObject(grid, "Setup Grid");
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = COLS;
            grid.spacing = new Vector2(SPACING, SPACING);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;

            // Calculate cell size to fit the board
            var rt = mergeBoard.GetComponent<RectTransform>();
            var boardWidth = rt.rect.width;
            var boardHeight = rt.rect.height;
            
            var cellW = (boardWidth - SPACING * (COLS - 1)) / COLS;
            var cellH = (boardHeight - SPACING * (ROWS - 1)) / ROWS;
            var cellSize = Mathf.Floor(Mathf.Min(cellW, cellH));
            grid.cellSize = new Vector2(cellSize, cellSize);

            // 3. Create all slot instances
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    var slot = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, transform);
                    slot.name = $"slot_{r:00}_{c:00}";
                    Undo.RegisterCreatedObjectUndo(slot, "Create Slot");
                }
            }

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[AQ] Rebuilt MergeBoard: {ROWS}×{COLS} grid with {SPACING}px spacing. " +
                      $"Created {ROWS * COLS} slots. Cell size: {cellSize}×{cellSize}");
            
            EditorUtility.DisplayDialog("Success", 
                $"MergeBoard rebuilt successfully!\n\n" +
                $"• {ROWS}×{COLS} grid ({ROWS * COLS} slots)\n" +
                $"• {SPACING}px spacing\n" +
                $"• Cell size: {cellSize}×{cellSize}\n\n" +
                $"Press Play to test.", "OK");
        }

        [MenuItem("AQ/Board/Rebuild MergeBoard_Demo Scene", true)]
        public static bool RebuildValidate()
        {
            return GameObject.Find("MergeBoard") != null;
        }
    }
}
#endif