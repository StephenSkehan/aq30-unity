using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using AQ.App.Analytics;
using AQ.App.Config;
using AQ.App.Economy;
using AQ.App.Generators;
using AQ.App.Items;
using AQ.App.Overflow;
using AQ.Domain.Board;         // MergeRules
using AQ.App.UI.Common;       // ToastService, TileInfoPopup
using AQ.App.UI.Board.FX;
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
        [Tooltip("Fallback odds per tier index (used when no GeneratorTypeSO is wired). Values repeat if shorter than maxTier.")]
        public int[] odds = new[] { 80, 15, 4, 1 };

        [Header("Generator Types")]
        [Tooltip("All GeneratorTypeSO assets used in this scene. Looked up by generatorTypeId at runtime.")]
        public GeneratorTypeSO[] generatorTypes;
        [Tooltip("GeneratorTypeSO assigned to the default generator placed at startup.")]
        public GeneratorTypeSO defaultGeneratorType;

        [Header("Defaults & Layout")]
        public int defaultGeneratorRow = 4;
        public int defaultGeneratorCol = 3;
        public float tileSpacing = 2f;

        [Header("Families")]
        [Tooltip("Fallback family key when no defaultGeneratorType SO is assigned.")]
        public string defaultGeneratorFamily = "stakeout_fuel";

        [Header("Item Definitions")]
        [SerializeField] private ItemDefinitionSO[] itemDefinitions = System.Array.Empty<ItemDefinitionSO>();

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

        /// <summary>
        /// Fired on any tap of a generator tile, regardless of outcome. Used by FTUE hint.
        /// </summary>
        public static event Action GeneratorTapped;

        // ---------------- Unity lifecycle ----------------

        private void OnEnable()  => BoardTileView.LongHeld += OnTileLongHeld;
        private void OnDisable() => BoardTileView.LongHeld -= OnTileLongHeld;

        private void OnTileLongHeld(BoardTileView tile)
        {
            if (tile == null || tile.IsEmpty) return;

            if (tile.Kind == TileKind.Generator)
            {
                var genType = FindGeneratorType(GetFamily(tile));
                GeneratorInfoPopup.Show(genType, tile.Tier, tile.Payload.sprite);
                return;
            }

            var family = GetFamily(tile);
            int tier   = tile.Tier;

            var def         = LookupItemDef(family, tier);
            string name     = def != null ? def.displayName : FormatFamilyTier(family, tier);
            Sprite icon     = def?.icon != null ? def.icon : tile.Payload.sprite;

            TileInfoPopup.Show(name, icon, family, tier);
        }

        private ItemDefinitionSO LookupItemDef(string family, int tier)
        {
            foreach (var def in itemDefinitions)
                if (def != null && def.family == family && def.tier == tier) return def;
            return null;
        }

        private static string FormatFamilyTier(string family, int tier)
            => $"{family.Replace('_', ' ')} T{tier + 1}";

        private void Awake()
        {
            if (!boardRoot) boardRoot = GetComponent<RectTransform>();
            GeneratorFamilyRegistry.Load();
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
            // Family must be transferred before CopyFrom (which triggers Refresh -> the
            // requirement-tick lookup on `to`) so GetItemId can resolve on the same pass.
            TransferFamily(from, to);
            to.CopyFrom(from);
            from.Clear();                           // tears down animator on source
            familyKeyByTile.Remove(from);
            if (to.Kind == TileKind.Generator)      // re-attach on destination
                AttachGeneratorAnimator(to, GetFamily(to), to.Tier);
            Log("MoveTile complete.");
        }

        private void SwapTiles(BoardTileView a, BoardTileView b)
        {
            var temp = b.Payload;
            b.Payload = a.Payload;
            a.Payload = temp;

            // Family must be swapped before the Refresh() calls below (the
            // requirement-tick lookup) so GetItemId resolves current, not stale, families.
            (familyKeyByTile[b], familyKeyByTile[a]) = (GetFamily(a), GetFamily(b));

            a.Refresh();
            b.Refresh();

            // Sync animators — payloads moved but component is on tile GO, not payload
            SyncGeneratorAnimator(a);
            SyncGeneratorAnimator(b);
            if (a.IsEmpty) familyKeyByTile.Remove(a);
            if (b.IsEmpty) familyKeyByTile.Remove(b);

            Log("SwapTiles complete.");
        }

        private void MergeTiles(BoardTileView from, BoardTileView into)
        {
            AQ.App.Services.HapticService.Light();
            int newTier = from.Tier + 1;

            if (from.Kind == TileKind.Generator && into.Kind == TileKind.Generator)
            {
                var genTypeId = GetFamily(from);
                int prevTier = from.Tier;
                var genSO = FindGeneratorType(genTypeId);
                var genSprite = genSO != null ? genSO.SpriteForTier(newTier) : generatorSprite;
                into.SetGenerator(genSprite, newTier);
                if (!HasFamily(into))
                    SetFamily(into, genTypeId);
                AttachGeneratorAnimator(into, genTypeId, newTier);
                from.Clear();
                familyKeyByTile.Remove(from);
                OnItemRemoved?.Invoke(genTypeId, prevTier);

                // Lock sub-gen drops once max tier is reached for this type
                if (genSO != null && newTier >= genSO.maxGeneratorTier)
                    GeneratorFamilyRegistry.SetSubGenLocked(genTypeId);

                Log($"MergeTiles (Generator): {newTier - 1}+{newTier - 1}->{newTier} type={genTypeId}");
                return;
            }

            // Capture both items' identities before any state changes.
            var fromFam  = GetFamily(from);
            var intoFam  = HasFamily(into) ? GetFamily(into) : GetFamily(from);
            int fromTier = from.Tier;
            int intoTier = into.Tier;

            // Family must be set before SetItem (which triggers Refresh -> the
            // requirement-tick lookup) so GetItemId can resolve on the same pass.
            var fam = intoFam;
            if (!string.IsNullOrEmpty(fam))
                SetFamily(into, fam);
            into.SetItem(SpriteForItem(fam, newTier), newTier);

            from.Clear();
            familyKeyByTile.Remove(from);

            // Fire removal for both consumed source items, then creation for the result.
            OnItemRemoved?.Invoke(fromFam, fromTier);
            OnItemRemoved?.Invoke(intoFam, intoTier);
            Log($"MergeTiles (Item): {newTier - 1}+{newTier - 1}->{newTier}");
            OnItemCreated?.Invoke(fam, newTier);
            GameAnalytics.LogMerge(fam, fromTier, newTier);
        }

        private void SpawnFromGenerator(BoardTileView generator)
        {
            GeneratorTapped?.Invoke();

            // 1) Find destination first (no energy charge on full board)
            var dst = FindFirstEmptyFrom(generator);
            if (dst == null)
            {
                Log("No empty cells found for spawn.");
                ToastService.Show("board_full", "Board full — try selling or free a slot.", 1.8f);
                return;
            }

            // 2) If EnergySystem is ON, consume exactly once now
            var featureFlags = FeatureFlagsRuntime.Current;
            if (featureFlags != null && featureFlags.EnergySystem)
            {
                var wallet = WalletLocator.Instance;
                if (wallet != null)
                {
                    if (!wallet.TrySpend(Currency.Energy, 1, "generator.spawn"))
                    {
                        Log("Energy insufficient — spawn cancelled.");
                        EnergyOutPopup.Show();
                        return;
                    }
                }
                else
                {
                    Log("[Energy] Wallet missing; allowing spawn (flag ON).");
                }
            }

            // 3) Roll drop table if SO is available
            var genTypeId = GetFamily(generator);
            var so = FindGeneratorType(genTypeId);
            DropEntry? drop = so != null ? DropRoller.Roll(so, generator.Tier) : null;

            // 4a) Sub-generator result → push to overflow bucket instead of board
            if (drop.HasValue && drop.Value.type == DropType.SubGenerator)
            {
                OverflowBucketService.Push(new OverflowTileData
                {
                    kind   = OverflowKind.Generator,
                    family = genTypeId,
                    tier   = 0
                });
                Log($"Sub-generator pushed to overflow (type={genTypeId}).");
                return;
            }

            // 4b) Item result (from SO or legacy fallback)
            var (r, c) = dst.Value;
            var v = Get(r, c);
            if (!v) return;

            string itemFamily;
            int itemTier;

            if (drop.HasValue)
            {
                itemFamily = drop.Value.itemFamily;
                itemTier   = drop.Value.itemTier;
            }
            else
            {
                // Legacy fallback: inherit generator's type id as family, roll by odds array
                itemFamily = string.IsNullOrEmpty(genTypeId) ? defaultGeneratorFamily : genTypeId;
                itemTier   = RollSpawnTier();
            }

            // Family must be set before SetItem (which triggers Refresh -> the
            // requirement-tick lookup) so GetItemId can resolve on the same pass.
            SetFamily(v, itemFamily);
            v.SetItem(SpriteForItem(itemFamily, itemTier), itemTier);

            Log($"Spawned item T{itemTier + 1} family={itemFamily} at ({r},{c}).");
            OnItemCreated?.Invoke(itemFamily, itemTier);
            GameAnalytics.LogSpawnRoll(itemFamily, itemTier);
        }

        // ---------------- Generator type lookup ----------------

        public GeneratorTypeSO FindGeneratorType(string typeId)
        {
            if (!string.IsNullOrEmpty(typeId) && generatorTypes != null)
                foreach (var so in generatorTypes)
                    if (so != null && so.generatorTypeId == typeId) return so;
            return defaultGeneratorType;
        }

        // ---------------- Generator tile animation ----------------

        public void AttachGeneratorAnimator(BoardTileView tile, string family, int tier)
        {
            if (!tile) return;
            var so = FindGeneratorType(family);
            if (so == null) return;
            var tierCfg = so.ConfigForTier(tier);
            if (tierCfg == null) return;

            var animator = tile.GetComponent<GeneratorTileAnimator>();
            if (animator == null) animator = tile.gameObject.AddComponent<GeneratorTileAnimator>();
            animator.Init(tierCfg.particles, (RectTransform)tile.transform);
        }

        // Attach or remove animator based on tile's current kind — call after any payload swap.
        private void SyncGeneratorAnimator(BoardTileView tile)
        {
            if (!tile) return;
            if (tile.Kind == TileKind.Generator)
                AttachGeneratorAnimator(tile, GetFamily(tile), tile.Tier);
            else
                tile.GetComponent<GeneratorTileAnimator>()?.Teardown();
        }

        // ---------------- Overflow bucket placement ----------------

        /// <summary>
        /// Attempts to place a tile from the overflow bucket onto the first available empty cell.
        /// Returns false if the board is full.
        /// </summary>
        public bool PlaceFromOverflow(OverflowTileData data)
        {
            var empty = FindAnyEmptyCell();
            if (empty == null) return false;

            var (r, c) = empty.Value;
            var v = Get(r, c);
            if (!v) return false;

            if (data.kind == OverflowKind.Generator)
            {
                var so = FindGeneratorType(data.family);
                var sprite = so != null ? so.SpriteForTier(data.tier) : generatorSprite;
                v.SetGenerator(sprite, data.tier);
                SetFamily(v, data.family);
                AttachGeneratorAnimator(v, data.family, data.tier);
                Log($"Placed generator from overflow: type={data.family} tier={data.tier} at ({r},{c}).");
            }
            else
            {
                // Family must be set before SetItem (which triggers Refresh -> the
                // requirement-tick lookup) so GetItemId can resolve on the same pass.
                SetFamily(v, data.family);
                v.SetItem(SpriteForItem(data.family, data.tier), data.tier);
                OnItemCreated?.Invoke(data.family, data.tier);
                Log($"Placed item from overflow: family={data.family} tier={data.tier} at ({r},{c}).");
            }

            return true;
        }

        private (int r, int c)? FindAnyEmptyCell()
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var v = grid[r, c];
                    if (v != null && v.IsEmpty) return (r, c);
                }
            return null;
        }

        // ---------------- Visuals & content ----------------

        private Sprite SpriteForItemTier(int tierZeroBased)
        {
            if (icons == null || icons.Count == 0) return null;
            int idx = Mathf.Clamp(tierZeroBased, 0, icons.Count - 1);
            return icons[idx];
        }

        public Sprite SpriteForItem(string family, int tier)
        {
            var def = LookupItemDef(family, tier);
            if (def != null && def.icon != null) return def.icon;
            return SpriteForItemTier(tier);
        }

        public Sprite SpriteForItemTierPublic(int tierZeroBased) => SpriteForItemTier(tierZeroBased);

        /// <summary>
        /// Resolves a tile's stable itemId (family+tier → ItemDefinitionSO.itemId) for
        /// requirement matching. Generators and empty tiles never satisfy a lead
        /// requirement, so this returns empty for them.
        /// </summary>
        public string GetItemId(BoardTileView v)
        {
            if (v == null || v.IsEmpty || v.Kind != TileKind.Item) return string.Empty;
            var fam = GetFamily(v);
            if (string.IsNullOrEmpty(fam)) return string.Empty;
            var def = LookupItemDef(fam, v.Tier);
            return def != null ? def.itemId : string.Empty;
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
            var typeId = defaultGeneratorType != null ? defaultGeneratorType.generatorTypeId : defaultGeneratorFamily;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (grid[r, c] && grid[r, c].Kind == TileKind.Generator)
                    {
                        var v = grid[r, c];
                        if (!HasFamily(v)) SetFamily(v, typeId);
                        return;
                    }
                }
            }

            var cell = Get(defaultGeneratorRow, defaultGeneratorCol);
            if (!cell) return;

            var sprite = defaultGeneratorType != null ? defaultGeneratorType.SpriteForTier(0)
                       : (generatorSprite != null   ? generatorSprite : SpriteForItemTier(0));
            cell.SetGenerator(sprite, 0);
            SetFamily(cell, typeId);
            AttachGeneratorAnimator(cell, typeId, 0);

            Log($"Generator ensured at ({defaultGeneratorRow},{defaultGeneratorCol}) typeId={typeId}.");
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
