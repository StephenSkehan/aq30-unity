using UnityEditor; using UnityEngine; using UnityEngine.UI;
public static class HUDPrefabRepair {
  const string Path="Assets/UI/HUD/HUD.prefab";
  [MenuItem("AQ/Prefabs/Repair HUD Prefab")]
  public static void Repair(){
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Path);
    if(prefab==null){ Debug.LogWarning("[HUD] No prefab at "+Path+" - run Make HUD first."); return; }
    var root=PrefabUtility.LoadPrefabContents(Path); bool changed=false;
    var canvas=root.GetComponent<Canvas>() ?? root.AddComponent<Canvas>(); if(canvas.renderMode!=RenderMode.ScreenSpaceOverlay){ canvas.renderMode=RenderMode.ScreenSpaceOverlay; changed=true; }
    var scaler=root.GetComponent<CanvasScaler>() ?? root.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1080,1920);
    var ctrl=root.transform.Find("ThemeController"); if(!ctrl){ var go=new GameObject("ThemeController", typeof(ThemeController)); go.transform.SetParent(root.transform,false); changed=true; }
    var panel=root.transform.Find("HUDPanel"); if(!panel){ var p=new GameObject("HUDPanel", typeof(RectTransform), typeof(Image), typeof(ThemeImageBinder)); p.transform.SetParent(root.transform,false); var rt=p.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; p.GetComponent<Image>().raycastTarget=false; changed=true; }
    if(changed){ PrefabUtility.SaveAsPrefabAsset(root, Path); Debug.Log("[HUD] Repaired prefab: "+Path); } else { Debug.Log("[HUD] Prefab already valid: "+Path); }
    PrefabUtility.UnloadPrefabContents(root); AssetDatabase.SaveAssets();
  }
}

