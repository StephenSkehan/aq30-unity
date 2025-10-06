using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using AQ.App.Config;
using AQ.App.Services;
using AQ.Domain.Board; // MergeRules

namespace AQ.App.UI.Board
{
    public enum TileKind { Empty, Item, Generator }

    [DisallowMultipleComponent]
    public class MergeBoardController : MonoBehaviour
    {
        [Header("Board")]
        [Min(1)] public int rows = 9;
        [Min(1)] public int cols = 7;
        public RectTransform boardRoot;
        public GraphicRaycaster raycaster;

        // Helpers
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

        [Header("Families")]
        [Tooltip("Family key assigned to the default generator; spawned items inherit their origin generator's family.")]
        public string defaultGeneratorFamily = "stakeout_fuel";

        [Header("Debug")]
        public bool debugLogs = false;

        // Backing state
        private BoardTileView[,] grid;
        private readonly Dictionary<BoardTileView, (int r, int c)> index = new();

        // NEW: family mapping for tiles (kept in controller for zero-churn integration)
        private readonly Dictionary<BoardTileView, string> familyKeyByTile = new();

        // ---------------- Unity lifecycle ----------------

        private void Awake()
        {
            if (!boardRoot) boardRoot = GetComponent<RectTransform>();
            BuildGridFromChildren();
            EnsureGeneratorExists();
        }

        private void Start()
        {
            Log("MergeBoardController.Start complete.");
        }

        // ---------------- Public helpers (used by Save/FX/etc.) ----------------

        public (int r, int c) GetIndex(BoardTileView v) => v != null && index.TryGetValue(v, out var rc) ? rc : (-1, -1);
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

            if (sr == tr && sc == tc)
            {
                a.SnapToGrid();
                return;
            }

            // Family-aware rules (families are now stamped on generator + spawns)
            var outcome = MergeRules.Decide(ToRulesTile(a), ToRulesTile(b), maxTier);

            switch (outcome)
            {
                case MergeRules.Outcome.Move:
                    MoveTile(a, b);
                    return;

                case MergeRules.Outcome.Merge:
                    MergeTiles(a, b);
                    return;

                case MergeRules.Outcome.CeilingSwap:
                    SwapTiles(a, b);
                    Log("Ceiling hit swap (max-tier vs max-tier).");
                    return;

                case MergeRules.Outcome.Swap:
                default:
                    SwapTiles(a, b);
                    return;
            }
        }

        // Convert a BoardTileView to a MergeRules.Tile (includes family if present)
        private MergeRules.Tile ToRulesTile(BoardTileView v)
        {
            if (v == null || v.IsEmpty)
                return new MergeRules.Tile(TileKind.Empty, 0, string.Empty);

            var fam = GetFamily(v); // empty string if unknown
            return new MergeRules.Tile(v.Kind, v.Tier, fam);
        }

        // ---------------- Actions ----------------

        private void MoveTile(BoardTileView from, BoardTileView to)
        {
            to.CopyFrom(from);
            TransferFamily(from, to);
            from.Clear();
            familyKeyByTile.Remove(from);
            Log("MoveTile complete.");
        }

        private void SwapTiles(BoardTileView a, BoardTileView b)
        {
            var temp = b.Payload;
            b.Payload = a.Payload;
            a.Payload = temp;
            a.Refresh();
            b.Refresh();

            // Swap family keys as well
            (familyKeyByTile[b], familyKeyByTile[a]) = (GetFamily(a), GetFamily(b));
            // Clean empties if any
            if (a.IsEmpty) familyKeyByTile.Remove(a);
            if (b.IsEmpty) familyKeyByTile.Remove(b);

            Log("SwapTiles complete.");
        }

        private void MergeTiles(BoardTileView from, BoardTileView into)
        {
            int newTier = from.Tier + 1;

            if (from.Kind == TileKind.Generator && into.Kind == TileKind.Generator)
            {
                into.SetGenerator(generatorSprite, newTier);
                // Keep the into's generator family; prefer into, otherwise inherit from 'from'
                if (!HasFamily(into))
                    SetFamily(into, GetFamily(from));
                from.Clear();
                familyKeyByTile.Remove(from);
                Log($"MergeTiles (Generator): {newTier - 1}+{newTier - 1}->{newTier}");
                return;
            }

            // Item merge — into keeps its family; if it had none, inherit 'from'
            var fam = HasFamily(into) ? GetFamily(into) : GetFamily(from);
            into.SetItem(SpriteForItemTier(newTier), newTier);
            if (!string.IsNullOrEmpty(fam))
                SetFamily(into, fam);

            from.Clear();
            familyKeyByTile.Remove(from);
            Log($"MergeTiles (Item): {newTier - 1}+{newTier - 1}->{newTier}");
        }

        private void SpawnFromGenerator(BoardTileView generator)
        {
            // 1) Find destination first (no energy charge on full board)
            var dst = FindFirstEmptyFrom(generator);
            if (dst == null)
            {
                Log("No empty cells found for spawn.");
                return;
            }

            // 2) If EnergySystem is ON, consume exactly once now
            var flags = FeatureFlagsRuntime.Current;
            if (flags != null && flags.EnergySystem)
            {
                var cfg = EnergyRuntime.Config;
                var mgr = EnergyRuntime.Manager;

                if (mgr != null && cfg != null)
                {
                    mgr.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);
                    if (!mgr.TryConsume(1))
                    {
                        Log("Energy insufficient — spawn cancelled.");
                        return; // nothing placed, nothing charged
                    }
                }
                else
                {
                    Log("[Energy] Config/Manager missing; allowing spawn (flag ON).");
                }
            }

