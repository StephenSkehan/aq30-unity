using UnityEditor; using UnityEngine; using UnityEngine.UI; using UnityEditor.SceneManagement;
public static class ThemeSceneRepair {
  // Call from CLI: -executeMethod ThemeSceneRepair.Repair "Assets/Scenes/WK2_ThemeDemo.unity"
  public static void Repair(){
    Repair("Assets/Scenes/WK2_ThemeDemo.unity");
  }
  public static void Repair(string scenePath){
    if(!System.IO.File.Exists(scenePath)){ Debug.LogWarning("[Theme] Scene not found: "+scenePath); return; }
    var s = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    // Prefer the controller inside HUD; if HUD exists, drop any extra root controllers
    var hud = GameObject.Find("HUD");
    var controllers = Object.FindObjectsByType<ThemeController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    if(hud!=null){
      var childCtrl = hud.GetComponentInChildren<ThemeController>(true);
      foreach(var c in controllers){ if(c!=childCtrl) Object.DestroyImmediate(c.gameObject); }
      if(childCtrl==null){
        var go=new GameObject("ThemeController", typeof(ThemeController)); go.transform.SetParent(hud.transform,false);
      }
    } else {
      // No HUD in scene? try to keep only one controller
      if(controllers.Length>1){ for(int i=1;i<controllers.Length;i++) Object.DestroyImmediate(controllers[i].gameObject); }
    }
    // Ensure HUDPanel has a ThemeImageBinder
    if(hud!=null){
      var hudPanel = GameObject.Find("HUDPanel");
      if(hudPanel==null){
        var p=new GameObject("HUDPanel", typeof(RectTransform), typeof(Image), typeof(ThemeImageBinder));
        p.transform.SetParent(hud.transform, false);
        var rt=p.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
      } else {
        if(hudPanel.GetComponent<ThemeImageBinder>()==null) hudPanel.AddComponent<ThemeImageBinder>();
      }
    }
    // Assign any ThemeSO if ActiveTheme is null
    var ctrlFinal = Object.FindFirstObjectByType<ThemeController>();
    if(ctrlFinal!=null && ctrlFinal.ActiveTheme==null){
      var guids=AssetDatabase.FindAssets("t:ThemeSO");
      if(guids!=null && guids.Length>0){
        var p=AssetDatabase.GUIDToAssetPath(guids[0]);
        ctrlFinal.ActiveTheme = AssetDatabase.LoadAssetAtPath<ThemeSO>(p);
      }
    }
    EditorSceneManager.MarkSceneDirty(s);
    EditorSceneManager.SaveScene(s);
    Debug.Log("[Theme] Scene repaired & saved: "+scenePath);
  }
}
