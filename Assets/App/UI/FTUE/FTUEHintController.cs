using UnityEngine;
using UnityEngine.UI;

public class FTUEHintController : MonoBehaviour
{
    public RectTransform Target;
    public Vector2 Offset = new Vector2(100, 60);
    public float PulseSpeed = 2f;
    public float PulseScale = 0.2f;
    public float PulseAlpha = 0.35f;

    RectTransform _rt;
    CanvasGroup _cg;
    Vector3 _baseScale;

    void Awake(){
        _rt = GetComponent<RectTransform>();
        _cg = gameObject.AddComponent<CanvasGroup>();
        _baseScale = transform.localScale;
    }

    void OnEnable(){
        BoardPresenter.FirstMerge += OnFirstMerge;
    }
    void OnDisable(){
        BoardPresenter.FirstMerge -= OnFirstMerge;
    }

    void Start(){
        // Try to auto-find a sensible target if none provided
        if(Target == null){
            var board = FindAnyObjectByType<BoardPresenter>();
            if(board != null && board.GridRoot != null && board.GridRoot.childCount > 0){
                var first = board.GridRoot.GetChild(0) as RectTransform;
                if(first != null) Target = first;
            }
        }
        // Render above UI
        transform.SetAsLastSibling();
    }

    void Update(){
        if(Target == null) return;

        // Follow target (same parent/same canvas space is expected)
        if(_rt.parent != Target.parent) _rt.SetParent(Target.parent, worldPositionStays:false);
        _rt.anchoredPosition = Target.anchoredPosition + Offset;

        // Pulse scale + alpha
        float t = (Mathf.Sin(Time.unscaledTime * (Mathf.PI * PulseSpeed)) + 1f) * 0.5f; // 0..1
        float s = 1f + t * PulseScale;
        transform.localScale = _baseScale * s;
        _cg.alpha = Mathf.Lerp(0f, PulseAlpha, t);
    }

    void OnFirstMerge(){
        // Hide permanently after the first successful merge
        Destroy(gameObject);
    }
}
