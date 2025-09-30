using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class MergeBoardController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform viewport;     // Assign BoardViewport
    [SerializeField] private int columns = 7;
    [SerializeField] private int rows = 9;
    [SerializeField] private float spacing = 6f;
    [SerializeField] private float padding = 8f;

    [Header("Prefabs")]
    [SerializeField] private BoardTileView tilePrefab;

    private GridLayoutGroup grid;
    private readonly List<BoardTileView> tiles = new List<BoardTileView>();
    public IReadOnlyList<BoardTileView> Tiles => tiles;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = new Vector2(spacing, spacing);
        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
    }

    private void OnEnable()
    {
        BuildGrid();
        AutoSizeCells();
    }

    private void OnTransformParentChanged() => AutoSizeCells();
    private void OnRectTransformDimensionsChange() => AutoSizeCells();

    public void BuildGrid()
    {
        if (!tilePrefab)
        {
            Debug.LogWarning("[MergeBoardController] Tile prefab not assigned.");
            return;
        }

        int needed = columns * rows;

        // Destroy extras
        for (int i = tiles.Count - 1; i >= needed; i--)
        {
            if (tiles[i]) DestroyImmediate(tiles[i].gameObject);
            tiles.RemoveAt(i);
        }

        // Spawn missing
        while (tiles.Count < needed)
        {
            var t = Instantiate(tilePrefab, transform);
            t.name = $"Tile_{tiles.Count:D2}";
            tiles.Add(t);
        }
    }

    public void AutoSizeCells()
    {
        if (!viewport || !grid) return;

        var rect = viewport.rect;
        float totalW = Mathf.Max(0, rect.width - padding * 2 - spacing * (columns - 1));
        float totalH = Mathf.Max(0, rect.height - padding * 2 - spacing * (rows - 1));

        float cellW = Mathf.Floor(totalW / columns);
        float cellH = Mathf.Floor(totalH / rows);
        float cell = Mathf.Max(24f, Mathf.Min(cellW, cellH)); // never smaller than 24

        grid.cellSize = new Vector2(cell, cell);
        grid.spacing = new Vector2(spacing, spacing);
        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        grid.constraintCount = columns;
    }

    public BoardTileView GetTile(int index)
    {
        if (index < 0 || index >= tiles.Count) return null;
        return tiles[index];
    }
}
