using UnityEditor; using UnityEngine; using UnityEngine.UI;

public static class MergeItemPrefabMaker {
    const string Path="Assets/UI/Board/MergeItem.prefab";

    [MenuItem("AQ/Prefabs/Make MergeItem")]
    public static void Make(){
        var go   = new GameObject("MergeItem", typeof(RectTransform), typeof(MergeItemView));
        var rt   = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(128,128);

        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(go.transform,false);
        var irt = iconGO.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0.5f,0.5f);
        irt.sizeDelta = new Vector2(128,128);

        var img = iconGO.GetComponent<Image>();
        img.raycastTarget = true; // <-- allow pointer events
#if UNITY_EDITOR
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
        var view = go.GetComponent<MergeItemView>();
        view.Icon = img;

        if(!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets","UI");
        if(!AssetDatabase.IsValidFolder("Assets/UI/Board")) AssetDatabase.CreateFolder("Assets/UI","Board");
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Path);
        Object.DestroyImmediate(go);
        Debug.Log("[Board] Wrote "+Path);
        Selection.activeObject = prefab;
    }
}
