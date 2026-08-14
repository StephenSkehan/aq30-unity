using UnityEngine;

namespace AQ.UI.HUD
{
    /// <summary>
    /// Shifts the HUD down by however much the device safe area actually
    /// overlaps it (notch / Dynamic Island). The scene's SafeAreaRoot has no
    /// driver script (cleanup casualty), so this self-installs onto HUDImage
    /// at runtime, same idiom as the other boot-installed components. Editor
    /// safe area is the full screen, so this is a no-op there. Measures real
    /// overlap rather than applying the full inset: the scene layout already
    /// carries most of the cushion and full-inset shifting would push the HUD
    /// into the board.
    /// </summary>
    public sealed class HudSafeAreaNudgeMB : MonoBehaviour
    {
        // The plate carries dead space above the pills (and the bust overflows
        // its frame by design), so the plate TOP may tuck under the notch edge
        // as long as the pills stay clear. Shifting by the full plate overlap
        // over-corrected and drove the HUD into the leads bar (device finding
        // 2026-08-14). Canvas units; raise to lift the HUD, lower if a device
        // ever clips pill content again.
        private const float ContentHeadroom = 28f;

        private float _applied;         // canvas units already shifted
        private Vector2Int _lastScreen; // re-evaluate on resolution change only

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var hud = GameObject.Find("HUDImage");
            if (hud != null && hud.GetComponent<HudSafeAreaNudgeMB>() == null)
                hud.AddComponent<HudSafeAreaNudgeMB>();
        }

        private void Start() => Apply();

        private void Update()
        {
            if (Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
                Apply();
        }

        private void Apply()
        {
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            var canvas = GetComponentInParent<Canvas>();
            var rt = (RectTransform)transform;
            if (canvas == null) return;

            // Undo any previous nudge so re-evaluation starts from the layout truth.
            if (_applied != 0f)
            {
                rt.anchoredPosition += new Vector2(0f, _applied);
                _applied = 0f;
            }

            // World corners of an overlay canvas are screen pixels; corner 1 = top-left.
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            float overlapPx = corners[1].y - Screen.safeArea.yMax - ContentHeadroom * scale;
            if (overlapPx <= 0f) return;

            _applied = overlapPx / scale;
            rt.anchoredPosition -= new Vector2(0f, _applied);
        }
    }
}
