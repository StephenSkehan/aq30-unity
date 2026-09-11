using UnityEngine;

namespace AQ.App.UI.EvidenceBoard
{
    public class EvidenceBoardZoomPan : MonoBehaviour
    {
        /// <summary>Fired with the screen-px position of a completed tap
        /// (short press, minimal drift). Raw-input path — the board canvas is
        /// boot-created, where GraphicRaycaster clicks are unreliable (same
        /// lesson as the splash and overflow bucket).</summary>
        public System.Action<Vector2> Tapped;

        /// <summary>When this returns true (e.g. a profile/location modal is up),
        /// tracking is dropped: a tap that closes the modal can then never
        /// complete as a board tap on the pin underneath it.</summary>
        public System.Func<bool> SuppressInput;

        private RectTransform _rt;
        private Canvas _canvas;
        private float _minZoom, _maxZoom;
        private Vector2 _boardSize;
        private float _prevPinchDist;

        private Vector2 _downPos;
        private float   _downTime;
        private bool    _tracking;
        private bool    _inputEnabled = true;
        private bool    _suppressUntilRelease;

        /// <summary>
        /// The screen calls this on Open()/Close(). This component lives on
        /// BoardContent, which stays active while the board is "closed" (only the
        /// CanvasGroup is zeroed) — so without the gate it tracked EVERY touch,
        /// including the one that dismissed a replayed dialogue: the board reopened
        /// under the finger and that same touch's Ended fired as a board tap.
        /// Enabling mid-touch suppresses until the finger lifts.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            _tracking = false;
            _suppressUntilRelease = enabled && (Input.touchCount > 0 || Input.GetMouseButton(0));
        }

        private const float RefW = 1080f;
        private const float RefH = 1920f;
        private const float TapMaxDrift = 30f;  // screen px
        private const float TapMaxTime  = 0.4f; // seconds

        public void Init(RectTransform rt, float minZoom, float maxZoom, Vector2 boardSize)
        {
            _rt       = rt;
            _minZoom  = minZoom;
            _maxZoom  = maxZoom;
            _boardSize = boardSize;
            _canvas   = GetComponentInParent<Canvas>();
        }

        /// <summary>Board content size changes per-populate (sized to content bounds).</summary>
        public void SetBoardSize(Vector2 size)
        {
            _boardSize = size;
            ClampPosition();
        }

        void Update()
        {
            if (_rt == null) return;

            if (!_inputEnabled || (SuppressInput != null && SuppressInput()))
            {
                _tracking = false;
                _suppressUntilRelease = Input.touchCount > 0 || Input.GetMouseButton(0);
                return;
            }

            if (_suppressUntilRelease)
            {
                if (Input.touchCount == 0 && !Input.GetMouseButton(0))
                    _suppressUntilRelease = false;
                else { _tracking = false; return; }
            }

            if (Input.touchCount == 1)
                HandleSingleTouch();
            else if (Input.touchCount >= 2)
            {
                _tracking = false;
                HandlePinch();
            }

#if UNITY_EDITOR
            HandleEditorInput();
#endif
        }

        private void HandleSingleTouch()
        {
            var touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _tracking = true;
                    _downPos  = touch.position;
                    _downTime = Time.unscaledTime;
                    break;

                case TouchPhase.Moved:
                    float sf = _canvas != null ? _canvas.scaleFactor : 1f;
                    _rt.anchoredPosition += touch.deltaPosition / sf;
                    ClampPosition();
                    break;

                case TouchPhase.Ended:
                    if (_tracking &&
                        (touch.position - _downPos).magnitude <= TapMaxDrift &&
                        Time.unscaledTime - _downTime <= TapMaxTime)
                        Tapped?.Invoke(touch.position);
                    _tracking = false;
                    break;

                case TouchPhase.Canceled:
                    _tracking = false;
                    break;
            }
        }

        private void HandlePinch()
        {
            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);
            float currDist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _prevPinchDist = currDist;
                return;
            }

            if (_prevPinchDist > 0f)
            {
                float ratio    = currDist / _prevPinchDist;
                float newScale = Mathf.Clamp(_rt.localScale.x * ratio, _minZoom, _maxZoom);
                _rt.localScale = Vector3.one * newScale;
                ClampPosition();
            }
            _prevPinchDist = currDist;
        }

#if UNITY_EDITOR
        private void HandleEditorInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _tracking = true;
                _downPos  = Input.mousePosition;
                _downTime = Time.unscaledTime;
            }

            if (Input.GetMouseButton(0))
            {
                float dx = Input.GetAxis("Mouse X") * 12f;
                float dy = Input.GetAxis("Mouse Y") * 12f;
                _rt.anchoredPosition += new Vector2(dx, dy);
                ClampPosition();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_tracking &&
                    ((Vector2)Input.mousePosition - _downPos).magnitude <= TapMaxDrift &&
                    Time.unscaledTime - _downTime <= TapMaxTime)
                    Tapped?.Invoke(Input.mousePosition);
                _tracking = false;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                float newScale = Mathf.Clamp(_rt.localScale.x + scroll * 0.6f, _minZoom, _maxZoom);
                _rt.localScale = Vector3.one * newScale;
                ClampPosition();
            }
        }
#endif

        private void ClampPosition()
        {
            float scale     = _rt.localScale.x;
            float overflowX = Mathf.Max(0f, (_boardSize.x * scale - RefW) / 2f);
            float overflowY = Mathf.Max(0f, (_boardSize.y * scale - RefH) / 2f);
            var pos         = _rt.anchoredPosition;
            pos.x           = Mathf.Clamp(pos.x, -overflowX, overflowX);
            pos.y           = Mathf.Clamp(pos.y, -overflowY, overflowY);
            _rt.anchoredPosition = pos;
        }
    }
}
