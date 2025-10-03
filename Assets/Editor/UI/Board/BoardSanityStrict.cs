// SPDX-License-Identifier: MIT
// In-Editor scene audit focused on the MergeBoard grid.
// Menu path: AQ/Board/Audit (Strict)/Run
// Outputs: Assets/_audit/board_sanity_strict_YYYYMMDD_HHMMSS.{txt,json}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.Editor.Board
{
    public static class BoardSanityStrict
    {
        // ---------- Data Model (serialized to JSON) ----------
        [Serializable] public class Vec2 { public float x, y; public Vec2() { } public Vec2(Vector2 v) { x = v.x; y = v.y; } }

        [Serializable]
        public class GridInfo
        {
            public string path;
            public Vec2 cellSize;
            public Vec2 spacing;
            public string constraint;     // e.g. "FixedColumnCount"
            public int constraintCount;   // e.g. 7
            public int childAlignment;    // enum int value
        }

        [Serializable]
        public class DragLayerInfo
        {
            public bool present;
            public string path;
            public bool topmostSibling;
            public bool hasOwnCanvas;
            public bool overrideSorting;
            public int sortingOrder;
            public Vec2 anchorMin;
            public Vec2 anchorMax;
            public Vec2 pivot;
        }

        [Serializable]
        public class GeneratorInfo
        {
            public bool present;
            public string slotName;
            public int row;
            public int col;
        }

        [Serializable]
        public class SlotInfo
        {
            public string name;
            public int row;
            public int col;
            public bool hasButton;
            public int  buttonOnClickCount;
            public bool hasBoardTileView;
            public bool hasIconImage;
            public bool hasIconCanvasGroup;
            public bool hasHighlightImage;
            public bool hasBadgeImage;
            public bool hasCountTMP;
            public bool hasBoxCollider2D;
        }

        [Serializable]
        public class Report
        {
            public string timestampIso;
            public string scenePath;

            public int expectedRows;
            public int expectedCols;
            public int expectedSlots;

            public int  slotCountFound;
            public bool slotsAllFound;
            public string[] missingSlots;
            public string[] extraSlots;

            public GridInfo grid;
            public bool gridSpacingIs2x2;

            public DragLayerInfo dragLayer;
            public bool eventSystemPresent;
            public GeneratorInfo generator;

            public List<SlotInfo> slots = new List<SlotInfo>();
            public List<string> log = new List<string>();
        }

        // ---------- Menu Entry ----------
        [MenuItem("AQ/Board/Audit (Strict)/Run", priority = 52)]
        public static void RunAuditStrict()
        {
            // Ensure we are working with the active scene.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var active = SceneManager.GetActiveScene();
            var report = BuildReport(active);

            // Console output (chunked to avoid truncation)
            var consoleTxt = BuildConsoleText(report);
            WriteBigToConsole(consoleTxt);

            // File outputs
            var (txtPath, jsonPath) = WriteFiles(report);

            Debug.Log($"AQ Board Sanity (Strict): wrote\n - {txtPath}\n - {jsonPath}");
        }

        // ---------- Core Audit ----------
        private static Report BuildReport(Scene scene)
        {
            var r = new Report
            {
                timestampIso = DateTime.UtcNow.ToString("o"),
                scenePath     = scene.path
            };

            // Find grid (Canvas_Board/MergeBoard preferred)
            GridLayoutGroup grid = null;
            string gridPath = "Canvas_Board/MergeBoard";

            var canvasBoard = GameObject.Find("Canvas_Board");
            if (canvasBoard != null)
            {
                var t = canvasBoard.transform.Find("MergeBoard");
                if (t != null) grid = t.GetComponent<GridLayoutGroup>();
            }

            // Fallbacks if not at the preferred path
            if (grid == null)
            {
                // first GridLayoutGroup named "MergeBoard"
                var all = UnityEngine.Object.FindObjectsByType<GridLayoutGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                grid = all.FirstOrDefault(g => g.name == "MergeBoard");
                if (grid != null)
                {
                    gridPath = GetHierarchyPath(grid.transform);
                }
            }

            // Basic grid info
            if (grid != null)
            {
                r.grid = new GridInfo
                {
                    path            = gridPath,
                    cellSize        = new Vec2(grid.cellSize),
                    spacing         = new Vec2(grid.spacing),
                    constraint      = grid.constraint.ToString(),
                    constraintCount = grid.constraintCount,
                    childAlignment  = (int)grid.childAlignment
                };
            }
            else
            {
                r.log.Add("ERROR: GridLayoutGroup (MergeBoard) not found.");
                r.grid = new GridInfo
                {
                    path            = "<not found>",
                    cellSize        = new Vec2(Vector2.zero),
                    spacing         = new Vec2(Vector2.zero),
                    constraint      = "Unknown",
                    constraintCount = 0,
                    childAlignment  = 0
                };
            }

            // Determine expected rows/cols from constraint + total children if possible.
            int totalSlots = 0;
            Transform gridT = grid != null ? grid.transform : null;

            if (gridT != null)
            {
                totalSlots = Enumerable.Range(0, gridT.childCount)
                                       .Select(i => gridT.GetChild(i))
                                       .Count(ch => ch.name.StartsWith("slot_", StringComparison.OrdinalIgnoreCase));

                if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount && grid.constraintCount > 0)
                {
                    r.expectedCols = grid.constraintCount;
                    r.expectedRows = totalSlots > 0 ? Mathf.CeilToInt((float)totalSlots / r.expectedCols) : 0;
                }
                else if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount && grid.constraintCount > 0)
                {
                    r.expectedRows = grid.constraintCount;
                    r.expectedCols = totalSlots > 0 ? Mathf.CeilToInt((float)totalSlots / r.expectedRows) : 0;
                }
                else
                {
                    // Default to 9x7 if indeterminate (project default)
                    r.expectedRows = 9;
                    r.expectedCols = 7;
                }
            }
            else
            {
                // If grid missing, use project defaults to keep report shape stable.
                r.expectedRows = 9;
                r.expectedCols = 7;
            }

            r.expectedSlots = r.expectedRows * r.expectedCols;

            // Slots present/missing/extra
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var slotInfos = new List<SlotInfo>();

            if (gridT != null)
            {
                // Build fast lookup for types by name (optional components)
                var boardTileViewType = FindTypeByName("AQ.App.UI.Board.BoardTileView");
                var generatorType     = FindTypeByName("AQ.App.UI.Board.GeneratorTile");

                for (int i = 0; i < gridT.childCount; i++)
                {
                    var child = gridT.GetChild(i);
                    if (!child.name.StartsWith("slot_", StringComparison.OrdinalIgnoreCase)) continue;

                    present.Add(child.name);

                    ParseRowCol(child.name, out int row, out int col);

                    var btn = child.GetComponent<Button>();
                    var slotInfo = new SlotInfo
                    {
                        name                = child.name,
                        row                 = row,
                        col                 = col,
                        hasButton           = btn != null,
                        buttonOnClickCount  = btn != null ? btn.onClick.GetPersistentEventCount() : 0,
                        hasBoardTileView    = boardTileViewType != null && child.GetComponent(boardTileViewType) != null,
                        hasIconImage        = HasImage(child, "Icon"),
                        hasIconCanvasGroup  = HasCanvasGroup(child, "Icon"),
                        hasHighlightImage   = HasImage(child, "Highlight"),
                        hasBadgeImage       = HasImage(child, "Badge"),
                        hasCountTMP         = HasTMP(child,   "Badge/Count"),
                        hasBoxCollider2D    = child.GetComponent<BoxCollider2D>() != null
                    };

                    slotInfos.Add(slotInfo);
                }

                r.slotCountFound = slotInfos.Count;

                // Detect generator location if any (by component type or child named "Generator")
                Transform generatorSlot = null;
                if (generatorType != null)
                {
                    foreach (var s in slotInfos)
                    {
                        var tr = gridT.Find(s.name);
                        if (tr != null && tr.GetComponent(generatorType) != null)
                        {
                            generatorSlot = tr;
                            r.generator = new GeneratorInfo { present = true, slotName = s.name, row = s.row, col = s.col };
                            break;
                        }
                    }
                }
                if (generatorSlot == null)
                {
                    // fallback by child name
                    var possible = gridT.GetComponentsInChildren<Transform>(true)
                                        .FirstOrDefault(t => t.name.Equals("Generator", StringComparison.OrdinalIgnoreCase));
                    if (possible != null)
                    {
                        var parentSlot = possible.parent;
                        while (parentSlot != null && !parentSlot.name.StartsWith("slot_", StringComparison.OrdinalIgnoreCase))
                            parentSlot = parentSlot.parent;

                        if (parentSlot != null)
                        {
                            ParseRowCol(parentSlot.name, out int rRow, out int rCol);
                            r.generator = new GeneratorInfo { present = true, slotName = parentSlot.name, row = rRow, col = rCol };
                        }
                    }
                }

                if (r.generator == null)
                    r.generator = new GeneratorInfo { present = false, slotName = "", row = 0, col = 0 };
            }

            // Compute missing / extra against expected grid shape (slot_rr_cc)
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int rr = 0; rr < r.expectedRows; rr++)
                for (int cc = 0; cc < r.expectedCols; cc++)
                    expected.Add($"slot_{rr:00}_{cc:00}");

            var missing = expected.Except(present).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
            var extra   = present.Except(expected).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();

            r.slots            = slotInfos.OrderBy(s => s.row).ThenBy(s => s.col).ToList();
            r.slotsAllFound    = missing.Length == 0 && extra.Length == 0;
            r.missingSlots     = missing;
            r.extraSlots       = extra;

            // Grid spacing rule (expect 2,2)
            r.gridSpacingIs2x2 = r.grid != null && Mathf.Approximately(r.grid.spacing.x, 2f) && Mathf.Approximately(r.grid.spacing.y, 2f);

            // DragLayer diagnostics (Canvas_Board/DragLayer)
            r.dragLayer = InspectDragLayer();

            // EventSystem present?
            bool evtPresent = UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null;
            r.eventSystemPresent = evtPresent;

            // Build summary lines
            r.log.Add($"INFO: Scene: {r.scenePath}");
            if (r.grid != null && !r.gridSpacingIs2x2)
                r.log.Add($"WARN: Grid spacing: ({r.grid.spacing.x},{r.grid.spacing.y}) expected (2,2).");

            r.log.Add($"INFO: Slots: {r.slotCountFound} found (expected {r.expectedSlots}) — {(r.slotsAllFound ? "OK" : "MISMATCH")}");

            // Colliders on UI tiles warning
            int colliderCount = slotInfos.Count(s => s.hasBoxCollider2D);
            if (colliderCount > 0)
                r.log.Add($"WARN: BoxCollider2D present on {colliderCount} UI slots (not needed for UI EventSystem).");

            // DragLayer line
            if (r.dragLayer.present)
            {
                var ownCanvasStr = r.dragLayer.hasOwnCanvas ? "own canvas" : "no own canvas";
                r.log.Add($"INFO: DragLayer: {(r.dragLayer.topmostSibling ? "topmost" : "not topmost")} | {ownCanvasStr}.");
                r.log.Add($"INFO: DragLayer anchors: min=({r.dragLayer.anchorMin.x:0.00}, {r.dragLayer.anchorMin.y:0.00}) max=({r.dragLayer.anchorMax.x:0.00}, {r.dragLayer.anchorMax.y:0.00}) pivot=({r.dragLayer.pivot.x:0.00}, {r.dragLayer.pivot.y:0.00}) (expect min(0,0) max(1,1) pivot 0.5).");
            }
            else
            {
                r.log.Add("WARN: DragLayer not found.");
            }

            r.log.Add($"INFO: EventSystem: {(evtPresent ? "present" : "missing")}");

            if (r.generator.present)
                r.log.Add($"INFO: GeneratorTile found in {r.generator.slotName} (r{r.generator.row}, c{r.generator.col}).");
            else
                r.log.Add("WARN: GeneratorTile not found.");

            return r;
        }

        // ---------- Helpers ----------
        private static DragLayerInfo InspectDragLayer()
        {
            var info = new DragLayerInfo { present = false };

            var canvasBoard = GameObject.Find("Canvas_Board");
            if (canvasBoard == null) return info;

            var drag = canvasBoard.transform.Find("DragLayer");
            if (drag == null) return info;

            info.present = true;
            info.path = $"{GetHierarchyPath(drag)}";

            // Topmost among siblings?
            info.topmostSibling = drag.GetSiblingIndex() == drag.parent.childCount - 1;

            // Own Canvas?
            var c = drag.GetComponent<Canvas>();
            info.hasOwnCanvas    = c != null;
            info.overrideSorting = c != null && c.overrideSorting;
            info.sortingOrder    = c != null ? c.sortingOrder : 0;

            var rt = drag.GetComponent<RectTransform>();
            if (rt != null)
            {
                info.anchorMin = new Vec2(rt.anchorMin);
                info.anchorMax = new Vec2(rt.anchorMax);
                info.pivot     = new Vec2(rt.pivot);
            }
            else
            {
                info.anchorMin = new Vec2(Vector2.zero);
                info.anchorMax = new Vec2(Vector2.one);
                info.pivot     = new Vec2(new Vector2(0.5f, 0.5f));
            }

            return info;
        }

        private static void ParseRowCol(string slotName, out int row, out int col)
        {
            row = col = 0;
            // slot_00_06
            var parts = slotName.Split('_');
            if (parts.Length >= 3)
            {
                int.TryParse(parts[1], out row);
                int.TryParse(parts[2], out col);
            }
        }

        private static bool HasImage(Transform root, string localPath)
        {
            var t = root.Find(localPath);
            return t != null && t.GetComponent<Image>() != null;
        }

        private static bool HasCanvasGroup(Transform root, string localPath)
        {
            var t = root.Find(localPath);
            return t != null && t.GetComponent<CanvasGroup>() != null;
        }

        private static bool HasTMP(Transform root, string localPath)
        {
            var t = root.Find(localPath);
            return t != null && t.GetComponent<TextMeshProUGUI>() != null;
        }

        private static string GetHierarchyPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        private static Type FindTypeByName(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var tp = asm.GetType(fullName);
                if (tp != null) return tp;
            }
            return null;
        }

        private static string BuildConsoleText(Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== BOARD SANITY AUDIT (STRICT): {r.scenePath} @ {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            foreach (var line in r.log)
                sb.AppendLine(line);

            if (!r.slotsAllFound)
            {
                if (r.missingSlots.Length > 0)
                {
                    sb.AppendLine("MISSING:");
                    foreach (var m in r.missingSlots) sb.AppendLine($"  - {m}");
                }
                if (r.extraSlots.Length > 0)
                {
                    sb.AppendLine("EXTRA:");
                    foreach (var e in r.extraSlots) sb.AppendLine($"  - {e}");
                }
            }

            // Print a brief preview of first few slots
            int preview = Mathf.Min(5, r.slots.Count);
            if (preview > 0)
            {
                sb.AppendLine($"SLOTS (first {preview} of {r.slots.Count}):");
                for (int i = 0; i < preview; i++)
                {
                    var s = r.slots[i];
                    sb.AppendLine($"  {s.name} r{s.row} c{s.col} | btn:{s.hasButton} view:{s.hasBoardTileView} icon:{s.hasIconImage} badge:{s.hasBadgeImage} tmp:{s.hasCountTMP} col2D:{s.hasBoxCollider2D}");
                }
            }

            return sb.ToString();
        }

        private static void WriteBigToConsole(string text)
        {
            // Unity truncates very long logs; chunk to ~6000 chars.
            const int chunk = 6000;
            if (text.Length <= chunk) { Debug.Log(text); return; }
            for (int i = 0; i < text.Length; i += chunk)
            {
                var part = text.Substring(i, Math.Min(chunk, text.Length - i));
                Debug.Log(part);
            }
        }

        private static (string txtPath, string jsonPath) WriteFiles(Report r)
        {
            var dir = "Assets/_audit";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = $"board_sanity_strict_{stamp}";
            var txtPath  = Path.Combine(dir, baseName + ".txt").Replace("\\", "/");
            var jsonPath = Path.Combine(dir, baseName + ".json").Replace("\\", "/");

            // TXT (console-style summary)
            var txt = BuildConsoleText(r);
            File.WriteAllText(txtPath, txt, Encoding.UTF8);

            // JSON (structured)
            var json = JsonUtility.ToJson(r, true);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);

            AssetDatabase.Refresh();
            return (txtPath, jsonPath);
        }
    }
}
#endif
