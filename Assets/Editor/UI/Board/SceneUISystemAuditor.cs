using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;

public class SceneUISystemAuditor : EditorWindow
{
    [MenuItem("Tools/Board/Audit UI System (DragLayer, Raycaster, EventSystem)")]
    static void ShowWindow()
    {
        var window = GetWindow<SceneUISystemAuditor>("UI System Audit");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    Vector2 scrollPos;

    void OnGUI()
    {
        GUILayout.Label("Scene UI System Audit", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Audit", GUILayout.Height(30)))
        {
            RunAudit();
        }

        GUILayout.Space(10);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.EndScrollView();
    }

    void RunAudit()
    {
        Debug.Log("=== UI SYSTEM AUDIT START ===\n");

        bool allGood = true;

        // === CHECK 1: Canvas_Board ===
        var canvasBoard = GameObject.Find("Canvas_Board");
        if (!canvasBoard)
        {
            Debug.LogError("[FAIL] Canvas_Board GameObject not found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log($"[OK] Canvas_Board found: {canvasBoard.name}");

            // Check for Canvas component
            var canvas = canvasBoard.GetComponent<Canvas>();
            if (canvas)
            {
                Debug.Log($"  [OK] Canvas component: RenderMode={canvas.renderMode}");
            }
            else
            {
                Debug.LogWarning("  [WARN] Canvas component missing!");
                allGood = false;
            }

            // Check for GraphicRaycaster (CRITICAL for UI interaction)
            var raycaster = canvasBoard.GetComponent<GraphicRaycaster>();
            if (raycaster)
            {
                Debug.Log($"  [OK] GraphicRaycaster component found");
            }
            else
            {
                Debug.LogError("  [FAIL] GraphicRaycaster component MISSING! UI clicks won't work!");
                Debug.LogError("         FIX: Select Canvas_Board and add Component > Event > GraphicRaycaster");
                allGood = false;
            }

            // === CHECK 2: DragLayer ===
            Transform dragLayer = canvasBoard.transform.Find("DragLayer");
            if (!dragLayer)
            {
                Debug.LogError("  [FAIL] DragLayer not found under Canvas_Board!");
                Debug.LogError("         FIX: Create empty GameObject named 'DragLayer' as child of Canvas_Board");
                Debug.LogError("         - Right-click Canvas_Board > Create Empty");
                Debug.LogError("         - Rename to 'DragLayer'");
                Debug.LogError("         - Should have RectTransform (added automatically)");
                allGood = false;
            }
            else
            {
                Debug.Log($"  [OK] DragLayer found: {dragLayer.name}");
                var rectTransform = dragLayer.GetComponent<RectTransform>();
                if (rectTransform)
                {
                    Debug.Log($"      RectTransform: anchors=({rectTransform.anchorMin}, {rectTransform.anchorMax})");
                }
            }

            // Check for MergeBoard under Canvas_Board
            Transform mergeBoard = canvasBoard.transform.Find("MergeBoard");
            if (mergeBoard)
            {
                Debug.Log($"  [OK] MergeBoard found with {mergeBoard.childCount} children");
            }
            else
            {
                Debug.LogWarning("  [WARN] MergeBoard not found as child of Canvas_Board");
            }
        }

        // === CHECK 3: EventSystem ===
        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (!eventSystem)
        {
            Debug.LogError("[FAIL] EventSystem not found in scene!");
            Debug.LogError("       FIX: GameObject > UI > Event System");
            allGood = false;
        }
        else
        {
            Debug.Log($"[OK] EventSystem found: {eventSystem.gameObject.name}");

            var inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule)
            {
                Debug.Log($"  [OK] StandaloneInputModule found");
            }
            else
            {
                Debug.LogWarning("  [WARN] StandaloneInputModule not found (might be using new Input System)");
            }
        }

        // === CHECK 4: BoardTileView interfaces ===
        // Use reflection to safely check for BoardTileView type
        System.Type boardTileViewType = System.Type.GetType("AQ.App.UI.Board.BoardTileView, Assembly-CSharp");
        
        if (boardTileViewType == null)
        {
            Debug.LogWarning("[WARN] BoardTileView type not found - checking all MonoBehaviours");
            var allMonos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mono in allMonos)
            {
                if (mono.GetType().Name == "BoardTileView")
                {
                    boardTileViewType = mono.GetType();
                    break;
                }
            }
        }

        if (boardTileViewType != null)
        {
            var boardTiles = FindObjectsByType(boardTileViewType, FindObjectsSortMode.None);
            Debug.Log($"\n[INFO] Found {boardTiles.Length} BoardTileView components");
            
            if (boardTiles.Length > 0)
            {
                var sampleTile = boardTiles[0];
                var tileType = sampleTile.GetType();
                
                // Check if BoardTileView implements required interfaces
                bool hasPointerDown = typeof(IPointerDownHandler).IsAssignableFrom(tileType);
                bool hasBeginDrag = typeof(IBeginDragHandler).IsAssignableFrom(tileType);
                bool hasDrag = typeof(IDragHandler).IsAssignableFrom(tileType);
                bool hasEndDrag = typeof(IEndDragHandler).IsAssignableFrom(tileType);

                Debug.Log($"\nBoardTileView Interface Check:");
                Debug.Log($"  IPointerDownHandler: {(hasPointerDown ? "[OK]" : "[MISSING]")}");
                Debug.Log($"  IBeginDragHandler: {(hasBeginDrag ? "[OK]" : "[MISSING]")}");
                Debug.Log($"  IDragHandler: {(hasDrag ? "[OK]" : "[MISSING]")}");
                Debug.Log($"  IEndDragHandler: {(hasEndDrag ? "[OK]" : "[MISSING]")}");

                if (!hasPointerDown || !hasBeginDrag || !hasDrag || !hasEndDrag)
                {
                    Debug.LogError("\n[FAIL] BoardTileView is missing drag/drop interfaces!");
                    Debug.LogError("       This is why drag & drop doesn't work.");
                    allGood = false;
                }

                // Check if tiles have UI Button component
                var button = sampleTile as Component;
                if (button != null)
                {
                    var buttonComp = button.GetComponent<Button>();
                    if (buttonComp)
                    {
                        Debug.Log($"  [INFO] Tiles have UI Button component");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[WARN] No BoardTileView components found in scene");
        }

        // === SUMMARY ===
        Debug.Log("\n=== AUDIT SUMMARY ===");
        if (allGood)
        {
            Debug.Log("<color=green><b>ALL CHECKS PASSED!</b></color>");
        }
        else
        {
            Debug.LogError("<color=red><b>ISSUES FOUND - See errors above for fixes</b></color>");
        }
        Debug.Log("=== UI SYSTEM AUDIT END ===\n");
    }
}
#endif