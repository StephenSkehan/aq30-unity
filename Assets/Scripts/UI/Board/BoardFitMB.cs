using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic board fit (Stephen-ruled 2026-07-17): the 7x9 grid used a fixed
/// 142px cell, clipping the playable bottom row 93-192px on 16:9 phones and
/// iPads. This measures the real vertical budget at runtime — container top
/// down to a reserve line above the corner widgets (locker + evidence board
/// buttons must never sit on top of cells) — and shrinks the cell size to fit.
/// Everything is measured in live world/local space so canvas scalers and
/// aspect ratios are handled uniformly. Re-fits when the resolution changes
/// (device rotation is locked, but the editor Game view sweeps sizes).
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
public class BoardFitMB : MonoBehaviour
{
    const float DesignCell    = 142f;  // BoardDensityPass design size — never exceeded
    const float Gutter        = 6f;
    const float MinCell       = 72f;   // absolute floor; below this the board is unusable anyway
    const float ReserveMargin = 28f;   // gap between grid bottom and the button strip
                                       // (covers the BoardFrame plate's 14px overhang)
    const int   Cols = 7, Rows = 9;

    GridLayoutGroup _grid;
    RectTransform   _rt;
    int _lastW, _lastH;
    float _lastCell = -1f;
    int _settleFrames = 150; // corner widgets self-install over the first frames

    void Awake()
    {
        _grid = GetComponent<GridLayoutGroup>();
        _rt   = (RectTransform)transform;
    }

    void LateUpdate()
    {
        bool settling = _settleFrames > 0;
        if (settling) _settleFrames--;

        if (Screen.width != _lastW || Screen.height != _lastH ||
            (settling && Time.frameCount % 10 == 0))
            Refit();
    }

    void Refit()
    {
        // Runtime-only: the QA sweep SendMessages this — in edit mode it would
        // mutate the serialized grid and dirty the scene (bitten 2026-07-18).
        if (!Application.isPlaying) return;

        _lastW = Screen.width;
        _lastH = Screen.height;

        // Reserve line: the highest top edge of the corner widgets, in screen px.
        // (Overlay canvases put world corners in screen pixels, whatever their
        // scaler; the overflow bucket sits below the locker button, so these two
        // cover the whole strip.)
        float reserveScreenY = 0f;
        var corners = new Vector3[4];
        foreach (var rootName in new[] { "__LockerBtn", "__EvidBoardBtn" })
        {
            var root = GameObject.Find(rootName);
            var btn  = root != null ? root.transform.Find("Btn") as RectTransform : null;
            if (btn == null) continue;
            btn.GetWorldCorners(corners);
            reserveScreenY = Mathf.Max(reserveScreenY, corners[1].y);
        }

        // Convert the reserve line into this rect's local space (handles the
        // board canvas's own scaling) and derive the height budget.
        var  rect        = _rt.rect;
        float localFloor = rect.yMin;
        if (reserveScreenY > 0f)
        {
            float localReserve = _rt.InverseTransformPoint(new Vector3(0f, reserveScreenY + ReserveMargin, 0f)).y;
            localFloor = Mathf.Max(localFloor, localReserve);
        }

        float availH = rect.yMax - localFloor;
        float availW = rect.width;

        float cell = Mathf.Min(DesignCell,
                               (availH - (Rows - 1) * Gutter) / Rows,
                               (availW - (Cols - 1) * Gutter) / Cols);
        cell = Mathf.Max(MinCell, Mathf.Floor(cell));

        if (Mathf.Abs(cell - _lastCell) < 0.5f) return;
        _lastCell = cell;

        _grid.cellSize       = new Vector2(cell, cell);
        _grid.spacing        = new Vector2(Gutter, Gutter);
        _grid.childAlignment = TextAnchor.UpperCenter; // shrunk grid stays centered

        FitFrame(cell);
        Debug.Log($"[BoardFit] {Screen.width}x{Screen.height}: cell {cell:F0}px (availH {availH:F0}, reserveY {reserveScreenY:F0})");
    }

    /// <summary>Resize the BoardFrame backdrop plate (ignoreLayout child) to hug the fitted grid.</summary>
    void FitFrame(float cell)
    {
        var frame = transform.Find("BoardFrame") as RectTransform;
        if (frame == null) return;

        float w = Cols * cell + (Cols - 1) * Gutter;
        float h = Rows * cell + (Rows - 1) * Gutter;
        const float pad = 14f;

        frame.anchorMin        = new Vector2(0.5f, 1f);
        frame.anchorMax        = new Vector2(0.5f, 1f);
        frame.pivot            = new Vector2(0.5f, 1f);
        frame.sizeDelta        = new Vector2(w + pad * 2f, h + pad * 2f);
        frame.anchoredPosition = new Vector2(0f, pad);
    }
}
