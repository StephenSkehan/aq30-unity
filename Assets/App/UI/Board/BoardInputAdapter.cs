using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BoardInputAdapter : MonoBehaviour, IDropHandler
{
    public static BoardInputAdapter Current;
    public BoardPresenter Presenter;

    void Awake(){ Current = this; }

    public void OnDrop(PointerEventData e){
        // fallback, not used directly
    }

    public void OnItemDropped(MergeItemView item, PointerEventData e){
        // Find overlapping item
        foreach(var other in Presenter.LiveItems){
            if(other==item) continue;
            if(RectTransformUtility.RectangleContainsScreenPoint(other.Rect, e.position)){
                Debug.Log("[Board] Drop on "+other.name);
                Presenter.TryMerge(item, other);
                return;
            }
        }
        item.ResetPos();
    }
}
