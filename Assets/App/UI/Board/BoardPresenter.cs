using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardPresenter : MonoBehaviour
{
    public RectTransform GridRoot;
    public MergeItemView ItemPrefab;
    public List<MergeItemView> LiveItems = new List<MergeItemView>();

    public static event Action FirstMerge;
    static bool _firstMergeFired = false;

    void Start(){
        if(LiveItems.Count == 0) SpawnTestBoard();
        // (FTUE arrow spawn omitted here to keep this file focused)
    }

    public void SpawnTestBoard(){
        Clear();
        for(int i=0;i<2;i++){
            var item = Instantiate(ItemPrefab, GridRoot);
            item.name = "Item_"+i;
            item.Rect.anchoredPosition = new Vector2(-100 + i*200, 0);
            LiveItems.Add(item);
        }
    }

    public void TryMerge(MergeItemView a, MergeItemView b){
        Debug.Log("[Board] Attempt merge "+a.name+" + "+b.name);
        bool ok = a.name.Split('_')[0] == b.name.Split('_')[0];
        if(ok){
            Destroy(b.gameObject);
            a.ResetPos();
            LiveItems.Remove(b);
            Debug.Log("[Board] Merge succeeded!");
            if(!_firstMergeFired){ _firstMergeFired = true; try{ FirstMerge?.Invoke(); } catch{} }
        } else {
            a.ResetPos();
            Debug.Log("[Board] Merge failed");
        }
    }

    public void Clear(){
        foreach(var i in LiveItems) if(i) Destroy(i.gameObject);
        LiveItems.Clear();
        if(GridRoot != null){
            for(int i=GridRoot.childCount-1;i>=0;i--) Destroy(GridRoot.GetChild(i).gameObject);
        }
    }
}
