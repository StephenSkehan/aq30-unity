using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    public class MergeBoardController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Board Setup")]
        [SerializeField] int Rows = 7;
        [SerializeField] int Cols = 9;

        [Header("Prefabs & Icons")]
        [SerializeField] GameObject TilePrefab;
        [SerializeField] GameObject GeneratorPrefab;
        [SerializeField] int GeneratorSlot = 0;
        [SerializeField] Sprite GeneratorSprite;
        [SerializeField] List<Sprite> Icons = new List<Sprite>();
        [SerializeField] int MaxTier = 5;

        [Header("Initial Spawn")]
        [SerializeField] int InitialIconCount = 0;
        [SerializeField] int InitialMaxTier = 1;
        #endregion

        #region Private Fields
        List<BoardTileView> _slots = new List<BoardTileView>();
        BoardTileView _selected;
        BoardTileView _generatorView;
        GridLayoutGroup _grid;
        #endregion

        #region Public Properties
        public RectTransform BoardRoot => transform as RectTransform;
        public GridLayoutGroup Grid => _grid;
        public List<BoardTileView> Slots => _slots;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            _grid = GetComponent<GridLayoutGroup>();
        }

        void Start()
        {
            Log("=== MergeBoardController.Start() BEGIN ===");
            
            var existing = GetComponentsInChildren<BoardTileView>(true);
            Log($"Found {existing.Length} existing BoardTileView components");

            if (existing.Length > 0)
            {
                Log("Using pre-placed slots from scene");
                MapExistingSlots(Rows, Cols);
            }
            else
            {
                Log("Building new grid at runtime");
                BuildGrid();
            }

            Log($"Total slots mapped: {_slots.Count}");

            // Spawn generator
            if (GeneratorPrefab && GeneratorSlot >= 0 && GeneratorSlot < _slots.Count)
            {
                var slot = _slots[GeneratorSlot];
                if (slot)
                {
                    var go = Instantiate(GeneratorPrefab, slot.transform);
                    _generatorView = go.GetComponent<BoardTileView>();
                    if (_generatorView)
                    {
                        _generatorView.SetIsGenerator(true);
                        
                        if (GeneratorSprite)
                        {
                            _generatorView.SetSprite(GeneratorSprite, -1, 0);
                            _generatorView.SetIsGenerator(true);
                            Log($"✓ Generator sprite set: {GeneratorSprite.name}");
                        }
                        
                        Log($"✓ Generator spawned at slot {GeneratorSlot} (row={slot.Row}, col={slot.Col})");
                    }
                }
            }

            if (InitialIconCount > 0)
            {
                Log($"Seeding {InitialIconCount} initial icons");
                SeedInitialIcons();
            }

            Log("=== MergeBoardController.Start() END ===");
        }
        #endregion

        #region Grid Building
        void BuildGrid()
        {
            if (!TilePrefab)
            {
                Log("ERROR: No TilePrefab assigned");
                return;
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var go = Instantiate(TilePrefab, transform);
                    go.name = $"slot_{r:00}_{c:00}";
                    var view = go.GetComponent<BoardTileView>();
                    if (view) _slots.Add(view);
                }
            }
            Log($"Built {_slots.Count} slots");
        }

        void MapExistingSlots(int rows, int cols)
        {
            _slots.Clear();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string slotName = $"slot_{r:00}_{c:00}";
                    var xform = transform.Find(slotName);
                    if (xform)
                    {
                        var view = xform.GetComponent<BoardTileView>();
                        if (view) _slots.Add(view);
                    }
                }
            }
            Log($"Mapped {_slots.Count} pre-placed slots");
        }
        #endregion

        #region Initial Seeding
        void SeedInitialIcons()
        {
            if (Icons == null || Icons.Count == 0) return;

            var emptySlots = new List<BoardTileView>();
            foreach (var slot in _slots)
            {
                if (slot && slot.IsEmpty && !slot.IsGenerator)
                    emptySlots.Add(slot);
            }

            int toSpawn = Mathf.Min(InitialIconCount, emptySlots.Count);
            for (int i = 0; i < toSpawn; i++)
            {
                int randomIndex = Random.Range(0, emptySlots.Count);
                var slot = emptySlots[randomIndex];
                emptySlots.RemoveAt(randomIndex);

                int iconIdx = Random.Range(0, Icons.Count);
                int tier = Random.Range(0, InitialMaxTier + 1);
                slot.SetSprite(GetSpriteFor(iconIdx), iconIdx, tier);
            }
        }
        #endregion

        #region Drag & Drop API
        public void BeginDrag(int row, int col)
        {
            Log($"BeginDrag from ({row},{col})");
        }

        public void EndDrag((int row, int col)? from, (int row, int col)? to)
        {
            if (from == null)
            {
                Log("EndDrag: No source tile");
                return;
            }

            if (to == null)
            {
                Log($"EndDrag: Dropped outside board, snap back");
                return;
            }

            Log($"EndDrag: ({from.Value.row},{from.Value.col}) → ({to.Value.row},{to.Value.col})");

            var fromTile = GetTileAt(from.Value.row, from.Value.col);
            var toTile = GetTileAt(to.Value.row, to.Value.col);

            if (fromTile == null || toTile == null)
            {
                Log("EndDrag: Invalid tiles");
                return;
            }

            if (toTile.IsEmpty)
            {
                Log("Action: MOVE");
                MoveTile(fromTile, toTile);
            }
            else if (CanMerge(fromTile, toTile))
            {
                Log("Action: MERGE");
                MergeTiles(fromTile, toTile);
            }
            else
            {
                Log("Action: SWAP");
                SwapTiles(fromTile, toTile);
            }
        }

        BoardTileView GetTileAt(int row, int col)
        {
            foreach (var tile in _slots)
            {
                if (tile.Row == row && tile.Col == col)
                    return tile;
            }
            return null;
        }

        bool CanMerge(BoardTileView a, BoardTileView b)
        {
            if (a == null || b == null) return false;
            if (a.IsEmpty || b.IsEmpty) return false;
            if (a.IsGenerator || b.IsGenerator) return false;
            return a.Tier == b.Tier && a.Tier < MaxTier;
        }

        void MoveTile(BoardTileView from, BoardTileView to)
        {
            Log($"MoveTile: ({from.Row},{from.Col}) -> ({to.Row},{to.Col})");
            to.SetSprite(GetSpriteFor(from.IconIndex), from.IconIndex, from.Tier);
            from.Clear();
            Log($"MoveTile complete");
        }

        void MergeTiles(BoardTileView from, BoardTileView to)
        {
            Log($"MergeTiles: ({from.Row},{from.Col}) + ({to.Row},{to.Col}) tier {to.Tier} -> {to.Tier + 1}");
            int newTier = Mathf.Min(MaxTier, to.Tier + 1);
            int iconIdx = to.IconIndex >= 0 ? to.IconIndex : from.IconIndex;
            to.SetSprite(GetSpriteFor(iconIdx), iconIdx, newTier);
            from.Clear();
            Log($"MergeTiles complete - target should be tier {newTier}");
        }

        void SwapTiles(BoardTileView a, BoardTileView b)
        {
            if (a.IsGenerator || b.IsGenerator) return;

            Log($"SwapTiles: ({a.Row},{a.Col}) <-> ({b.Row},{b.Col})");
            
            var spriteA = GetSpriteFor(a.IconIndex);
            var spriteB = GetSpriteFor(b.IconIndex);
            int idxA = a.IconIndex, tierA = a.Tier;
            int idxB = b.IconIndex, tierB = b.Tier;

            b.SetSprite(spriteA, idxA, tierA);
            a.SetSprite(spriteB, idxB, tierB);
            
            Log($"SwapTiles complete");
        }
        #endregion

        #region Click Interaction
        public void OnTileClicked(BoardTileView cell)
        {
            if (cell == null) return;

            if (cell.IsGenerator)
            {
                SpawnFromGenerator();
                return;
            }

            if (_selected == null || ReferenceEquals(_selected, _generatorView))
            {
                Select(cell);
                return;
            }

            TryMergeOrSwap(_selected, cell);
            Select(null);
        }

        void SpawnFromGenerator()
        {
            if (Icons == null || Icons.Count == 0) return;
            if (_generatorView == null) return;

            int iconIdx = Random.Range(0, Icons.Count);
            var sprite = GetSpriteFor(iconIdx);

            var targetSlot = FindFirstEmptyFromGenerator();

            if (targetSlot != null)
            {
                targetSlot.SetSprite(sprite, iconIdx, 0);
                Log($"Generator spawned icon {iconIdx} at slot ({targetSlot.Row},{targetSlot.Col})");
            }
            else
            {
                Log("No empty slots for generator spawn");
            }
        }

        BoardTileView FindFirstEmptyFromGenerator()
        {
            if (_generatorView == null) return null;
            
            var generatorSlot = _slots[GeneratorSlot];
            if (generatorSlot == null) return null;
            
            int startRow = generatorSlot.Row;
            int startCol = generatorSlot.Col;
            int totalCells = Rows * Cols;
            
            Log($"FindFirstEmptyFromGenerator starting from slot ({startRow},{startCol})");
            
            for (int i = 1; i < totalCells; i++)
            {
                int idx = ((startRow * Cols + startCol) + i) % totalCells;
                int r = idx / Cols;
                int c = idx % Cols;
                
                var slot = GetTileAt(r, c);
                if (slot != null && slot.IsEmpty)
                {
                    Log($"  Found empty slot at ({r},{c})");
                    return slot;
                }
            }
            
            Log("  No empty slots found");
            return null;
        }

        void TryMergeOrSwap(BoardTileView a, BoardTileView b)
        {
            if (a == null || b == null || a == b) return;

            if (a.IsEmpty && !b.IsEmpty) { var t = a; a = b; b = t; }

            if (!a.IsEmpty && b.IsEmpty)
            {
                MoveTile(a, b);
                return;
            }

            if (!a.IsEmpty && !b.IsEmpty)
            {
                if (CanMerge(a, b))
                {
                    MergeTiles(a, b);
                }
                else
                {
                    SwapTiles(a, b);
                }
            }
        }

        void Select(BoardTileView v)
        {
            if (_selected == v) return;
            if (_selected) _selected.SetSelected(false);
            _selected = v;
            if (_selected) _selected.SetSelected(true);
        }

        public void ClearSelection()
        {
            Select(null);
        }
        #endregion

        #region Helper Methods
        Sprite GetSpriteFor(int iconIndex)
        {
            if (Icons == null || Icons.Count == 0) return null;
            if (iconIndex < 0 || iconIndex >= Icons.Count)
                iconIndex = Mathf.Clamp(iconIndex, 0, Icons.Count - 1);
            return Icons[iconIndex];
        }

        void Log(string msg)
        {
            Debug.Log($"[AQ] {msg}");
        }
        #endregion
    }
}