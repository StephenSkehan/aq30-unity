using UnityEngine; using UnityEditor; using UnityEngine.UI;
public static class HUDPrefabMaker {
  const string Path="Assets/UI/HUD/HUD.prefab";
  [MenuItem("AQ/Prefabs/Make HUD")]
  public static void Make(){
    var canvas=new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    var c=canvas.GetComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay;
    var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1080,1920);
    var ctrl=new GameObject("ThemeController", typeof(ThemeController)); ctrl.transform.SetParent(canvas.transform, false);
    var panel=new GameObject("HUDPanel", typeof(RectTransform), typeof(Image), typeof(ThemeImageBinder));
    panel.transform.SetParent(canvas.transform, false); var rt=panel.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; panel.GetComponent<Image>().raycastTarget=false;
    if(!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets","UI");
    if(!AssetDatabase.IsValidFolder("Assets/UI/HUD")) AssetDatabase.CreateFolder("Assets/UI","HUD");
    var prefab=PrefabUtility.SaveAsPrefabAsset(canvas, Path); Object.DestroyImmediate(canvas);
    Debug.Log("[HUD] Wrote "+Path+" (includes ThemeController & HUDPanel/ThemeImageBinder)"); Selection.activeObject=prefab;
  }
}
