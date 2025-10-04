// Assets/Editor/UI/Board/BoardSanityStrict.cs
// Strict board auditor — generator is OPTIONAL and may be free-floating.
// Patched: unique menu path + no obsolete API usage.

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
    public static class BoardSanityStrict
    {
        // ---- config ---------------------------------------------------------------------------

        private const int    ExpectedSlots   = 63;     // 7 x 9 typical merge board
        private const float  ExpectedSpacing = 2f;     // expected GridLayoutGroup.spacing (x = y = 2)
        private static readonly Regex SlotNameRx = new Regex(@"^slot_(\d{2})_(\d{2})$", RegexOptions.Compiled);

        // ---- menu -----------------------------------------------------------------------------
        // NOTE: Use a unique path to avoid clashing with existing menu items.
        [MenuItem("AQ/Board/Audit (Strict)/Run (Patched)")]
        public static void RunAuditStrict()
        {
            var (txt, json, txtPath, jsonPath) = BuildAudit();
            WriteBigToConsole(txt);

            // write files
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

        // ---- core -----------------------------------------------------------------------------

        private static (string txt, string json, string txtPath, string jsonPath) BuildAudit()
        {
            var now       = DateTime.Now;
            var scene     = EditorSceneManager.GetActiveScene();
            var scenePath = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;

            // lookups
            var boardCanvas = GameObject.Find("Canvas_Board");
            var mergeBoard  = GameObject.Find("MergeBoard");
            var dragLayer   = GameObject.Find("DragLayer");

            // slots
            var slots = new List<Transform>();
            if (mergeBoard != null)
            {
                foreach (Transform t in mergeBoard.transform)
                {
                    if (SlotNameRx.IsMatch(t.name))
                        slots.Add(t);
                }
            }
            slots = slots.OrderBy(t => t.name).ToList();

            // spacing (GridLayoutGroup on MergeBoard or ancestors)
            Vector2 spacing = Vector2.zero;
            {
                GridLayoutGroup grid = null;
                if (mergeBoard) grid = mergeBoard.GetComponentInChildren<GridLayoutGroup>(true);
                if (grid != null) spacing = grid.spacing;
            }

            // event system (no obsolete API)
            bool hasEventSystem = Object.FindFirstObjectByType<EventSystem>() != null;

            // drag layer info
            string dragTopmost = "absent";
            string dragCanvas  = "";
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

            // generator — OPTIONAL and can be free-floating or inside a slot
            var gen = GameObject.Find("GeneratorTile");
            bool genPresent   = gen != null;
            bool genUnderSlot = false;
            if (genPresent)
            {
                genUnderSlot = gen.transform
                    .GetComponentsInParent<Transform>(true)
                    .Any(t => SlotNameRx.IsMatch(t.name));
            }

            // text report
            var sb = new StringBuilder();
            sb.AppendLine($"=== BOARD SANITY AUDIT (STRICT): {scenePath} @ {now:yyyy-MM-dd HH:mm:ss} ===");
            sb.AppendLine($"INFO: Scene: {scenePath}");

            if (spacing != Vector2.zero)
                sb.AppendLine($"INFO: Grid spacing: ({spacing.x:0},{spacing.y:0}) expected ({ExpectedSpacing:0},{ExpectedSpacing:0}).");

            var slotsLine = $"INFO: Slots: {slots.Count} found (expected {ExpectedSlots}) — " +
                            (slots.Count == ExpectedSlots ? "OK" : "MISMATCH");
            sb.AppendLine(slotsLine);

            if (dragLayer != null)
                sb.AppendLine($"INFO: DragLayer: {dragTopmost} | {dragCanvas}.");

            if (!string.IsNullOrEmpty(dragAnchors))
                sb.AppendLine($"INFO: DragLayer anchors: {dragAnchors} (expect min(0,0) max(1,1) pivot 0.5).");

            sb.AppendLine($"INFO: EventSystem: {(hasEventSystem ? "present" : "missing")}");

            // patched behavior — only INFO lines for generator presence/placement
            if (!genPresent)
                sb.AppendLine("INFO: GeneratorTile: not present (allowed).");
            else
                sb.AppendLine($"INFO: GeneratorTile present {(genUnderSlot ? "under slot" : "(free-floating)")}.");

            // slot details preview
            sb.AppendLine("SLOTS (first 5 of " + slots.Count + "):");
            foreach (var s in slots.Take(5))
            {
                var flags = new List<string>();
                flags.Add("btn:"   + HasAny<Button>(s).ToString());
                flags.Add("view:"  + HasScriptByNames(s, "BoardTileView", "TileView").ToString());
                flags.Add("icon:"  + HasChildWith<Image>(s, "Icon").ToString());
                flags.Add("badge:" + HasChildWith<Image>(s, "Badge").ToString());
                flags.Add("tmp:"   + (s.GetComponentsInChildren<TMP_Text>(true).Length > 0).ToString());
                flags.Add("col2D:" + (s.GetComponent<BoxCollider2D>() != null).ToString());

                var m = SlotNameRx.Match(s.name);
                var r = m.Success ? m.Groups[1].Value : "??";
                var c = m.Success ? m.Groups[2].Value : "??";

                sb.AppendLine($"  {s.name} r{int.Parse(r)} c{int.Parse(c)} | {string.Join(" ", flags)}");
            }
            sb.AppendLine();

            var txt = sb.ToString();

            // json (minimal, for scripts)
            var json = CreateJson(scenePath, spacing, slots.Count, hasEventSystem, genPresent, genUnderSlot);

            var stamp = now.ToString("yyyyMMdd_HHmmss");
            var txtPath  = $"Assets/_audit/board_sanity_strict_{stamp}.txt";
            var jsonPath = $"Assets/_audit/board_sanity_strict_{stamp}.json";
            return (txt, json, txtPath, jsonPath);
        }

        private static void WriteBigToConsole(string text)
        {
            Debug.Log(text);
        }

        // ---- helpers --------------------------------------------------------------------------

        private static bool HasAny<T>(Transform t) where T : Component =>
            t.GetComponentInChildren<T>(true) != null;

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
                if (mb == null) continue; // missing script
                if (names.Contains(mb.GetType().Name))
                    return true;
            }
            return false;
        }

        private static string CreateJson(string scenePath, Vector2 spacing, int slotCount,
            bool hasEventSystem, bool genPresent, bool genUnderSlot)
        {
            var json = new StringBuilder();
            json.Append("{");
            json.Append($"\"scene\":\"{Escape(scenePath)}\"");
            json.Append($",\"gridSpacing\":{{\"x\":{spacing.x:0.##},\"y\":{spacing.y:0.##}}}");
            json.Append($",\"slots\":{slotCount}");
            json.Append($",\"eventSystem\":{(hasEventSystem ? "true" : "false")}");
            json.Append($",\"generator\":{{\"present\":{(genPresent ? "true" : "false")},\"underSlot\":{(genUnderSlot ? "true" : "false")}}}");
            json.Append("}");
            return json.ToString();
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
