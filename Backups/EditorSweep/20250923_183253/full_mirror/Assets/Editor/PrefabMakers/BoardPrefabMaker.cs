using UnityEditor; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems;

public static class BoardPrefabMaker {
    const string Path="Assets/UI/Board/Board.prefab";

    [MenuItem("AQ/Prefabs/Make Board")]
    public static void Make(){
        var canvas = new GameObject("BoardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = canvas.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920);

        // Just an EventSystem object; module selection handled by Scene fixer or user project defaults
        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.transform.SetParent(canvas.transform.parent, false);

        var board = new GameObject("Board", typeof(RectTransform), typeof(BoardPresenter), typeof(BoardInputAdapter));
        board.transform.SetParent(canvas.transform, false);
        var brt = board.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f,0.5f);
        brt.sizeDelta = new Vector2(600, 600);

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(board.transform, false);
        var bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one; bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
#if UNITY_EDITOR
        bg.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        bg.GetComponent<Image>().color = new Color(0,0,0,0.15f);
#endif

        var grid = new GameObject("GridRoot", typeof(RectTransform));
        grid.transform.SetParent(board.transform, false);
        var grt = grid.GetComponent<RectTransform>();
        grt.anchorMin = grt.anchorMax = grt.pivot = new Vector2(0.5f,0.5f);
        grt.sizeDelta = new Vector2(560,560);

        var pres  = board.GetComponent<BoardPresenter>();
        var adapt = board.GetComponent<BoardInputAdapter>();
        pres.GridRoot = grt; adapt.Presenter = pres;

        if(!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets","UI");
        if(!AssetDatabase.IsValidFolder("Assets/UI/Board")) AssetDatabase.CreateFolder("Assets/UI","Board");
        var prefab = PrefabUtility.SaveAsPrefabAsset(canvas, Path);
        Object.DestroyImmediate(canvas); Object.DestroyImmediate(es);
        Debug.Log("[Board] Wrote "+Path+" (Canvas-only; use scene fixer to pick input module)");
        Selection.activeObject = prefab;
    }
}
