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
        [SerializeField] List<Sprite> Icons = new List<Sprite>();
        [SerializeField] int MaxTier = 5;

        [Header("Initial Spawn")]
        [SerializeField] int InitialIconCount = 5;
        [SerializeField] int InitialMaxTier = 1;
        #endregion

        #region Private Fields
        List<BoardTileView> _slots = new List<BoardTileView>();
        BoardTileView _selected;
        BoardTileView _generatorView;
        GridLayoutGroup _grid;
        #endregion

        #region Public Properties (for other scripts)
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
            string tilePrefabName = (TilePrefab != null) ? TilePrefab.name : "null";
            Log($"Configuration: Rows={Rows}, Cols={Cols}, TilePrefab={tilePrefabName}");
            
            var existing = GetComponentsInChildren<BoardTileView>(true);
            Log($"Found {existing.Length} existing BoardTileView components in children");

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

            // GENERATOR SPAWNING WITH DEBUG LOGGING
            Log($"=== GENERATOR SPAWN ATTEMPT ===");
            Log($"  GeneratorPrefab: {(GeneratorPrefab != null ? GeneratorPrefab.name : "NULL")}");
            Log($"  GeneratorSlot: {GeneratorSlot}");
            Log($"  _slots.Count: {_slots.Count}");
            Log($"  Prefab check: {GeneratorPrefab != null}");
            Log($"  Slot >= 0: {GeneratorSlot >= 0}");
            Log($"  Slot < Count: {GeneratorSlot < _slots.Count}");

            if (GeneratorPrefab && GeneratorSlot >= 0 && GeneratorSlot < _slots.Count)
            {
                var slot = _slots[GeneratorSlot];
                Log($"  Target slot at index {GeneratorSlot}: {(slot != null ? slot.name : "NULL")}");

                if (slot)
                {
                    Log($"  Instantiating generator prefab as child of: {slot.name}");
                    var go = Instantiate(GeneratorPrefab, slot.transform);
                    Log($"  Generator GameObject created: {go.name}");

                    _generatorView = go.GetComponent<BoardTileView>();
                    if (_generatorView)
                    {
                        _generatorView.SetIsGenerator(true);
                        Log($"  ✓ Generator view configured successfully on {_generatorView.name}");
                        Log($"  Generator IsGenerator flag: {_generatorView.IsGenerator}");
                    }
                    else
                    {
                        Log("  ✗ ERROR: Generator prefab is missing BoardTileView component!");
                    }
                }
                else
                {
                    Log($"  ✗ ERROR: Slot at index {GeneratorSlot} is NULL!");
                }
            }
            else
            {
                Log($"  ✗ Generator spawn SKIPPED:");
                Log($"    - Prefab assigned: {GeneratorPrefab != null}");
                Log($"    - Slot index valid: {GeneratorSlot >= 0 && GeneratorSlot < _slots.Count} (slot={GeneratorSlot}, count={_slots.Count})");
            }
            Log($"=== GENERATOR SPAWN COMPLETE ===");

            if (InitialIconCount > 0)
            {
                Log($"Seeding {InitialIconCount} initial icons (max tier: {InitialMaxTier})");
                SeedInitialIcons();
            }

            Log("=== MergeBoardController.Start() END ===");
        }

        void Update()
        {
            // NOTE: Disabled because project uses new Input System package
            // BoardTileView handles clicks via OnMouseDown() instead
            /*
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f))
                {
                    var cell = hit.collider.GetComponentInParent<BoardTileView>();
                    if (cell) OnTileClicked(cell);
                }
            }
            */
        }
        #endregion

        #region Grid Building
        void BuildGrid()
        {
            if (!TilePrefab)
            {
                Log("ERROR: No TilePrefab assigned, cannot build grid");
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
            Log($"Built {_slots.Count} slots at runtime");
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
                    if (!xform)
                    {
                        Log($"WARNING: Expected slot not found: {slotName}");
                        continue;
                    }

                    var view = xform.GetComponent<BoardTileView>();
                    if (view)
                    {
                        _slots.Add(view);
                    }
                    else
                    {
                        Log($"WARNING: Slot {slotName} missing BoardTileView component");
                    }
                }
            }
            Log($"Mapped {_slots.Count} pre-placed slots to controller");
        }
        #endregion

        #region Initial Seeding
        void SeedInitialIcons()
        {
            if (Icons == null || Icons.Count == 0)
            {
                Log("WARNING: No icons available for seeding");
                return;
            }

            var emptySlots = new List<BoardTileView>();
            foreach (var slot in _slots)
            {
                if (slot && slot.IsEmpty && !slot.IsGenerator)
                {
                    emptySlots.Add(slot);
                }
            }

            Log($"Found {emptySlots.Count} empty slots available for seeding");

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

            Log($"Seeded {toSpawn} initial icons");
        }
        #endregion

        #region Interaction Logic (Public for BoardTileView)
        public void OnTileClicked(BoardTileView cell)
        {
            if (cell == null) return;

            if (cell.IsGenerator)
            {
                if (Icons != null && Icons.Count > 0)
                {
                    int icon = Random.Range(0, Icons.Count);
                    cell.SetSprite(GetSpriteFor(icon), icon, 0);
                    cell.SetIsGenerator(false);
                    if (_generatorView == cell) _generatorView = null;
                }
                else
                    Select(_generatorView);
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

        void TryMergeOrSwap(BoardTileView a, BoardTileView b)
        {
            if (a == null || b == null || a == b) return;

            if (a.IsEmpty && !b.IsEmpty) { var t = a; a = b; b = t; }

            if (!a.IsEmpty && b.IsEmpty)
            {
                b.SetSprite(GetSpriteFor(a.IconIndex), a.IconIndex, a.Tier);
                a.Clear();
                return;
            }

            if (!a.IsEmpty && !b.IsEmpty)
            {
                if (a.Tier == b.Tier)
                {
                    if (a.Tier < MaxTier)
                    {
                        int newTier = Mathf.Min(MaxTier, a.Tier + 1);
                        int icon = (b.IconIndex >= 0) ? b.IconIndex : a.IconIndex;
                        b.SetSprite(GetSpriteFor(icon), icon, newTier);
                        a.Clear();
                        return;
                    }
                    else
                    {
                        SwapTiles(a, b);
                        return;
                    }
                }
                SwapTiles(a, b);
            }
        }

        void SwapTiles(BoardTileView x, BoardTileView y)
        {
            if (x.IsGenerator || y.IsGenerator) return;

            var sx = GetSpriteFor(x.IconIndex);
            var sy = GetSpriteFor(y.IconIndex);
            int ix = x.IconIndex; int tx = x.Tier;
            int iy = y.IconIndex; int ty = y.Tier;

            y.SetSprite(sx, ix, tx);
            x.SetSprite(sy, iy, ty);
        }

        Sprite GetSpriteFor(int iconIndex)
        {
            if (Icons == null || Icons.Count == 0) return null;
            if (iconIndex < 0 || iconIndex >= Icons.Count)
                iconIndex = Mathf.Clamp(iconIndex, 0, Icons.Count - 1);
            return Icons[iconIndex];
        }

        void Select(BoardTileView v)
        {
            if (_selected == v) return;
            if (_selected) _selected.SetSelected(false);
            _selected = v;
            if (_selected) _selected.SetSelected(true);
        }
        #endregion

        #region Logging
        void Log(string msg)
        {
            Debug.Log($"[AQ] {msg}");
        }
        #endregion
    }
}