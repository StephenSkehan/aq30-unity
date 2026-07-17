using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>
    /// Automated form-factor QA: forces the Game view through a ladder of real
    /// device resolutions (play mode), captures a screenshot at each, and runs
    /// basic layout assertions (key widgets on-screen, corner buttons not
    /// overlapping). One-line log per device; captures land in Screenshots/.
    /// </summary>
    public static class QAFormFactorSweep
    {
        // (label, width, height) — portrait. Covers the deployment floor
        // (iPad Air 2 / iPhone SE) through current flagships + a tall Android.
        static readonly (string name, int w, int h)[] Presets =
        {
            ("iPhoneSE3_16x9",    750, 1334),
            ("iPhone11_19.5x9",   828, 1792),
            ("iPhone16e",        1170, 2532),
            ("iPhone16ProMax",   1320, 2868),
            ("iPadAir2_4x3",     1536, 2048),
            ("iPadPro13_3x4",    2064, 2752),
            ("AndroidTall_20x9", 1080, 2400),
            ("DevBaseline",      1080, 1920),
        };

        [MenuItem("AQ/Dev/QA Form Factor Sweep")]
        public static void Sweep()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[FormFactor] enter play mode first.");
                return;
            }

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            Directory.CreateDirectory(dir);

            foreach (var (name, w, h) in Presets)
            {
                SetGameViewSize(w, h, $"FF {name}");
                StepFrames(10);

                var path = Path.Combine(dir, $"ff_{name}.png");
                ScreenCapture.CaptureScreenshot(path, 1);
                StepFrames(6); // flush the queued capture

                Debug.Log($"[FormFactor] {name} {w}x{h}: {AuditLayout()}");
            }

            // Restore the dev baseline so later captures aren't surprise-sized.
            SetGameViewSize(1080, 1920, "AQ Portrait");
            StepFrames(4);
            Debug.Log("[FormFactor] sweep complete — captures in Screenshots/ff_*.png");
        }

        static void StepFrames(int n)
        {
            for (int i = 0; i < n; i++) EditorApplication.Step();
            EditorApplication.isPaused = false;
        }

        // ---- layout audit ----

        static string AuditLayout()
        {
            // The fit component reacts to size changes in LateUpdate and the
            // GridLayoutGroup applies a frame later — force both so the audit
            // measures settled geometry, not the previous preset's layout.
            var fitGo = GameObject.Find("Canvas_Board_Grid");
            var fit = fitGo != null ? fitGo.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>(true) : null;
            if (fit != null) fit.SendMessage("Refit", SendMessageOptions.DontRequireReceiver);
            Canvas.ForceUpdateCanvases();

            var sb = new StringBuilder();
            float sw = Screen.width, sh = Screen.height;
            sb.Append($"render {sw}x{sh}");

            var rects = new Dictionary<string, Rect>();
            void Probe(string label, string goName, string childName = null)
            {
                var go = GameObject.Find(goName);
                if (go == null) { sb.Append($" | {label}: MISSING"); return; }
                var t = childName == null ? go.transform : go.transform.Find(childName);
                if (t == null) { sb.Append($" | {label}: no-child"); return; }
                var rt = t as RectTransform;
                if (rt == null) rt = t.GetComponentInChildren<RectTransform>();
                if (rt == null) { sb.Append($" | {label}: no-rect"); return; }

                var c = new Vector3[4];
                rt.GetWorldCorners(c);
                var r = Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
                rects[label] = r;

                if (r.xMin < -1f || r.yMin < -1f || r.xMax > sw + 1f || r.yMax > sh + 1f)
                    sb.Append($" | {label}: OFFSCREEN ({r.xMin:F0},{r.yMin:F0})-({r.xMax:F0},{r.yMax:F0})");
            }

            // Runtime-installed corner widgets + core scene anchors.
            // __LockerBtn / __EvidBoardBtn are full-screen canvas roots — the
            // actual buttons are their "Btn" children.
            Probe("locker",   "__LockerBtn",   "Btn");
            Probe("evidence", "__EvidBoardBtn", "Btn");
            Probe("hudCash",  "Txt_Soft_Currency");
            Probe("hudEnergy","Txt_Value");

            // Board content = union of the grid's cell rects (the canvas rect can
            // fit while GridLayoutGroup content overflows it — measure the cells).
            Rect? gridBounds = null;
            var gridGo = GameObject.Find("Canvas_Board_Grid");
            var layout = gridGo != null ? gridGo.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>(true) : null;
            if (layout != null)
            {
                var corners = new Vector3[4];
                foreach (RectTransform cell in layout.transform)
                {
                    if (!cell.gameObject.activeInHierarchy) continue;
                    // Skip ignoreLayout children (BoardFrame backdrop) — the
                    // requirement is about playable cells, not the plate.
                    var le = cell.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (le != null && le.ignoreLayout) continue;
                    cell.GetWorldCorners(corners);
                    var r = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                    gridBounds = gridBounds == null ? r :
                        Rect.MinMaxRect(Mathf.Min(gridBounds.Value.xMin, r.xMin), Mathf.Min(gridBounds.Value.yMin, r.yMin),
                                        Mathf.Max(gridBounds.Value.xMax, r.xMax), Mathf.Max(gridBounds.Value.yMax, r.yMax));
                }
            }
            if (layout != null)
            {
                // A GridLayoutGroup adopts every non-ignoreLayout child as a cell;
                // anything beyond the 63 slots creates a phantom row.
                var names = new Dictionary<string, int>();
                int adopted = 0;
                foreach (RectTransform cell in layout.transform)
                {
                    if (!cell.gameObject.activeInHierarchy) continue;
                    var le2 = cell.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (le2 != null && le2.ignoreLayout) continue;
                    adopted++;
                    names[cell.name] = (names.TryGetValue(cell.name, out int n) ? n : 0) + 1;
                }
                if (adopted != 63)
                {
                    sb.Append($" | ADOPTED {adopted} cells:");
                    foreach (var kv in names) sb.Append($" {kv.Key}x{kv.Value}");
                }
            }
            if (gridBounds == null) sb.Append(" | grid: MISSING");
            else
            {
                var g = gridBounds.Value;
                if (g.yMin < -1f)      sb.Append($" | grid BOTTOM-CLIPPED {-g.yMin:F0}px");
                if (g.xMin < -1f)      sb.Append($" | grid LEFT-CLIPPED {-g.xMin:F0}px");
                if (g.xMax > sw + 1f)  sb.Append($" | grid RIGHT-CLIPPED {g.xMax - sw:F0}px");
                if (g.yMax > sh + 1f)  sb.Append($" | grid TOP-CLIPPED {g.yMax - sh:F0}px");
            }

            // Leads bar (card strip) vs grid content — cards must not cover cells.
            var barGo = GameObject.Find("LeadsBarRuntime");
            var barRt = barGo != null ? barGo.GetComponentInChildren<RectTransform>(true) : null;
            if (barRt != null && gridBounds != null)
            {
                var corners = new Vector3[4];
                barRt.GetWorldCorners(corners);
                var barRect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                if (barRect.Overlaps(gridBounds.Value))
                    sb.Append($" | card-over-grid {barRect.yMin - gridBounds.Value.yMax:F0}..{gridBounds.Value.yMax - barRect.yMin:F0}px");
            }

            // The two corner buttons must not collide.
            if (rects.TryGetValue("locker", out var a) && rects.TryGetValue("evidence", out var b)
                && a.Overlaps(b))
                sb.Append(" | locker/evidence OVERLAP");

            // Ruled 2026-07-17: corner buttons must never sit on top of grid cells.
            if (gridBounds != null)
            {
                if (rects.TryGetValue("locker", out var lk) && lk.Overlaps(gridBounds.Value))
                    sb.Append($" | locker-over-grid lk({lk.xMin:F0},{lk.yMin:F0},{lk.xMax:F0},{lk.yMax:F0}) grid({gridBounds.Value.xMin:F0},{gridBounds.Value.yMin:F0},{gridBounds.Value.xMax:F0},{gridBounds.Value.yMax:F0})");
                if (rects.TryGetValue("evidence", out var ev) && ev.Overlaps(gridBounds.Value))
                    sb.Append(" | evidence-over-grid");
            }

            return sb.ToString();
        }

        // ---- Game view sizing (reflection — mechanism proven in CaptureGameView) ----

        static void SetGameViewSize(int width, int height, string label)
        {
            try
            {
                var asm = typeof(EditorWindow).Assembly;
                var gameViewType = asm.GetType("UnityEditor.GameView");
                var gameView = EditorWindow.GetWindow(gameViewType, false, null, true);

                var sizesType  = asm.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var sizes      = singleType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                var group      = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] {
                                     sizesType.GetProperty("currentGroupType").GetValue(sizes) });

                var groupType  = group.GetType();
                int builtin    = (int)groupType.GetMethod("GetBuiltinCount").Invoke(group, null);
                int custom     = (int)groupType.GetMethod("GetCustomCount").Invoke(group, null);
                int foundIndex = -1;
                for (int i = 0; i < builtin + custom; i++)
                {
                    var size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                    int w = (int)size.GetType().GetProperty("width").GetValue(size);
                    int h = (int)size.GetType().GetProperty("height").GetValue(size);
                    if (w == width && h == height) { foundIndex = i; break; }
                }

                if (foundIndex < 0)
                {
                    var sizeType     = asm.GetType("UnityEditor.GameViewSize");
                    var sizeTypeEnum = asm.GetType("UnityEditor.GameViewSizeType");
                    var ctor = sizeType.GetConstructor(new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    var size = ctor.Invoke(new object[] { 1, width, height, label }); // 1 = FixedResolution
                    groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                    foundIndex = builtin + custom;
                }

                gameViewType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SetValue(gameView, foundIndex);
                gameView.Repaint();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[FormFactor] could not set game view size: " + e.Message);
            }
        }
    }
}
