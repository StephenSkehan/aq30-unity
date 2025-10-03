// Assets/Editor/UI/Board/BoardSanityAudit_Strict.cs
// Editor-only scene audit (strict, read-only). Writes TXT+JSON to _audit/.
// Menu: AQ > Board > Run Sanity Audit (Strict)

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.App.Editor.Board
{
    public static class BoardSanityAuditStrict
    {
        private const int ExpectedRows = 9;
        private const int ExpectedCols = 7;

        // --------- DTOs serialized to JSON ---------
        [Serializable]
        private class SlotInfo
        {
            public string name;
            public int row;
            public int col;
            public bool hasButton;
            public int buttonOnClickCount;
            public bool hasBoardTileView;
            public bool hasIconImage;
            public bool hasIconCanvasGroup;
            public bool hasHighlightImage;
            public bool hasBadgeImage;
            public bool hasCountTMP;
            public bool hasBoxCollider2D; // should be false for UI pointer system
        }

        [Serializable]
        private class DragLayerInfo
        {
            public bool present;
            public string path;
            public bool topmostSibling;
            public bool hasOwnCanvas;
            public bool overrideSorting;
            public int sortingOrder;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
        }

        [Serializable]
        private class GridInfo
        {
            public string path;
            public Vector2 cellSize;
            public Vector2 spacing;
            public string constraint;   // Flexible / FixedColumnCount / FixedRowCount
            public int constraintCount; // when fixed, else 0
            public TextAnchor childAlignment;
        }

        [Serializable]
        private class GeneratorInfo
        {
            public bool present;
            public string slotName;
            public int row;
            public int col;
        }

        [Serializable]
        private class Report
        {
            public string timestampIso;
            public string scenePath;
            public int expectedRows;
            public int expectedCols;
            public int expectedSlots;
            public int slotCountFound;
            public bool slotsAllFound;
            public List<string> missingSlots = new();
            public List<string> extraSlots = new();
            public GridInfo grid;
            public bool gridSpacingIs2x2;
            public DragLayerInfo dragLayer;
            public bool eventSystemPresent;
            public GeneratorInfo generator;
            public List<SlotInfo> slots = new();
            public List<string> log = new();
        }

        // slot_00_00 style
        private static readonly Regex SlotRx = new Regex(@"^slot_(\d{2})_(\d{2})$", RegexOptions.Compiled);

        [MenuItem("AQ/Board/Run Sanity Audit (Strict)")]
        public static void AuditCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Board Audit (Strict)", "No active scene loaded.", "OK");
                return;
            }

            var report = new Report
            {
                timestampIso = DateTime.UtcNow.ToString("o"),
                scenePath = scene.path,
                expectedRows = ExpectedRows,
                expectedCols = ExpectedCols,
                expectedSlots = ExpectedRows * ExpectedCols
            };

            var sb = new StringBuilder();
            void Log(string level, string msg)
            {
                var line = $"{level}: {msg}";
                report.log.Add(line);
                sb.AppendLine(line);
            }

            Log("INFO", $"Scene: {scene.path}");

            // ----- Canvas & Grid detection -----
            var allCanvases = FindAll<Canvas>();
            var canvasBoard = allCanvases.FirstOrDefault(c => c.name == "Canvas_Board")
                              ?? allCanvases.FirstOrDefault(c => c.GetComponentInChildren<GridLayoutGroup>(true) != null);

            if (canvasBoard == null)
            {
                Log("FAIL", "No Canvas found (Canvas_Board not present).");
            }

            GridLayoutGroup grid = null;
            Transform mergeBoardTf = null;
            if (canvasBoard != null)
            {
                mergeBoardTf = canvasBoard.transform.Find("MergeBoard");
                if (mergeBoardTf != null)
                    grid = mergeBoardTf.GetComponent<GridLayoutGroup>();

                if (grid == null)
                {
                    grid = canvasBoard.GetComponentInChildren<GridLayoutGroup>(true);
                    mergeBoardTf = grid != null ? grid.transform : null;
                }
            }

            var gridInfo = new GridInfo();
            if (grid != null)
            {
                gridInfo.path = GetPath(grid.transform);
                gridInfo.cellSize = grid.cellSize;
                gridInfo.spacing = grid.spacing;
                gridInfo.constraint = grid.constraint.ToString();
                gridInfo.constraintCount = (grid.constraint == GridLayoutGroup.Constraint.Flexible) ? 0 : grid.constraintCount;
                gridInfo.childAlignment = grid.childAlignment;

                var spacingOk = Nearly(grid.spacing, new Vector2(2, 2));
                report.grid = gridInfo;
                report.gridSpacingIs2x2 = spacingOk;

                Log(spacingOk ? "INFO" : "WARN",
                    $"Grid spacing: ({grid.spacing.x},{grid.spacing.y}) expected (2,2).");
            }
            else
            {
                Log("FAIL", "GridLayoutGroup (MergeBoard) not found.");
                report.grid = null;
                report.gridSpacingIs2x2 = false;
            }

            // ----- Enumerate slots under MergeBoard -----
            var slotsFound = new List<SlotInfo>();
            if (mergeBoardTf != null)
            {
                foreach (Transform child in mergeBoardTf)
                {
                    var m = SlotRx.Match(child.name);
                    if (!m.Success) continue;

                    var r = int.Parse(m.Groups[1].Value);
                    var c = int.Parse(m.Groups[2].Value);

                    var slot = new SlotInfo
                    {
                        name = child.name,
                        row = r,
                        col = c,
                        hasButton = child.GetComponent<Button>() != null,
                        buttonOnClickCount = child.GetComponent<Button>()?.onClick.GetPersistentEventCount() ?? 0,
                        hasBoardTileView = HasComponentByFullName(child.gameObject, "AQ.App.UI.Board.BoardTileView"),
                        hasBoxCollider2D = child.GetComponent<BoxCollider2D>() != null
                    };

                    var icon = child.Find("Icon");
                    slot.hasIconImage = icon && icon.GetComponent<Image>() != null;
                    slot.hasIconCanvasGroup = icon && icon.GetComponent<CanvasGroup>() != null;

                    var highlight = child.Find("Highlight");
                    slot.hasHighlightImage = highlight && highlight.GetComponent<Image>() != null;

                    var badge = child.Find("Badge");
                    slot.hasBadgeImage = badge && badge.GetComponent<Image>() != null;
                    if (badge)
                    {
                        var count = badge.Find("Count");
                        slot.hasCountTMP = count && count.GetComponent<TextMeshProUGUI>() != null;
                    }

                    slotsFound.Add(slot);
                }
            }

            // expected vs actual
            var expectedNames = new HashSet<string>();
            for (var r = 0; r < ExpectedRows; r++)
                for (var c = 0; c < ExpectedCols; c++)
                    expectedNames.Add($"slot_{r:00}_{c:00}");

            var actualNames = new HashSet<string>(slotsFound.Select(s => s.name));
            var missing = expectedNames.Where(n => !actualNames.Contains(n)).OrderBy(n => n).ToList();
            var extra = actualNames.Where(n => !expectedNames.Contains(n)).OrderBy(n => n).ToList();

            report.slotCountFound = actualNames.Count;
            report.slotsAllFound = missing.Count == 0 && extra.Count == 0;
            report.missingSlots = missing;
            report.extraSlots = extra;
            report.slots = slotsFound;

            if (report.slotsAllFound)
                Log("INFO", $"Slots: {report.slotCountFound} found (expected {report.expectedSlots}) — OK");
            else
            {
                Log("WARN", $"Slots: {report.slotCountFound} found (expected {report.expectedSlots})");
                if (missing.Count > 0) Log("FAIL", "Missing: " + string.Join(", ", missing));
                if (extra.Count > 0) Log("WARN", "Extra: " + string.Join(", ", extra));
            }

            // Button wiring & BoxCollider2D
            var wired = slotsFound.Where(s => s.hasButton && s.buttonOnClickCount > 0).ToList();
            if (wired.Count > 0)
                Log("WARN", $"Buttons with onClick wiring found on {wired.Count} slots (IPointer flow expected).");
            var colliderSlots = slotsFound.Where(s => s.hasBoxCollider2D).ToList();
            if (colliderSlots.Count > 0)
                Log("WARN", $"BoxCollider2D present on {colliderSlots.Count} UI slots (not needed for UI EventSystem).");

            // ----- DragLayer checks -----
            var dragLayerTf = canvasBoard != null ? canvasBoard.transform.Find("DragLayer") : null;
            var dl = new DragLayerInfo();
            if (dragLayerTf == null)
            {
                Log("FAIL", "DragLayer is missing under the board Canvas.");
                dl.present = false;
            }
            else
            {
                dl.present = true;
                dl.path = GetPath(dragLayerTf);
                var parent = dragLayerTf.parent;
                dl.topmostSibling = parent != null && dragLayerTf.GetSiblingIndex() == parent.childCount - 1;

                var dlCanvas = dragLayerTf.GetComponent<Canvas>();
                dl.hasOwnCanvas = dlCanvas != null;
                dl.overrideSorting = dlCanvas != null && dlCanvas.overrideSorting;
                dl.sortingOrder = dlCanvas != null ? dlCanvas.sortingOrder : 0;

                var rt = (RectTransform)dragLayerTf;
                dl.anchorMin = rt != null ? rt.anchorMin : Vector2.zero;
                dl.anchorMax = rt != null ? rt.anchorMax : Vector2.one;
                dl.pivot    = rt != null ? rt.pivot    : new Vector2(0.5f, 0.5f);

                var layerOk = dl.topmostSibling || (dl.hasOwnCanvas && dl.overrideSorting && dl.sortingOrder >= 1000);
                Log(layerOk ? "INFO" : "WARN",
                    $"DragLayer: {(dl.topmostSibling ? "topmost" : "not topmost")} {(dl.hasOwnCanvas ? $"| own canvas (override={dl.overrideSorting}, order={dl.sortingOrder})" : "| no own canvas")}.");

                var anchorsOk = Nearly(dl.anchorMin, Vector2.zero) && Nearly(dl.anchorMax, Vector2.one) && Nearly(dl.pivot, new Vector2(0.5f, 0.5f));
                Log(anchorsOk ? "INFO" : "WARN",
                    $"DragLayer anchors: min={dl.anchorMin} max={dl.anchorMax} pivot={dl.pivot} (expect min(0,0) max(1,1) pivot 0.5).");
            }
            report.dragLayer = dl;

            // ----- EventSystem presence -----
            report.eventSystemPresent = FindOne<EventSystem>() != null;
            Log(report.eventSystemPresent ? "INFO" : "FAIL", $"EventSystem: {(report.eventSystemPresent ? "present" : "missing")}");

            // ----- Generator detection (child named contains "GeneratorTile") -----
            var allTransforms = FindAll<Transform>();
            var genIcon = allTransforms.FirstOrDefault(t => t.name.IndexOf("GeneratorTile", StringComparison.OrdinalIgnoreCase) >= 0);

            var gen = new GeneratorInfo();
            if (genIcon != null)
            {
                var slot = GetAncestors(genIcon).FirstOrDefault(t => SlotRx.IsMatch(t.name));
                if (slot != null)
                {
                    var m = SlotRx.Match(slot.name);
                    gen.present = true;
                    gen.slotName = slot.name;
                    gen.row = int.Parse(m.Groups[1].Value);
                    gen.col = int.Parse(m.Groups[2].Value);
                    Log("INFO", $"Generator at {gen.slotName} → ({gen.row},{gen.col}).");
                }
                else
                {
                    gen.present = true;
                    gen.slotName = "(parent slot not found)";
                    gen.row = -1; gen.col = -1;
                    Log("WARN", "GeneratorTile found but not under a slot_(rr)_(cc) transform.");
                }
            }
            else
            {
                gen.present = false;
                Log("WARN", "GeneratorTile not found.");
            }
            report.generator = gen;

            // ----- Write reports -----
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var auditDir = Path.Combine(projectRoot, "_audit");
            Directory.CreateDirectory(auditDir);
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = Path.Combine(auditDir, $"board_sanity_strict_{stamp}");

            var header = $"=== BOARD SANITY AUDIT (STRICT): {report.scenePath} @ {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n";
            File.WriteAllText(baseName + ".txt", header + sb.ToString());
            File.WriteAllText(baseName + ".json", JsonUtility.ToJson(report, true));

            EditorUtility.RevealInFinder(baseName + ".txt");

            // concise summary
            var pass = report.eventSystemPresent
                       && report.slotsAllFound
                       && report.dragLayer != null && report.dragLayer.present
                       && report.grid != null && report.gridSpacingIs2x2;

            EditorUtility.DisplayDialog("Board Audit (Strict)",
                pass ? "Audit PASS (see _audit for report)." : "Audit finished with warnings/failures. See _audit folder for details.",
                "OK");
        }

        // ---------- helpers ----------
        private static string GetPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        private static IEnumerable<Transform> GetAncestors(Transform t)
        {
            var cur = t.parent;
            while (cur != null)
            {
                yield return cur;
                cur = cur.parent;
            }
        }

        private static bool HasComponentByFullName(GameObject go, string fullName)
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue; // missing script
                if (string.Equals(c.GetType().FullName, fullName, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private static bool Nearly(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) < 0.0001f && Mathf.Abs(a.y - b.y) < 0.0001f;
        }

        // Version-safe finders to avoid obsolete/compatibility warnings across Unity versions.
        private static T FindOne<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>();
#else
            return UnityEngine.Object.FindObjectOfType<T>();
#endif
        }

        private static T[] FindAll<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Object.FindObjectsOfType<T>(true);
#else
            return UnityEngine.Object.FindObjectsOfType<T>();
#endif
#endif
        }
    }
}
#endif
