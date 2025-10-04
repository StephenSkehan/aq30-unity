using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal boot helper: ensures GridLayout spacing and logs basic info.
/// Does NOT rebuild the grid when pre-placed slots exist.
/// </summary>
public class MergeBoardBoot : MonoBehaviour
{
    [SerializeField] RectTransform mergeBoard;       // root that owns the GridLayoutGroup
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] Vector2 spacing = new Vector2(2, 2);

    void Awake()
    {
        if (mergeBoard == null)
            mergeBoard = GetComponent<RectTransform>();
        if (grid == null && mergeBoard != null)
            grid = mergeBoard.GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        if (grid != null)
            grid.spacing = spacing;

        int childCount = mergeBoard != null ? mergeBoard.childCount : 0;
        Debug.Log($"MergeBoardBoot: Found {childCount} pre-placed slots, skipping runtime build.");
        Debug.Log($"MergeBoardBoot: Boot done. children={childCount}, grid.spacing=({spacing.x:F2}, {spacing.y:F2})");
    }
}
