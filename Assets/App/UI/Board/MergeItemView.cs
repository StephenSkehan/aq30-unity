using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MergeItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image Icon;
    [HideInInspector] public RectTransform Rect;
    private Canvas _canvas;
    private CanvasGroup _group;
    private Vector2 _startPos;

    void Awake(){
        Rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _group = gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(Sprite icon){ if(Icon!=null) Icon.sprite = icon; }

    public void OnBeginDrag(PointerEventData e){
        _startPos = Rect.anchoredPosition;
        _group.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e){
        if(_canvas == null) return;
        Rect.anchoredPosition += e.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e){
        _group.blocksRaycasts = true;
        // let BoardInputAdapter decide merge
        BoardInputAdapter.Current?.OnItemDropped(this, e);
    }

    public void ResetPos(){ Rect.anchoredPosition = _startPos; }
}
