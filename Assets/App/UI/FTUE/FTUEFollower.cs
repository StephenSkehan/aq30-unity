using UnityEngine;

public class FTUEFollower : MonoBehaviour
{
    public RectTransform Target;
    public Vector2 Offset = new Vector2(100, 60);
    public float PulseSpeed = 2f;
    public float PulseScale = 0.2f;
    public float PulseAlpha = 0.35f;

    RectTransform _rt; CanvasGroup _cg; Vector3 _base;

    void Awake(){ _rt = GetComponent<RectTransform>(); _cg = GetComponent<CanvasGroup>(); _base = transform.localScale; }
    void OnEnable(){ BoardPresenter.FirstMerge += Hide; }
    void OnDisable(){ BoardPresenter.FirstMerge -= Hide; }

    void Update(){
        if(Target == null) return;
        if(_rt.parent != Target.parent) _rt.SetParent(Target.parent, false);
        _rt.anchoredPosition = Target.anchoredPosition + Offset;
        float t = (Mathf.Sin(Time.unscaledTime * (Mathf.PI * PulseSpeed)) + 1f) * 0.5f;
        transform.localScale = _base * (1f + t * PulseScale);
        if(_cg) _cg.alpha = Mathf.Lerp(0f, PulseAlpha, t);
    }
    void Hide(){ Destroy(gameObject); }
}
