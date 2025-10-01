// Assets/Editor/UI/Board/RebuildMergeBoardDemo.cs
// Idempotently creates a minimal Merge Board demo scene with camera, canvas, grid, slots,
// and best-guess wiring for Presenter/Controller/Populator.
//
// Menu: AQ / Board / Rebuild Merge Board Demo (idempotent)

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.Editor.UI.Board
{
    public static class MergeBoardDemoBuilder
    {
        private const string ScenePathPrimary   = "Assets/Scenes/MergeBoard_Demo.unity";
        private const string ScenePathFallback  = "Assets/_Recovery/MergeBoard_Demo.unity";
        private const string CanvasName         = "Canvas_Board";
        private const string BoardRootName      = "BoardRoot";
        private const string EventSystemName    = "EventSystem";
        private const string CameraName         = "Camera_Main";
        private const string SlotPrefabSearch   = "board_tile_slot t:prefab";

        // Grid config
        private const int Rows   = 5;
        private const int Cols   = 5;
        private static readonly Vector2 CellSize = new Vector2(128, 128);
        private static readonly Vector2 Spacing  = new Vector2(8, 8);

        [MenuItem("AQ/Board/Rebuild Merge Board Demo (idempotent)")]
        public static void BuildDemoScene()
        {
            // 1) New scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MergeBoard_Demo";

            // 2) Ensure Camera
            EnsureCamera();

            // 3) Ensure EventSystem (Input System–aware)
            EnsureEventSystem();

            // 4) Ensure Canvas + BoardRoot + Grid
            var canvas = EnsureCanvas(out RectTransform boardRoot, out GridLayoutGroup grid);
            ConfigureGrid(grid);

            // 5) Get slot prefab
            var slotPrefab = FindSlotPrefab();
            if (slotPrefab == null)
                Debug.LogWarning("[AQ] Could not find 'board_tile_slot' prefab. The grid will be empty.");

            // 6) Populate visual grid
            ClearImmediateChildren(boardRoot);
            var total = Rows * Cols;
            for (int i = 0; i < total; i++)
            {
                if (slotPrefab != null)
                {
                    // Instantiate into this scene then parent under BoardRoot
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, canvas.gameObject.scene);
                    var rt = inst.transform as RectTransform;
                    rt.SetParent(boardRoot, false);
                    inst.name = $"slot_{i:D2}";
                }
                else
                {
                    // placeholder
                    var go = new GameObject($"slot_{i:D2}", typeof(RectTransform), typeof(Image));
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(boardRoot, false);
                    rt.sizeDelta = CellSize;
                    go.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
                }
            }

            // 7) Add & bind runtime scripts if present
            TryAddAndBindRuntime(canvas.gameObject.scene, boardRoot, slotPrefab);

            // 8) Save
            var savePath = EnsureFolderAndPickPath(ScenePathPrimary, ScenePathFallback);
            if (EditorSceneManager.SaveScene(scene, savePath))
                Debug.Log($"[AQ] MergeBoard demo scene saved: {savePath}");
            else
                Debug.LogWarning("[AQ] Scene save reported false; please Save manually.");
        }

        private static string EnsureFolderAndPickPath(string primary, string fallback)
        {
            var primaryDir = System.IO.Path.GetDirectoryName(primary).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(primaryDir))
            {
                var parts = primaryDir.Split('/');
                var path = "Assets";
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = $"{path}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(path, parts[i]);
                    path = next;
                }
            }
            return primary;
        }

        private static Camera EnsureCamera()
        {
            var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam != null) return cam;

            var go = new GameObject(CameraName);
            cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.07f, 1f);
            go.transform.position = new Vector3(0, 0, -10);
            return cam;
        }

        private static void EnsureEventSystem()
        {
            var existing = GameObject.Find(EventSystemName);
            if (existing == null)
            {
                existing = new GameObject(EventSystemName);
                existing.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // Try Input System module first (no compile-time dependency).
            var inputSystemType = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem",
                throwOnError: false);

            var legacy = existing.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            var inputSystem = inputSystemType != null ? existing.GetComponent(inputSystemType) : null;

            if (inputSystemType != null)
            {
                // Ensure InputSystemUIInputModule is present
                if (inputSystem == null) inputSystem = existing.AddComponent(inputSystemType);
                // Remove legacy to avoid InvalidOperationException spam
                if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy);
            }
            else
            {
                // Fall back to legacy StandaloneInputModule
                if (legacy == null) existing.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static Canvas EnsureCanvas(out RectTransform boardRoot, out GridLayoutGroup grid)
        {
            var canvasGO = GameObject.Find(CanvasName);
            if (canvasGO == null)
                canvasGO = new GameObject(CanvasName, typeof(RectTransform));

            if (canvasGO.GetComponent<RectTransform>() == null)
                canvasGO.AddComponent<RectTransform>();

            var canvas = canvasGO.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>() ?? canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f;

            if (canvasGO.GetComponent<GraphicRaycaster>() == null)
                canvasGO.AddComponent<GraphicRaycaster>();

            // BoardRoot
            var boardGO = canvas.transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .FirstOrDefault(g => g.name == BoardRootName);
            if (boardGO == null)
            {
                boardGO = new GameObject(BoardRootName, typeof(RectTransform));
                boardGO.transform.SetParent(canvasGO.transform, false);
            }

            boardRoot = boardGO.GetComponent<RectTransform>();
            StretchToFull(boardRoot);

            grid = boardGO.GetComponent<GridLayoutGroup>() ?? boardGO.AddComponent<GridLayoutGroup>();
            return canvas;
        }

        private static void ConfigureGrid(GridLayoutGroup grid)
        {
            grid.cellSize = CellSize;
            grid.spacing  = Spacing;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis   = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Cols;
        }

        private static void StretchToFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static GameObject FindSlotPrefab()
        {
            var guids = AssetDatabase.FindAssets(SlotPrefabSearch);
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            // Known path fallback
            const string known = "Assets/UI/Prefabs/board_tile_slot.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(known);
        }

        private static void ClearImmediateChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
        }

        private static void TryAddAndBindRuntime(Scene scene, RectTransform boardRoot, GameObject slotPrefab)
        {
            var root = new GameObject("MergeBoard");
            var presenter = TryAddBySimpleName(root, "MergeBoardPresenter") as MonoBehaviour;
            var controller = TryAddBySimpleName(root, "MergeBoardController") as MonoBehaviour;
            var populator  = TryAddBySimpleName(root, "DemoBoardPopulator")  as MonoBehaviour;

            // Presenter bindings
            if (presenter != null)
            {
                var so = new SerializedObject(presenter);
                TrySetInt(so,   new[] { "rows","rowCount","Rows","RowCount" }, Rows);
                TrySetInt(so,   new[] { "cols","columns","Cols","Columns"   }, Cols);
                TrySetObj(so,   new[] { "gridParent","grid","root","GridParent","GridRoot","Parent","BoardRoot" }, boardRoot);
                TrySetObj(so,   new[] { "slotPrefab","tilePrefab","itemPrefab","boardTilePrefab","SlotPrefab"   }, slotPrefab);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Controller bindings (now also sets prefab!)
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                TrySetObj(so, new[] { "presenter","boardPresenter","mergeBoardPresenter" }, presenter);
                TrySetObj(so, new[] { "gridParent","boardRoot","GridParent","BoardRoot"  }, boardRoot);
                TrySetInt(so, new[] { "rows","rowCount","Rows","RowCount"                }, Rows);
                TrySetInt(so, new[] { "cols","columns","Cols","Columns"                  }, Cols);
                TrySetObj(so, new[] { "slotPrefab","tilePrefab","itemPrefab","boardTilePrefab","Prefab" }, slotPrefab);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Populator optional links
            if (populator != null)
            {
                var so = new SerializedObject(populator);
                TrySetObj(so, new[] { "controller","mergeController","boardController" }, controller);
                TrySetObj(so, new[] { "presenter","mergePresenter","boardPresenter"    }, presenter);
                TrySetObj(so, new[] { "gridParent","boardRoot"                         }, boardRoot);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Component TryAddBySimpleName(GameObject go, string typeName)
        {
            var type = TypeCache.GetTypesDerivedFrom<MonoBehaviour>().FirstOrDefault(t => t.Name == typeName);
            if (type == null) return null;
            return go.GetComponent(type) ?? go.AddComponent(type);
        }

        private static void TrySetInt(SerializedObject so, string[] names, int value)
        {
            foreach (var n in names)
            {
                var p = so.FindProperty(n);
                if (p != null && p.propertyType == SerializedPropertyType.Integer)
                    p.intValue = value;
            }
        }

        private static void TrySetObj(SerializedObject so, string[] names, UnityEngine.Object obj)
        {
            foreach (var n in names)
            {
                var p = so.FindProperty(n);
                if (p != null && p.propertyType == SerializedPropertyType.ObjectReference)
                    p.objectReferenceValue = obj;
            }
        }
    }
}
#endif
