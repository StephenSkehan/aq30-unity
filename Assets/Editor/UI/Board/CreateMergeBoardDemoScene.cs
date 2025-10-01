#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.Editor.MergeBoardDemo
{
    public static class CreateMergeBoardDemoScene
    {
        private const string SceneDir = "Assets/Scenes/Demo";
        private const string ScenePath = SceneDir + "/MergeBoard_Demo.unity";
        private const string TilePrefabPath = "Assets/UI/Prefabs/board_tile_slot.prefab";

        [MenuItem("AQ/Scenes/Create Merge Board Demo Scene")]
        public static void CreateFromMenu() => CreateScene();

        public static void CreateScene()
        {
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "MergeBoard_Demo";

                // EventSystem (+ Input System UI Input Module if available)
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                AddIfExists(es, "UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule") == null)
                    es.AddComponent<StandaloneInputModule>();

                // Canvas (+ scaler)
                var canvasGO = new GameObject("Canvas_Board", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGO.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                // Board + Grid roots
                var boardRoot = MakeUI("BoardRoot", canvasGO.transform, stretch: true);
                var gridRoot  = MakeUI("GridRoot",  boardRoot,        stretch: true);
                var gridRT    = gridRoot.GetComponent<RectTransform>();
                gridRT.offsetMin = new Vector2(60, 300);
                gridRT.offsetMax = new Vector2(-60, -200);

                // ❗ FIX: gridRoot is a Transform -> use .gameObject.AddComponent
                var grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(160, 160);
                grid.spacing  = new Vector2(12, 12);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;

                // Try canonical prefab; fallback to simple tiles
                var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TilePrefabPath);

                const int rows = 5;
                const int cols = 5;
                for (var r = 0; r < rows; r++)
                {
                    for (var c = 0; c < cols; c++)
                    {
                        GameObject tile;
                        if (slotPrefab != null)
                        {
                            tile = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, gridRoot.transform);
                            tile.name = $"slot_{r}_{c}";
                        }
                        else
                        {
                            // ❗ FIX: MakeUI returns Transform -> add .gameObject
                            tile = MakeUI($"slot_{r}_{c}", gridRoot.transform).gameObject;
                            var img = tile.AddComponent<Image>();
                            img.raycastTarget = true;
                        }
                    }
                }

                // Attach runtime pieces if they exist (reflection keeps deps loose)
                AddIfExists(boardRoot.gameObject, "MergeBoardController");
                AddIfExists(boardRoot.gameObject, "DemoBoardPopulator");

                // Ensure folder & save
                if (!AssetDatabase.IsValidFolder(SceneDir))
                {
                    var parts = SceneDir.Substring("Assets/".Length).Split('/');
                    var acc = "Assets";
                    foreach (var p in parts)
                    {
                        var next = $"{acc}/{p}";
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(acc, p);
                        acc = next;
                    }
                }

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
                Debug.Log($"✅ Merge Board demo scene created at {ScenePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Merge Board demo creation failed: {ex}");
                throw;
            }
        }

        // ---- helpers ----

        private static Transform MakeUI(string name, Transform parent, bool stretch = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            go.transform.SetParent(parent, worldPositionStays: false);
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100, 100);
            }
            return go.transform;
        }

        private static void AddIfExists(GameObject go, string typeNameOrFullName)
        {
            var t = GetTypeByName(typeNameOrFullName);
            if (t != null && typeof(MonoBehaviour).IsAssignableFrom(t))
            {
                go.AddComponent(t);
                Debug.Log($"[AddIfExists] Attached {t.FullName} to {go.name}");
            }
            else
            {
                Debug.Log($"[AddIfExists] Type not found: {typeNameOrFullName} (skipped)");
            }
        }

        private static Type GetTypeByName(string nameOrFull)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(nameOrFull, throwOnError: false);
                    if (t != null) return t;
                    t = asm.GetTypes().FirstOrDefault(x => x.Name == nameOrFull);
                    if (t != null) return t;
                }
                catch { /* skip */ }
            }
            return null;
        }
    }
}
#endif
