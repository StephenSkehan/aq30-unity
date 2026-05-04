using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using AQ.App.Config;
using AQ.App.Economy;
using AQ.Domain.Board;         // MergeRules
using AQ.App.UI.Common;       // ToastService
using AQ.SharedKernel.Economy;

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

        // Family mapping (in-memory)
        private readonly Dictionary<BoardTileView, string> familyKeyByTile = new();

        /// <summary>
        /// Fired after any board action that places a new item (spawn or merge result).
        /// Parameters: (family, tier) — consumed by MergeEventsBridge to publish
        /// ItemCreatedOnBoard on the GlobalBus for lead requirement matching.
        /// </summary>
        public static event Action<string, int> OnItemCreated;

        /// <summary>
        /// Fired when an item is removed from the board (consumed by a merge or lead activation).
        /// Parameters: (family, tier) — consumed by MergeEventsBridge to publish
        /// ItemRemovedFromBoard on the GlobalBus so LeadRequirementChecker can decrement live counts.
        /// </summary>
        public static event Action<string, int> OnItemRemoved;

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

        // ---------------- Public helpers ----------------

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

        private MergeRules.Tile ToRulesTile(BoardTileView v)
        {
            if (v == null || v.IsEmpty)
                return new MergeRules.Tile(TileKind.Empty, 0, string.Empty);

            var fam = GetFamily(v);
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

            (familyKeyByTile[b], familyKeyByTile[a]) = (GetFamily(a), GetFamily(b));
            if (a.IsEmpty) familyKeyByTile.Remove(a);
            if (b.IsEmpty) familyKeyByTile.Remove(b);

            Log("SwapTiles complete.");
        }

        private void MergeTiles(BoardTileView from, BoardTileView into)
        {
            int newTier = from.Tier + 1;

            if (from.Kind == TileKind.Generator && into.Kind == TileKind.Generator)
            {
                var genFam = GetFamily(from);
                int genTier = from.Tier;
                into.SetGenerator(generatorSprite, newTier);
                if (!HasFamily(into))
                    SetFamily(into, genFam);
                from.Clear();
                familyKeyByTile.Remove(from);
                OnItemRemoved?.Invoke(genFam, genTier);
                Log($"MergeTiles (Generator): {newTier - 1}+{newTier - 1}->{newTier}");
                return;
            }

            // Capture both items' identities before any state changes.
            var fromFam  = GetFamily(from);
            var intoFam  = HasFamily(into) ? GetFamily(into) : GetFamily(from);
            int fromTier = from.Tier;
            int intoTier = into.Tier;

            var fam = intoFam;
            into.SetItem(SpriteForItemTier(newTier), newTier);
            if (!string.IsNullOrEmpty(fam))
                SetFamily(into, fam);

            from.Clear();
            familyKeyByTile.Remove(from);

            // Fire removal for both consumed source items, then creation for the result.
            OnItemRemoved?.Invoke(fromFam, fromTier);
            OnItemRemoved?.Invoke(intoFam, intoTier);
            Log($"MergeTiles (Item): {newTier - 1}+{newTier - 1}->{newTier}");
            OnItemCreated?.Invoke(fam, newTier);
        }

        private void SpawnFromGenerator(BoardTileView generator)
        {
            // 1) Find destination first (no energy charge on full board)
            var dst = FindFirstEmptyFrom(generator);
            if (dst == null)
            {
                Log("No empty cells found for spawn.");
                ToastService.Show("board_full", "Board full — try selling or free a slot.", 1.8f);
                return;
            }

            // 2) If EnergySystem is ON, consume exactly once now
            var flags = FeatureFlagsRuntime.Current;
            if (flags != null && flags.EnergySystem)
            {
                var wallet = WalletLocator.Instance;
                if (wallet != null)
                {
                    if (!wallet.TrySpend(Currency.Energy, 1, "generator.spawn"))
                    {
                        Log("Energy insufficient — spawn cancelled.");
                        ToastService.Show("out_of_energy", "Out of energy.", 1.8f);
                        return;
                    }
                }
                else
                {
                    Log("[Energy] Wallet missing; allowing spawn (flag ON).");
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
            OnItemCreated?.Invoke(genFamily, tier);
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

                view.Bind(this, r, c);

                grid[r, c] = view;
                index[view] = (r, c);
                bound++;
            }

            Log($"BuildGridFromChildren: bound={bound} of expected={rows * cols}. boardRoot='{boardRoot?.name}'.");
        }

        private void EnsureGeneratorExists()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (grid[r, c] && grid[r, c].Kind == TileKind.Generator)
                    {
                        var v = grid[r, c];
                        if (!HasFamily(v)) SetFamily(v, defaultGeneratorFamily);
                        return;
                    }
                }
            }

            var cell = Get(defaultGeneratorRow, defaultGeneratorCol);
            if (!cell) return;

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

        // ---------------- Family helpers (now public for save system) ----------------

        public bool HasFamily(BoardTileView v)
        {
            return v != null && familyKeyByTile.ContainsKey(v);
        }

        public string GetFamily(BoardTileView v)
        {
            if (v == null || v.IsEmpty) return string.Empty;
            return familyKeyByTile.TryGetValue(v, out var fam) ? fam : string.Empty;
        }

        public void SetFamily(BoardTileView v, string fam)
        {
            if (v == null) return;
            if (string.IsNullOrEmpty(fam))
            {
                familyKeyByTile.Remove(v);
                return;
            }
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

        // ---------------- Lead activation ----------------

        /// <summary>
        /// Finds the first tile matching (family, tier), fires OnItemRemoved, and clears it.
        /// Called once per lead requirement during lead activation to consume qualifying items.
        /// Returns false if no matching tile was found.
        /// </summary>
        public bool TryClearItem(string family, int tier)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var v = grid[r, c];
                    if (v == null || v.IsEmpty || v.Kind == TileKind.Generator) continue;
                    if (v.Tier != tier) continue;
                    if (GetFamily(v) != family) continue;

                    OnItemRemoved?.Invoke(family, tier);
                    v.Clear();
                    familyKeyByTile.Remove(v);
                    return true;
                }
            }
            return false;
        }

        // ---------------- Save-restore helpers ----------------

        /// <summary>
        /// Re-fires OnItemCreated for every non-generator tile that has a family.
        /// Called by BoardSaveSystem after restoring board state so LeadRequirementChecker
        /// can process items that were placed without going through a spawn/merge path.
        /// </summary>
        public void FireItemCreatedForCurrentBoard()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var v = grid[r, c];
                    if (v == null || v.IsEmpty || v.Kind == TileKind.Generator) continue;
                    var fam = GetFamily(v);
                    if (!string.IsNullOrEmpty(fam))
                        OnItemCreated?.Invoke(fam, v.Tier);
                }
            }
        }

        // ---------------- Logging ----------------

        public void Log(string msg)
        {
            if (debugLogs) Debug.Log(msg);
        }
    }
}
