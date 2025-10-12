#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.UI.Board
{
    /// <summary>
    /// Audits & fixes MergeBoard gutters so the visible line between items
    /// is a specific *total* thickness in pixels (default 2 px).
    /// Works without a typed reference to MergeBoardController.
    /// </summary>
    public static class AuditAndFix_Gutters
    {
        // ---- CONFIG ----
        // Total visible line BETWEEN icons. With two adjacent tiles this is split 50/50 (floor/ceil).
        const int TotalGutterPx = 2;
        const string TilePrefabName = "board_tile_slot";
        const string IconNodeName   = "Icon";
        const string BoardRootName  = "BoardRoot";

        [MenuItem("AQ/Board/Audit & Fix Gutters (total 2 px)")]
        public static void RunMenu() => Run(verboseDialog: true);

        [InitializeOnLoadMethod]
        static void HookPlayMode()
        {
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.ExitingEditMode) Run(verboseDialog: false);
            };
        }

        static void Run(bool verboseDialog)
        {
            var log = new System.Text.StringBuilder();
            int fixes = 0;

            // 1) Scene grid
            var boardRoot = GameObject.Find(BoardRootName)?.GetComponent<RectTransform>();
            if (!boardRoot)
            {
                Log(log, $"Scene: '{BoardRootName}' not found (skipping grid checks).");
            }
            else
            {
                var grid = boardRoot.GetComponent<GridLayoutGroup>() ?? boardRoot.gameObject.AddComponent<GridLayoutGroup>();
                Undo.RecordObject(grid, "AQ Grid Fix");
                var before = $"spacing={grid.spacing}, constraint={grid.constraint}, count={grid.constraintCount}";
                grid.spacing = Vector2.zero;
                grid.childAlignment = TextAnchor.UpperLeft;
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                if (grid.constraintCount <= 0) grid.constraintCount = 7; // harmless default; controller will set properly
                EditorUtility.SetDirty(grid);
                fixes++;
                Log(log, $"Scene Grid: {before}  →  spacing=0, constraint=FixedColumnCount, count={grid.constraintCount}");
            }

            // 2) Tile prefab
            var prefab = FindTilePrefab();
            if (!prefab)
            {
                Log(log, $"Prefab '{TilePrefabName}' not found.");
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(prefab);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    // 2a) Remove outlines/shadows that thicken edges
                    int removedFx = 0;
                    foreach (var fx in root.GetComponentsInChildren<Shadow>(true)) { Undo.DestroyObjectImmediate(fx); removedFx++; }
                    foreach (var fx in root.GetComponentsInChildren<Outline>(true)) { Undo.DestroyObjectImmediate(fx); removedFx++; }
                    if (removedFx > 0) Log(log, $"Prefab: removed {removedFx} Outline/Shadow component(s).");

                    // 2b) Disable fat backgrounds/frames
                    var disabled = DisableBackgrounds(root);
                    if (disabled > 0) Log(log, $"Prefab: disabled {disabled} background/frame Image(s) (Bg/Frame/Border).");

                    // 2c) Ensure Icon exists and fills with exact insets for TOTAL gutter
                    var icon = EnsureIcon(root);
                    var rt = icon.GetComponent<RectTransform>();
                    var before = $"offMin={rt.offsetMin}, offMax={rt.offsetMax}, anchors={rt.anchorMin}->{rt.anchorMax}";
                    Undo.RecordObject(rt, "AQ Icon Insets");

                    // Split total gutter between left/right and bottom/top.
                    int left  = TotalGutterPx / 2;    // floor
                    int right = TotalGutterPx - left; // ceil
                    int bottom= left;
                    int top   = right;

                    rt.anchorMin = Vector2.zero;   // stretch
                    rt.anchorMax = Vector2.one;    // stretch
                    rt.offsetMin = new Vector2(left, bottom);
                    rt.offsetMax = new Vector2(-right, -top);
                    EditorUtility.SetDirty(rt);

                    // Image settings for crisp fill
                    var img = icon.GetComponent<Image>() ?? icon.AddComponent<Image>();
                    Undo.RecordObject(img, "AQ Icon Image");
                    img.type = Image.Type.Simple;
                    img.preserveAspect = true;
                    img.raycastTarget = false;
                    EditorUtility.SetDirty(img);

                    fixes++;
                    Log(log, $"Prefab Icon: {before}  →  offMin=({left},{bottom}), offMax=({-right},{-top}), stretch fill; Image.Simple preserveAspect.");
                }
                finally
                {
                    PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(prefab));
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            // 3) Nudge any live controllers’ grids in Edit mode (no typed reference)
            foreach (var ctrl in FindControllers())
            {
                var so = new SerializedObject(ctrl);
                var pCols = so.FindProperty("Cols");
                var pBoardRoot = so.FindProperty("boardRoot");
                var grid = (pBoardRoot?.objectReferenceValue as RectTransform)?.GetComponent<GridLayoutGroup>();
                if (grid)
                {
                    Undo.RecordObject(grid, "AQ Grid Column Count");
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    if (pCols != null && pCols.intValue > 0) grid.constraintCount = pCols.intValue;
                    grid.spacing = Vector2.zero;
                    EditorUtility.SetDirty(grid);
                }
            }

            var msg = (fixes > 0 ? "Audit & fix complete.\n" : "Nothing to change.\n") + log.ToString();
            if (verboseDialog) EditorUtility.DisplayDialog("AQ Gutters", msg, "OK");
            else Debug.Log($"[AQ] Gutters audit\n{msg}");
        }

        // ---------- helpers ----------
        static void Log(System.Text.StringBuilder sb, string line) => sb.AppendLine("• " + line);

        static GameObject FindTilePrefab()
        {
            // Prefer the prefab currently referenced by any MergeBoardController
            foreach (var ctrl in FindControllers())
            {
                var so = new SerializedObject(ctrl);
                var pTile = so.FindProperty("tilePrefab");
                var go = pTile?.objectReferenceValue as GameObject;
                if (go) return go;
            }
            // Fallback: search by name
            foreach (var guid in AssetDatabase.FindAssets($"{TilePrefabName} t:prefab"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go && go.name == TilePrefabName) return go;
            }
            return null;
        }

        static IEnumerable<MonoBehaviour> FindControllers()
        {
            var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in all)
            {
                var t = mb ? mb.GetType() : null;
                if (t == null) continue;
                if (t.Name == "MergeBoardController" || (t.FullName?.EndsWith(".MergeBoardController") ?? false))
                    yield return mb;
            }
        }

        static int DisableBackgrounds(GameObject prefabRoot)
        {
            int n = 0;
            var names = new HashSet<string>(new[] { "Bg", "Background", "Frame", "Border" });
            foreach (var img in prefabRoot.GetComponentsInChildren<Image>(true))
            {
                var nm = img.gameObject.name;
                if (names.Contains(nm) || nm.ToLowerInvariant().Contains("frame") || nm.ToLowerInvariant().Contains("border"))
                {
                    if (img.enabled)
                    {
                        Undo.RecordObject(img, "AQ Disable Background");
                        img.enabled = false; // safest: render off, keeps hierarchy intact
                        EditorUtility.SetDirty(img);
                        n++;
                    }
                }
            }
            return n;
        }

        static GameObject EnsureIcon(GameObject prefabRoot)
        {
            // Prefer existing child named "Icon"; otherwise create one under "Item" if present (or root).
            var icon = prefabRoot.GetComponentsInChildren<Transform>(true)
                                 .FirstOrDefault(t => t.name == IconNodeName)?.gameObject;
            if (!icon)
            {
                icon = new GameObject(IconNodeName, typeof(RectTransform), typeof(Image));
                var parent = prefabRoot.transform;
                var item = parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Item");
                if (item) parent = item;
                icon.transform.SetParent(parent, false);
            }
            return icon;
        }
    }
}
#endif
