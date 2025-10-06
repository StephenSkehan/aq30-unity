using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using AQ.App.Config;

namespace AQ.App.UI.Board
{
    public enum TileKind { Empty, Item, Generator }

    [DisallowMultipleComponent]
    public class MergeBoardController : MonoBehaviour
    {
        [Header("Board")]
        [Min(1)] public int rows = 9;
        [Min(1)] public int cols = 7;
        public RectTransform boardRoot;                  // MergeBoard
        public GraphicRaycaster raycaster;

        // Expose Rows/Cols for helper utilities
        public int Rows => rows;
        public int Cols => cols;
        public bool IsInside(int r, int c) => r >= 0 && c >= 0 && r < rows && c < cols;

        [Header("Content")]
        public Sprite generatorSprite;
        public List<Sprite> icons;
        [Range(1, 9)] public int maxTier = 6;
        [Tooltip("Odds per tier index: T1..Tn (values repeat if shorter than tiers)")]
        public int[] odds = new[] { 80, 15, 4, 1 };

        [Header("Defaults & Layout")]
        public int defaultGeneratorRow = 4;
        public int defaultGeneratorCol = 3;
        public float tileSpacing = 2f;

        [Header("Debug")]
        public bool debugLogs = false;

        // --- Backing state ---
        BoardTileView[,] grid;
        Dictionary<BoardTileView, (int r, int c)> index = new();

        // ---------------- Unity lifecycle ----------------

        void Awake()
        {
            if (!boardRoot) boardRoot = GetComponent<RectTransform>();
            BuildGridFromChildren();
            EnsureGeneratorExists();
        }

        void Start()
        {
            Log("MergeBoardController.Start complete.");
        }

        // ---------------- Public helpers used by other systems ----------------

        public (int r, int c) GetIndex(BoardTileView v) => index.TryGetValue(v, out var rc) ? rc : (-1, -1);
        public BoardTileView Get(int r, int c) => IsInside(r, c) ? grid[r, c] : null;

        // ---------------- Input entrypoints ----------------

        public void OnTileClicked(BoardTileView view)
        {
            var (r, c) = GetIndex(view);
            if (r < 0) return;

            if (view.Kind == TileKind.Generator)
            {
                Log($"Generator clicked at ({r},{c}).");
                SpawnFromGenerator(view);
            }
        }

        public void EndDrag((int r, int c)? src, (int r, int c)? dst)
        {
            if (src == null || dst == null) return;

            var (sr, sc) = src.Value;
            var (tr, tc) = dst.Value;

            var a = Get(sr, sc);
            var b = Get(tr, tc);
            if (!a || !b) return;
            if (a.IsEmpty) return;
            if (sr == tr && sc == tc) { a.SnapToGrid(); return; }

            if (b.IsEmpty)
            {
                MoveTile(a, b);
                return;
            }

            if (a.Tier == b.Tier)
            {
                if (a.Tier < maxTier) MergeTiles(a, b);
                else SwapTiles(a, b);
                return;
            }

            SwapTiles(a, b);
        }

        // ---------------- Actions ----------------

        void MoveTile(BoardTileView from, BoardTileView to)
        {
            to.CopyFrom(from);
            from.Clear();
            Log("MoveTile complete.");
        }

        void SwapTiles(BoardTileView a, BoardTileView b)
        {
            var temp = b.Payload;
            b.Payload = a.Payload;
            a.Payload = temp;
            a.Refresh();
            b.Refresh();
            Log("SwapTiles complete.");
        }

        void MergeTiles(BoardTileView from, BoardTileView into)
        {
            int newTier = from.Tier + 1;

            if (from.Kind == TileKind.Generator && into.Kind == TileKind.Generator)
            {
                into.SetGenerator(generatorSprite, newTier);
                from.Clear();
                Log($"MergeTiles (Generator): tier {newTier - 1} + {newTier - 1} -> {newTier}");
                return;
            }

            into.SetItem(SpriteForItemTier(newTier), newTier);
            from.Clear();
            Log($"MergeTiles (Item): tier {newTier - 1} + {newTier - 1} -> {newTier}");
        }

        void SpawnFromGenerator(BoardTileView generator)
        {
            var dst = FindFirstEmptyFrom(generator);
            if (dst == null)
            {
                Log("No empty cells found for spawn.");
                return;
            }

            var (r, c) = dst.Value;
            var v = Get(r, c);
            if (!v) return;

            int tier = RollSpawnTier();
            v.SetItem(SpriteForItemTier(tier), tier);
            Log($"Spawned item T{tier + 1} at ({r},{c}).");
        }

