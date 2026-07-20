using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App;
using AQ.App.UI.Board;

/// <summary>
/// One-time FTUE hint: pulsing gold arrow above the generator tile.
/// Self-installs via RuntimeInitialize. Dismisses on first generator tap
/// and sets a NarrativeFlag so it never shows again.
/// </summary>
public class GeneratorTapHintMB : MonoBehaviour
{
    const string FtueFlag = "aq.ftue.tap_generator.seen";

    TextMeshProUGUI      _label;
    RectTransform        _rt;
    MergeBoardController _board;
    BoardTileView        _targetView;
    RectTransform        _targetTile;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstalled();
        SceneManager.sceneLoaded += (_, __) => EnsureInstalled();
    }

    /// <summary>
    /// Idempotent install — also used by the FTUE first-merge choreography to
    /// restore the hint after suppressing it during the guided merge.
    /// </summary>
    public static void EnsureInstalled()
    {
        if (NarrativeFlags.Has(FtueFlag)) return;
        if (GameObject.Find("GeneratorTapHint") != null) return;
        var go = new GameObject("GeneratorTapHint");
        go.AddComponent<RectTransform>();
        go.AddComponent<GeneratorTapHintMB>();
    }

    IEnumerator Start()
    {
        yield return null; // wait one frame for board grid to initialise

        _board = FindAnyObjectByType<MergeBoardController>();
        if (_board == null) { Destroy(gameObject); yield break; }

        _targetView = FindGeneratorTile();
        if (_targetView == null) { Destroy(gameObject); yield break; }
        _targetTile = _targetView.GetComponent<RectTransform>();

        var canvas = _targetTile.GetComponentInParent<Canvas>();
        if (canvas == null) { Destroy(gameObject); yield break; }

        _rt = GetComponent<RectTransform>();
        transform.SetParent(canvas.transform, false);
        _rt.sizeDelta = new Vector2(80f, 60f);
        transform.SetAsLastSibling();

        _label = gameObject.AddComponent<TextMeshProUGUI>();
        _label.text = "▼"; // ▼
        _label.fontSize = 52f;
        _label.color = new Color(1f, 0.85f, 0.1f, 1f);
        _label.alignment = TextAlignmentOptions.Center;
        _label.raycastTarget = false;

        MergeBoardController.GeneratorTapped += Dismiss;
    }

    void Update()
    {
        if (_label == null || _board == null) return;

        // Re-find only if the cached tile is no longer a generator (moved/swapped away)
        if (_targetView == null || _targetView.Kind != TileKind.Generator)
        {
            _targetView = FindGeneratorTile();
            if (_targetView == null) return;
            _targetTile = _targetView.GetComponent<RectTransform>();
        }

        // Hover above the generator tile in world space
        var pos = _targetTile.position;
        pos.y += _targetTile.rect.height * _targetTile.lossyScale.y * 0.8f;
        transform.position = pos;

        // Pulse scale + alpha
        float t = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 1.5f) + 1f) * 0.5f;
        transform.localScale = Vector3.one * (1f + t * 0.25f);
        var c = _label.color;
        c.a = Mathf.Lerp(0.4f, 1f, t);
        _label.color = c;
    }

    void OnDestroy() => MergeBoardController.GeneratorTapped -= Dismiss;

    void Dismiss()
    {
        NarrativeFlags.Set(FtueFlag);
        Destroy(gameObject);
    }

    BoardTileView FindGeneratorTile()
    {
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var tile = _board.Get(r, c);
                if (tile != null && tile.Kind == TileKind.Generator)
                    return tile;
            }
        return null;
    }
}
