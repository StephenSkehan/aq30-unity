using UnityEditor; using UnityEngine; using UnityEngine.UI;

public static class FTUEArrowPrefabMaker
{
    const string Path = "Assets/Resources/FTUE/FTUE_Arrow.prefab";

    [MenuItem("AQ/Prefabs/Make FTUE Arrow (Resources)")]
    public static void Make(){
        var root = new GameObject("FTUE_Arrow", typeof(RectTransform));
        var cg = root.AddComponent<CanvasGroup>();
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140, 140);

        // Shaft
        var shaft = new GameObject("Shaft", typeof(RectTransform), typeof(Image));
        shaft.transform.SetParent(root.transform, false);
        var srt = shaft.GetComponent<RectTransform>();
        srt.sizeDelta = new Vector2(90, 14);
        srt.anchoredPosition = new Vector2(20, -5);

        // Head
        var head = new GameObject("Head", typeof(RectTransform), typeof(Image));
        head.transform.SetParent(root.transform, false);
        var hrt = head.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(28, 28);
        hrt.anchoredPosition = new Vector2(70, -5);
        head.transform.localRotation = Quaternion.Euler(0,0,45);

#if UNITY_EDITOR
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        shaft.GetComponent<Image>().sprite = sprite;
        head.GetComponent<Image>().sprite  = sprite;
#endif
        shaft.GetComponent<Image>().color = new Color(1,1,1,0.95f);
        head.GetComponent<Image>().color  = new Color(1,1,1,0.95f);

        if(!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets","Resources");
        if(!AssetDatabase.IsValidFolder("Assets/Resources/FTUE")) AssetDatabase.CreateFolder("Assets/Resources","FTUE");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Path);
        Object.DestroyImmediate(root);
        Debug.Log("[FTUE] Wrote "+Path);
        Selection.activeObject = prefab;
    }
}
