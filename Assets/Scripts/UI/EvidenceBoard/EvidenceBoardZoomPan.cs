using UnityEngine;

namespace AQ.App.UI.EvidenceBoard
{
    public class EvidenceBoardZoomPan : MonoBehaviour
    {
        private RectTransform _rt;
        private Canvas _canvas;
        private float _minZoom, _maxZoom;
        private Vector2 _boardSize;
        private float _prevPinchDist;

        private const float RefW = 1080f;
        private const float RefH = 1920f;

        public void Init(RectTransform rt, float minZoom, float maxZoom, Vector2 boardSize)
        {
            _rt       = rt;
            _minZoom  = minZoom;
            _maxZoom  = maxZoom;
            _boardSize = boardSize;
            _canvas   = GetComponentInParent<Canvas>();
        }

        void Update()
        {
            if (_rt == null) return;

            if (Input.touchCount == 1)
                HandleSingleTouch();
            else if (Input.touchCount >= 2)
                HandlePinch();

#if UNITY_EDITOR
            HandleEditorInput();
#endif
        }

        private void HandleSingleTouch()
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float sf = _canvas != null ? _canvas.scaleFactor : 1f;
                _rt.anchoredPosition += touch.deltaPosition / sf;
                ClampPosition();
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
            if (Input.GetMouseButton(0))
            {
                float dx = Input.GetAxis("Mouse X") * 12f;
                float dy = Input.GetAxis("Mouse Y") * 12f;
                _rt.anchoredPosition += new Vector2(dx, dy);
                ClampPosition();
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
