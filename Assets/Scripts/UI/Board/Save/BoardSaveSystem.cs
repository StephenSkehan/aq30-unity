// Assets/Scripts/UI/Board/Save/BoardSaveSystem.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Autosaves the merge board to JSON when tiles change, and autoloads on start.
    /// Non-invasive: reads/writes via MergeBoardController + BoardTileView public API.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MergeBoardController))]
    public sealed class BoardSaveSystem : MonoBehaviour
    {
        [Header("Save")]
        [Tooltip("Relative filename under Application.persistentDataPath")]
        public string fileName = "board_state.json";

        [Tooltip("Debounce writes to avoid hammering the disk (seconds).")]
        [Min(0f)] public float saveDebounce = 0.15f;

        MergeBoardController ctrl;
        float nextSaveAt = -1f;
        bool initialized;
        Snap[,] prev;

        struct Snap
        {
            public TileKind kind;
            public int tier;

            public static Snap Of(BoardTileView v)
            {
                if (!v) return default;
                return new Snap { kind = v.Kind, tier = v.Tier };
            }

            public bool Equals(Snap other) => kind == other.kind && tier == other.tier;
            public bool IsEmpty => kind == TileKind.Empty;
        }

        void Awake()
        {
            ctrl = GetComponent<MergeBoardController>();
        }

        void Start() { /* defer to LateUpdate until grid is ready */ }

        void LateUpdate()
        {
            if (!initialized)
            {
                if (!GridReady()) return;
                TryLoad();
                Capture();
                initialized = true;
                return;
            }

            if (DetectChanges())
                nextSaveAt = Time.unscaledTime + saveDebounce;

            if (nextSaveAt > 0f && Time.unscaledTime >= nextSaveAt)
            {
                nextSaveAt = -1f;
                TrySave();
            }
        }

        void OnApplicationQuit()    { TrySave(); }
        void OnApplicationPause(bool pause) { if (pause) TrySave(); }

        bool GridReady()
        {
            if (ctrl == null || ctrl.Rows <= 0 || ctrl.Cols <= 0) return false;
            try { return ctrl.Get(0, 0) != null; }
            catch { return false; }
        }

        void Capture()
        {
            prev = new Snap[ctrl.Rows, ctrl.Cols];
            for (int r = 0; r < ctrl.Rows; r++)
                for (int c = 0; c < ctrl.Cols; c++)
                    prev[r, c] = Snap.Of(ctrl.Get(r, c));
        }

        bool DetectChanges()
        {
            bool changed = false;
            for (int r = 0; r < ctrl.Rows; r++)
            for (int c = 0; c < ctrl.Cols; c++)
            {
                var now = Snap.Of(ctrl.Get(r, c));
                if (!now.Equals(prev[r, c]))
                {
                    prev[r, c] = now;
                    changed = true;
                }
            }
            return changed;
        }

        string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        void TrySave()
        {
            if (ctrl == null) return;

            var data = new BoardSaveData
            {
                schemaVersion = 1,
                rows = ctrl.Rows,
                cols = ctrl.Cols,
                cells = BuildCells()
            };

            var json = JsonUtility.ToJson(data, prettyPrint: false);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                File.WriteAllText(SavePath, json);
                ctrl.Log($"[Save] wrote {data.cells.Length} cells → {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] failed: {e.Message}");
            }
        }

        BoardCellData[] BuildCells()
        {
            var list = new List<BoardCellData>(ctrl.Rows * ctrl.Cols);
            for (int r = 0; r < ctrl.Rows; r++)
            for (int c = 0; c < ctrl.Cols; c++)
            {
                var v = ctrl.Get(r, c);
                var k = v.Kind;
                if (k == TileKind.Empty) continue;

                list.Add(new BoardCellData
                {
                    r = r,
                    c = c,
                    kind = k.ToString(),
                    tier = v.Tier
                });
            }
            return list.ToArray();
        }

        void TryLoad()
        {
            try
            {
                if (!File.Exists(SavePath)) return;

                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<BoardSaveData>(json) ?? BoardSaveData.Empty;

                if (data.rows != ctrl.Rows || data.cols != ctrl.Cols)
                {
                    Debug.LogWarning($"[Save] grid size mismatch; ignoring save ({data.rows}x{data.cols} vs {ctrl.Rows}x{ctrl.Cols})");
                    return;
                }

                // 1) Clear current board
                for (int r = 0; r < ctrl.Rows; r++)
                for (int c = 0; c < ctrl.Cols; c++)
                    ctrl.Get(r, c).Clear();

                // 2) Restore cells
                foreach (var cell in data.cells)
                {
                    if (cell.r < 0 || cell.r >= ctrl.Rows || cell.c < 0 || cell.c >= ctrl.Cols) continue;
                    var v = ctrl.Get(cell.r, cell.c);
                    if (v == null) continue;

                    if (cell.kind == TileKind.Generator.ToString())
                    {
                        // Use controller's generator sprite
                        v.SetGenerator(ctrl.generatorSprite, Mathf.Max(0, cell.tier));
                    }
                    else if (cell.kind == TileKind.Item.ToString())
                    {
                        // Use controller's icons[] by tier (clamped)
                        var ti = Mathf.Clamp(cell.tier, 0, Mathf.Max(0, ctrl.icons.Count - 1));
                        var sprite = (ti >= 0 && ti < ctrl.icons.Count) ? ctrl.icons[ti] : null;
                        v.SetItem(sprite, ti);
                    }
                }

                ctrl.Log($"[Save] loaded {data.cells?.Length ?? 0} cells from {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] load failed: {e.Message}");
            }
        }
    }
}
