using UnityEditor; using UnityEngine;
public static class ThemeEditorMenu {
  const string ThemeDir = "Assets/ScriptableObjects/Theme";
  [MenuItem("AQ/Theme/Create Noir & Light")]
  public static void CreateThemes(){
    if(!AssetDatabase.IsValidFolder("Assets/ScriptableObjects")) AssetDatabase.CreateFolder("Assets","ScriptableObjects");
    if(!AssetDatabase.IsValidFolder(ThemeDir)) AssetDatabase.CreateFolder("Assets/ScriptableObjects","Theme");
    var noir=ScriptableObject.CreateInstance<ThemeSO>(); noir.name="Theme_Noir";
    noir.Primary=new(0.94f,0.27f,0.27f); noir.Secondary=new(0.10f,0.10f,0.12f); noir.Accent=new(1f,0.85f,0.40f); noir.Primary=Color.white; noir.Panel=new(0.07f,0.07f,0.09f,0.95f);
    AssetDatabase.CreateAsset(noir, ThemeDir+"/Theme_Noir.asset");
    var light=ScriptableObject.CreateInstance<ThemeSO>(); light.name="Theme_Light";
    light.Primary=new(0.20f,0.35f,0.80f); light.Secondary=new(0.92f,0.93f,0.96f); light.Accent=new(0.10f,0.10f,0.12f); light.Primary=Color.black; light.Panel=new(1f,1f,1f,0.95f);
    AssetDatabase.CreateAsset(light, ThemeDir+"/Theme_Light.asset");
    AssetDatabase.SaveAssets(); Selection.activeObject=noir; Debug.Log("[Theme] Created Theme_Noir & Theme_Light in "+ThemeDir);
  } //
  static ThemeSO FindAnyTheme(){ var guids=AssetDatabase.FindAssets("t:ThemeSO"); if(guids!=null && guids.Length>0){ var p=AssetDatabase.GUIDToAssetPath(guids[0]); return AssetDatabase.LoadAssetAtPath<ThemeSO>(p);} return null; }
  [MenuItem("AQ/Theme/Apply Active Theme")]
  public static void ApplyActive(){
    var ctrl = ThemeController.Instance ?? Object.FindFirstObjectByType<ThemeController>();
    if(!ctrl){ var go=new GameObject("ThemeController", typeof(ThemeController)); ctrl=go.GetComponent<ThemeController>(); Debug.Log("[Theme] No ThemeController found - created one in the scene."); }
    if(ctrl.ActiveTheme==null){ var t=FindAnyTheme(); if(t){ ctrl.ActiveTheme=t; Debug.Log("[Theme] ActiveTheme was null - assigned: "+t.name);} else Debug.LogWarning("[Theme] No ThemeSO assets found. Run AQ/Theme/Create Noir & Light first."); }
    (ThemeController.Instance ?? ctrl).ApplyTheme();
  }
}


