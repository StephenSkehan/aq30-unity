using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using AQ.App.Config;
using AQ.App.Services;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Saves and loads the board state (items/generators) and global energy in one JSON file.
    /// Atomic write with rolling .prev.json backup. Schema 0.5.0.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoardSaveSystem : MonoBehaviour
    {
        [Tooltip("If not assigned, will search in scene.")]
        public MergeBoardController board;

        [Header("Save Settings")]
        [Tooltip("Debounce time for saves after a detected change.")]
        public float saveDebounceSeconds = 0.25f;

        [Tooltip("Optional filename override. When empty uses 'board_state.json'.")]
        public string fileName = "board_state.json";

        private string _pathLive;
        private string _pathTmp;
        private string _pathPrev;

        private float _nextSaveAt = -1f;
        private int _lastSnapshotHash;

        // --------------- Unity ---------------

        private void Awake()
        {
            if (!board)
                board = FindFirstObjectByType<MergeBoardController>();

            var root = Application.persistentDataPath;
            _pathLive = Path.Combine(root, fileName);
            _pathTmp  = _pathLive + ".tmp";
            _pathPrev = Path.Combine(root, Path.GetFileNameWithoutExtension(_pathLive) + ".prev.json");
        }

        private void Start()
        {
            TryLoad();
            _lastSnapshotHash = SnapshotHash();
        }

        private void LateUpdate()
        {
            if (_nextSaveAt > 0f && Time.unscaledTime >= _nextSaveAt)
            {
                TrySave();
                _nextSaveAt = -1f;
                return;
            }

            int h = SnapshotHash();
            if (h != _lastSnapshotHash)
            {
                _lastSnapshotHash = h;
                _nextSaveAt = Time.unscaledTime + Mathf.Max(0.05f, saveDebounceSeconds);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) TrySave();
        }

        private void OnApplicationQuit()
        {
            TrySave();
        }

        // --------------- Save / Load ---------------

        [Serializable]
        private sealed class CellDTO
        {
            public int r;
            public int c;
            public string kind;   // "Item" | "Generator"
            public int tier;      // 0-based
            public string family; // e.g. "corner_diner" — empty in legacy saves
        }

        [Serializable]
        private sealed class EnergyDTO
        {
            public int current;
            public string lastTickUtc; // ISO-8601 string
        }

        [Serializable]
        private sealed class SaveDTO
        {
            public string schemaVersion = "0.5.0";
            public string timestampUtc;

            public int rows;
            public int cols;

            public List<CellDTO> cells = new List<CellDTO>();
            public EnergyDTO energy;
        }

        public void TrySave()
        {
            if (board == null) return;

            var dto = new SaveDTO
            {
                timestampUtc = DateTime.UtcNow.ToString("o"),
                rows = board.Rows,
                cols = board.Cols,
                energy = BuildEnergyDTO()
            };
            FillCells(dto.cells);

            string json = JsonUtility.ToJson(dto, prettyPrint: false);
            Directory.CreateDirectory(Path.GetDirectoryName(_pathLive));

            try
            {
                File.WriteAllText(_pathTmp, json, Encoding.UTF8);

                if (File.Exists(_pathPrev))
                    File.Delete(_pathPrev);

                if (File.Exists(_pathLive))
                    File.Move(_pathLive, _pathPrev);

                File.Move(_pathTmp, _pathLive);

                //Debug.Log($"[Save] wrote {dto.cells.Count} cells → {_pathLive}");
            }
            catch (Exception ex)
            {
                if (File.Exists(_pathTmp))
                    File.Delete(_pathTmp);

                Debug.LogError($"[Save] write failed: {ex.Message}\nPath={_pathLive}");
            }
        }

        public void TryLoad()
        {
            if (board == null) return;
            if (!File.Exists(_pathLive)) return;

            try
            {
                string json = File.ReadAllText(_pathLive, Encoding.UTF8);
                var dto = JsonUtility.FromJson<SaveDTO>(json);

                if (dto == null || dto.cells == null)
                {
                    Debug.LogWarning("[Save] load failed (schema mismatch). Resetting with notice.");
                    return;
                }

                ApplyCells(dto);
                ApplyEnergy(dto.energy);

                Debug.Log($"[Save] loaded {dto.cells.Count} cells from {_pathLive}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save] load failed: {ex.Message}. Resetting with notice.");
            }
        }

        // --------------- Helpers ---------------

        private void FillCells(List<CellDTO> outList)
        {
            outList.Clear();
            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    var v = board.Get(r, c);
                    if (v == null || v.IsEmpty) continue;

                    outList.Add(new CellDTO
                    {
                        r = r,
                        c = c,
                        kind   = v.Kind == TileKind.Generator ? "Generator" : "Item",
                        tier   = v.Tier,
                        family = board.GetFamily(v)
                    });
                }
            }
        }

        private void ApplyCells(SaveDTO dto)
        {
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    board.Get(r, c)?.Clear();

            foreach (var cell in dto.cells)
            {
                if (cell.r < 0 || cell.c < 0 || cell.r >= board.Rows || cell.c >= board.Cols) continue;
                var v = board.Get(cell.r, cell.c);
                if (v == null) continue;

                var family = string.IsNullOrEmpty(cell.family) ? board.defaultGeneratorFamily : cell.family;

                if (string.Equals(cell.kind, "Generator", StringComparison.OrdinalIgnoreCase))
                {
                    var sprite = board.generatorSprite != null ? board.generatorSprite :
                                 (board.icons != null && board.icons.Count > 0 ? board.icons[0] : null);
                    v.SetGenerator(sprite, Mathf.Max(0, cell.tier));
                }
                else
                {
                    Sprite icon = null;
                    if (board.icons != null && board.icons.Count > 0)
                    {
                        int idx = Mathf.Clamp(cell.tier, 0, board.icons.Count - 1);
                        icon = board.icons[idx];
                    }
                    v.SetItem(icon, Mathf.Max(0, cell.tier));
                }

                board.SetFamily(v, family);
            }

            board.FireItemCreatedForCurrentBoard();
        }

        private static EnergyDTO BuildEnergyDTO()
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return null;

            var cfg = EnergyRuntime.Config;
            var mgr = EnergyRuntime.Manager;
            if (cfg == null || mgr == null) return null;

            mgr.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);

            return new EnergyDTO
            {
                current = mgr.Current,
                lastTickUtc = mgr.LastTickUtc.ToString("o")
            };
        }

        private static void ApplyEnergy(EnergyDTO energy)
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return;

            var cfg = EnergyRuntime.Config;
            if (cfg == null) return;

            if (energy == null)
            {
                if (EnergyRuntime.Manager == null)
                    EnergyRuntime.Manager = new EnergyManager(cfg.Start, cfg.Cap, DateTime.UtcNow);
                return;
            }

            if (!DateTime.TryParse(energy.lastTickUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var last))
                last = DateTime.UtcNow;

            EnergyRuntime.Manager = new EnergyManager(
                start: Mathf.Clamp(energy.current, 0, int.MaxValue),
                cap: cfg.Cap,
                lastTickUtc: last
            );

            EnergyRuntime.Manager.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);
        }

        private int SnapshotHash()
        {
            unchecked
            {
                int h = 17;
                for (int r = 0; r < board.Rows; r++)
                {
                    for (int c = 0; c < board.Cols; c++)
                    {
                        var v = board.Get(r, c);
                        int kind = v == null ? 0 : (v.IsEmpty ? 0 : (v.Kind == TileKind.Generator ? 2 : 1));
                        int tier = v == null || v.IsEmpty ? -1 : v.Tier;

                        h = h * 31 + r;
                        h = h * 31 + c;
                        h = h * 31 + kind;
                        h = h * 31 + tier;
                    }
                }

                var flags = FeatureFlagsRuntime.Current;
                if (flags != null && flags.EnergySystem && EnergyRuntime.Manager != null)
                {
                    h = h * 31 + EnergyRuntime.Manager.Current;
                    h = h * 31 + (int)(DateTime.UtcNow - EnergyRuntime.Manager.LastTickUtc).TotalSeconds;
                }

                return h;
            }
        }
    }
}
