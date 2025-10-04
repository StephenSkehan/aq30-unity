using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        // Expose Rows/Cols for helper utilities (e.g., BoardTools)
        public int Rows => rows;
        public int Cols => cols;

        [Header("Content")]
        public Sprite generatorSprite;
        public List<Sprite> icons = new List<Sprite>();  // index = tier
        [Min(0)] public int maxTier = 5;

        [Header("Start")]
        public int defaultGeneratorRow = 4;
        public int defaultGeneratorCol = 3;

        [Header("Debug")]
        public bool debugLogs = true;

        // --- runtime ---
        public BoardTileView[,] grid;                    // [r,c] -> view
        readonly Dictionary<BoardTileView, (int r, int c)> index = new();

        static readonly Regex SlotName = new Regex(@"^slot_(\d{2})_(\d{2})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        void Awake()
        {
            if (!boardRoot) boardRoot = (RectTransform)transform;
        }

        void Start()
        {
            BuildGridFromChildren();
            if (icons == null) icons = new List<Sprite>();
            Log($"Start: icons.Count={icons.Count}, generatorSprite={(generatorSprite ? generatorSprite.name : "null")}");
            EnsureGeneratorExists();
        }

        // ---------------- Grid bootstrap ----------------

        void BuildGridFromChildren()
        {
            grid = new BoardTileView[rows, cols];
            index.Clear();

            int bound = 0;
            foreach (Transform child in boardRoot)
            {
                var m = SlotName.Match(child.name);
                if (!m.Success) continue;

                int r = int.Parse(m.Groups[1].Value);
                int c = int.Parse(m.Groups[2].Value);
                if (r < 0 || r >= rows || c < 0 || c >= cols) continue;

                var view = child.GetComponent<BoardTileView>();
                if (!view) view = child.gameObject.AddComponent<BoardTileView>();

                view.Bind(this, r, c);
                grid[r, c] = view;
                index[view] = (r, c);
                bound++;
            }

            Log($"BuildGridFromChildren: bound={bound} of expected={rows * cols}. boardRoot='{boardRoot?.name}'.");
        }

        void EnsureGeneratorExists()
        {
            var found = FindFirst(TileKind.Generator);
            if (found.r >= 0) return; // already have one

            var v = grid[defaultGeneratorRow, defaultGeneratorCol];
            if (!v) return;

            v.SetGenerator(generatorSprite, 0);
            Log($"EnsureGeneratorExists: placed generator at ({defaultGeneratorRow},{defaultGeneratorCol}) using sprite '{generatorSprite?.name}'.");
        }

        // ---------------- Tile helpers ----------------

        public (int r, int c) GetIndex(BoardTileView v) => index.TryGetValue(v, out var rc) ? rc : (-1, -1);

        public bool IsInside(int r, int c) => r >= 0 && r < rows && c >= 0 && c < cols;

        public BoardTileView Get(int r, int c) => IsInside(r, c) ? grid[r, c] : null;

        public Sprite SpriteForItemTier(int tier) => (tier >= 0 && tier < icons.Count) ? icons[tier] : null;

        public (int r, int c) FindFirst(TileKind kind)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var v = grid[r, c];
                    if (v != null && v.Kind == kind) return (r, c);
                }
            }
            return (-1, -1);
        }

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

        // ---------------- Spawning ----------------

        void SpawnFromGenerator(BoardTileView generator)
        {
            var dst = FindFirstEmptyFrom(generator);
            if (dst == null) return;

            var (r, c) = dst.Value;
            var target = Get(r, c);

            int tier = RollSpawnTier();
            target.SetItem(SpriteForItemTier(tier), tier);

            Log($"SpawnFromGenerator: spawned T{tier + 1} at ({r},{c}).");
        }

        int RollSpawnTier()
        {
            int[] odds = { 70, 20, 7, 2, 1, 0, 0, 0 };
            int highest = Mathf.Min(maxTier, icons.Count - 1);
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