            // 3) Place the item + stamp family from the origin generator
            var (r, c) = dst.Value;
            var v = Get(r, c);
            if (!v) return;

            int tier = RollSpawnTier();
            v.SetItem(SpriteForItemTier(tier), tier);

            var genFamily = GetFamily(generator);
            if (string.IsNullOrEmpty(genFamily)) genFamily = defaultGeneratorFamily;
            SetFamily(v, genFamily);

            Log($"Spawned item T{tier + 1} at ({r},{c}) family={genFamily}.");
        }

        // ---------------- Visuals & content ----------------

        private Sprite SpriteForItemTier(int tierZeroBased)
        {
            if (icons == null || icons.Count == 0) return null;
            int idx = Mathf.Clamp(tierZeroBased, 0, icons.Count - 1);
            return icons[idx];
        }

        // ---------------- Init grid ----------------

        private void BuildGridFromChildren()
        {
            grid = new BoardTileView[rows, cols];
            index.Clear();
            familyKeyByTile.Clear();

            // Children named: slot_00_00 .. slot_rr_cc (two-digit indices)
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

                // IMPORTANT: bind so the Item Image is cached
                view.Bind(this, r, c);

                grid[r, c] = view;
                index[view] = (r, c);
                bound++;
            }

            Log($"BuildGridFromChildren: bound={bound} of expected={rows * cols}. boardRoot='{boardRoot?.name}'.");
        }

        private void EnsureGeneratorExists()
        {
            // Any generator already placed?
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (grid[r, c] && grid[r, c].Kind == TileKind.Generator)
                    {
                        // If it wasn't stamped (older saves), give it a default family
                        var v = grid[r, c];
                        if (!HasFamily(v)) SetFamily(v, defaultGeneratorFamily);
                        return;
                    }
                }
            }

            var cell = Get(defaultGeneratorRow, defaultGeneratorCol);
            if (!cell) return;

            // Fallback: if generatorSprite is unset, use icons[0] so it’s visible
            var sprite = generatorSprite != null ? generatorSprite : SpriteForItemTier(0);
            cell.SetGenerator(sprite, 0);
            SetFamily(cell, defaultGeneratorFamily);

            Log($"Generator ensured at ({defaultGeneratorRow},{defaultGeneratorCol}) family={defaultGeneratorFamily}.");
        }

        // ---------------- RNG helpers ----------------

        private int RollSpawnTier()
        {
            int highest = Mathf.Min(maxTier, icons != null ? icons.Count - 1 : maxTier);
            int sum = 0;
            for (int i = 0; i <= highest; i++)
                sum += odds[Mathf.Min(i, odds.Length - 1)];

            int roll = UnityEngine.Random.Range(0, sum);
            int acc = 0;
            for (int i = 0; i <= highest; i++)
            {
                acc += odds[Mathf.Min(i, odds.Length - 1)];
                if (roll < acc) return i;
            }
            return 0;
        }

        // ---------------- Spawn placement ----------------

        private (int r, int c)? FindFirstEmptyFrom(BoardTileView origin)
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

            // Legacy path (scan Manhattan ring via bounding box)
            for (int radius = 1; radius <= rows + cols; radius++)
            {
                int rMin = Mathf.Max(0, sr - radius);
                int rMax = Mathf.Min(rows - 1, sr + radius);
                int cMin = Mathf.Max(0, sc - radius);
                int cMax = Mathf.Min(cols - 1, sc + radius);

                for (int r = rMin; r <= rMax; r++)
                {
                    for (int c = cMin; c <= cMax; c++)
                    {
                        if (Mathf.Abs(r - sr) + Mathf.Abs(c - sc) != radius) continue;
                        var v = grid[r, c];
                        if (v != null && v.IsEmpty) return (r, c);
                    }
                }
            }

            return null;
        }

        // ---------------- Family helpers ----------------

        private bool HasFamily(BoardTileView v) => v != null && familyKeyByTile.ContainsKey(v);

        private string GetFamily(BoardTileView v)
        {
            if (v == null || v.IsEmpty) return string.Empty;
            return familyKeyByTile.TryGetValue(v, out var fam) ? fam : string.Empty;
        }

        private void SetFamily(BoardTileView v, string fam)
        {
            if (v == null) return;
            if (string.IsNullOrEmpty(fam)) { familyKeyByTile.Remove(v); return; }
            familyKeyByTile[v] = fam;
        }

        private void TransferFamily(BoardTileView from, BoardTileView to)
        {
            if (from == null || to == null) return;
            if (familyKeyByTile.TryGetValue(from, out var fam))
            {
                familyKeyByTile[to] = fam;
            }
            else
            {
                familyKeyByTile.Remove(to);
            }
        }

        // ---------------- Logging ----------------

        public void Log(string msg)
        {
            if (debugLogs) Debug.Log(msg);
        }
    }
}
