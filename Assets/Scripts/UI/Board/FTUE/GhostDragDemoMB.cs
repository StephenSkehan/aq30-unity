using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI.Board;

/// <summary>
/// The genre's hand-cursor convention, in item form (Stephen-ruled 2026-08-21):
/// a highlight says "these matter" but only a demonstration says "do this" —
/// so a translucent copy of one item repeatedly slides onto its twin and fades,
/// until the player performs the merge themselves. Purely visual: its canvas is
/// never a raycast target, so board input is untouched. Used by the L1
/// first-merge choreography, the guided case loop, and the stall nudge.
/// </summary>
public sealed class GhostDragDemoMB : MonoBehaviour
{
    private static GhostDragDemoMB _active;

    private BoardTileView _from, _to;
    private RectTransform _ghostRt;
    private Image _ghost;
    private CanvasGroup _cg;

    private const float SlideSeconds = 0.75f;
    private const float FadeSeconds  = 0.18f;
    private const float RestSeconds  = 1.1f;
    private const float GhostAlpha   = 0.65f;

    /// <summary>Start (or retarget) the looping demo from one tile onto another.</summary>
    public static void Show(BoardTileView from, BoardTileView to)
    {
        if (from == null || to == null) return;
        Hide();

        var go = new GameObject("__GhostDragDemo", typeof(Canvas));
        DontDestroyOnLoad(go);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4600; // over board + chips, under DragGhost (5000)

        _active = go.AddComponent<GhostDragDemoMB>();
        _active._from = from;
        _active._to   = to;
        _active.BuildGhost();
        _active.StartCoroutine(_active.Loop());
    }

    public static void Hide()
    {
        if (_active == null) return;
        Destroy(_active.gameObject);
        _active = null;
    }

    private void BuildGhost()
    {
        var ghostGo = new GameObject("Ghost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        ghostGo.transform.SetParent(transform, false);
        _ghostRt = (RectTransform)ghostGo.transform;
        _ghost   = ghostGo.GetComponent<Image>();
        _cg      = ghostGo.GetComponent<CanvasGroup>();
        _ghost.raycastTarget = false;
        _ghost.preserveAspect = true;
        _cg.alpha = 0f;
        _cg.blocksRaycasts = false;
        _cg.interactable = false;
    }

    private void Update()
    {
        // Either tile gone/emptied (merged, moved away, consumed) → job done.
        if (!TileUsable(_from) || !TileUsable(_to))
            Hide();
    }

    private static bool TileUsable(BoardTileView v)
        => v != null && !v.IsEmpty && v.itemImage != null;

    private IEnumerator Loop()
    {
        while (true)
        {
            // Pause invisibly while the player is actually dragging (the source
            // icon hides during a real drag) — never fight the player's finger.
            if (!TileUsable(_from) || !TileUsable(_to) ||
                !_from.itemImage.enabled || !_to.itemImage.enabled)
            {
                _cg.alpha = 0f;
                yield return null;
                continue;
            }

            // Overlay canvases: world position == screen pixels, so the source
            // image's world corners give both position and pixel size directly.
            var corners = new Vector3[4];
            ((RectTransform)_from.itemImage.transform).GetWorldCorners(corners);
            var size  = new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
            var start = (Vector3)((corners[0] + (Vector3)(size * 0.5f)));
            var end   = _to.itemImage.transform.position;

            _ghost.sprite = _from.itemImage.sprite;
            _ghostRt.sizeDelta = size;
            _ghostRt.position = start;

            // fade in
            for (float t = 0f; t < FadeSeconds; t += Time.unscaledDeltaTime)
            {
                _cg.alpha = Mathf.Lerp(0f, GhostAlpha, t / FadeSeconds);
                yield return null;
            }
            _cg.alpha = GhostAlpha;

            // slide (smoothstep ease)
            for (float t = 0f; t < SlideSeconds; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / SlideSeconds);
                _ghostRt.position = Vector3.Lerp(start, end, k);
                yield return null;
            }

            // fade out at the destination — reads as "they become one"
            for (float t = 0f; t < FadeSeconds; t += Time.unscaledDeltaTime)
            {
                _cg.alpha = Mathf.Lerp(GhostAlpha, 0f, t / FadeSeconds);
                yield return null;
            }
            _cg.alpha = 0f;

            yield return new WaitForSecondsRealtime(RestSeconds);
        }
    }

    private void OnDestroy()
    {
        if (_active == this) _active = null;
    }
}