        // ---------------- Visuals & content ----------------

        Sprite SpriteForItemTier(int tierZeroBased)
        {
            if (icons == null || icons.Count == 0) return null;
            int idx = Mathf.Clamp(tierZeroBased, 0, icons.Count - 1);
            return icons[idx];
        }

        // ---------------- Init grid ----------------

        void BuildGridFromChildren()
        {
            grid = new BoardTileView[rows, cols];
            index.Clear();

            // Expect children named slot_00_00 .. slot_rr_cc
            var re = new Regex(@"slot_(\d{2})_(\d{2})");
            int bound = 0;

            foreach (Transform child in boardRoot)
            {
                var m = re.Match(child.name);
                if (!m.Success) continue;

                int r = int.Parse(m.Groups[1].Value);
                int c = int.Parse(m.Groups[2].Value);
                if (!IsInside(r, c)) continue;

                var view = child.GetComponent<BoardTileView>();
                if (!view) continue;

                // CRITICAL: bind view so it finds its Item image and caches references
                view.Bind(this, r, c);

                grid[r, c] = view;
                index[view] = (r, c);
                bound++;
            }

            Log($"BuildGridFromChildren: bound={bound} of expected={rows * cols}. boardRoot='{boardRoot?.name}'.");
        }

        void EnsureGeneratorExists()
        {
            // scan for any generator first
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (grid[r,c] && grid[r,c].Kind == TileKind.Generator)
                        return;

            var v = Get(defaultGeneratorRow, defaultGeneratorCol);
            if (v == null) return;

            // Fallback: if generatorSprite is not assigned, prefer icons[0] so it's visible
            var sprite = generatorSprite != null ? generatorSprite : SpriteForItemTier(0);
            v.SetGenerator(sprite, 0);
            Log($"Generator ensured at ({defaultGeneratorRow},{defaultGeneratorCol}).");
        }

        // ---------------- RNG helpers ----------------

        int RollSpawnTier()
        {
            int highest = Mathf.Min(maxTier, icons != null ? icons.Count - 1 : maxTier);
            int sum = 0;
            for (int i = 0; i <= highest; i++) sum += odds[Mathf.Min(i, odds.Length - 1)];

            int roll = UnityEngine.Random.Range(0, sum);
            int acc = 0;
            for (int i = 0; i <= highest; i++)
            {
                acc += odds[Mathf.Min(i, odds.Length - 1)];
                if (roll < acc) return i;
            }
            return 0;
        }

        (int r, int c)? FindFirstEmptyFrom(BoardTileView origin)
        {
            var (sr, sc) = GetIndex(origin);
            if (sr < 0) return null;

            // Flag-gated deterministic spawn order: Manhattan rings, clockwise-from-North
            var flags = FeatureFlagsRuntime.Current;
            if (flags != null && flags.SpawnRingTraversal == SpawnRingMode.ManhattanClockwiseFromNorth)
            {
                int maxD = Math.Max(sr, rows - 1 - sr) + Math.Max(sc, cols - 1 - sc);
                for (int d = 1; d <= maxD; d++)
                {
                    foreach (var cell in AQ.Domain.Board.BoardRingTraversal.EnumerateRing(rows, cols, sr, sc, d))
                    {
                        int r = cell.r;
                        int c = cell.c;
                        var v = Get(r, c);
                        if (v != null && v.IsEmpty) return (r, c);
                    }
                }
                return null;
            }

            // Legacy path (row/col scanline within bounding box per radius)
            for (int radius = 1; radius <= rows + cols; radius++)
            {
                int rMin = Mathf.Max(0, sr - radius);
                int rMax = Mathf.Min(rows - 1, sr + radius);
                int cMin = Mathf.Max(0, sc - radius);
                int cMax = Mathf.Min(cols - 1, sc + radius);

                for (int r = rMin; r <= rMax; r++)
                for (int c = cMin; c <= cMax; c++)
                {
                    if (Mathf.Abs(r - sr) + Mathf.Abs(c - sc) != radius) continue;
                    var v = grid[r, c];
                    if (v != null && v.IsEmpty) return (r, c);
                }
            }
            return null;
        }

        // ---------------- Logging ----------------

        public void Log(string msg)
        {
            if (debugLogs) Debug.Log(msg);
        }
    }
}
