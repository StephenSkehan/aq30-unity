// Assets/Editor/UI/Board/BoardSanityAudit_Strict.cs
// Strict board auditor — GeneratorTile is OPTIONAL and may be free-floating.
// Replaces any previous strict auditor that warned when generator was missing.

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace AQ.App.Editor.Board
{
    public static class BoardSanityAuditStrict
    {
        private const int ExpectedSlots = 63;
        private const float ExpectedSpacing = 2f;
        private static readonly Regex SlotNameRx = new Regex(@"^slot_(\d{2})_(\d{2})$", RegexOptions.Compiled);

        [MenuItem("AQ/Board/Run Sanity Audit (Strict)")]
        public static void AuditCurrentScene()
        {
            var (txt, json, txtPath, jsonPath) = BuildAudit();
            Debug.Log(txt);

            Directory.CreateDirectory("Assets/_audit");
            File.WriteAllText(txtPath, txt, Encoding.UTF8);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log(
                "AQ Board Sanity (Strict): wrote\n" +
                $" - {txtPath}\n" +
                $" - {jsonPath}"
            );
        }

        private static (string txt, string json, string txtPath, string jsonPath) BuildAudit()
        {
            var now = DateTime.Now;
            var scene = EditorSceneManager.GetActiveScene();
            var scenePath = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;

            // Walk all objects (including inactive) in active scene
            var all = EnumerateAll(scene).ToList();

            Transform FindByName(string name) =>
                all.FirstOrDefault(t => t.name.Equals(name, StringComparison.Ordinal));

            var boardCanvas = FindByName("Canvas_Board")?.gameObject;
            var mergeBoard = FindByName("MergeBoard")?.gameObject;
            var dragLayer = FindByName("DragLayer")?.gameObject;

            // Slots
            var slots = all.Where(t => SlotNameRx.IsMatch(t.name))
                           .OrderBy(t => t.name)
                           .ToList();

            // Grid spacing (look under MergeBoard first, then anywhere)
            Vector2 spacing = Vector2.zero;
            GridLayoutGroup grid = null;
            if (mergeBoard) grid = mergeBoard.GetComponentInChildren<GridLayoutGroup>(true);
            if (grid == null) grid = Object.FindFirstObjectByType<GridLayoutGroup>(FindObjectsInactive.Include);
            if (grid != null) spacing = grid.spacing;

            // EventSystem (no obsolete API)
            bool hasEventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null;

            // DragLayer details
            string dragTopmost = "absent";
            string dragCanvas = "";
            string dragAnchors = "";
            if (dragLayer != null)
            {
                var parent = dragLayer.transform.parent;
                if (parent != null && dragLayer.transform.GetSiblingIndex() == parent.childCount - 1)
                    dragTopmost = "topmost";
                else
                    dragTopmost = "not topmost";

                var hasOwnCanvas = dragLayer.GetComponent<Canvas>() != null;
                dragCanvas = hasOwnCanvas ? "own canvas" : "no own canvas";

                var rt = dragLayer.GetComponent<RectTransform>();
                if (rt != null)
                {
                    dragAnchors =
                        $"min=({rt.anchorMin.x:0.00}, {rt.anchorMin.y:0.00}) " +
                        $"max=({rt.anchorMax.x:0.00}, {rt.anchorMax.y:0.00}) " +
                        $"pivot=({rt.pivot.x:0.00}, {rt.pivot.y:0.00})";
                }
            }

            // Generator — OPTIONAL; allow free-floating placement
            var generator = FindByName("GeneratorTile")?.gameObject
                            ?? all.Select(t => t.gameObject)
                                  .FirstOrDefault(go => go.name.StartsWith("GeneratorTile", StringComparison.Ordinal));
            bool genPresent = generator != null;
            bool genUnderSlot = false;
            if (genPresent)
            {
                genUnderSlot = generator.transform
                    .GetComponentsInParent<Transform>(true)
                    .Any(t => SlotNameRx.IsMatch(t.name));
            }

            // Build text
            var sb = new StringBuilder();
            sb.AppendLine($"=== BOARD SANITY AUDIT (STRICT): {scenePath} @ {now:yyyy-MM-dd HH:mm:ss} ===");
            sb.AppendLine($"INFO: Scene: {scenePath}");

            if (grid != null)
                sb.AppendLine($"INFO: Grid spacing: ({spacing.x:0},{spacing.y:0}) expected ({ExpectedSpacing:0},{ExpectedSpacing:0}).");

            var slotsLine = $"INFO: Slots: {slots.Count} found (expected {ExpectedSlots}) — " +
                            (slots.Count == ExpectedSlots ? "OK" : "MISMATCH");
            sb.AppendLine(slotsLine);

            if (dragLayer != null)
                sb.AppendLine($"INFO: DragLayer: {dragTopmost} | {dragCanvas}.");

            if (!string.IsNullOrEmpty(dragAnchors))
                sb.AppendLine($"INFO: DragLayer anchors: {dragAnchors} (expect min(0,0) max(1,1) pivot 0.5).");

            sb.AppendLine($"INFO: EventSystem: {(hasEventSystem ? "present" : "missing")}");

            // Patched: generator is informational only (no WARN)
            if (!genPresent)
                sb.AppendLine("INFO: GeneratorTile: not present (allowed).");
            else
                sb.AppendLine($"INFO: GeneratorTile present {(genUnderSlot ? "under slot" : "(free-floating)")}.");

            sb.AppendLine("SLOTS (first 5 of " + slots.Count + "):");
            foreach (var s in slots.Take(5))
            {
                var flags = new List<string>
                {
                    "btn:"   + (s.GetComponentInChildren<Button>(true) != null),
                    "view:"  + HasScriptByNames(s, "BoardTileView", "TileView"),
                    "icon:"  + HasChildWith<Image>(s, "Icon"),
                    "badge:" + HasChildWith<Image>(s, "Badge"),
                    "tmp:"   + (s.GetComponentsInChildren<TMP_Text>(true).Length > 0),
                    "col2D:" + (s.GetComponent<BoxCollider2D>() != null)
                };

                var m = SlotNameRx.Match(s.name);
                var r = m.Success ? int.Parse(m.Groups[1].Value) : -1;
                var c = m.Success ? int.Parse(m.Groups[2].Value) : -1;

                sb.AppendLine($"  {s.name} r{r} c{c} | {string.Join(" ", flags)}");
            }
            sb.AppendLine();

            var txt = sb.ToString();

            // Minimal JSON
            var json = new StringBuilder();
            json.Append("{");
            json.Append($"\"scene\":\"{Escape(scenePath)}\"");
            json.Append($",\"gridSpacing\":{{\"x\":{spacing.x:0.##},\"y\":{spacing.y:0.##}}}");
            json.Append($",\"slots\":{slots.Count}");
            json.Append($",\"eventSystem\":{(hasEventSystem ? "true" : "false")}");
            json.Append($",\"generator\":{{\"present\":{(genPresent ? "true" : "false")},\"underSlot\":{(genUnderSlot ? "true" : "false")}}}");
            json.Append("}");
            var jsonStr = json.ToString();

            var stamp = now.ToString("yyyyMMdd_HHmmss");
            var txtPath = $"Assets/_audit/board_sanity_strict_{stamp}.txt";
            var jsonPath = $"Assets/_audit/board_sanity_strict_{stamp}.json";
            return (txt, jsonStr, txtPath, jsonPath);
        }

        private static IEnumerable<Transform> EnumerateAll(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var stack = new Stack<Transform>();
            foreach (var go in roots)
            {
                var t = go.transform;
                stack.Push(t);
                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    yield return cur;
                    for (int i = 0; i < cur.childCount; i++)
                        stack.Push(cur.GetChild(i));
                }
            }
        }

        private static bool HasChildWith<T>(Transform t, string childName) where T : Component
        {
            var child = t.GetComponentsInChildren<Transform>(true)
                         .FirstOrDefault(x => x.name.Equals(childName, StringComparison.Ordinal));
            return child != null && child.GetComponent<T>() != null;
        }

        private static bool HasScriptByNames(Transform t, params string[] typeNames)
        {
            var names = new HashSet<string>(typeNames);
            var mbs = t.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb == null) continue;
                if (names.Contains(mb.GetType().Name))
                    return true;
            }
            return false;
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
